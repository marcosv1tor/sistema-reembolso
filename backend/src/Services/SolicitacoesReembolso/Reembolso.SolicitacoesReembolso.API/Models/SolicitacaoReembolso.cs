using Reembolso.Shared.Enums;
using Reembolso.Shared.Models;
using System.ComponentModel.DataAnnotations;

namespace Reembolso.SolicitacoesReembolso.API.Models;

public class SolicitacaoReembolso : BaseEntity
{
    [Required]
    public Guid ColaboradorId { get; set; }

    [Required]
    [StringLength(200)]
    public string Titulo { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Descricao { get; set; }

    [Required]
    public TipoDespesa TipoDespesa { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Valor deve ser maior que zero")]
    public decimal ValorSolicitado { get; set; }

    public decimal? ValorAprovado { get; set; }

    [Required]
    public DateTime DataDespesa { get; set; }

    [Required]
    public StatusSolicitacao Status { get; set; } = StatusSolicitacao.Rascunho;

    public DateTime? DataAprovacao { get; set; }

    public Guid? AprovadoPorId { get; set; }

    [StringLength(500)]
    public string? ObservacaoAprovacao { get; set; }

    public DateTime? DataPagamento { get; set; }

    public Guid? PagoPorId { get; set; }

    [StringLength(500)]
    public string? ObservacaoPagamento { get; set; }

    public DateTime? DataCancelamento { get; set; }

    [StringLength(500)]
    public string? MotivoCancelamento { get; set; }

    // Propriedades calculadas
    public bool PodeSerEditada => Status == StatusSolicitacao.Rascunho;
    
    public bool PodeSerCancelada => Status == StatusSolicitacao.Rascunho || 
                                   Status == StatusSolicitacao.PendenteAprovacaoFinanceira;
    
    public bool PodeSerAprovada => Status == StatusSolicitacao.PendenteAprovacaoFinanceira;
    
    public bool PodeSerPaga => Status == StatusSolicitacao.Aprovada;

    // Coleção de anexos
    public virtual ICollection<AnexoSolicitacao> Anexos { get; set; } = new List<AnexoSolicitacao>();

    // Histórico de status
    public virtual ICollection<HistoricoStatusSolicitacao> HistoricoStatus { get; set; } = new List<HistoricoStatusSolicitacao>();
}