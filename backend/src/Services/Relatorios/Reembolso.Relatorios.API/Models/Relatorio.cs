using Reembolso.Shared.Entities;
using Reembolso.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace Reembolso.Relatorios.API.Models;

public class Relatorio : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Nome { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Descricao { get; set; }

    [Required]
    public TipoRelatorio Tipo { get; set; }

    [Required]
    public StatusRelatorio Status { get; set; }

    [Required]
    public DateTime DataInicio { get; set; }

    [Required]
    public DateTime DataFim { get; set; }

    public Guid? ColaboradorId { get; set; }

    public Guid? DepartamentoId { get; set; }

    public TipoDespesa? TipoDespesa { get; set; }

    public StatusSolicitacao? StatusSolicitacao { get; set; }

    [Required]
    public Guid GeradoPorId { get; set; }

    public DateTime? DataGeracao { get; set; }

    public DateTime? DataConclusao { get; set; }

    [MaxLength(500)]
    public string? CaminhoArquivo { get; set; }

    [MaxLength(100)]
    public string? NomeArquivo { get; set; }

    public long? TamanhoBytes { get; set; }

    [MaxLength(50)]
    public string? TipoConteudo { get; set; }

    [MaxLength(1000)]
    public string? MensagemErro { get; set; }

    public int? TotalRegistros { get; set; }

    public decimal? ValorTotal { get; set; }

    // Propriedades calculadas
    public bool EstaProcessando => Status == StatusRelatorio.Processando;
    
    public bool EstaConcluido => Status == StatusRelatorio.Concluido;
    
    public bool TemErro => Status == StatusRelatorio.Erro;
    
    public bool PodeSerBaixado => EstaConcluido && !string.IsNullOrEmpty(CaminhoArquivo);
    
    public TimeSpan? TempoProcessamento
    {
        get
        {
            if (DataGeracao.HasValue && DataConclusao.HasValue)
            {
                return DataConclusao.Value - DataGeracao.Value;
            }
            return null;
        }
    }
    
    public string TamanhoFormatado
    {
        get
        {
            if (!TamanhoBytes.HasValue) return "N/A";
            
            var bytes = TamanhoBytes.Value;
            string[] suffixes = { "B", "KB", "MB", "GB" };
            int counter = 0;
            decimal number = bytes;
            
            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }
            
            return $"{number:n1} {suffixes[counter]}";
        }
    }
    
    public int DiasRetencao
    {
        get
        {
            return Tipo switch
            {
                TipoRelatorio.SolicitacoesPorPeriodo => 30,
                TipoRelatorio.ReembolsosPorColaborador => 60,
                TipoRelatorio.ReembolsosPorDepartamento => 90,
                TipoRelatorio.EstatisticasGerais => 30,
                TipoRelatorio.RelatorioFinanceiro => 365,
                _ => 30
            };
        }
    }
    
    public bool DeveSerExcluido
    {
        get
        {
            if (!DataConclusao.HasValue) return false;
            return DateTime.UtcNow > DataConclusao.Value.AddDays(DiasRetencao);
        }
    }
}