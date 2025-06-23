using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reembolso.Shared.DTOs;
using Reembolso.Usuarios.API.DTOs;
using Reembolso.Usuarios.API.Services;

namespace Reembolso.Usuarios.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;

    public UsuariosController(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    /// <summary>
    /// Obtém lista paginada de usuários
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<ApiResponse<PagedResult<UsuarioDto>>>> ObterTodos(
        [FromQuery] int pagina = 1,
        [FromQuery] int itensPorPagina = 10,
        [FromQuery] string? filtro = null)
    {
        if (pagina < 1) pagina = 1;
        if (itensPorPagina < 1 || itensPorPagina > 100) itensPorPagina = 10;

        var resultado = await _usuarioService.ObterTodosAsync(pagina, itensPorPagina, filtro);
        
        return Ok(ApiResponse<PagedResult<UsuarioDto>>.SucessoComDados(
            resultado, "Usuários obtidos com sucesso"));
    }

    /// <summary>
    /// Obtém usuário por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<ApiResponse<UsuarioDto>>> ObterPorId(Guid id)
    {
        var usuario = await _usuarioService.ObterPorIdAsync(id);
        
        if (usuario == null)
        {
            return NotFound(ApiResponse<UsuarioDto>.Erro("Usuário não encontrado"));
        }

        return Ok(ApiResponse<UsuarioDto>.SucessoComDados(usuario, "Usuário obtido com sucesso"));
    }

    /// <summary>
    /// Obtém usuário por email
    /// </summary>
    [HttpGet("email/{email}")]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<ApiResponse<UsuarioDto>>> ObterPorEmail(string email)
    {
        var usuario = await _usuarioService.ObterPorEmailAsync(email);
        
        if (usuario == null)
        {
            return NotFound(ApiResponse<UsuarioDto>.Erro("Usuário não encontrado"));
        }

        return Ok(ApiResponse<UsuarioDto>.SucessoComDados(usuario, "Usuário obtido com sucesso"));
    }

    /// <summary>
    /// Cria novo usuário
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<ApiResponse<UsuarioDto>>> Criar([FromBody] CriarUsuarioDto criarUsuarioDto)
    {
        if (!ModelState.IsValid)
        {
            var erros = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(ApiResponse<UsuarioDto>.Erro(erros));
        }

        try
        {
            var usuario = await _usuarioService.CriarAsync(criarUsuarioDto);
            
            return CreatedAtAction(
                nameof(ObterPorId),
                new { id = usuario.Id },
                ApiResponse<UsuarioDto>.SucessoComDados(usuario, "Usuário criado com sucesso"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<UsuarioDto>.Erro(ex.Message));
        }
    }

    /// <summary>
    /// Atualiza usuário existente
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<ApiResponse<UsuarioDto>>> Atualizar(
        Guid id, 
        [FromBody] AtualizarUsuarioDto atualizarUsuarioDto)
    {
        if (!ModelState.IsValid)
        {
            var erros = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(ApiResponse<UsuarioDto>.Erro(erros));
        }

        try
        {
            var usuario = await _usuarioService.AtualizarAsync(id, atualizarUsuarioDto);
            
            if (usuario == null)
            {
                return NotFound(ApiResponse<UsuarioDto>.Erro("Usuário não encontrado"));
            }

            return Ok(ApiResponse<UsuarioDto>.SucessoComDados(usuario, "Usuário atualizado com sucesso"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<UsuarioDto>.Erro(ex.Message));
        }
    }

    /// <summary>
    /// Exclui usuário (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<ApiResponse<object>>> Excluir(Guid id)
    {
        var sucesso = await _usuarioService.ExcluirAsync(id);
        
        if (!sucesso)
        {
            return NotFound(ApiResponse<object>.Erro("Usuário não encontrado"));
        }

        return Ok(ApiResponse<object>.SucessoSemDados("Usuário excluído com sucesso"));
    }

    /// <summary>
    /// Ativa/desativa usuário
    /// </summary>
    [HttpPatch("{id:guid}/toggle-ativo")]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<ApiResponse<object>>> ToggleAtivo(Guid id)
    {
        var sucesso = await _usuarioService.ToggleAtivoAsync(id);
        
        if (!sucesso)
        {
            return NotFound(ApiResponse<object>.Erro("Usuário não encontrado"));
        }

        return Ok(ApiResponse<object>.SucessoSemDados("Status do usuário alterado com sucesso"));
    }
}