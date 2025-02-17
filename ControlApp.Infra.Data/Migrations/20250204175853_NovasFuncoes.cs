using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControlApp.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class NovasFuncoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LONGITUDE",
                table: "PONTOS",
                newName: "LONGITUDE_RETORNO_PAUSA");

            migrationBuilder.RenameColumn(
                name: "LATITUDE",
                table: "PONTOS",
                newName: "LONGITUDE_INICIO_PAUSA");

            migrationBuilder.AddColumn<double>(
                name: "LATITUDE_FIM_EXPEDIENTE",
                table: "PONTOS",
                type: "FLOAT",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "LATITUDE_INICIO_EXPEDIENTE",
                table: "PONTOS",
                type: "FLOAT",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "LATITUDE_INICIO_PAUSA",
                table: "PONTOS",
                type: "FLOAT",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "LATITUDE_RETORNO_PAUSA",
                table: "PONTOS",
                type: "FLOAT",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "LONGITUDE_FIM_EXPEDIENTE",
                table: "PONTOS",
                type: "FLOAT",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "LONGITUDE_INICIO_EXPEDIENTE",
                table: "PONTOS",
                type: "FLOAT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LATITUDE_FIM_EXPEDIENTE",
                table: "PONTOS");

            migrationBuilder.DropColumn(
                name: "LATITUDE_INICIO_EXPEDIENTE",
                table: "PONTOS");

            migrationBuilder.DropColumn(
                name: "LATITUDE_INICIO_PAUSA",
                table: "PONTOS");

            migrationBuilder.DropColumn(
                name: "LATITUDE_RETORNO_PAUSA",
                table: "PONTOS");

            migrationBuilder.DropColumn(
                name: "LONGITUDE_FIM_EXPEDIENTE",
                table: "PONTOS");

            migrationBuilder.DropColumn(
                name: "LONGITUDE_INICIO_EXPEDIENTE",
                table: "PONTOS");

            migrationBuilder.RenameColumn(
                name: "LONGITUDE_RETORNO_PAUSA",
                table: "PONTOS",
                newName: "LONGITUDE");

            migrationBuilder.RenameColumn(
                name: "LONGITUDE_INICIO_PAUSA",
                table: "PONTOS",
                newName: "LATITUDE");
        }
    }
}
