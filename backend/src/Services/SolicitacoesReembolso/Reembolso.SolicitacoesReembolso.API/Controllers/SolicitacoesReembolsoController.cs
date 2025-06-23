using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reembolso.Shared.DTOs;
using Reembolso.Shared.Enums;
using Reembolso.SolicitacoesReembolso.API.DTOs;
using Reembolso.SolicitacoesReembolso.API.Services;
using System.Security.Claims;

namespace Reembolso.SolicitacoesReembolso.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SolicitacoesReembolsoController : ControllerBase
{
    private readonly ISolicitacaoReembolsoService _solicitacaoService;
    private readonly ILogger<SolicitacoesReembolsoController> _logger;

    public SolicitacoesReembolsoController(
        ISolicitacaoReembolsoService solicitacaoService,
        ILogger<SolicitacoesReembolsoController> logger)
    {
        _solicitacaoService = solicitacaoService;
        _logger = logger;
    }

    private Guid ObterUsuarioId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    private string ObterNomeUsuario()
    {
        return User.FindFirst(ClaimTypes.Name)?.Value ?? "Sistema";
    }

    /// <summary>
    /// Obtém todas as solicitações de reembolso com paginação e filtros
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<SolicitacaoReembolsoResumoDto>>>> ObterTodos(
        [FromQuery] int pagina = 1,
        [FromQuery] int itensPorPagina = 10,
        [FromQuery] Guid? colaboradorId = null,
        [FromQuery] StatusSolicitacao? status = null,
        [FromQuery] TipoDespesa? tipoDespesa = null,
        [FromQuery] DateTime? dataInicio = null,
        [FromQuery] DateTime? dataFim = null,
        [FromQuery] string? filtro = null)
    {
        try
        {
            var resultado = await _solicitacaoService.ObterTodosAsync(pagina, itensPorPagina, 
                colaboradorId, status, tipoDespesa, dataInicio, dataFim, filtro);
            
            return Ok(ApiResponse<PagedResult<SolicitacaoReembolsoResumoDto>>.Success(resultado));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter solicitações de reembolso");
            return StatusCode(500, ApiResponse<PagedResult<SolicitacaoReembolsoResumoDto>>.Error(
                "Erro interno do servidor"));
        }
    }

    /// <summary>
    /// Obtém uma solicitação de reembolso por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<SolicitacaoReembolsoDto>>> ObterPorId(Guid id)
    {
        try
        {
            var solicitacao = await _solicitacaoService.ObterPorIdAsync(id);
            
            if (solicitacao == null)
            {
                return NotFound(ApiResponse<SolicitacaoReembolsoDto>.Error("Solicitação não encontrada"));
            }

            return Ok(ApiResponse<SolicitacaoReembolsoDto>.Success(solicitacao));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter solicitação {SolicitacaoId}", id);
            return StatusCode(500, ApiResponse<SolicitacaoReembolsoDto>.Error(
                "Erro interno do servidor"));
        }
    }

    /// <summary>
    /// Obtém solicitações de reembolso por colaborador
    /// </summary>
    [HttpGet("colaborador/{colaboradorId}")]
    public async Task<ActionResult<ApiResponse<List<SolicitacaoReembolsoResumoDto>>>> ObterPorColaborador(Guid colaboradorId)
    {
        try
        {
            var solicitacoes = await _solicitacaoService.ObterPorColaboradorAsync(colaboradorId);
            return Ok(ApiResponse<List<SolicitacaoReembolsoResumoDto>>.Success(solicitacoes));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter solicitações do colaborador {ColaboradorId}", colaboradorId);
            return StatusCode(500, ApiResponse<List<SolicitacaoReembolsoResumoDto>>.Error(
                "Erro interno do servidor"));
        }
    }

    /// <summary>
    /// Obtém solicitações pendentes de aprovação
    /// </summary>
    [HttpGet("pendente-aprovacao")]
    [Authorize(Roles = "Administrator,AnalistaFinanceiro")]
    public async Task<ActionResult<ApiResponse<List<SolicitacaoReembolsoResumoDto>>>> ObterPendenteAprovacao()
    {
        try
        {
            var solicitacoes = await _solicitacaoService.ObterPendenteAprovacaoAsync();
            return Ok(ApiResponse<List<SolicitacaoReembolsoResumoDto>>.Success(solicitacoes));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter solicitações pendentes de aprovação");
            return StatusCode(500, ApiResponse<List<SolicitacaoReembolsoResumoDto>>.Error(
                "Erro interno do servidor"));
        }
    }

    /// <summary>
    /// Obtém solicitações aprovadas
    /// </summary>
    [HttpGet("aprovadas")]
    [Authorize(Roles = "Administrator,AnalistaFinanceiro")]
    public async Task<ActionResult<ApiResponse<List<SolicitacaoReembolsoResumoDto>>>> ObterAprovadas()
    {
        try
        {
            var solicitacoes = await _solicitacaoService.ObterAprovadasAsync();
            return Ok(ApiResponse<List<SolicitacaoReembolsoResumoDto>>.Success(solicitacoes));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter solicitações aprovadas");
            return StatusCode(500, ApiResponse<List<SolicitacaoReembolsoResumoDto>>.Error(
                "Erro interno do servidor"));
        }
    }

    /// <summary>
    /// Cria uma nova solicitação de reembolso
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<SolicitacaoReembolsoDto>>> Criar(
        [FromBody] CriarSolicitacaoReembolsoDto criarDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var erros = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<SolicitacaoReembolsoDto>.Error(
                    "Dados inválidos", erros));
            }

            var usuarioId = ObterUsuarioId();
            var solicitacao = await _solicitacaoService.CriarAsync(criarDto, usuarioId);
            
            return CreatedAtAction(nameof(ObterPorId), new { id = solicitacao.Id }, 
                ApiResponse<SolicitacaoReembolsoDto>.Success(solicitacao));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<SolicitacaoReembolsoDto>.Error(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar solicitação de reembolso");
            return StatusCode(500, ApiResponse<SolicitacaoReembolsoDto>.Error(
                "Erro interno do servidor"));
        }
    }

    /// <summary>
    /// Atualiza uma solicitação de reembolso
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<SolicitacaoReembolsoDto>>> Atualizar(
        Guid id, [FromBody] AtualizarSolicitacaoReembolsoDto atualizarDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var erros = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<SolicitacaoReembolsoDto>.Error(
                    "Dados inválidos", erros));
            }

            var usuarioId = ObterUsuarioId();
            var solicitacao = await _solicitacaoService.AtualizarAsync(id, atualizarDto, usuarioId);
            
            if (solicitacao == null)
            {
                return NotFound(ApiResponse<SolicitacaoReembolsoDto>.Error("Solicitação não encontrada"));
            }

            return Ok(ApiResponse<SolicitacaoReembolsoDto>.Success(solicitacao));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<SolicitacaoReembolsoDto>.Error(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<SolicitacaoReembolsoDto>.Error(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar solicitação {SolicitacaoId}", id);
            return StatusCode(500, ApiResponse<SolicitacaoReembolsoDto>.Error(
                "Erro interno do servidor"));
        }
    }

    /// <summary>
    /// Exclui uma solicitação de reembolso
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> Excluir(Guid id)
    {
        try
        {
            var usuarioId = ObterUsuarioId();
            var sucesso = await _solicitacaoService.ExcluirAsync(id, usuarioId);
            
            if (!sucesso)
            {
                return NotFound(ApiResponse<bool>.Error("Solicitação não encontrada"));
            }

            return Ok(ApiResponse<bool>.Success(true));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<bool>.Error(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir solicitação {SolicitacaoId}", id);
            return StatusCode(500, ApiResponse<bool>.Error("Erro interno do servidor"));
        }
    }

    /// <summary>
    /// Envia solicitação para aprovação
    /// </summary>
    [HttpPost("{id}/enviar-aprovacao")]
    public async Task<ActionResult<ApiResponse<SolicitacaoReembolsoDto>>> EnviarParaAprovacao(Guid id)
    {
        try
        {
            var usuarioId = ObterUsuarioId();
            var solicitacao = await _solicitacaoService.EnviarParaAprovacaoAsync(id, usuarioId);
            
            if (solicitacao == null)
            {
                return NotFound(ApiResponse<SolicitacaoReembolsoDto>.Error("Solicitação não encontrada"));
            }

            return Ok(ApiResponse<SolicitacaoReembolsoDto>.Success(solicitacao));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<SolicitacaoReembolsoDto>.Error(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar solicitação {SolicitacaoId} para aprovação", id);
            return StatusCode(500, ApiResponse<SolicitacaoReembolsoDto>.Error(
                "Erro interno do servidor"));
        }
    }

    /// <summary>
    /// Aprova uma solicitação de reembolso
    /// </summary>
    [HttpPost("{id}/aprovar")]
    [Authorize(Roles = "Administrator,AnalistaFinanceiro")]
    public async Task<ActionResult<ApiResponse<SolicitacaoReembolsoDto>>> Aprovar(
        Guid id, [FromBody] AprovarSolicitacaoDto aprovarDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var erros = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<SolicitacaoReembolsoDto>.Error(
                    "Dados inválidos", erros));
            }

            var usuarioId = ObterUsuarioId();
            var nomeUsuario = ObterNomeUsuario();
            var solicitacao = await _solicitacaoService.AprovarAsync(id, aprovarDto, usuarioId, nomeUsuario);
            
            if (solicitacao == null)
            {
                return NotFound(ApiResponse<SolicitacaoReembolsoDto>.Error("Solicitação não encontrada"));
            }

            return Ok(ApiResponse<SolicitacaoReembolsoDto>.Success(solicitacao));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<SolicitacaoReembolsoDto>.Error(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<SolicitacaoReembolsoDto>.Error(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao aprovar solicitação {SolicitacaoId}", id);
            return StatusCode(500, ApiResponse<SolicitacaoReembolsoDto>.Error(
                "Erro interno do servidor"));
        }
    }

    /// <summary>
    /// Rejeita uma solicitação de reembolso
    /// </summary>
    [HttpPost("{id}/rejeitar")]
    [Authorize(Roles = "Administrator,AnalistaFinanceiro")]
    public async Task<ActionResult<ApiResponse<SolicitacaoReembolsoDto>>> Rejeitar(
        Guid id, [FromBody] RejeitarSolicitacaoDto rejeitarDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var erros = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<SolicitacaoReembolsoDto>.Error(
                    "Dados inválidos", erros));
            }

            var usuarioId = ObterUsuarioId();
            var nomeUsuario = ObterNomeUsuario();
            var solicitacao = await _solicitacaoService.RejeitarAsync(id, rejeitarDto, usuarioId, nomeUsuario);
            
            if (solicitacao == null)
            {
                return NotFound(ApiResponse<SolicitacaoReembolsoDto>.Error("Solicitação não encontrada"));
            }

            return Ok(ApiResponse<SolicitacaoReembolsoDto>.Success(solicitacao));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<SolicitacaoReembolsoDto>.Error(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao rejeitar solicitação {SolicitacaoId}", id);
            return StatusCode(500, ApiResponse<SolicitacaoReembolsoDto>.Error(
                "Erro interno do servidor"));
        }
    }

    /// <summary>
    /// Marca uma solicitação como paga
    /// </summary>
    [HttpPost("{id}/pagar")]
    [Authorize(Roles = "Administrator,AnalistaFinanceiro")]
    public async Task<ActionResult<ApiResponse<SolicitacaoReembolsoDto>>> Pagar(
        Guid id, [FromBody] PagarSolicitacaoDto pagarDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var erros = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<SolicitacaoReembolsoDto>.Error(
                    "Dados inválidos", erros));
            }

            var usuarioId = ObterUsuarioId();
            var nomeUsuario = ObterNomeUsuario();
            var solicitacao = await _solicitacaoService.PagarAsync(id, pagarDto, usuarioId, nomeUsuario);
            
            if (solicitacao == null)
            {
                return NotFound(ApiResponse<SolicitacaoReembolsoDto>.Error("Solicitação não encontrada"));
            }

            return Ok(ApiResponse<SolicitacaoReembolsoDto>.Success(solicitacao));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<SolicitacaoReembolsoDto>.Error(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao marcar solicitação {SolicitacaoId} como paga", id);
            return StatusCode(500, ApiResponse<SolicitacaoReembolsoDto>.Error(
                "Erro interno do servidor"));
        }
    }

    /// <summary>
    /// Cancela uma solicitação de reembolso
    /// </summary>
    [HttpPost("{id}/cancelar")]
    public async Task<ActionResult<ApiResponse<SolicitacaoReembolsoDto>>> Cancelar(
        Guid id, [FromBody] CancelarSolicitacaoDto cancelarDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var erros = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<SolicitacaoReembolsoDto>.Error(
                    "Dados inválidos", erros));
            }

            var usuarioId = ObterUsuarioId();
            var nomeUsuario = ObterNomeUsuario();
            var solicitacao = await _solicitacaoService.CancelarAsync(id, cancelarDto, usuarioId, nomeUsuario);
            
            if (solicitacao == null)
            {
                return NotFound(ApiResponse<SolicitacaoReembolsoDto>.Error("Solicitação não encontrada"));
            }

            return Ok(ApiResponse<SolicitacaoReembolsoDto>.Success(solicitacao));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<SolicitacaoReembolsoDto>.Error(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao cancelar solicitação {SolicitacaoId}", id);
            return StatusCode(500, ApiResponse<SolicitacaoReembolsoDto>.Error(
                "Erro interno do servidor"));
        }
    }

    /// <summary>
    /// Obtém estatísticas das solicitações de reembolso
    /// </summary>
    [HttpGet("estatisticas")]
    public async Task<ActionResult<ApiResponse<Dictionary<string, object>>>> ObterEstatisticas(
        [FromQuery] Guid? colaboradorId = null)
    {
        try
        {
            var estatisticas = await _solicitacaoService.ObterEstatisticasAsync(colaboradorId);
            return Ok(ApiResponse<Dictionary<string, object>>.Success(estatisticas));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter estatísticas");
            return StatusCode(500, ApiResponse<Dictionary<string, object>>.Error(
                "Erro interno do servidor"));
        }
    }
}