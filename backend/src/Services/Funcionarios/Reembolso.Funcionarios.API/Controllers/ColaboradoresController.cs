using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reembolso.Funcionarios.API.DTOs;
using Reembolso.Funcionarios.API.Services;
using Reembolso.Shared.DTOs;

namespace Reembolso.Funcionarios.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ColaboradoresController : ControllerBase
{
    private readonly IColaboradorService _colaboradorService;

    public ColaboradoresController(IColaboradorService colaboradorService)
    {
        _colaboradorService = colaboradorService;
    }

    /// <summary>
    /// Obtém lista paginada de colaboradores
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Administrador,AnalistaFinanceiro")]
    public async Task<ActionResult<ApiResponse<PagedResult<ColaboradorDto>>>> ObterTodos(
        [FromQuery] int pagina = 1,
        [FromQuery] int itensPorPagina = 10,
        [FromQuery] string? filtro = null)
    {
        if (pagina < 1) pagina = 1;
        if (itensPorPagina < 1 || itensPorPagina > 100) itensPorPagina = 10;

        var resultado = await _colaboradorService.ObterTodosAsync(pagina, itensPorPagina, filtro);
        
        return Ok(ApiResponse<PagedResult<ColaboradorDto>>.SucessoComDados(
            resultado, "Colaboradores obtidos com sucesso"));
    }

    /// <summary>
    /// Obtém colaborador por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Administrador,AnalistaFinanceiro")]
    public async Task<ActionResult<ApiResponse<ColaboradorDto>>> ObterPorId(Guid id)
    {
        var colaborador = await _colaboradorService.ObterPorIdAsync(id);
        
        if (colaborador == null)
        {
            return NotFound(ApiResponse<ColaboradorDto>.Erro("Colaborador não encontrado"));
        }

        return Ok(ApiResponse<ColaboradorDto>.SucessoComDados(colaborador, "Colaborador obtido com sucesso"));
    }

    /// <summary>
    /// Obtém colaborador por matrícula
    /// </summary>
    [HttpGet("matricula/{matricula}")]
    [Authorize(Roles = "Administrador,AnalistaFinanceiro")]
    public async Task<ActionResult<ApiResponse<ColaboradorDto>>> ObterPorMatricula(string matricula)
    {
        var colaborador = await _colaboradorService.ObterPorMatriculaAsync(matricula);
        
        if (colaborador == null)
        {
            return NotFound(ApiResponse<ColaboradorDto>.Erro("Colaborador não encontrado"));
        }

        return Ok(ApiResponse<ColaboradorDto>.SucessoComDados(colaborador, "Colaborador obtido com sucesso"));
    }

    /// <summary>
    /// Obtém colaborador por email
    /// </summary>
    [HttpGet("email/{email}")]
    [Authorize(Roles = "Administrador,AnalistaFinanceiro")]
    public async Task<ActionResult<ApiResponse<ColaboradorDto>>> ObterPorEmail(string email)
    {
        var colaborador = await _colaboradorService.ObterPorEmailAsync(email);
        
        if (colaborador == null)
        {
            return NotFound(ApiResponse<ColaboradorDto>.Erro("Colaborador não encontrado"));
        }

        return Ok(ApiResponse<ColaboradorDto>.SucessoComDados(colaborador, "Colaborador obtido com sucesso"));
    }

    /// <summary>
    /// Obtém resumo de todos os colaboradores
    /// </summary>
    [HttpGet("resumo")]
    [Authorize(Roles = "Administrador,AnalistaFinanceiro")]
    public async Task<ActionResult<ApiResponse<List<ColaboradorResumoDto>>>> ObterResumoTodos()
    {
        var colaboradores = await _colaboradorService.ObterResumoTodosAsync();
        
        return Ok(ApiResponse<List<ColaboradorResumoDto>>.SucessoComDados(
            colaboradores, "Resumo dos colaboradores obtido com sucesso"));
    }

    /// <summary>
    /// Obtém colaboradores ativos
    /// </summary>
    [HttpGet("ativos")]
    [Authorize(Roles = "Administrador,AnalistaFinanceiro")]
    public async Task<ActionResult<ApiResponse<List<ColaboradorResumoDto>>>> ObterAtivos()
    {
        var colaboradores = await _colaboradorService.ObterAtivosAsync();
        
        return Ok(ApiResponse<List<ColaboradorResumoDto>>.SucessoComDados(
            colaboradores, "Colaboradores ativos obtidos com sucesso"));
    }

    /// <summary>
    /// Cria novo colaborador
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<ApiResponse<ColaboradorDto>>> Criar([FromBody] CriarColaboradorDto criarColaboradorDto)
    {
        if (!ModelState.IsValid)
        {
            var erros = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(ApiResponse<ColaboradorDto>.Erro(erros));
        }

        try
        {
            var colaborador = await _colaboradorService.CriarAsync(criarColaboradorDto);
            
            return CreatedAtAction(
                nameof(ObterPorId),
                new { id = colaborador.Id },
                ApiResponse<ColaboradorDto>.SucessoComDados(colaborador, "Colaborador criado com sucesso"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<ColaboradorDto>.Erro(ex.Message));
        }
    }

    /// <summary>
    /// Atualiza colaborador existente
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<ApiResponse<ColaboradorDto>>> Atualizar(
        Guid id, 
        [FromBody] AtualizarColaboradorDto atualizarColaboradorDto)
    {
        if (!ModelState.IsValid)
        {
            var erros = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(ApiResponse<ColaboradorDto>.Erro(erros));
        }

        try
        {
            var colaborador = await _colaboradorService.AtualizarAsync(id, atualizarColaboradorDto);
            
            if (colaborador == null)
            {
                return NotFound(ApiResponse<ColaboradorDto>.Erro("Colaborador não encontrado"));
            }

            return Ok(ApiResponse<ColaboradorDto>.SucessoComDados(colaborador, "Colaborador atualizado com sucesso"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<ColaboradorDto>.Erro(ex.Message));
        }
    }

    /// <summary>
    /// Exclui colaborador (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<ApiResponse<object>>> Excluir(Guid id)
    {
        var sucesso = await _colaboradorService.ExcluirAsync(id);
        
        if (!sucesso)
        {
            return NotFound(ApiResponse<object>.Erro("Colaborador não encontrado"));
        }

        return Ok(ApiResponse<object>.SucessoSemDados("Colaborador excluído com sucesso"));
    }

    /// <summary>
    /// Ativa/desativa colaborador
    /// </summary>
    [HttpPatch("{id:guid}/toggle-ativo")]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<ApiResponse<object>>> ToggleAtivo(Guid id)
    {
        var sucesso = await _colaboradorService.ToggleAtivoAsync(id);
        
        if (!sucesso)
        {
            return NotFound(ApiResponse<object>.Erro("Colaborador não encontrado"));
        }

        return Ok(ApiResponse<object>.SucessoSemDados("Status do colaborador alterado com sucesso"));
    }

    /// <summary>
    /// Verifica se matrícula já existe
    /// </summary>
    [HttpGet("verificar-matricula/{matricula}")]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<ApiResponse<bool>>> VerificarMatricula(string matricula, [FromQuery] Guid? ignorarId = null)
    {
        var existe = await _colaboradorService.ExisteMatriculaAsync(matricula, ignorarId);
        
        return Ok(ApiResponse<bool>.SucessoComDados(existe, "Verificação realizada com sucesso"));
    }

    /// <summary>
    /// Verifica se email já existe
    /// </summary>
    [HttpGet("verificar-email/{email}")]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<ApiResponse<bool>>> VerificarEmail(string email, [FromQuery] Guid? ignorarId = null)
    {
        var existe = await _colaboradorService.ExisteEmailAsync(email, ignorarId);
        
        return Ok(ApiResponse<bool>.SucessoComDados(existe, "Verificação realizada com sucesso"));
    }

    /// <summary>
    /// Verifica se CPF já existe
    /// </summary>
    [HttpGet("verificar-cpf/{cpf}")]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<ApiResponse<bool>>> VerificarCPF(string cpf, [FromQuery] Guid? ignorarId = null)
    {
        var existe = await _colaboradorService.ExisteCPFAsync(cpf, ignorarId);
        
        return Ok(ApiResponse<bool>.SucessoComDados(existe, "Verificação realizada com sucesso"));
    }
}