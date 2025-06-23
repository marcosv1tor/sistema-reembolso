using Reembolso.Shared.Enums;

namespace Reembolso.Usuarios.API.DTOs;

public class UsuarioDto
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public TipoUsuario TipoUsuario { get; set; }
    public string TipoUsuarioDescricao { get; set; } = string.Empty;
    public DateTime DataCriacao { get; set; }
    public DateTime? UltimoLogin { get; set; }
    public bool Ativo { get; set; }
    public bool PrimeiroLogin { get; set; }
}

public class CriarUsuarioDto
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public TipoUsuario TipoUsuario { get; set; }
}

public class AtualizarUsuarioDto
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public TipoUsuario TipoUsuario { get; set; }
    public bool Ativo { get; set; }
}

public class AlterarSenhaDto
{
    public string SenhaAtual { get; set; } = string.Empty;
    public string NovaSenha { get; set; } = string.Empty;
    public string ConfirmarSenha { get; set; } = string.Empty;
}

public class LoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime Expiracao { get; set; }
    public UsuarioDto Usuario { get; set; } = null!;
}

public class RefreshTokenDto
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}