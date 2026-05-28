using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Ecologica.Migrations
{
    /// <inheritdoc />
    public partial class InicialLimpo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "atividades",
                columns: table => new
                {
                    id_atividade = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    nome = table.Column<string>(type: "TEXT", nullable: true),
                    fator_emissao = table.Column<double>(type: "REAL", nullable: true),
                    unidade_medida = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_atividades", x => x.id_atividade);
                });

            migrationBuilder.CreateTable(
                name: "trilha_progresso",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Titulo = table.Column<string>(type: "TEXT", nullable: true),
                    Descricao = table.Column<string>(type: "TEXT", nullable: true),
                    Progresso = table.Column<double>(type: "REAL", nullable: true),
                    EstaBloqueado = table.Column<bool>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trilha_progresso", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", nullable: true),
                    Email = table.Column<string>(type: "TEXT", nullable: true),
                    Senha = table.Column<string>(type: "TEXT", nullable: true),
                    Pontos = table.Column<int>(type: "INTEGER", nullable: true),
                    QuantidadeArvores = table.Column<int>(type: "INTEGER", nullable: true),
                    PosicaoNoMapa = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "conquistas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    nome = table.Column<string>(type: "TEXT", nullable: true),
                    descricao = table.Column<string>(type: "TEXT", nullable: true),
                    data_aquisicao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    usuario_id = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_conquistas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_conquistas_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "registros_carbono",
                columns: table => new
                {
                    id_registro = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    id_usuario = table.Column<int>(type: "INTEGER", nullable: true),
                    id_atividade = table.Column<int>(type: "INTEGER", nullable: true),
                    quantidade = table.Column<double>(type: "REAL", nullable: true),
                    emissao_total = table.Column<double>(type: "REAL", nullable: true),
                    data_registro = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_registros_carbono", x => x.id_registro);
                    table.ForeignKey(
                        name: "FK_registros_carbono_atividades_id_atividade",
                        column: x => x.id_atividade,
                        principalTable: "atividades",
                        principalColumn: "id_atividade");
                    table.ForeignKey(
                        name: "FK_registros_carbono_usuarios_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "trilha_progresso",
                columns: new[] { "Id", "Descricao", "EstaBloqueado", "Progresso", "Titulo" },
                values: new object[,]
                {
                    { 1, "Aprenda o básico", false, 0.0, "Introdução ao CO2" },
                    { 2, "Hora de praticar", true, 0.0, "Cálculo de Emissões" },
                    { 3, "Mestre da Ecologia", true, 0.0, "Desafio Final" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_conquistas_usuario_id",
                table: "conquistas",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_registros_carbono_id_atividade",
                table: "registros_carbono",
                column: "id_atividade");

            migrationBuilder.CreateIndex(
                name: "IX_registros_carbono_id_usuario",
                table: "registros_carbono",
                column: "id_usuario");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "conquistas");

            migrationBuilder.DropTable(
                name: "registros_carbono");

            migrationBuilder.DropTable(
                name: "trilha_progresso");

            migrationBuilder.DropTable(
                name: "atividades");

            migrationBuilder.DropTable(
                name: "usuarios");
        }
    }
}
