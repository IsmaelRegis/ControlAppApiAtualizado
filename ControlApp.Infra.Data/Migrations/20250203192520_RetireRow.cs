using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControlApp.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class RetireRow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ROW_VERSION",
                table: "TRAJETOS");

            migrationBuilder.DropColumn(
                name: "ROW_VERSION",
                table: "PONTOS");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "TRAJETOS",
                newName: "STATUS");

            migrationBuilder.AlterColumn<string>(
                name: "STATUS",
                table: "TRAJETOS",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Em andamento",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "STATUS",
                table: "TRAJETOS",
                newName: "Status");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "TRAJETOS",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldDefaultValue: "Em andamento");

            migrationBuilder.AddColumn<byte[]>(
                name: "ROW_VERSION",
                table: "TRAJETOS",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "ROW_VERSION",
                table: "PONTOS",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);
        }
    }
}
