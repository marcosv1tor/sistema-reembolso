using Reembolso.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace Reembolso.Relatorios.API.DTOs;

public class RelatorioDto
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public TipoRelatorio Tipo { get; set; }
    public StatusRelatorio Status { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public Guid? ColaboradorId { get; set; }
    public Guid? DepartamentoId { get; set; }
    public TipoDespesa? TipoDespesa { get; set; }
    public StatusSolicitacao? StatusSolicitacao { get; set; }
    public Guid GeradoPorId { get; set; }
    public DateTime? DataGeracao { get; set; }
    public DateTime? DataConclusao { get; set; }
    public string? CaminhoArquivo { get; set; }
    public string? NomeArquivo { get; set; }
    public long? TamanhoBytes { get; set; }
    public string? TipoConteudo { get; set; }
    public string? MensagemErro { get; set; }
    public int? TotalRegistros { get; set; }
    public decimal? ValorTotal { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime DataAtualizacao { get; set; }
    
    // Propriedades calculadas
    public bool EstaProcessando { get; set; }
    public bool EstaConcluido { get; set; }
    public bool TemErro { get; set; }
    public bool PodeSerBaixado { get; set; }
    public TimeSpan? TempoProcessamento { get; set; }
    public string TamanhoFormatado { get; set; } = string.Empty;
    public int DiasRetencao { get; set; }
    public bool DeveSerExcluido { get; set; }
}

public class CriarRelatorioDto
{
    [Required(ErrorMessage = "Nome é obrigatório")]
    [StringLength(200, ErrorMessage = "Nome deve ter no máximo 200 caracteres")]
    public string Nome { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Descrição deve ter no máximo 500 caracteres")]
    public string? Descricao { get; set; }

    [Required(ErrorMessage = "Tipo do relatório é obrigatório")]
    public TipoRelatorio Tipo { get; set; }

    [Required(ErrorMessage = "Data de início é obrigatória")]
    public DateTime DataInicio { get; set; }

    [Required(ErrorMessage = "Data de fim é obrigatória")]
    public DateTime DataFim { get; set; }

    public Guid? ColaboradorId { get; set; }

    public Guid? DepartamentoId { get; set; }

    public TipoDespesa? TipoDespesa { get; set; }

    public StatusSolicitacao? StatusSolicitacao { get; set; }
}

public class RelatorioResumoDto
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public TipoRelatorio Tipo { get; set; }
    public StatusRelatorio Status { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public Guid GeradoPorId { get; set; }
    public DateTime? DataGeracao { get; set; }
    public DateTime? DataConclusao { get; set; }
    public string? NomeArquivo { get; set; }
    public string TamanhoFormatado { get; set; } = string.Empty;
    public int? TotalRegistros { get; set; }
    public decimal? ValorTotal { get; set; }
    public DateTime DataCriacao { get; set; }
    
    // Propriedades calculadas
    public bool PodeSerBaixado { get; set; }
    public TimeSpan? TempoProcessamento { get; set; }
}

public class FiltroRelatorioDto
{
    public TipoRelatorio? Tipo { get; set; }
    public StatusRelatorio? Status { get; set; }
    public DateTime? DataInicioGeracao { get; set; }
    public DateTime? DataFimGeracao { get; set; }
    public Guid? GeradoPorId { get; set; }
    public string? Nome { get; set; }
}

public class EstatisticasRelatorioDto
{
    public int TotalRelatorios { get; set; }
    public int RelatoriosProcessando { get; set; }
    public int RelatoriosConcluidos { get; set; }
    public int RelatoriosComErro { get; set; }
    public Dictionary<TipoRelatorio, int> RelatoriosPorTipo { get; set; } = new();
    public Dictionary<string, int> RelatoriosPorMes { get; set; } = new();
    public long TamanhoTotalBytes { get; set; }
    public string TamanhoTotalFormatado { get; set; } = string.Empty;
    public TimeSpan? TempoMedioProcessamento { get; set; }
    public int RelatoriosParaExclusao { get; set; }
}

public class DadosRelatorioDto
{
    public string Titulo { get; set; } = string.Empty;
    public DateTime DataGeracao { get; set; }
    public DateTime PeriodoInicio { get; set; }
    public DateTime PeriodoFim { get; set; }
    public Dictionary<string, object> Parametros { get; set; } = new();
    public List<Dictionary<string, object>> Dados { get; set; } = new();
    public Dictionary<string, object> Resumo { get; set; } = new();
}