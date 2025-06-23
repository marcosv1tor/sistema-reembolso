using Reembolso.Shared.Models;
using System.ComponentModel.DataAnnotations;

namespace Reembolso.Funcionarios.API.Models;

public class Colaborador : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Matricula { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [StringLength(20)]
    public string? Telefone { get; set; }

    [StringLength(100)]
    public string? Cargo { get; set; }

    [StringLength(100)]
    public string? Departamento { get; set; }

    public DateTime DataAdmissao { get; set; }

    public DateTime? DataDemissao { get; set; }

    [StringLength(14)]
    public string? CPF { get; set; }

    [StringLength(20)]
    public string? RG { get; set; }

    public DateTime DataNascimento { get; set; }

    [StringLength(200)]
    public string? Endereco { get; set; }

    [StringLength(50)]
    public string? Cidade { get; set; }

    [StringLength(2)]
    public string? Estado { get; set; }

    [StringLength(10)]
    public string? CEP { get; set; }

    public decimal? Salario { get; set; }

    // Relacionamento com usuário
    public Guid? UsuarioId { get; set; }

    // Propriedades calculadas
    public bool EstaAtivo => Ativo && DataDemissao == null;
    
    public int TempoEmpresaAnos => DataDemissao.HasValue 
        ? (DataDemissao.Value - DataAdmissao).Days / 365
        : (DateTime.Now - DataAdmissao).Days / 365;
}