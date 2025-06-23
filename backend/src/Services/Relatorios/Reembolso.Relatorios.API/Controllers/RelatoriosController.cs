using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reembolso.Relatorios.API.DTOs;
using Reembolso.Relatorios.API.Services;
using Reembolso.Shared.DTOs;
using System.Security.Claims;

namespace Reembolso.Relatorios.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RelatoriosController : ControllerBase
{
    private readonly IRelatorioService _relatorioService;
    private readonly ILogger<RelatoriosController> _logger;

    public RelatoriosController(IRelatorioService relatorioService, ILogger<RelatoriosController> logger)
    {
        _relatorioService = relatorioService;
        _logger = logger;
    }

    /// <summary>
    /// Obtém todos os relatórios com paginação e filtros
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Administrator,AnalistaFinanceiro")]
    public async Task<ActionResult<ApiResponse<PagedResult<RelatorioResumoDto>>>> ObterTodos(
        [FromQuery] int pagina = 1,
        [FromQuery] int itensPorPagina = 10,
        [FromQuery] FiltroRelatorioDto? filtro = null)
    {
        try
        {
            if (pagina <= 0 || itensPorPagina <= 0 || itensPorPagina > 100)
            {
                return BadRequest(ApiResponse<PagedResult<RelatorioResumoDto>>.Error(
                    "Parâmetros de paginação inválidos. Página deve ser > 0 e itens por página entre 1 e 100."));
            }

            var resultado = await _relatorioService.ObterTodosAsync(pagina, itensPorPagina, filtro);
            return Ok(ApiResponse<PagedResult<RelatorioResumoDto>>.Success(resultado));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter relatórios");
            return StatusCode(500, ApiResponse<PagedResult<RelatorioResumoDto>>.Error(
                "Erro interno do servidor ao obter relatórios"));
        }
    }

    /// <summary>
    /// Obtém um relatório específico por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<RelatorioDto>>> ObterPorId(Guid id)
    {
        try
        {
            var relatorio = await _relatorioService.ObterPorIdAsync(id);
            if (relatorio == null)
            {
                return NotFound(ApiResponse<RelatorioDto>.Error("Relatório não encontrado"));
            }

            // Verificar se o usuário pode acessar este relatório
            var usuarioId = ObterUsuarioId();
            var isAdmin = User.IsInRole("Administrator") || User.IsInRole("AnalistaFinanceiro");
            
            if (!isAdmin && relatorio.GeradoPorId != usuarioId)
            {
                return Forbid();
            }

            return Ok(ApiResponse<RelatorioDto>.Success(relatorio));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter relatório {RelatorioId}", id);
            return StatusCode(500, ApiResponse<RelatorioDto>.Error(
                "Erro interno do servidor ao obter relatório"));
        }
    }

    /// <summary>
    /// Obtém relatórios do usuário logado
    /// </summary>
    [HttpGet("meus-relatorios")]
    public async Task<ActionResult<ApiResponse<List<RelatorioResumoDto>>>> ObterMeusRelatorios()
    {
        try
        {
            var usuarioId = ObterUsuarioId();
            var relatorios = await _relatorioService.ObterPorUsuarioAsync(usuarioId);
            return Ok(ApiResponse<List<RelatorioResumoDto>>.Success(relatorios));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter relatórios do usuário {UsuarioId}", ObterUsuarioId());
            return StatusCode(500, ApiResponse<List<RelatorioResumoDto>>.Error(
                "Erro interno do servidor ao obter seus relatórios"));
        }
    }

    /// <summary>
    /// Cria um novo relatório
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<RelatorioDto>>> Criar([FromBody] CriarRelatorioDto criarDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var erros = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<RelatorioDto>.Error(
                    "Dados inválidos", erros));
            }

            var usuarioId = ObterUsuarioId();
            var relatorio = await _relatorioService.CriarAsync(criarDto, usuarioId);
            
            _logger.LogInformation("Relatório criado: {RelatorioId} por usuário {UsuarioId}", 
                relatorio.Id, usuarioId);

            return CreatedAtAction(
                nameof(ObterPorId),
                new { id = relatorio.Id },
                ApiResponse<RelatorioDto>.Success(relatorio));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<RelatorioDto>.Error(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar relatório");
            return StatusCode(500, ApiResponse<RelatorioDto>.Error(
                "Erro interno do servidor ao criar relatório"));
        }
    }

    /// <summary>
    /// Exclui um relatório
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> Excluir(Guid id)
    {
        try
        {
            var usuarioId = ObterUsuarioId();
            var sucesso = await _relatorioService.ExcluirAsync(id, usuarioId);
            
            if (!sucesso)
            {
                return NotFound(ApiResponse<bool>.Error("Relatório não encontrado"));
            }

            _logger.LogInformation("Relatório excluído: {RelatorioId} por usuário {UsuarioId}", 
                id, usuarioId);

            return Ok(ApiResponse<bool>.Success(true, "Relatório excluído com sucesso"));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir relatório {RelatorioId}", id);
            return StatusCode(500, ApiResponse<bool>.Error(
                "Erro interno do servidor ao excluir relatório"));
        }
    }

    /// <summary>
    /// Força o reprocessamento de um relatório
    /// </summary>
    [HttpPost("{id:guid}/reprocessar")]
    [Authorize(Roles = "Administrator,AnalistaFinanceiro")]
    public async Task<ActionResult<ApiResponse<RelatorioDto>>> Reprocessar(Guid id)
    {
        try
        {
            var relatorio = await _relatorioService.ProcessarRelatorioAsync(id);
            if (relatorio == null)
            {
                return NotFound(ApiResponse<RelatorioDto>.Error("Relatório não encontrado"));
            }

            _logger.LogInformation("Relatório reprocessado: {RelatorioId} por usuário {UsuarioId}", 
                id, ObterUsuarioId());

            return Ok(ApiResponse<RelatorioDto>.Success(relatorio, "Relatório reprocessado com sucesso"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao reprocessar relatório {RelatorioId}", id);
            return StatusCode(500, ApiResponse<RelatorioDto>.Error(
                "Erro interno do servidor ao reprocessar relatório"));
        }
    }

    /// <summary>
    /// Baixa o arquivo de um relatório
    /// </summary>
    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> BaixarRelatorio(Guid id)
    {
        try
        {
            // Verificar se o relatório existe e se o usuário pode acessá-lo
            var relatorio = await _relatorioService.ObterPorIdAsync(id);
            if (relatorio == null)
            {
                return NotFound();
            }

            var usuarioId = ObterUsuarioId();
            var isAdmin = User.IsInRole("Administrator") || User.IsInRole("AnalistaFinanceiro");
            
            if (!isAdmin && relatorio.GeradoPorId != usuarioId)
            {
                return Forbid();
            }

            if (!relatorio.PodeSerBaixado)
            {
                return BadRequest("Relatório ainda não está disponível para download");
            }

            var conteudoArquivo = await _relatorioService.BaixarRelatorioAsync(id);
            if (conteudoArquivo == null)
            {
                return NotFound("Arquivo do relatório não encontrado");
            }

            _logger.LogInformation("Download do relatório {RelatorioId} por usuário {UsuarioId}", 
                id, usuarioId);

            return File(conteudoArquivo, relatorio.TipoConteudo!, relatorio.NomeArquivo!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao baixar relatório {RelatorioId}", id);
            return StatusCode(500, "Erro interno do servidor ao baixar relatório");
        }
    }

    /// <summary>
    /// Obtém estatísticas dos relatórios
    /// </summary>
    [HttpGet("estatisticas")]
    [Authorize(Roles = "Administrator,AnalistaFinanceiro")]
    public async Task<ActionResult<ApiResponse<EstatisticasRelatorioDto>>> ObterEstatisticas()
    {
        try
        {
            var estatisticas = await _relatorioService.ObterEstatisticasAsync();
            return Ok(ApiResponse<EstatisticasRelatorioDto>.Success(estatisticas));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter estatísticas de relatórios");
            return StatusCode(500, ApiResponse<EstatisticasRelatorioDto>.Error(
                "Erro interno do servidor ao obter estatísticas"));
        }
    }

    /// <summary>
    /// Limpa relatórios antigos (manutenção)
    /// </summary>
    [HttpPost("limpar-antigos")]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<ApiResponse<bool>>> LimparRelatoriosAntigos()
    {
        try
        {
            await _relatorioService.LimparRelatoriosAntigosAsync();
            
            _logger.LogInformation("Limpeza de relatórios antigos executada por usuário {UsuarioId}", 
                ObterUsuarioId());

            return Ok(ApiResponse<bool>.Success(true, "Limpeza de relatórios antigos executada com sucesso"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao limpar relatórios antigos");
            return StatusCode(500, ApiResponse<bool>.Error(
                "Erro interno do servidor ao limpar relatórios antigos"));
        }
    }

    /// <summary>
    /// Obtém os dados de um relatório (preview)
    /// </summary>
    [HttpGet("{id:guid}/dados")]
    public async Task<ActionResult<ApiResponse<DadosRelatorioDto>>> ObterDadosRelatorio(Guid id)
    {
        try
        {
            // Verificar se o relatório existe e se o usuário pode acessá-lo
            var relatorio = await _relatorioService.ObterPorIdAsync(id);
            if (relatorio == null)
            {
                return NotFound(ApiResponse<DadosRelatorioDto>.Error("Relatório não encontrado"));
            }

            var usuarioId = ObterUsuarioId();
            var isAdmin = User.IsInRole("Administrator") || User.IsInRole("AnalistaFinanceiro");
            
            if (!isAdmin && relatorio.GeradoPorId != usuarioId)
            {
                return Forbid();
            }

            var dados = await _relatorioService.GerarDadosRelatorioAsync(id);
            return Ok(ApiResponse<DadosRelatorioDto>.Success(dados));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<DadosRelatorioDto>.Error(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter dados do relatório {RelatorioId}", id);
            return StatusCode(500, ApiResponse<DadosRelatorioDto>.Error(
                "Erro interno do servidor ao obter dados do relatório"));
        }
    }

    /// <summary>
    /// Obtém o status de processamento de um relatório
    /// </summary>
    [HttpGet("{id:guid}/status")]
    public async Task<ActionResult<ApiResponse<object>>> ObterStatusProcessamento(Guid id)
    {
        try
        {
            var relatorio = await _relatorioService.ObterPorIdAsync(id);
            if (relatorio == null)
            {
                return NotFound(ApiResponse<object>.Error("Relatório não encontrado"));
            }

            var usuarioId = ObterUsuarioId();
            var isAdmin = User.IsInRole("Administrator") || User.IsInRole("AnalistaFinanceiro");
            
            if (!isAdmin && relatorio.GeradoPorId != usuarioId)
            {
                return Forbid();
            }

            var status = new
            {
                relatorio.Id,
                relatorio.Status,
                relatorio.DataGeracao,
                relatorio.DataConclusao,
                relatorio.TempoProcessamento,
                relatorio.MensagemErro,
                relatorio.PodeSerBaixado,
                relatorio.EstaProcessando,
                relatorio.EstaConcluido,
                relatorio.TemErro
            };

            return Ok(ApiResponse<object>.Success(status));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter status do relatório {RelatorioId}", id);
            return StatusCode(500, ApiResponse<object>.Error(
                "Erro interno do servidor ao obter status do relatório"));
        }
    }

    #region Métodos Auxiliares

    private Guid ObterUsuarioId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Token de usuário inválido");
        }
        return userId;
    }

    private string ObterNomeUsuario()
    {
        return User.FindFirst(ClaimTypes.Name)?.Value ?? "Usuário Desconhecido";
    }

    #endregion
}