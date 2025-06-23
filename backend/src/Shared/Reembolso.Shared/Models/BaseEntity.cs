using System.ComponentModel.DataAnnotations;

namespace Reembolso.Shared.Models;

public abstract class BaseEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    
    public DateTime? DataAtualizacao { get; set; }
    
    public bool Ativo { get; set; } = true;
    
    public string? CriadoPor { get; set; }
    
    public string? AtualizadoPor { get; set; }
}