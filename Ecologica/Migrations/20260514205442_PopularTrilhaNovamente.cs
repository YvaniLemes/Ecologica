using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecologica.Migrations
{
    public partial class PopularTrilhaNovamente : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // MANTENHA APENAS ISSO: Os dados que vão popular sua tela
            migrationBuilder.InsertData(
                table: "trilha_progresso",
                columns: new[] { "Id", "Descricao", "EstaBloqueado", "Progresso", "Titulo" },
                values: new object[,]
                {
                    { 1, "Aprenda o básico sobre pegada de carbono.", false, 1.0, "Introdução ao CO2" },
                    { 2, "Vamos calcular o impacto do seu transporte.", false, 0.4, "Cálculo de Emissões" },
                    { 3, "Teste seus conhecimentos finais.", true, 0.0, "Desafio Final" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Limpa os dados se a migration for revertida
            migrationBuilder.DeleteData(table: "trilha_progresso", keyColumn: "Id", keyValue: 1);
            migrationBuilder.DeleteData(table: "trilha_progresso", keyColumn: "Id", keyValue: 2);
            migrationBuilder.DeleteData(table: "trilha_progresso", keyColumn: "Id", keyValue: 3);
        }
    }
}