using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecologica.Migrations
{
    /// <inheritdoc />
    public partial class AjustesModelagem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "QuantidadeArvores",
                table: "Usuarios",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "trilha_progresso",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Descricao", "Progresso" },
                values: new object[] { "Aprenda o básico", 0.0 });

            migrationBuilder.UpdateData(
                table: "trilha_progresso",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Descricao", "EstaBloqueado", "Progresso" },
                values: new object[] { "Hora de praticar", true, 0.0 });

            migrationBuilder.UpdateData(
                table: "trilha_progresso",
                keyColumn: "Id",
                keyValue: 3,
                column: "Descricao",
                value: "Mestre da Ecologia");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QuantidadeArvores",
                table: "Usuarios");

            migrationBuilder.UpdateData(
                table: "trilha_progresso",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Descricao", "Progresso" },
                values: new object[] { null, 1.0 });

            migrationBuilder.UpdateData(
                table: "trilha_progresso",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Descricao", "EstaBloqueado", "Progresso" },
                values: new object[] { null, false, 0.40000000000000002 });

            migrationBuilder.UpdateData(
                table: "trilha_progresso",
                keyColumn: "Id",
                keyValue: 3,
                column: "Descricao",
                value: null);
        }
    }
}
