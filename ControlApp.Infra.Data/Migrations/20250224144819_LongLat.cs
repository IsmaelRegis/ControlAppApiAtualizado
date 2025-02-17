using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControlApp.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class LongLat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "LONGITUDE_INICIO_EXPEDIENTE",
                table: "PONTOS",
                type: "VARCHAR(50)",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "FLOAT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LONGITUDE_FIM_EXPEDIENTE",
                table: "PONTOS",
                type: "VARCHAR(50)",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "FLOAT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LATITUDE_INICIO_EXPEDIENTE",
                table: "PONTOS",
                type: "VARCHAR(50)",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "FLOAT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LATITUDE_FIM_EXPEDIENTE",
                table: "PONTOS",
                type: "VARCHAR(50)",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "FLOAT",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "LONGITUDE_INICIO_EXPEDIENTE",
                table: "PONTOS",
                type: "FLOAT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VARCHAR(50)",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "LONGITUDE_FIM_EXPEDIENTE",
                table: "PONTOS",
                type: "FLOAT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VARCHAR(50)",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "LATITUDE_INICIO_EXPEDIENTE",
                table: "PONTOS",
                type: "FLOAT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VARCHAR(50)",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "LATITUDE_FIM_EXPEDIENTE",
                table: "PONTOS",
                type: "FLOAT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VARCHAR(50)",
                oldNullable: true);
        }
    }
}
