namespace Reembolso.Shared.Entities;

public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    public DateTime? DataAtualizacao { get; set; }
    public string? CriadoPor { get; set; }
    public string? AtualizadoPor { get; set; }
    public bool Ativo { get; set; } = true;
}