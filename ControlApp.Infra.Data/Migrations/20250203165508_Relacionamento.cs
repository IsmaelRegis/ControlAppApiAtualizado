using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControlApp.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class Relacionamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LOCALIZACOES_PONTOS_PontoId",
                table: "LOCALIZACOES");

            migrationBuilder.DropIndex(
                name: "IX_LOCALIZACOES_PontoId",
                table: "LOCALIZACOES");

            migrationBuilder.DropColumn(
                name: "PontoId",
                table: "LOCALIZACOES");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PontoId",
                table: "LOCALIZACOES",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_LOCALIZACOES_PontoId",
                table: "LOCALIZACOES",
                column: "PontoId");

            migrationBuilder.AddForeignKey(
                name: "FK_LOCALIZACOES_PONTOS_PontoId",
                table: "LOCALIZACOES",
                column: "PontoId",
                principalTable: "PONTOS",
                principalColumn: "Id");
        }
    }
}
