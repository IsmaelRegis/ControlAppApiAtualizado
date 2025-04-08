using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControlApp.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class nomeempresa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NOME_EMPRESA",
                table: "USUARIOS",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NOME_EMPRESA",
                table: "USUARIOS");
        }
    }
}
