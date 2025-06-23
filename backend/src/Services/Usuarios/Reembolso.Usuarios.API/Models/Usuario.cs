using Reembolso.Shared.Enums;
using Reembolso.Shared.Models;
using System.ComponentModel.DataAnnotations;

namespace Reembolso.Usuarios.API.Models;

public class Usuario : BaseEntity
{
    [Required(ErrorMessage = "Nome é obrigatório")]
    [StringLength(100, ErrorMessage = "Nome deve ter no máximo 100 caracteres")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email deve ter um formato válido")]
    [StringLength(150, ErrorMessage = "Email deve ter no máximo 150 caracteres")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Senha é obrigatória")]
    public string SenhaHash { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tipo de usuário é obrigatório")]
    public TipoUsuario TipoUsuario { get; set; }

    public DateTime? UltimoLogin { get; set; }

    public bool PrimeiroLogin { get; set; } = true;

    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiry { get; set; }

    // Propriedades de navegação
    public virtual ICollection<HistoricoLogin> HistoricoLogins { get; set; } = new List<HistoricoLogin>();
}