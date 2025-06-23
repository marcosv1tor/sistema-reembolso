using Microsoft.EntityFrameworkCore;
using Reembolso.Relatorios.API.Models;
using Reembolso.Shared.Enums;

namespace Reembolso.Relatorios.API.Data;

public class RelatoriosDbContext : DbContext
{
    public RelatoriosDbContext(DbContextOptions<RelatoriosDbContext> options) : base(options)
    {
    }

    public DbSet<Relatorio> Relatorios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuração da entidade Relatorio
        modelBuilder.Entity<Relatorio>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Nome)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Descricao)
                .HasMaxLength(500);

            entity.Property(e => e.Tipo)
                .IsRequired()
                .HasConversion<string>();

            entity.Property(e => e.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasDefaultValue(StatusRelatorio.Pendente);

            entity.Property(e => e.DataInicio)
                .IsRequired();

            entity.Property(e => e.DataFim)
                .IsRequired();

            entity.Property(e => e.TipoDespesa)
                .HasConversion<string>();

            entity.Property(e => e.StatusSolicitacao)
                .HasConversion<string>();

            entity.Property(e => e.GeradoPorId)
                .IsRequired();

            entity.Property(e => e.CaminhoArquivo)
                .HasMaxLength(500);

            entity.Property(e => e.NomeArquivo)
                .HasMaxLength(100);

            entity.Property(e => e.TipoConteudo)
                .HasMaxLength(50);

            entity.Property(e => e.MensagemErro)
                .HasMaxLength(1000);

            entity.Property(e => e.ValorTotal)
                .HasColumnType("decimal(18,2)");

            entity.Property(e => e.DataCriacao)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.DataAtualizacao)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.CriadoPor)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.AtualizadoPor)
                .HasMaxLength(100);

            entity.Property(e => e.Ativo)
                .IsRequired()
                .HasDefaultValue(true);

            // Índices
            entity.HasIndex(e => e.Tipo)
                .HasDatabaseName("IX_Relatorios_Tipo");

            entity.HasIndex(e => e.Status)
                .HasDatabaseName("IX_Relatorios_Status");

            entity.HasIndex(e => e.GeradoPorId)
                .HasDatabaseName("IX_Relatorios_GeradoPorId");

            entity.HasIndex(e => e.DataGeracao)
                .HasDatabaseName("IX_Relatorios_DataGeracao");

            entity.HasIndex(e => new { e.DataInicio, e.DataFim })
                .HasDatabaseName("IX_Relatorios_Periodo");

            entity.HasIndex(e => e.Ativo)
                .HasDatabaseName("IX_Relatorios_Ativo");
        });

        // Dados iniciais
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        var adminUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var dataAtual = DateTime.UtcNow;

        modelBuilder.Entity<Relatorio>().HasData(
            new Relatorio
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                Nome = "Relatório de Reembolsos - Janeiro 2024",
                Descricao = "Relatório mensal de todas as solicitações de reembolso do mês de janeiro",
                Tipo = TipoRelatorio.SolicitacoesPorPeriodo,
                Status = StatusRelatorio.Concluido,
                DataInicio = new DateTime(2024, 1, 1),
                DataFim = new DateTime(2024, 1, 31),
                GeradoPorId = adminUserId,
                DataGeracao = dataAtual.AddDays(-30),
                DataConclusao = dataAtual.AddDays(-30).AddMinutes(5),
                NomeArquivo = "relatorio_reembolsos_jan2024.pdf",
                CaminhoArquivo = "/relatorios/relatorio_reembolsos_jan2024.pdf",
                TamanhoBytes = 1024000, // 1MB
                TipoConteudo = "application/pdf",
                TotalRegistros = 45,
                ValorTotal = 15750.50m,
                DataCriacao = dataAtual.AddDays(-30),
                DataAtualizacao = dataAtual.AddDays(-30),
                CriadoPor = adminUserId.ToString(),
                Ativo = true
            },
            new Relatorio
            {
                Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                Nome = "Estatísticas Gerais - Q1 2024",
                Descricao = "Relatório de estatísticas gerais do primeiro trimestre de 2024",
                Tipo = TipoRelatorio.EstatisticasGerais,
                Status = StatusRelatorio.Concluido,
                DataInicio = new DateTime(2024, 1, 1),
                DataFim = new DateTime(2024, 3, 31),
                GeradoPorId = adminUserId,
                DataGeracao = dataAtual.AddDays(-15),
                DataConclusao = dataAtual.AddDays(-15).AddMinutes(8),
                NomeArquivo = "estatisticas_q1_2024.xlsx",
                CaminhoArquivo = "/relatorios/estatisticas_q1_2024.xlsx",
                TamanhoBytes = 2048000, // 2MB
                TipoConteudo = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                TotalRegistros = 120,
                ValorTotal = 48250.75m,
                DataCriacao = dataAtual.AddDays(-15),
                DataAtualizacao = dataAtual.AddDays(-15),
                CriadoPor = adminUserId.ToString(),
                Ativo = true
            },
            new Relatorio
            {
                Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                Nome = "Relatório Financeiro - Março 2024",
                Descricao = "Relatório financeiro detalhado do mês de março",
                Tipo = TipoRelatorio.RelatorioFinanceiro,
                Status = StatusRelatorio.Processando,
                DataInicio = new DateTime(2024, 3, 1),
                DataFim = new DateTime(2024, 3, 31),
                GeradoPorId = adminUserId,
                DataGeracao = dataAtual.AddMinutes(-30),
                DataCriacao = dataAtual.AddMinutes(-30),
                DataAtualizacao = dataAtual.AddMinutes(-30),
                CriadoPor = adminUserId.ToString(),
                Ativo = true
            }
        );
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<Relatorio>()
            .Where(e => e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            entry.Entity.DataAtualizacao = DateTime.UtcNow;
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}