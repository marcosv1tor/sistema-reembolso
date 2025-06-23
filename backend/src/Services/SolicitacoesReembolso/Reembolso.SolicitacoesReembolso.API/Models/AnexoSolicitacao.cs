using Reembolso.Shared.Models;
using System.ComponentModel.DataAnnotations;

namespace Reembolso.SolicitacoesReembolso.API.Models;

public class AnexoSolicitacao : BaseEntity
{
    [Required]
    public Guid SolicitacaoReembolsoId { get; set; }

    [Required]
    [StringLength(255)]
    public string NomeArquivo { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string NomeArquivoOriginal { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string TipoConteudo { get; set; } = string.Empty;

    [Required]
    public long TamanhoBytes { get; set; }

    [Required]
    [StringLength(500)]
    public string CaminhoArquivo { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Descricao { get; set; }

    // Relacionamento
    public virtual SolicitacaoReembolso SolicitacaoReembolso { get; set; } = null!;

    // Propriedades calculadas
    public string TamanhoFormatado
    {
        get
        {
            if (TamanhoBytes < 1024)
                return $"{TamanhoBytes} B";
            else if (TamanhoBytes < 1024 * 1024)
                return $"{TamanhoBytes / 1024:F1} KB";
            else
                return $"{TamanhoBytes / (1024 * 1024):F1} MB";
        }
    }

    public string ExtensaoArquivo
    {
        get
        {
            var extensao = Path.GetExtension(NomeArquivoOriginal);
            return string.IsNullOrEmpty(extensao) ? "" : extensao.ToLowerInvariant();
        }
    }

    public bool EhImagem
    {
        get
        {
            var extensoesImagem = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
            return extensoesImagem.Contains(ExtensaoArquivo);
        }
    }

    public bool EhPDF => ExtensaoArquivo == ".pdf";
}