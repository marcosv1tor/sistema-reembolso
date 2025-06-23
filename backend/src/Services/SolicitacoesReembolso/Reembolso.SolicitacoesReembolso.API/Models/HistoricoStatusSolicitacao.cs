using Reembolso.Shared.Enums;
using Reembolso.Shared.Models;
using System.ComponentModel.DataAnnotations;

namespace Reembolso.SolicitacoesReembolso.API.Models;

public class HistoricoStatusSolicitacao : BaseEntity
{
    [Required]
    public Guid SolicitacaoReembolsoId { get; set; }

    [Required]
    public StatusSolicitacao StatusAnterior { get; set; }

    [Required]
    public StatusSolicitacao StatusNovo { get; set; }

    [Required]
    public DateTime DataMudanca { get; set; }

    [Required]
    public Guid AlteradoPorId { get; set; }

    [StringLength(500)]
    public string? Observacao { get; set; }

    [StringLength(100)]
    public string? NomeUsuario { get; set; }

    // Relacionamento
    public virtual SolicitacaoReembolso SolicitacaoReembolso { get; set; } = null!;

    // Propriedades calculadas
    public string DescricaoMudanca
    {
        get
        {
            return $"{GetStatusDescricao(StatusAnterior)} → {GetStatusDescricao(StatusNovo)}";
        }
    }

    public string TempoDecorrido
    {
        get
        {
            var tempo = DateTime.UtcNow - DataMudanca;
            
            if (tempo.TotalMinutes < 1)
                return "Agora mesmo";
            else if (tempo.TotalHours < 1)
                return $"{(int)tempo.TotalMinutes} minuto(s) atrás";
            else if (tempo.TotalDays < 1)
                return $"{(int)tempo.TotalHours} hora(s) atrás";
            else if (tempo.TotalDays < 30)
                return $"{(int)tempo.TotalDays} dia(s) atrás";
            else
                return DataMudanca.ToString("dd/MM/yyyy HH:mm");
        }
    }

    private static string GetStatusDescricao(StatusSolicitacao status)
    {
        return status switch
        {
            StatusSolicitacao.Rascunho => "Rascunho",
            StatusSolicitacao.PendenteAprovacaoFinanceira => "Pendente Aprovação",
            StatusSolicitacao.Aprovada => "Aprovada",
            StatusSolicitacao.Rejeitada => "Rejeitada",
            StatusSolicitacao.Pago => "Pago",
            StatusSolicitacao.Cancelada => "Cancelada",
            _ => status.ToString()
        };
    }
}