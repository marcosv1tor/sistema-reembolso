using Reembolso.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace Reembolso.SolicitacoesReembolso.API.DTOs;

public class SolicitacaoReembolsoDto
{
    public Guid Id { get; set; }
    public Guid ColaboradorId { get; set; }
    public string? NomeColaborador { get; set; }
    public string? MatriculaColaborador { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public TipoDespesa TipoDespesa { get; set; }
    public string TipoDespesaDescricao { get; set; } = string.Empty;
    public decimal ValorSolicitado { get; set; }
    public decimal? ValorAprovado { get; set; }
    public DateTime DataDespesa { get; set; }
    public StatusSolicitacao Status { get; set; }
    public string StatusDescricao { get; set; } = string.Empty;
    public DateTime? DataAprovacao { get; set; }
    public Guid? AprovadoPorId { get; set; }
    public string? NomeAprovador { get; set; }
    public string? ObservacaoAprovacao { get; set; }
    public DateTime? DataPagamento { get; set; }
    public Guid? PagoPorId { get; set; }
    public string? NomePagador { get; set; }
    public string? ObservacaoPagamento { get; set; }
    public DateTime? DataCancelamento { get; set; }
    public string? MotivoCancelamento { get; set; }
    public bool PodeSerEditada { get; set; }
    public bool PodeSerCancelada { get; set; }
    public bool PodeSerAprovada { get; set; }
    public bool PodeSerPaga { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
    public int QuantidadeAnexos { get; set; }
    public List<AnexoSolicitacaoDto> Anexos { get; set; } = new();
    public List<HistoricoStatusSolicitacaoDto> HistoricoStatus { get; set; } = new();
}

public class CriarSolicitacaoReembolsoDto
{
    [Required(ErrorMessage = "Colaborador é obrigatório")]
    public Guid ColaboradorId { get; set; }

    [Required(ErrorMessage = "Título é obrigatório")]
    [StringLength(200, ErrorMessage = "Título deve ter no máximo 200 caracteres")]
    public string Titulo { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Descrição deve ter no máximo 1000 caracteres")]
    public string? Descricao { get; set; }

    [Required(ErrorMessage = "Tipo de despesa é obrigatório")]
    public TipoDespesa TipoDespesa { get; set; }

    [Required(ErrorMessage = "Valor solicitado é obrigatório")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Valor deve ser maior que zero")]
    public decimal ValorSolicitado { get; set; }

    [Required(ErrorMessage = "Data da despesa é obrigatória")]
    public DateTime DataDespesa { get; set; }
}

public class AtualizarSolicitacaoReembolsoDto
{
    [Required(ErrorMessage = "Título é obrigatório")]
    [StringLength(200, ErrorMessage = "Título deve ter no máximo 200 caracteres")]
    public string Titulo { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Descrição deve ter no máximo 1000 caracteres")]
    public string? Descricao { get; set; }

    [Required(ErrorMessage = "Tipo de despesa é obrigatório")]
    public TipoDespesa TipoDespesa { get; set; }

    [Required(ErrorMessage = "Valor solicitado é obrigatório")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Valor deve ser maior que zero")]
    public decimal ValorSolicitado { get; set; }

    [Required(ErrorMessage = "Data da despesa é obrigatória")]
    public DateTime DataDespesa { get; set; }
}

public class AprovarSolicitacaoDto
{
    [Required(ErrorMessage = "Valor aprovado é obrigatório")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Valor aprovado deve ser maior que zero")]
    public decimal ValorAprovado { get; set; }

    [StringLength(500, ErrorMessage = "Observação deve ter no máximo 500 caracteres")]
    public string? ObservacaoAprovacao { get; set; }
}

public class RejeitarSolicitacaoDto
{
    [Required(ErrorMessage = "Observação é obrigatória para rejeição")]
    [StringLength(500, ErrorMessage = "Observação deve ter no máximo 500 caracteres")]
    public string ObservacaoAprovacao { get; set; } = string.Empty;
}

public class PagarSolicitacaoDto
{
    [StringLength(500, ErrorMessage = "Observação deve ter no máximo 500 caracteres")]
    public string? ObservacaoPagamento { get; set; }
}

public class CancelarSolicitacaoDto
{
    [Required(ErrorMessage = "Motivo do cancelamento é obrigatório")]
    [StringLength(500, ErrorMessage = "Motivo deve ter no máximo 500 caracteres")]
    public string MotivoCancelamento { get; set; } = string.Empty;
}

public class SolicitacaoReembolsoResumoDto
{
    public Guid Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public TipoDespesa TipoDespesa { get; set; }
    public string TipoDespesaDescricao { get; set; } = string.Empty;
    public decimal ValorSolicitado { get; set; }
    public decimal? ValorAprovado { get; set; }
    public DateTime DataDespesa { get; set; }
    public StatusSolicitacao Status { get; set; }
    public string StatusDescricao { get; set; } = string.Empty;
    public string? NomeColaborador { get; set; }
    public string? MatriculaColaborador { get; set; }
    public DateTime DataCriacao { get; set; }
    public int QuantidadeAnexos { get; set; }
}

public class AnexoSolicitacaoDto
{
    public Guid Id { get; set; }
    public Guid SolicitacaoReembolsoId { get; set; }
    public string NomeArquivo { get; set; } = string.Empty;
    public string NomeArquivoOriginal { get; set; } = string.Empty;
    public string TipoConteudo { get; set; } = string.Empty;
    public long TamanhoBytes { get; set; }
    public string TamanhoFormatado { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public bool EhImagem { get; set; }
    public bool EhPDF { get; set; }
    public DateTime DataCriacao { get; set; }
}

public class HistoricoStatusSolicitacaoDto
{
    public Guid Id { get; set; }
    public StatusSolicitacao StatusAnterior { get; set; }
    public StatusSolicitacao StatusNovo { get; set; }
    public string DescricaoMudanca { get; set; } = string.Empty;
    public DateTime DataMudanca { get; set; }
    public string TempoDecorrido { get; set; } = string.Empty;
    public string? NomeUsuario { get; set; }
    public string? Observacao { get; set; }
}