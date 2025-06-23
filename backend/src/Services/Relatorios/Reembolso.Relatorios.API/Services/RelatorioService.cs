using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Reembolso.Relatorios.API.Data;
using Reembolso.Relatorios.API.DTOs;
using Reembolso.Relatorios.API.Models;
using Reembolso.Shared.DTOs;
using Reembolso.Shared.Enums;
using System.Text;
using System.Text.Json;

namespace Reembolso.Relatorios.API.Services;

public interface IRelatorioService
{
    Task<PagedResult<RelatorioResumoDto>> ObterTodosAsync(int pagina, int itensPorPagina, FiltroRelatorioDto? filtro = null);
    Task<RelatorioDto?> ObterPorIdAsync(Guid id);
    Task<List<RelatorioResumoDto>> ObterPorUsuarioAsync(Guid usuarioId);
    Task<RelatorioDto> CriarAsync(CriarRelatorioDto criarDto, Guid usuarioId);
    Task<bool> ExcluirAsync(Guid id, Guid usuarioId);
    Task<RelatorioDto?> ProcessarRelatorioAsync(Guid id);
    Task<byte[]?> BaixarRelatorioAsync(Guid id);
    Task<EstatisticasRelatorioDto> ObterEstatisticasAsync();
    Task LimparRelatoriosAntigosAsync();
    Task<DadosRelatorioDto> GerarDadosRelatorioAsync(Guid relatorioId);
}

public class RelatorioService : IRelatorioService
{
    private readonly RelatoriosDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<RelatorioService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public RelatorioService(
        RelatoriosDbContext context,
        IMapper mapper,
        ILogger<RelatorioService> logger,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<PagedResult<RelatorioResumoDto>> ObterTodosAsync(int pagina, int itensPorPagina, FiltroRelatorioDto? filtro = null)
    {
        var query = _context.Relatorios
            .Where(r => r.Ativo)
            .AsQueryable();

        // Aplicar filtros
        if (filtro != null)
        {
            if (filtro.Tipo.HasValue)
            {
                query = query.Where(r => r.Tipo == filtro.Tipo.Value);
            }

            if (filtro.Status.HasValue)
            {
                query = query.Where(r => r.Status == filtro.Status.Value);
            }

            if (filtro.GeradoPorId.HasValue)
            {
                query = query.Where(r => r.GeradoPorId == filtro.GeradoPorId.Value);
            }

            if (filtro.DataInicioGeracao.HasValue)
            {
                query = query.Where(r => r.DataGeracao >= filtro.DataInicioGeracao.Value);
            }

            if (filtro.DataFimGeracao.HasValue)
            {
                query = query.Where(r => r.DataGeracao <= filtro.DataFimGeracao.Value);
            }

            if (!string.IsNullOrWhiteSpace(filtro.Nome))
            {
                query = query.Where(r => r.Nome.Contains(filtro.Nome));
            }
        }

        var totalItens = await query.CountAsync();

        var relatorios = await query
            .OrderByDescending(r => r.DataCriacao)
            .Skip((pagina - 1) * itensPorPagina)
            .Take(itensPorPagina)
            .ToListAsync();

        var relatoriosDto = _mapper.Map<List<RelatorioResumoDto>>(relatorios);

        return new PagedResult<RelatorioResumoDto>(relatoriosDto, totalItens, pagina, itensPorPagina);
    }

    public async Task<RelatorioDto?> ObterPorIdAsync(Guid id)
    {
        var relatorio = await _context.Relatorios
            .FirstOrDefaultAsync(r => r.Id == id && r.Ativo);

        return relatorio != null ? _mapper.Map<RelatorioDto>(relatorio) : null;
    }

    public async Task<List<RelatorioResumoDto>> ObterPorUsuarioAsync(Guid usuarioId)
    {
        var relatorios = await _context.Relatorios
            .Where(r => r.GeradoPorId == usuarioId && r.Ativo)
            .OrderByDescending(r => r.DataCriacao)
            .ToListAsync();

        return _mapper.Map<List<RelatorioResumoDto>>(relatorios);
    }

    public async Task<RelatorioDto> CriarAsync(CriarRelatorioDto criarDto, Guid usuarioId)
    {
        // Validações de negócio
        if (criarDto.DataFim <= criarDto.DataInicio)
        {
            throw new ArgumentException("Data de fim deve ser posterior à data de início");
        }

        if (criarDto.DataInicio > DateTime.Now)
        {
            throw new ArgumentException("Data de início não pode ser futura");
        }

        var relatorio = _mapper.Map<Relatorio>(criarDto);
        relatorio.Id = Guid.NewGuid();
        relatorio.Status = StatusRelatorio.Pendente;
        relatorio.GeradoPorId = usuarioId;
        relatorio.CriadoPor = usuarioId.ToString();

        _context.Relatorios.Add(relatorio);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Relatório criado: {RelatorioId} - {Nome}", relatorio.Id, relatorio.Nome);

        // Processar relatório em background
        _ = Task.Run(async () => await ProcessarRelatorioAsync(relatorio.Id));

        return _mapper.Map<RelatorioDto>(relatorio);
    }

    public async Task<bool> ExcluirAsync(Guid id, Guid usuarioId)
    {
        var relatorio = await _context.Relatorios.FindAsync(id);
        if (relatorio == null || !relatorio.Ativo)
        {
            return false;
        }

        // Verificar se o usuário pode excluir (apenas quem criou ou admin)
        if (relatorio.GeradoPorId != usuarioId)
        {
            throw new UnauthorizedAccessException("Usuário não autorizado a excluir este relatório");
        }

        // Soft delete
        relatorio.Ativo = false;
        relatorio.AtualizadoPor = usuarioId.ToString();

        // Remover arquivo físico se existir
        if (!string.IsNullOrEmpty(relatorio.CaminhoArquivo))
        {
            try
            {
                var caminhoCompleto = Path.Combine("wwwroot", relatorio.CaminhoArquivo.TrimStart('/'));
                if (File.Exists(caminhoCompleto))
                {
                    File.Delete(caminhoCompleto);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao excluir arquivo do relatório {RelatorioId}", id);
            }
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Relatório excluído: {RelatorioId}", id);
        return true;
    }

    public async Task<RelatorioDto?> ProcessarRelatorioAsync(Guid id)
    {
        var relatorio = await _context.Relatorios.FindAsync(id);
        if (relatorio == null || !relatorio.Ativo)
        {
            return null;
        }

        try
        {
            relatorio.Status = StatusRelatorio.Processando;
            relatorio.DataGeracao = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Iniciando processamento do relatório {RelatorioId}", id);

            // Gerar dados do relatório
            var dadosRelatorio = await GerarDadosRelatorioAsync(id);

            // Gerar arquivo (PDF ou Excel)
            var (caminhoArquivo, nomeArquivo, tamanhoBytes, tipoConteudo) = await GerarArquivoRelatorioAsync(relatorio, dadosRelatorio);

            // Atualizar relatório com informações do arquivo
            relatorio.Status = StatusRelatorio.Concluido;
            relatorio.DataConclusao = DateTime.UtcNow;
            relatorio.CaminhoArquivo = caminhoArquivo;
            relatorio.NomeArquivo = nomeArquivo;
            relatorio.TamanhoBytes = tamanhoBytes;
            relatorio.TipoConteudo = tipoConteudo;
            relatorio.TotalRegistros = dadosRelatorio.Dados.Count;
            relatorio.ValorTotal = CalcularValorTotal(dadosRelatorio);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Relatório processado com sucesso: {RelatorioId}", id);

            return _mapper.Map<RelatorioDto>(relatorio);
        }
        catch (Exception ex)
        {
            relatorio.Status = StatusRelatorio.Erro;
            relatorio.MensagemErro = ex.Message;
            await _context.SaveChangesAsync();

            _logger.LogError(ex, "Erro ao processar relatório {RelatorioId}", id);
            throw;
        }
    }

    public async Task<byte[]?> BaixarRelatorioAsync(Guid id)
    {
        var relatorio = await _context.Relatorios.FindAsync(id);
        if (relatorio == null || !relatorio.Ativo || !relatorio.PodeSerBaixado)
        {
            return null;
        }

        try
        {
            var caminhoCompleto = Path.Combine("wwwroot", relatorio.CaminhoArquivo!.TrimStart('/'));
            if (File.Exists(caminhoCompleto))
            {
                return await File.ReadAllBytesAsync(caminhoCompleto);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao ler arquivo do relatório {RelatorioId}", id);
        }

        return null;
    }

    public async Task<EstatisticasRelatorioDto> ObterEstatisticasAsync()
    {
        var relatorios = await _context.Relatorios
            .Where(r => r.Ativo)
            .ToListAsync();

        var estatisticas = new EstatisticasRelatorioDto
        {
            TotalRelatorios = relatorios.Count,
            RelatoriosProcessando = relatorios.Count(r => r.Status == StatusRelatorio.Processando),
            RelatoriosConcluidos = relatorios.Count(r => r.Status == StatusRelatorio.Concluido),
            RelatoriosComErro = relatorios.Count(r => r.Status == StatusRelatorio.Erro),
            TamanhoTotalBytes = relatorios.Where(r => r.TamanhoBytes.HasValue).Sum(r => r.TamanhoBytes!.Value),
            RelatoriosParaExclusao = relatorios.Count(r => r.DeveSerExcluido)
        };

        // Relatórios por tipo
        estatisticas.RelatoriosPorTipo = relatorios
            .GroupBy(r => r.Tipo)
            .ToDictionary(g => g.Key, g => g.Count());

        // Relatórios por mês
        estatisticas.RelatoriosPorMes = relatorios
            .Where(r => r.DataGeracao.HasValue)
            .GroupBy(r => r.DataGeracao!.Value.ToString("yyyy-MM"))
            .ToDictionary(g => g.Key, g => g.Count());

        // Tempo médio de processamento
        var temposProcessamento = relatorios
            .Where(r => r.TempoProcessamento.HasValue)
            .Select(r => r.TempoProcessamento!.Value)
            .ToList();

        if (temposProcessamento.Any())
        {
            var ticksMedia = (long)temposProcessamento.Average(t => t.Ticks);
            estatisticas.TempoMedioProcessamento = new TimeSpan(ticksMedia);
        }

        // Tamanho total formatado
        estatisticas.TamanhoTotalFormatado = FormatarTamanho(estatisticas.TamanhoTotalBytes);

        return estatisticas;
    }

    public async Task LimparRelatoriosAntigosAsync()
    {
        var relatoriosParaExcluir = await _context.Relatorios
            .Where(r => r.Ativo && r.DeveSerExcluido)
            .ToListAsync();

        foreach (var relatorio in relatoriosParaExcluir)
        {
            // Remover arquivo físico
            if (!string.IsNullOrEmpty(relatorio.CaminhoArquivo))
            {
                try
                {
                    var caminhoCompleto = Path.Combine("wwwroot", relatorio.CaminhoArquivo.TrimStart('/'));
                    if (File.Exists(caminhoCompleto))
                    {
                        File.Delete(caminhoCompleto);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Erro ao excluir arquivo do relatório {RelatorioId}", relatorio.Id);
                }
            }

            // Soft delete
            relatorio.Ativo = false;
        }

        if (relatoriosParaExcluir.Any())
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Excluídos {Count} relatórios antigos", relatoriosParaExcluir.Count);
        }
    }

    public async Task<DadosRelatorioDto> GerarDadosRelatorioAsync(Guid relatorioId)
    {
        var relatorio = await _context.Relatorios.FindAsync(relatorioId);
        if (relatorio == null)
        {
            throw new ArgumentException("Relatório não encontrado");
        }

        var dadosRelatorio = new DadosRelatorioDto
        {
            Titulo = relatorio.Nome,
            DataGeracao = DateTime.UtcNow,
            PeriodoInicio = relatorio.DataInicio,
            PeriodoFim = relatorio.DataFim
        };

        // Adicionar parâmetros
        dadosRelatorio.Parametros["Tipo"] = relatorio.Tipo.ToString();
        if (relatorio.ColaboradorId.HasValue)
            dadosRelatorio.Parametros["ColaboradorId"] = relatorio.ColaboradorId.Value;
        if (relatorio.DepartamentoId.HasValue)
            dadosRelatorio.Parametros["DepartamentoId"] = relatorio.DepartamentoId.Value;
        if (relatorio.TipoDespesa.HasValue)
            dadosRelatorio.Parametros["TipoDespesa"] = relatorio.TipoDespesa.Value.ToString();
        if (relatorio.StatusSolicitacao.HasValue)
            dadosRelatorio.Parametros["StatusSolicitacao"] = relatorio.StatusSolicitacao.Value.ToString();

        // Buscar dados baseado no tipo de relatório
        dadosRelatorio.Dados = await BuscarDadosRelatorioAsync(relatorio);

        // Gerar resumo
        dadosRelatorio.Resumo = GerarResumoRelatorio(dadosRelatorio.Dados, relatorio.Tipo);

        return dadosRelatorio;
    }

    private async Task<List<Dictionary<string, object>>> BuscarDadosRelatorioAsync(Relatorio relatorio)
    {
        // Simular busca de dados dos outros microserviços
        // Em um cenário real, isso seria feito via HTTP calls para os outros serviços
        var dados = new List<Dictionary<string, object>>();

        switch (relatorio.Tipo)
        {
            case TipoRelatorio.SolicitacoesPorPeriodo:
                dados = await BuscarSolicitacoesPorPeriodoAsync(relatorio);
                break;
            case TipoRelatorio.ReembolsosPorColaborador:
                dados = await BuscarReembolsosPorColaboradorAsync(relatorio);
                break;
            case TipoRelatorio.ReembolsosPorDepartamento:
                dados = await BuscarReembolsosPorDepartamentoAsync(relatorio);
                break;
            case TipoRelatorio.EstatisticasGerais:
                dados = await BuscarEstatisticasGeraisAsync(relatorio);
                break;
            case TipoRelatorio.RelatorioFinanceiro:
                dados = await BuscarRelatorioFinanceiroAsync(relatorio);
                break;
        }

        return dados;
    }

    private async Task<List<Dictionary<string, object>>> BuscarSolicitacoesPorPeriodoAsync(Relatorio relatorio)
    {
        // Simular dados de solicitações
        await Task.Delay(100); // Simular chamada HTTP
        
        var dados = new List<Dictionary<string, object>>();
        var random = new Random();
        
        for (int i = 1; i <= 50; i++)
        {
            dados.Add(new Dictionary<string, object>
            {
                ["Id"] = Guid.NewGuid(),
                ["Titulo"] = $"Solicitação {i}",
                ["ColaboradorNome"] = $"Colaborador {i}",
                ["TipoDespesa"] = ((TipoDespesa)(i % 5)).ToString(),
                ["ValorSolicitado"] = random.Next(100, 2000),
                ["Status"] = ((StatusSolicitacao)(i % 6)).ToString(),
                ["DataCriacao"] = relatorio.DataInicio.AddDays(random.Next(0, (relatorio.DataFim - relatorio.DataInicio).Days))
            });
        }
        
        return dados;
    }

    private async Task<List<Dictionary<string, object>>> BuscarReembolsosPorColaboradorAsync(Relatorio relatorio)
    {
        await Task.Delay(100);
        return new List<Dictionary<string, object>>();
    }

    private async Task<List<Dictionary<string, object>>> BuscarReembolsosPorDepartamentoAsync(Relatorio relatorio)
    {
        await Task.Delay(100);
        return new List<Dictionary<string, object>>();
    }

    private async Task<List<Dictionary<string, object>>> BuscarEstatisticasGeraisAsync(Relatorio relatorio)
    {
        await Task.Delay(100);
        return new List<Dictionary<string, object>>();
    }

    private async Task<List<Dictionary<string, object>>> BuscarRelatorioFinanceiroAsync(Relatorio relatorio)
    {
        await Task.Delay(100);
        return new List<Dictionary<string, object>>();
    }

    private async Task<(string caminhoArquivo, string nomeArquivo, long tamanhoBytes, string tipoConteudo)> GerarArquivoRelatorioAsync(
        Relatorio relatorio, DadosRelatorioDto dadosRelatorio)
    {
        var diretorioRelatorios = Path.Combine("wwwroot", "relatorios");
        Directory.CreateDirectory(diretorioRelatorios);

        var nomeArquivo = $"{relatorio.Nome.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd_HHmmss}";
        var extensao = relatorio.Tipo == TipoRelatorio.RelatorioFinanceiro ? ".xlsx" : ".pdf";
        var nomeArquivoCompleto = $"{nomeArquivo}{extensao}";
        var caminhoCompleto = Path.Combine(diretorioRelatorios, nomeArquivoCompleto);

        byte[] conteudoArquivo;
        string tipoConteudo;

        if (extensao == ".pdf")
        {
            conteudoArquivo = await GerarPDFAsync(dadosRelatorio);
            tipoConteudo = "application/pdf";
        }
        else
        {
            conteudoArquivo = await GerarExcelAsync(dadosRelatorio);
            tipoConteudo = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        }

        await File.WriteAllBytesAsync(caminhoCompleto, conteudoArquivo);

        var caminhoRelativo = $"/relatorios/{nomeArquivoCompleto}";
        return (caminhoRelativo, nomeArquivoCompleto, conteudoArquivo.Length, tipoConteudo);
    }

    private async Task<byte[]> GerarPDFAsync(DadosRelatorioDto dadosRelatorio)
    {
        // Simular geração de PDF
        await Task.Delay(1000);
        
        var conteudo = $"Relatório: {dadosRelatorio.Titulo}\n";
        conteudo += $"Período: {dadosRelatorio.PeriodoInicio:dd/MM/yyyy} a {dadosRelatorio.PeriodoFim:dd/MM/yyyy}\n";
        conteudo += $"Gerado em: {dadosRelatorio.DataGeracao:dd/MM/yyyy HH:mm}\n\n";
        conteudo += $"Total de registros: {dadosRelatorio.Dados.Count}\n";
        
        return Encoding.UTF8.GetBytes(conteudo);
    }

    private async Task<byte[]> GerarExcelAsync(DadosRelatorioDto dadosRelatorio)
    {
        // Simular geração de Excel
        await Task.Delay(1500);
        
        var json = JsonSerializer.Serialize(dadosRelatorio, new JsonSerializerOptions { WriteIndented = true });
        return Encoding.UTF8.GetBytes(json);
    }

    private Dictionary<string, object> GerarResumoRelatorio(List<Dictionary<string, object>> dados, TipoRelatorio tipo)
    {
        var resumo = new Dictionary<string, object>
        {
            ["TotalRegistros"] = dados.Count,
            ["DataGeracao"] = DateTime.UtcNow
        };

        if (dados.Any())
        {
            // Calcular estatísticas baseadas no tipo
            switch (tipo)
            {
                case TipoRelatorio.SolicitacoesPorPeriodo:
                    var valores = dados.Where(d => d.ContainsKey("ValorSolicitado"))
                                      .Select(d => Convert.ToDecimal(d["ValorSolicitado"]))
                                      .ToList();
                    if (valores.Any())
                    {
                        resumo["ValorTotal"] = valores.Sum();
                        resumo["ValorMedio"] = valores.Average();
                        resumo["ValorMaximo"] = valores.Max();
                        resumo["ValorMinimo"] = valores.Min();
                    }
                    break;
            }
        }

        return resumo;
    }

    private decimal? CalcularValorTotal(DadosRelatorioDto dadosRelatorio)
    {
        if (dadosRelatorio.Resumo.ContainsKey("ValorTotal"))
        {
            return Convert.ToDecimal(dadosRelatorio.Resumo["ValorTotal"]);
        }
        return null;
    }

    private string FormatarTamanho(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB" };
        int counter = 0;
        decimal number = bytes;
        
        while (Math.Round(number / 1024) >= 1)
        {
            number /= 1024;
            counter++;
        }
        
        return $"{number:n1} {suffixes[counter]}";
    }
}