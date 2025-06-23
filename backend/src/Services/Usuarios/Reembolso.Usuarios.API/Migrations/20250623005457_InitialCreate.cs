using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Reembolso.Usuarios.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    SenhaHash = table.Column<string>(type: "text", nullable: false),
                    TipoUsuario = table.Column<int>(type: "integer", nullable: false),
                    UltimoLogin = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PrimeiroLogin = table.Column<bool>(type: "boolean", nullable: false),
                    RefreshToken = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RefreshTokenExpiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    CriadoPor = table.Column<string>(type: "text", nullable: true),
                    AtualizadoPor = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HistoricoLogins",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    DataLogin = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EnderecoIP = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LoginSucesso = table.Column<bool>(type: "boolean", nullable: false),
                    MotivoFalha = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    CriadoPor = table.Column<string>(type: "text", nullable: true),
                    AtualizadoPor = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricoLogins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistoricoLogins_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "Ativo", "AtualizadoPor", "CriadoPor", "DataAtualizacao", "DataCriacao", "Email", "Nome", "PrimeiroLogin", "RefreshToken", "RefreshTokenExpiry", "SenhaHash", "TipoUsuario", "UltimoLogin" },
                values: new object[,]
                {
                    { new Guid("0147325e-3def-489a-a235-f0f597768758"), true, null, null, null, new DateTime(2025, 6, 23, 0, 54, 56, 447, DateTimeKind.Utc).AddTicks(9846), "admin@reembolso.com", "Administrador do Sistema", false, null, null, "$2a$11$W0ci924tI9LXiXQiLPrT6us21fBoAR81dgFhFNSWFHUfpdqyibD1y", 1, null },
                    { new Guid("9110ecc2-65f5-499a-a828-e523cf74681b"), true, null, null, null, new DateTime(2025, 6, 23, 0, 54, 56, 447, DateTimeKind.Utc).AddTicks(9863), "analista@reembolso.com", "Analista Financeiro", false, null, null, "$2a$11$W0ci924tI9LXiXQiLPrT6us21fBoAR81dgFhFNSWFHUfpdqyibD1y", 2, null },
                    { new Guid("91d0a3d7-4c69-4c1e-887c-7908fbf9187b"), true, null, null, null, new DateTime(2025, 6, 23, 0, 54, 56, 447, DateTimeKind.Utc).AddTicks(9953), "colaborador@reembolso.com", "Colaborador Teste", false, null, null, "$2a$11$W0ci924tI9LXiXQiLPrT6us21fBoAR81dgFhFNSWFHUfpdqyibD1y", 3, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_HistoricoLogins_UsuarioId",
                table: "HistoricoLogins",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HistoricoLogins");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
