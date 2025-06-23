using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Reembolso.Shared.DTOs;
using Reembolso.Shared.Enums;
using Reembolso.SolicitacoesReembolso.API.Data;
using Reembolso.SolicitacoesReembolso.API.DTOs;
using Reembolso.SolicitacoesReembolso.API.Models;

namespace Reembolso.SolicitacoesReembolso.API.Services;

public interface ISolicitacaoReembolsoService
{
    Task<PagedResult<SolicitacaoReembolsoResumoDto>> ObterTodosAsync(int pagina, int itensPorPagina, 
        Guid? colaboradorId = null, StatusSolicitacao? status = null, TipoDespesa? tipoDespesa = null, 
        DateTime? dataInicio = null, DateTime? dataFim = null, string? filtro = null);
    Task<SolicitacaoReembolsoDto?> ObterPorIdAsync(Guid id);
    Task<List<SolicitacaoReembolsoResumoDto>> ObterPorColaboradorAsync(Guid colaboradorId);
    Task<List<SolicitacaoReembolsoResumoDto>> ObterPendenteAprovacaoAsync();
    Task<List<SolicitacaoReembolsoResumoDto>> ObterAprovadasAsync();
    Task<SolicitacaoReembolsoDto> CriarAsync(CriarSolicitacaoReembolsoDto criarDto, Guid usuarioId);
    Task<SolicitacaoReembolsoDto?> AtualizarAsync(Guid id, AtualizarSolicitacaoReembolsoDto atualizarDto, Guid usuarioId);
    Task<bool> ExcluirAsync(Guid id, Guid usuarioId);
    Task<SolicitacaoReembolsoDto?> EnviarParaAprovacaoAsync(Guid id, Guid usuarioId);
    Task<SolicitacaoReembolsoDto?> AprovarAsync(Guid id, AprovarSolicitacaoDto aprovarDto, Guid usuarioId, string nomeUsuario);
    Task<SolicitacaoReembolsoDto?> RejeitarAsync(Guid id, RejeitarSolicitacaoDto rejeitarDto, Guid usuarioId, string nomeUsuario);
    Task<SolicitacaoReembolsoDto?> PagarAsync(Guid id, PagarSolicitacaoDto pagarDto, Guid usuarioId, string nomeUsuario);
    Task<SolicitacaoReembolsoDto?> CancelarAsync(Guid id, CancelarSolicitacaoDto cancelarDto, Guid usuarioId, string nomeUsuario);
    Task<Dictionary<string, object>> ObterEstatisticasAsync(Guid? colaboradorId = null);
}

public class SolicitacaoReembolsoService : ISolicitacaoReembolsoService
{
    private readonly SolicitacoesReembolsoDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<SolicitacaoReembolsoService> _logger;

    public SolicitacaoReembolsoService(
        SolicitacoesReembolsoDbContext context,
        IMapper mapper,
        ILogger<SolicitacaoReembolsoService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PagedResult<SolicitacaoReembolsoResumoDto>> ObterTodosAsync(int pagina, int itensPorPagina,
        Guid? colaboradorId = null, StatusSolicitacao? status = null, TipoDespesa? tipoDespesa = null,
        DateTime? dataInicio = null, DateTime? dataFim = null, string? filtro = null)
    {
        var query = _context.SolicitacoesReembolso
            .Include(s => s.Anexos)
            .Where(s => s.Ativo)
            .AsQueryable();

        // Aplicar filtros
        if (colaboradorId.HasValue)
        {
            query = query.Where(s => s.ColaboradorId == colaboradorId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(s => s.Status == status.Value);
        }

        if (tipoDespesa.HasValue)
        {
            query = query.Where(s => s.TipoDespesa == tipoDespesa.Value);
        }

        if (dataInicio.HasValue)
        {
            query = query.Where(s => s.DataDespesa >= dataInicio.Value);
        }

        if (dataFim.HasValue)
        {
            query = query.Where(s => s.DataDespesa <= dataFim.Value);
        }

        if (!string.IsNullOrWhiteSpace(filtro))
        {
            query = query.Where(s => s.Titulo.Contains(filtro) || 
                                   (s.Descricao != null && s.Descricao.Contains(filtro)));
        }

        var totalItens = await query.CountAsync();

        var solicitacoes = await query
            .OrderByDescending(s => s.DataCriacao)
            .Skip((pagina - 1) * itensPorPagina)
            .Take(itensPorPagina)
            .ToListAsync();

        var solicitacoesDto = _mapper.Map<List<SolicitacaoReembolsoResumoDto>>(solicitacoes);

        return new PagedResult<SolicitacaoReembolsoResumoDto>(solicitacoesDto, totalItens, pagina, itensPorPagina);
    }

    public async Task<SolicitacaoReembolsoDto?> ObterPorIdAsync(Guid id)
    {
        var solicitacao = await _context.SolicitacoesReembolso
            .Include(s => s.Anexos.Where(a => a.Ativo))
            .Include(s => s.HistoricoStatus)
            .FirstOrDefaultAsync(s => s.Id == id && s.Ativo);

        return solicitacao != null ? _mapper.Map<SolicitacaoReembolsoDto>(solicitacao) : null;
    }

    public async Task<List<SolicitacaoReembolsoResumoDto>> ObterPorColaboradorAsync(Guid colaboradorId)
    {
        var solicitacoes = await _context.SolicitacoesReembolso
            .Include(s => s.Anexos)
            .Where(s => s.ColaboradorId == colaboradorId && s.Ativo)
            .OrderByDescending(s => s.DataCriacao)
            .ToListAsync();

        return _mapper.Map<List<SolicitacaoReembolsoResumoDto>>(solicitacoes);
    }

    public async Task<List<SolicitacaoReembolsoResumoDto>> ObterPendenteAprovacaoAsync()
    {
        var solicitacoes = await _context.SolicitacoesReembolso
            .Include(s => s.Anexos)
            .Where(s => s.Status == StatusSolicitacao.PendenteAprovacaoFinanceira && s.Ativo)
            .OrderBy(s => s.DataCriacao)
            .ToListAsync();

        return _mapper.Map<List<SolicitacaoReembolsoResumoDto>>(solicitacoes);
    }

    public async Task<List<SolicitacaoReembolsoResumoDto>> ObterAprovadasAsync()
    {
        var solicitacoes = await _context.SolicitacoesReembolso
            .Include(s => s.Anexos)
            .Where(s => s.Status == StatusSolicitacao.Aprovada && s.Ativo)
            .OrderBy(s => s.DataAprovacao)
            .ToListAsync();

        return _mapper.Map<List<SolicitacaoReembolsoResumoDto>>(solicitacoes);
    }

    public async Task<SolicitacaoReembolsoDto> CriarAsync(CriarSolicitacaoReembolsoDto criarDto, Guid usuarioId)
    {
        // Validações de negócio
        if (criarDto.DataDespesa > DateTime.Now)
        {
            throw new ArgumentException("Data da despesa não pode ser futura");
        }

        if (criarDto.DataDespesa < DateTime.Now.AddYears(-1))
        {
            throw new ArgumentException("Data da despesa não pode ser superior a 1 ano");
        }

        var solicitacao = _mapper.Map<SolicitacaoReembolso>(criarDto);
        solicitacao.Id = Guid.NewGuid();
        solicitacao.Status = StatusSolicitacao.Rascunho;
        solicitacao.CriadoPor = usuarioId.ToString();

        _context.SolicitacoesReembolso.Add(solicitacao);

        // Adicionar histórico inicial
        await AdicionarHistoricoStatusAsync(solicitacao.Id, StatusSolicitacao.Rascunho, StatusSolicitacao.Rascunho, 
            usuarioId, "Solicitação criada", "Sistema");

        await _context.SaveChangesAsync();

        _logger.LogInformation("Solicitação de reembolso criada: {SolicitacaoId} - {Titulo}", 
            solicitacao.Id, solicitacao.Titulo);

        return _mapper.Map<SolicitacaoReembolsoDto>(solicitacao);
    }

    public async Task<SolicitacaoReembolsoDto?> AtualizarAsync(Guid id, AtualizarSolicitacaoReembolsoDto atualizarDto, Guid usuarioId)
    {
        var solicitacao = await _context.SolicitacoesReembolso.FindAsync(id);
        if (solicitacao == null || !solicitacao.Ativo)
        {
            return null;
        }

        if (!solicitacao.PodeSerEditada)
        {
            throw new InvalidOperationException("Solicitação não pode ser editada no status atual");
        }

        // Validações de negócio
        if (atualizarDto.DataDespesa > DateTime.Now)
        {
            throw new ArgumentException("Data da despesa não pode ser futura");
        }

        if (atualizarDto.DataDespesa < DateTime.Now.AddYears(-1))
        {
            throw new ArgumentException("Data da despesa não pode ser superior a 1 ano");
        }

        _mapper.Map(atualizarDto, solicitacao);
        solicitacao.AtualizadoPor = usuarioId.ToString();

        await _context.SaveChangesAsync();

        _logger.LogInformation("Solicitação de reembolso atualizada: {SolicitacaoId} - {Titulo}", 
            solicitacao.Id, solicitacao.Titulo);

        return _mapper.Map<SolicitacaoReembolsoDto>(solicitacao);
    }

    public async Task<bool> ExcluirAsync(Guid id, Guid usuarioId)
    {
        var solicitacao = await _context.SolicitacoesReembolso.FindAsync(id);
        if (solicitacao == null || !solicitacao.Ativo)
        {
            return false;
        }

        if (!solicitacao.PodeSerEditada)
        {
            throw new InvalidOperationException("Solicitação não pode ser excluída no status atual");
        }

        // Soft delete
        solicitacao.Ativo = false;
        solicitacao.AtualizadoPor = usuarioId.ToString();

        await _context.SaveChangesAsync();

        _logger.LogInformation("Solicitação de reembolso excluída: {SolicitacaoId} - {Titulo}", 
            solicitacao.Id, solicitacao.Titulo);

        return true;
    }

    public async Task<SolicitacaoReembolsoDto?> EnviarParaAprovacaoAsync(Guid id, Guid usuarioId)
    {
        var solicitacao = await _context.SolicitacoesReembolso.FindAsync(id);
        if (solicitacao == null || !solicitacao.Ativo)
        {
            return null;
        }

        if (solicitacao.Status != StatusSolicitacao.Rascunho)
        {
            throw new InvalidOperationException("Apenas solicitações em rascunho podem ser enviadas para aprovação");
        }

        var statusAnterior = solicitacao.Status;
        solicitacao.Status = StatusSolicitacao.PendenteAprovacaoFinanceira;
        solicitacao.AtualizadoPor = usuarioId.ToString();

        await AdicionarHistoricoStatusAsync(id, statusAnterior, solicitacao.Status, usuarioId, 
            "Solicitação enviada para aprovação", "Sistema");

        await _context.SaveChangesAsync();

        _logger.LogInformation("Solicitação enviada para aprovação: {SolicitacaoId}", id);

        return _mapper.Map<SolicitacaoReembolsoDto>(solicitacao);
    }

    public async Task<SolicitacaoReembolsoDto?> AprovarAsync(Guid id, AprovarSolicitacaoDto aprovarDto, Guid usuarioId, string nomeUsuario)
    {
        var solicitacao = await _context.SolicitacoesReembolso.FindAsync(id);
        if (solicitacao == null || !solicitacao.Ativo)
        {
            return null;
        }

        if (!solicitacao.PodeSerAprovada)
        {
            throw new InvalidOperationException("Solicitação não pode ser aprovada no status atual");
        }

        if (aprovarDto.ValorAprovado > solicitacao.ValorSolicitado)
        {
            throw new ArgumentException("Valor aprovado não pode ser maior que o valor solicitado");
        }

        var statusAnterior = solicitacao.Status;
        solicitacao.Status = StatusSolicitacao.Aprovada;
        solicitacao.ValorAprovado = aprovarDto.ValorAprovado;
        solicitacao.DataAprovacao = DateTime.UtcNow;
        solicitacao.AprovadoPorId = usuarioId;
        solicitacao.ObservacaoAprovacao = aprovarDto.ObservacaoAprovacao;
        solicitacao.AtualizadoPor = usuarioId.ToString();

        await AdicionarHistoricoStatusAsync(id, statusAnterior, solicitacao.Status, usuarioId, 
            aprovarDto.ObservacaoAprovacao, nomeUsuario);

        await _context.SaveChangesAsync();

        _logger.LogInformation("Solicitação aprovada: {SolicitacaoId} - Valor: {Valor}", id, aprovarDto.ValorAprovado);

        return _mapper.Map<SolicitacaoReembolsoDto>(solicitacao);
    }

    public async Task<SolicitacaoReembolsoDto?> RejeitarAsync(Guid id, RejeitarSolicitacaoDto rejeitarDto, Guid usuarioId, string nomeUsuario)
    {
        var solicitacao = await _context.SolicitacoesReembolso.FindAsync(id);
        if (solicitacao == null || !solicitacao.Ativo)
        {
            return null;
        }

        if (!solicitacao.PodeSerAprovada)
        {
            throw new InvalidOperationException("Solicitação não pode ser rejeitada no status atual");
        }

        var statusAnterior = solicitacao.Status;
        solicitacao.Status = StatusSolicitacao.Rejeitada;
        solicitacao.DataAprovacao = DateTime.UtcNow;
        solicitacao.AprovadoPorId = usuarioId;
        solicitacao.ObservacaoAprovacao = rejeitarDto.ObservacaoAprovacao;
        solicitacao.AtualizadoPor = usuarioId.ToString();

        await AdicionarHistoricoStatusAsync(id, statusAnterior, solicitacao.Status, usuarioId, 
            rejeitarDto.ObservacaoAprovacao, nomeUsuario);

        await _context.SaveChangesAsync();

        _logger.LogInformation("Solicitação rejeitada: {SolicitacaoId}", id);

        return _mapper.Map<SolicitacaoReembolsoDto>(solicitacao);
    }

    public async Task<SolicitacaoReembolsoDto?> PagarAsync(Guid id, PagarSolicitacaoDto pagarDto, Guid usuarioId, string nomeUsuario)
    {
        var solicitacao = await _context.SolicitacoesReembolso.FindAsync(id);
        if (solicitacao == null || !solicitacao.Ativo)
        {
            return null;
        }

        if (!solicitacao.PodeSerPaga)
        {
            throw new InvalidOperationException("Solicitação não pode ser paga no status atual");
        }

        var statusAnterior = solicitacao.Status;
        solicitacao.Status = StatusSolicitacao.Pago;
        solicitacao.DataPagamento = DateTime.UtcNow;
        solicitacao.PagoPorId = usuarioId;
        solicitacao.ObservacaoPagamento = pagarDto.ObservacaoPagamento;
        solicitacao.AtualizadoPor = usuarioId.ToString();

        await AdicionarHistoricoStatusAsync(id, statusAnterior, solicitacao.Status, usuarioId, 
            pagarDto.ObservacaoPagamento, nomeUsuario);

        await _context.SaveChangesAsync();

        _logger.LogInformation("Solicitação paga: {SolicitacaoId} - Valor: {Valor}", id, solicitacao.ValorAprovado);

        return _mapper.Map<SolicitacaoReembolsoDto>(solicitacao);
    }

    public async Task<SolicitacaoReembolsoDto?> CancelarAsync(Guid id, CancelarSolicitacaoDto cancelarDto, Guid usuarioId, string nomeUsuario)
    {
        var solicitacao = await _context.SolicitacoesReembolso.FindAsync(id);
        if (solicitacao == null || !solicitacao.Ativo)
        {
            return null;
        }

        if (!solicitacao.PodeSerCancelada)
        {
            throw new InvalidOperationException("Solicitação não pode ser cancelada no status atual");
        }

        var statusAnterior = solicitacao.Status;
        solicitacao.Status = StatusSolicitacao.Cancelada;
        solicitacao.DataCancelamento = DateTime.UtcNow;
        solicitacao.MotivoCancelamento = cancelarDto.MotivoCancelamento;
        solicitacao.AtualizadoPor = usuarioId.ToString();

        await AdicionarHistoricoStatusAsync(id, statusAnterior, solicitacao.Status, usuarioId, 
            cancelarDto.MotivoCancelamento, nomeUsuario);

        await _context.SaveChangesAsync();

        _logger.LogInformation("Solicitação cancelada: {SolicitacaoId}", id);

        return _mapper.Map<SolicitacaoReembolsoDto>(solicitacao);
    }

    public async Task<Dictionary<string, object>> ObterEstatisticasAsync(Guid? colaboradorId = null)
    {
        var query = _context.SolicitacoesReembolso.Where(s => s.Ativo);
        
        if (colaboradorId.HasValue)
        {
            query = query.Where(s => s.ColaboradorId == colaboradorId.Value);
        }

        var estatisticas = new Dictionary<string, object>
        {
            ["TotalSolicitacoes"] = await query.CountAsync(),
            ["PendenteAprovacao"] = await query.CountAsync(s => s.Status == StatusSolicitacao.PendenteAprovacaoFinanceira),
            ["Aprovadas"] = await query.CountAsync(s => s.Status == StatusSolicitacao.Aprovada),
            ["Pagas"] = await query.CountAsync(s => s.Status == StatusSolicitacao.Pago),
            ["Rejeitadas"] = await query.CountAsync(s => s.Status == StatusSolicitacao.Rejeitada),
            ["Canceladas"] = await query.CountAsync(s => s.Status == StatusSolicitacao.Cancelada),
            ["ValorTotalSolicitado"] = await query.SumAsync(s => s.ValorSolicitado),
            ["ValorTotalAprovado"] = await query.Where(s => s.ValorAprovado.HasValue).SumAsync(s => s.ValorAprovado!.Value),
            ["ValorTotalPago"] = await query.Where(s => s.Status == StatusSolicitacao.Pago && s.ValorAprovado.HasValue)
                .SumAsync(s => s.ValorAprovado!.Value)
        };

        return estatisticas;
    }

    private async Task AdicionarHistoricoStatusAsync(Guid solicitacaoId, StatusSolicitacao statusAnterior, 
        StatusSolicitacao statusNovo, Guid usuarioId, string? observacao, string? nomeUsuario)
    {
        var historico = new HistoricoStatusSolicitacao
        {
            Id = Guid.NewGuid(),
            SolicitacaoReembolsoId = solicitacaoId,
            StatusAnterior = statusAnterior,
            StatusNovo = statusNovo,
            DataMudanca = DateTime.UtcNow,
            AlteradoPorId = usuarioId,
            Observacao = observacao,
            NomeUsuario = nomeUsuario,
            CriadoPor = usuarioId.ToString()
        };

        _context.HistoricoStatusSolicitacao.Add(historico);
    }
}