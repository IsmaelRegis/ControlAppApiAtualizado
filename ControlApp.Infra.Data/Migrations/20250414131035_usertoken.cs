using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControlApp.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class usertoken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EXPIRES_AT",
                table: "USER_TOKENS",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_USER_TOKENS_EXPIRES_AT",
                table: "USER_TOKENS",
                column: "EXPIRES_AT");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_USER_TOKENS_EXPIRES_AT",
                table: "USER_TOKENS");

            migrationBuilder.DropColumn(
                name: "EXPIRES_AT",
                table: "USER_TOKENS");
        }
    }
}
