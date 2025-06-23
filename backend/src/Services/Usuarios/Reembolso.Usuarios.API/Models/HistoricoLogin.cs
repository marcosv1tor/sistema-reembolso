using Reembolso.Shared.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Reembolso.Usuarios.API.Models;

public class HistoricoLogin : BaseEntity
{
    [Required]
    public Guid UsuarioId { get; set; }

    [Required]
    public DateTime DataLogin { get; set; } = DateTime.UtcNow;

    [StringLength(45)]
    public string? EnderecoIP { get; set; }

    [StringLength(500)]
    public string? UserAgent { get; set; }

    public bool LoginSucesso { get; set; }

    [StringLength(200)]
    public string? MotivoFalha { get; set; }

    // Propriedades de navegação
    [ForeignKey(nameof(UsuarioId))]
    public virtual Usuario Usuario { get; set; } = null!;
}