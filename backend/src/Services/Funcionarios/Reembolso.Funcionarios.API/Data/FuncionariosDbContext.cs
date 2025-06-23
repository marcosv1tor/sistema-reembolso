using Microsoft.EntityFrameworkCore;
using Reembolso.Funcionarios.API.Models;

namespace Reembolso.Funcionarios.API.Data;

public class FuncionariosDbContext : DbContext
{
    public FuncionariosDbContext(DbContextOptions<FuncionariosDbContext> options) : base(options)
    {
    }

    public DbSet<Colaborador> Colaboradores { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuração da entidade Colaborador
        modelBuilder.Entity<Colaborador>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Nome)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Matricula)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Telefone)
                .HasMaxLength(20);

            entity.Property(e => e.Cargo)
                .HasMaxLength(100);

            entity.Property(e => e.Departamento)
                .HasMaxLength(100);

            entity.Property(e => e.CPF)
                .HasMaxLength(14);

            entity.Property(e => e.RG)
                .HasMaxLength(20);

            entity.Property(e => e.Endereco)
                .HasMaxLength(200);

            entity.Property(e => e.Cidade)
                .HasMaxLength(50);

            entity.Property(e => e.Estado)
                .HasMaxLength(2);

            entity.Property(e => e.CEP)
                .HasMaxLength(10);

            entity.Property(e => e.Salario)
                .HasColumnType("decimal(18,2)");

            entity.Property(e => e.DataCriacao)
                .IsRequired();

            entity.Property(e => e.Ativo)
                .IsRequired()
                .HasDefaultValue(true);

            // Índices únicos
            entity.HasIndex(e => e.Matricula)
                .IsUnique()
                .HasDatabaseName("IX_Colaboradores_Matricula");

            entity.HasIndex(e => e.Email)
                .IsUnique()
                .HasDatabaseName("IX_Colaboradores_Email");

            entity.HasIndex(e => e.CPF)
                .IsUnique()
                .HasDatabaseName("IX_Colaboradores_CPF")
                .HasFilter("[CPF] IS NOT NULL");

            // Índices para consultas
            entity.HasIndex(e => e.Departamento)
                .HasDatabaseName("IX_Colaboradores_Departamento");

            entity.HasIndex(e => e.Ativo)
                .HasDatabaseName("IX_Colaboradores_Ativo");

            entity.HasIndex(e => e.DataAdmissao)
                .HasDatabaseName("IX_Colaboradores_DataAdmissao");
        });

        // Dados iniciais (seed)
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        var colaboradores = new List<Colaborador>
        {
            new Colaborador
            {
                Id = Guid.NewGuid(),
                Nome = "João Silva",
                Matricula = "COL001",
                Email = "joao.silva@empresa.com",
                Telefone = "(11) 99999-1111",
                Cargo = "Analista de Sistemas",
                Departamento = "TI",
                DataAdmissao = new DateTime(2020, 1, 15),
                CPF = "123.456.789-01",
                RG = "12.345.678-9",
                DataNascimento = new DateTime(1990, 5, 20),
                Endereco = "Rua das Flores, 123",
                Cidade = "São Paulo",
                Estado = "SP",
                CEP = "01234-567",
                Salario = 5000.00m,
                DataCriacao = DateTime.UtcNow,
                Ativo = true,
                CriadoPor = "Sistema"
            },
            new Colaborador
            {
                Id = Guid.NewGuid(),
                Nome = "Maria Santos",
                Matricula = "COL002",
                Email = "maria.santos@empresa.com",
                Telefone = "(11) 99999-2222",
                Cargo = "Gerente Financeiro",
                Departamento = "Financeiro",
                DataAdmissao = new DateTime(2019, 3, 10),
                CPF = "987.654.321-09",
                RG = "98.765.432-1",
                DataNascimento = new DateTime(1985, 8, 15),
                Endereco = "Av. Paulista, 456",
                Cidade = "São Paulo",
                Estado = "SP",
                CEP = "01310-100",
                Salario = 8000.00m,
                DataCriacao = DateTime.UtcNow,
                Ativo = true,
                CriadoPor = "Sistema"
            },
            new Colaborador
            {
                Id = Guid.NewGuid(),
                Nome = "Pedro Oliveira",
                Matricula = "COL003",
                Email = "pedro.oliveira@empresa.com",
                Telefone = "(11) 99999-3333",
                Cargo = "Desenvolvedor",
                Departamento = "TI",
                DataAdmissao = new DateTime(2021, 6, 1),
                CPF = "456.789.123-45",
                RG = "45.678.912-3",
                DataNascimento = new DateTime(1992, 12, 3),
                Endereco = "Rua Augusta, 789",
                Cidade = "São Paulo",
                Estado = "SP",
                CEP = "01305-000",
                Salario = 4500.00m,
                DataCriacao = DateTime.UtcNow,
                Ativo = true,
                CriadoPor = "Sistema"
            }
        };

        modelBuilder.Entity<Colaborador>().HasData(colaboradores);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<Colaborador>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.DataCriacao = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.DataAtualizacao = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}