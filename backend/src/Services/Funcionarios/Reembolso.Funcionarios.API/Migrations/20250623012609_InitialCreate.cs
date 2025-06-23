using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Reembolso.Funcionarios.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Colaboradores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Matricula = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Telefone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Cargo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Departamento = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DataAdmissao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataDemissao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CPF = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: true),
                    RG = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    DataNascimento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Endereco = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Cidade = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Estado = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    CEP = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Salario = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CriadoPor = table.Column<string>(type: "text", nullable: true),
                    AtualizadoPor = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Colaboradores", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Colaboradores",
                columns: new[] { "Id", "Ativo", "AtualizadoPor", "CEP", "CPF", "Cargo", "Cidade", "CriadoPor", "DataAdmissao", "DataAtualizacao", "DataCriacao", "DataDemissao", "DataNascimento", "Departamento", "Email", "Endereco", "Estado", "Matricula", "Nome", "RG", "Salario", "Telefone", "UsuarioId" },
                values: new object[,]
                {
                    { new Guid("2b822389-df64-4635-85cd-714160ed3bef"), true, null, "01234-567", "123.456.789-01", "Analista de Sistemas", "São Paulo", "Sistema", new DateTime(2020, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2025, 6, 23, 1, 26, 8, 555, DateTimeKind.Utc).AddTicks(9708), null, new DateTime(1990, 5, 20, 0, 0, 0, 0, DateTimeKind.Utc), "TI", "joao.silva@empresa.com", "Rua das Flores, 123", "SP", "COL001", "João Silva", "12.345.678-9", 5000.00m, "(11) 99999-1111", null },
                    { new Guid("2d87e508-08da-4485-85c9-057ab16eafab"), true, null, "01310-100", "987.654.321-09", "Gerente Financeiro", "São Paulo", "Sistema", new DateTime(2019, 3, 10, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2025, 6, 23, 1, 26, 8, 555, DateTimeKind.Utc).AddTicks(9717), null, new DateTime(1985, 8, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Financeiro", "maria.santos@empresa.com", "Av. Paulista, 456", "SP", "COL002", "Maria Santos", "98.765.432-1", 8000.00m, "(11) 99999-2222", null },
                    { new Guid("b7a187e5-ddcb-4472-883a-c6bdfa3d6e6d"), true, null, "01305-000", "456.789.123-45", "Desenvolvedor", "São Paulo", "Sistema", new DateTime(2021, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2025, 6, 23, 1, 26, 8, 555, DateTimeKind.Utc).AddTicks(9722), null, new DateTime(1992, 12, 3, 0, 0, 0, 0, DateTimeKind.Utc), "TI", "pedro.oliveira@empresa.com", "Rua Augusta, 789", "SP", "COL003", "Pedro Oliveira", "45.678.912-3", 4500.00m, "(11) 99999-3333", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Colaboradores_Ativo",
                table: "Colaboradores",
                column: "Ativo");

            migrationBuilder.CreateIndex(
                name: "IX_Colaboradores_CPF",
                table: "Colaboradores",
                column: "CPF",
                unique: true,
                filter: "\"CPF\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Colaboradores_DataAdmissao",
                table: "Colaboradores",
                column: "DataAdmissao");

            migrationBuilder.CreateIndex(
                name: "IX_Colaboradores_Departamento",
                table: "Colaboradores",
                column: "Departamento");

            migrationBuilder.CreateIndex(
                name: "IX_Colaboradores_Email",
                table: "Colaboradores",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Colaboradores_Matricula",
                table: "Colaboradores",
                column: "Matricula",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Colaboradores");
        }
    }
}
