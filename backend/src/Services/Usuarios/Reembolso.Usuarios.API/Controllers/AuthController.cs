using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reembolso.Shared.DTOs;
using Reembolso.Usuarios.API.DTOs;
using Reembolso.Usuarios.API.Services;
using System.Security.Claims;

namespace Reembolso.Usuarios.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Realiza login no sistema
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginDto loginDto)
    {
        if (!ModelState.IsValid)
        {
            var erros = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(ApiResponse<LoginResponseDto>.Erro(erros));
        }

        var enderecoIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

        var resultado = await _authService.LoginAsync(loginDto, enderecoIp, userAgent);
        
        if (resultado == null)
        {
            return Unauthorized(ApiResponse<LoginResponseDto>.Erro("Email ou senha inválidos"));
        }

        return Ok(ApiResponse<LoginResponseDto>.SucessoComDados(resultado, "Login realizado com sucesso"));
    }

    /// <summary>
    /// Renova o token de acesso
    /// </summary>
    [HttpPost("refresh-token")]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
    {
        if (!ModelState.IsValid)
        {
            var erros = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(ApiResponse<LoginResponseDto>.Erro(erros));
        }

        var resultado = await _authService.RefreshTokenAsync(refreshTokenDto);
        
        if (resultado == null)
        {
            return Unauthorized(ApiResponse<LoginResponseDto>.Erro("Token inválido ou expirado"));
        }

        return Ok(ApiResponse<LoginResponseDto>.SucessoComDados(resultado, "Token renovado com sucesso"));
    }

    /// <summary>
    /// Realiza logout do sistema
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> Logout()
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
        {
            return BadRequest(ApiResponse<object>.Erro("Token inválido"));
        }

        var sucesso = await _authService.LogoutAsync(usuarioId);
        
        if (!sucesso)
        {
            return BadRequest(ApiResponse<object>.Erro("Erro ao realizar logout"));
        }

        return Ok(ApiResponse<object>.SucessoSemDados("Logout realizado com sucesso"));
    }

    /// <summary>
    /// Altera a senha do usuário logado
    /// </summary>
    [HttpPost("alterar-senha")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> AlterarSenha([FromBody] AlterarSenhaDto alterarSenhaDto)
    {
        if (!ModelState.IsValid)
        {
            var erros = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(ApiResponse<object>.Erro(erros));
        }

        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
        {
            return BadRequest(ApiResponse<object>.Erro("Token inválido"));
        }

        var sucesso = await _authService.AlterarSenhaAsync(usuarioId, alterarSenhaDto);
        
        if (!sucesso)
        {
            return BadRequest(ApiResponse<object>.Erro("Senha atual incorreta ou nova senha inválida"));
        }

        return Ok(ApiResponse<object>.SucessoSemDados("Senha alterada com sucesso"));
    }

    /// <summary>
    /// Obtém informações do usuário logado
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public ActionResult<ApiResponse<object>> ObterUsuarioLogado()
    {
        var claims = new
        {
            Id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            Nome = User.FindFirst(ClaimTypes.Name)?.Value,
            Email = User.FindFirst(ClaimTypes.Email)?.Value,
            TipoUsuario = User.FindFirst(ClaimTypes.Role)?.Value
        };

        return Ok(ApiResponse<object>.SucessoComDados(claims, "Dados do usuário obtidos com sucesso"));
    }
}