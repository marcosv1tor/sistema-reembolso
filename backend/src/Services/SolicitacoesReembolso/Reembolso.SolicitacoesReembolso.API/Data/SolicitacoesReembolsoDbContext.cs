using Microsoft.EntityFrameworkCore;
using Reembolso.Shared.Enums;
using Reembolso.SolicitacoesReembolso.API.Models;

namespace Reembolso.SolicitacoesReembolso.API.Data;

public class SolicitacoesReembolsoDbContext : DbContext
{
    public SolicitacoesReembolsoDbContext(DbContextOptions<SolicitacoesReembolsoDbContext> options) : base(options)
    {
    }

    public DbSet<SolicitacaoReembolso> SolicitacoesReembolso { get; set; }
    public DbSet<AnexoSolicitacao> AnexosSolicitacao { get; set; }
    public DbSet<HistoricoStatusSolicitacao> HistoricoStatusSolicitacao { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuração da entidade SolicitacaoReembolso
        modelBuilder.Entity<SolicitacaoReembolso>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.ColaboradorId)
                .IsRequired();

            entity.Property(e => e.Titulo)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Descricao)
                .HasMaxLength(1000);

            entity.Property(e => e.TipoDespesa)
                .IsRequired()
                .HasConversion<int>();

            entity.Property(e => e.ValorSolicitado)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            entity.Property(e => e.ValorAprovado)
                .HasColumnType("decimal(18,2)");

            entity.Property(e => e.DataDespesa)
                .IsRequired();

            entity.Property(e => e.Status)
                .IsRequired()
                .HasConversion<int>()
                .HasDefaultValue(StatusSolicitacao.Rascunho);

            entity.Property(e => e.ObservacaoAprovacao)
                .HasMaxLength(500);

            entity.Property(e => e.ObservacaoPagamento)
                .HasMaxLength(500);

            entity.Property(e => e.MotivoCancelamento)
                .HasMaxLength(500);

            entity.Property(e => e.DataCriacao)
                .IsRequired();

            entity.Property(e => e.Ativo)
                .IsRequired()
                .HasDefaultValue(true);

            // Relacionamentos
            entity.HasMany(e => e.Anexos)
                .WithOne(a => a.SolicitacaoReembolso)
                .HasForeignKey(a => a.SolicitacaoReembolsoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.HistoricoStatus)
                .WithOne(h => h.SolicitacaoReembolso)
                .HasForeignKey(h => h.SolicitacaoReembolsoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Índices
            entity.HasIndex(e => e.ColaboradorId)
                .HasDatabaseName("IX_SolicitacoesReembolso_ColaboradorId");

            entity.HasIndex(e => e.Status)
                .HasDatabaseName("IX_SolicitacoesReembolso_Status");

            entity.HasIndex(e => e.DataDespesa)
                .HasDatabaseName("IX_SolicitacoesReembolso_DataDespesa");

            entity.HasIndex(e => e.DataCriacao)
                .HasDatabaseName("IX_SolicitacoesReembolso_DataCriacao");

            entity.HasIndex(e => e.TipoDespesa)
                .HasDatabaseName("IX_SolicitacoesReembolso_TipoDespesa");

            entity.HasIndex(e => e.Ativo)
                .HasDatabaseName("IX_SolicitacoesReembolso_Ativo");
        });

        // Configuração da entidade AnexoSolicitacao
        modelBuilder.Entity<AnexoSolicitacao>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.SolicitacaoReembolsoId)
                .IsRequired();

            entity.Property(e => e.NomeArquivo)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.NomeArquivoOriginal)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.TipoConteudo)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.TamanhoBytes)
                .IsRequired();

            entity.Property(e => e.CaminhoArquivo)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.Descricao)
                .HasMaxLength(200);

            entity.Property(e => e.DataCriacao)
                .IsRequired();

            entity.Property(e => e.Ativo)
                .IsRequired()
                .HasDefaultValue(true);

            // Índices
            entity.HasIndex(e => e.SolicitacaoReembolsoId)
                .HasDatabaseName("IX_AnexosSolicitacao_SolicitacaoReembolsoId");

            entity.HasIndex(e => e.Ativo)
                .HasDatabaseName("IX_AnexosSolicitacao_Ativo");
        });

        // Configuração da entidade HistoricoStatusSolicitacao
        modelBuilder.Entity<HistoricoStatusSolicitacao>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.SolicitacaoReembolsoId)
                .IsRequired();

            entity.Property(e => e.StatusAnterior)
                .IsRequired()
                .HasConversion<int>();

            entity.Property(e => e.StatusNovo)
                .IsRequired()
                .HasConversion<int>();

            entity.Property(e => e.DataMudanca)
                .IsRequired();

            entity.Property(e => e.AlteradoPorId)
                .IsRequired();

            entity.Property(e => e.Observacao)
                .HasMaxLength(500);

            entity.Property(e => e.NomeUsuario)
                .HasMaxLength(100);

            entity.Property(e => e.DataCriacao)
                .IsRequired();

            entity.Property(e => e.Ativo)
                .IsRequired()
                .HasDefaultValue(true);

            // Índices
            entity.HasIndex(e => e.SolicitacaoReembolsoId)
                .HasDatabaseName("IX_HistoricoStatusSolicitacao_SolicitacaoReembolsoId");

            entity.HasIndex(e => e.DataMudanca)
                .HasDatabaseName("IX_HistoricoStatusSolicitacao_DataMudanca");

            entity.HasIndex(e => e.AlteradoPorId)
                .HasDatabaseName("IX_HistoricoStatusSolicitacao_AlteradoPorId");
        });

        // Dados iniciais (seed)
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Dados de exemplo para demonstração
        var colaboradorId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var colaboradorId2 = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var usuarioId1 = Guid.Parse("33333333-3333-3333-3333-333333333333");

        var solicitacoes = new List<SolicitacaoReembolso>
        {
            new SolicitacaoReembolso
            {
                Id = Guid.NewGuid(),
                ColaboradorId = colaboradorId1,
                Titulo = "Combustível - Viagem São Paulo",
                Descricao = "Abastecimento para viagem de negócios",
                TipoDespesa = TipoDespesa.Combustivel,
                ValorSolicitado = 150.00m,
                DataDespesa = DateTime.Now.AddDays(-5),
                Status = StatusSolicitacao.PendenteAprovacaoFinanceira,
                DataCriacao = DateTime.UtcNow.AddDays(-3),
                Ativo = true,
                CriadoPor = "Sistema"
            },
            new SolicitacaoReembolso
            {
                Id = Guid.NewGuid(),
                ColaboradorId = colaboradorId2,
                Titulo = "Almoço - Reunião com Cliente",
                Descricao = "Almoço de negócios com cliente importante",
                TipoDespesa = TipoDespesa.Alimentacao,
                ValorSolicitado = 85.50m,
                ValorAprovado = 85.50m,
                DataDespesa = DateTime.Now.AddDays(-2),
                Status = StatusSolicitacao.Aprovada,
                DataAprovacao = DateTime.UtcNow.AddDays(-1),
                AprovadoPorId = usuarioId1,
                ObservacaoAprovacao = "Aprovado conforme política da empresa",
                DataCriacao = DateTime.UtcNow.AddDays(-2),
                Ativo = true,
                CriadoPor = "Sistema"
            }
        };

        modelBuilder.Entity<SolicitacaoReembolso>().HasData(solicitacoes);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Entity is SolicitacaoReembolso solicitacao)
            {
                if (entry.State == EntityState.Added)
                {
                    solicitacao.DataCriacao = DateTime.UtcNow;
                }
                else if (entry.State == EntityState.Modified)
                {
                    solicitacao.DataAtualizacao = DateTime.UtcNow;
                }
            }
            else if (entry.Entity is AnexoSolicitacao anexo)
            {
                if (entry.State == EntityState.Added)
                {
                    anexo.DataCriacao = DateTime.UtcNow;
                }
                else if (entry.State == EntityState.Modified)
                {
                    anexo.DataAtualizacao = DateTime.UtcNow;
                }
            }
            else if (entry.Entity is HistoricoStatusSolicitacao historico)
            {
                if (entry.State == EntityState.Added)
                {
                    historico.DataCriacao = DateTime.UtcNow;
                    historico.DataMudanca = DateTime.UtcNow;
                }
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}