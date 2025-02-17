using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControlApp.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class ObervacoesAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OBSERVACOES",
                table: "PONTOS",
                newName: "Observacoes");

            migrationBuilder.AlterColumn<string>(
                name: "Observacoes",
                table: "PONTOS",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OBSERVACAO_FIM_EXPEDIENTE",
                table: "PONTOS",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OBSERVACAO_FIM_PAUSA",
                table: "PONTOS",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OBSERVACAO_INICIO_EXPEDIENTE",
                table: "PONTOS",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OBSERVACAO_INICIO_PAUSA",
                table: "PONTOS",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OBSERVACAO_FIM_EXPEDIENTE",
                table: "PONTOS");

            migrationBuilder.DropColumn(
                name: "OBSERVACAO_FIM_PAUSA",
                table: "PONTOS");

            migrationBuilder.DropColumn(
                name: "OBSERVACAO_INICIO_EXPEDIENTE",
                table: "PONTOS");

            migrationBuilder.DropColumn(
                name: "OBSERVACAO_INICIO_PAUSA",
                table: "PONTOS");

            migrationBuilder.RenameColumn(
                name: "Observacoes",
                table: "PONTOS",
                newName: "OBSERVACOES");

            migrationBuilder.AlterColumn<string>(
                name: "OBSERVACOES",
                table: "PONTOS",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
