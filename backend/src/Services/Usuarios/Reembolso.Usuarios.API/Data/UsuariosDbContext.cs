using Microsoft.EntityFrameworkCore;
using Reembolso.Usuarios.API.Models;
using Reembolso.Shared.Enums;

namespace Reembolso.Usuarios.API.Data;

public class UsuariosDbContext : DbContext
{
    public UsuariosDbContext(DbContextOptions<UsuariosDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<HistoricoLogin> HistoricoLogins { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuração da entidade Usuario
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
            entity.Property(e => e.SenhaHash).IsRequired();
            entity.Property(e => e.TipoUsuario).HasConversion<int>();
            entity.Property(e => e.RefreshToken).HasMaxLength(500);
        });

        // Configuração da entidade HistoricoLogin
        modelBuilder.Entity<HistoricoLogin>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EnderecoIP).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.MotivoFalha).HasMaxLength(200);
            
            entity.HasOne(e => e.Usuario)
                  .WithMany(u => u.HistoricoLogins)
                  .HasForeignKey(e => e.UsuarioId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Seed de dados iniciais
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        var adminId = Guid.NewGuid();
        var analistaId = Guid.NewGuid();
        var colaboradorId = Guid.NewGuid();

        // Senha padrão: "123456" (hash BCrypt)
        var senhaHash = BCrypt.Net.BCrypt.HashPassword("123456");

        modelBuilder.Entity<Usuario>().HasData(
            new Usuario
            {
                Id = adminId,
                Nome = "Administrador do Sistema",
                Email = "admin@reembolso.com",
                SenhaHash = senhaHash,
                TipoUsuario = TipoUsuario.Administrador,
                DataCriacao = DateTime.UtcNow,
                Ativo = true,
                PrimeiroLogin = false
            },
            new Usuario
            {
                Id = analistaId,
                Nome = "Analista Financeiro",
                Email = "analista@reembolso.com",
                SenhaHash = senhaHash,
                TipoUsuario = TipoUsuario.AnalistaFinanceiro,
                DataCriacao = DateTime.UtcNow,
                Ativo = true,
                PrimeiroLogin = false
            },
            new Usuario
            {
                Id = colaboradorId,
                Nome = "Colaborador Teste",
                Email = "colaborador@reembolso.com",
                SenhaHash = senhaHash,
                TipoUsuario = TipoUsuario.Colaborador,
                DataCriacao = DateTime.UtcNow,
                Ativo = true,
                PrimeiroLogin = false
            }
        );
    }
}