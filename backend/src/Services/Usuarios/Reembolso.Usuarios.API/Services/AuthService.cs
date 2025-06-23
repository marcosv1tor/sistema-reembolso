using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Reembolso.Shared.Configuration;
using Reembolso.Usuarios.API.Data;
using Reembolso.Usuarios.API.DTOs;
using Reembolso.Usuarios.API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Reembolso.Usuarios.API.Services;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginDto loginDto, string? enderecoIp, string? userAgent);
    Task<LoginResponseDto?> RefreshTokenAsync(RefreshTokenDto refreshTokenDto);
    Task<bool> LogoutAsync(Guid usuarioId);
    Task<bool> AlterarSenhaAsync(Guid usuarioId, AlterarSenhaDto alterarSenhaDto);
}

public class AuthService : IAuthService
{
    private readonly UsuariosDbContext _context;
    private readonly JwtSettings _jwtSettings;

    public AuthService(UsuariosDbContext context, JwtSettings jwtSettings)
    {
        _context = context;
        _jwtSettings = jwtSettings;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginDto loginDto, string? enderecoIp, string? userAgent)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email == loginDto.Email && u.Ativo);

        var loginSucesso = false;
        string? motivoFalha = null;

        if (usuario == null)
        {
            motivoFalha = "Usuário não encontrado";
        }
        else if (!BCrypt.Net.BCrypt.Verify(loginDto.Senha, usuario.SenhaHash))
        {
            motivoFalha = "Senha incorreta";
        }
        else
        {
            loginSucesso = true;
        }

        // Registrar histórico de login
        var historicoLogin = new HistoricoLogin
        {
            UsuarioId = usuario?.Id ?? Guid.Empty,
            DataLogin = DateTime.UtcNow,
            EnderecoIP = enderecoIp,
            UserAgent = userAgent,
            LoginSucesso = loginSucesso,
            MotivoFalha = motivoFalha
        };

        _context.HistoricoLogins.Add(historicoLogin);

        if (!loginSucesso || usuario == null)
        {
            await _context.SaveChangesAsync();
            return null;
        }

        // Atualizar último login
        usuario.UltimoLogin = DateTime.UtcNow;

        // Gerar tokens
        var token = GerarJwtToken(usuario);
        var refreshToken = GerarRefreshToken();

        usuario.RefreshToken = refreshToken;
        usuario.RefreshTokenExpiry = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationInDays);

        await _context.SaveChangesAsync();

        return new LoginResponseDto
        {
            Token = token,
            RefreshToken = refreshToken,
            Expiracao = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
            Usuario = new UsuarioDto
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email,
                TipoUsuario = usuario.TipoUsuario,
                TipoUsuarioDescricao = usuario.TipoUsuario.ToString(),
                DataCriacao = usuario.DataCriacao,
                UltimoLogin = usuario.UltimoLogin,
                Ativo = usuario.Ativo,
                PrimeiroLogin = usuario.PrimeiroLogin
            }
        };
    }

    public async Task<LoginResponseDto?> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
    {
        var principal = GetPrincipalFromExpiredToken(refreshTokenDto.Token);
        if (principal == null)
            return null;

        var usuarioIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return null;

        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == usuarioId && u.Ativo);

        if (usuario == null || usuario.RefreshToken != refreshTokenDto.RefreshToken ||
            usuario.RefreshTokenExpiry <= DateTime.UtcNow)
            return null;

        var newToken = GerarJwtToken(usuario);
        var newRefreshToken = GerarRefreshToken();

        usuario.RefreshToken = newRefreshToken;
        usuario.RefreshTokenExpiry = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationInDays);

        await _context.SaveChangesAsync();

        return new LoginResponseDto
        {
            Token = newToken,
            RefreshToken = newRefreshToken,
            Expiracao = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
            Usuario = new UsuarioDto
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email,
                TipoUsuario = usuario.TipoUsuario,
                TipoUsuarioDescricao = usuario.TipoUsuario.ToString(),
                DataCriacao = usuario.DataCriacao,
                UltimoLogin = usuario.UltimoLogin,
                Ativo = usuario.Ativo,
                PrimeiroLogin = usuario.PrimeiroLogin
            }
        };
    }

    public async Task<bool> LogoutAsync(Guid usuarioId)
    {
        var usuario = await _context.Usuarios.FindAsync(usuarioId);
        if (usuario == null)
            return false;

        usuario.RefreshToken = null;
        usuario.RefreshTokenExpiry = null;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AlterarSenhaAsync(Guid usuarioId, AlterarSenhaDto alterarSenhaDto)
    {
        var usuario = await _context.Usuarios.FindAsync(usuarioId);
        if (usuario == null)
            return false;

        if (!BCrypt.Net.BCrypt.Verify(alterarSenhaDto.SenhaAtual, usuario.SenhaHash))
            return false;

        if (alterarSenhaDto.NovaSenha != alterarSenhaDto.ConfirmarSenha)
            return false;

        usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(alterarSenhaDto.NovaSenha);
        usuario.PrimeiroLogin = false;
        usuario.DataAtualizacao = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    private string GerarJwtToken(Usuario usuario)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.Nome),
            new(ClaimTypes.Email, usuario.Email),
            new(ClaimTypes.Role, usuario.TipoUsuario.ToString()),
            new("TipoUsuario", ((int)usuario.TipoUsuario).ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private static string GerarRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwtSettings.SecretKey)),
            ValidateLifetime = false
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
        
        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                StringComparison.InvariantCultureIgnoreCase))
            throw new SecurityTokenException("Token inválido");

        return principal;
    }
}