using System.ComponentModel.DataAnnotations;

namespace Reembolso.Funcionarios.API.DTOs;

public class ColaboradorDto
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Matricula { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public string? Cargo { get; set; }
    public string? Departamento { get; set; }
    public DateTime DataAdmissao { get; set; }
    public DateTime? DataDemissao { get; set; }
    public string? CPF { get; set; }
    public string? RG { get; set; }
    public DateTime DataNascimento { get; set; }
    public string? Endereco { get; set; }
    public string? Cidade { get; set; }
    public string? Estado { get; set; }
    public string? CEP { get; set; }
    public decimal? Salario { get; set; }
    public Guid? UsuarioId { get; set; }
    public bool Ativo { get; set; }
    public bool EstaAtivo { get; set; }
    public int TempoEmpresaAnos { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
}

public class CriarColaboradorDto
{
    [Required(ErrorMessage = "Nome é obrigatório")]
    [StringLength(100, ErrorMessage = "Nome deve ter no máximo 100 caracteres")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Matrícula é obrigatória")]
    [StringLength(20, ErrorMessage = "Matrícula deve ter no máximo 20 caracteres")]
    public string Matricula { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email deve ter um formato válido")]
    [StringLength(100, ErrorMessage = "Email deve ter no máximo 100 caracteres")]
    public string Email { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "Telefone deve ter no máximo 20 caracteres")]
    public string? Telefone { get; set; }

    [StringLength(100, ErrorMessage = "Cargo deve ter no máximo 100 caracteres")]
    public string? Cargo { get; set; }

    [StringLength(100, ErrorMessage = "Departamento deve ter no máximo 100 caracteres")]
    public string? Departamento { get; set; }

    [Required(ErrorMessage = "Data de admissão é obrigatória")]
    public DateTime DataAdmissao { get; set; }

    public DateTime? DataDemissao { get; set; }

    [StringLength(14, ErrorMessage = "CPF deve ter no máximo 14 caracteres")]
    public string? CPF { get; set; }

    [StringLength(20, ErrorMessage = "RG deve ter no máximo 20 caracteres")]
    public string? RG { get; set; }

    [Required(ErrorMessage = "Data de nascimento é obrigatória")]
    public DateTime DataNascimento { get; set; }

    [StringLength(200, ErrorMessage = "Endereço deve ter no máximo 200 caracteres")]
    public string? Endereco { get; set; }

    [StringLength(50, ErrorMessage = "Cidade deve ter no máximo 50 caracteres")]
    public string? Cidade { get; set; }

    [StringLength(2, ErrorMessage = "Estado deve ter no máximo 2 caracteres")]
    public string? Estado { get; set; }

    [StringLength(10, ErrorMessage = "CEP deve ter no máximo 10 caracteres")]
    public string? CEP { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Salário deve ser um valor positivo")]
    public decimal? Salario { get; set; }

    public Guid? UsuarioId { get; set; }
}

public class AtualizarColaboradorDto
{
    [Required(ErrorMessage = "Nome é obrigatório")]
    [StringLength(100, ErrorMessage = "Nome deve ter no máximo 100 caracteres")]
    public string Nome { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "Telefone deve ter no máximo 20 caracteres")]
    public string? Telefone { get; set; }

    [StringLength(100, ErrorMessage = "Cargo deve ter no máximo 100 caracteres")]
    public string? Cargo { get; set; }

    [StringLength(100, ErrorMessage = "Departamento deve ter no máximo 100 caracteres")]
    public string? Departamento { get; set; }

    public DateTime? DataDemissao { get; set; }

    [StringLength(20, ErrorMessage = "RG deve ter no máximo 20 caracteres")]
    public string? RG { get; set; }

    [StringLength(200, ErrorMessage = "Endereço deve ter no máximo 200 caracteres")]
    public string? Endereco { get; set; }

    [StringLength(50, ErrorMessage = "Cidade deve ter no máximo 50 caracteres")]
    public string? Cidade { get; set; }

    [StringLength(2, ErrorMessage = "Estado deve ter no máximo 2 caracteres")]
    public string? Estado { get; set; }

    [StringLength(10, ErrorMessage = "CEP deve ter no máximo 10 caracteres")]
    public string? CEP { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Salário deve ser um valor positivo")]
    public decimal? Salario { get; set; }

    public Guid? UsuarioId { get; set; }
}

public class ColaboradorResumoDto
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Matricula { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Cargo { get; set; }
    public string? Departamento { get; set; }
    public bool EstaAtivo { get; set; }
}