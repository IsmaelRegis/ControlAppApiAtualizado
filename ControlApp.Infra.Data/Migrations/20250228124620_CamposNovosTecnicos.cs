using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControlApp.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class CamposNovosTecnicos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "EmpresaId",
                table: "USUARIOS",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NumeroMatricula",
                table: "USUARIOS",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "USUARIOS");

            migrationBuilder.DropColumn(
                name: "NumeroMatricula",
                table: "USUARIOS");
        }
    }
}
