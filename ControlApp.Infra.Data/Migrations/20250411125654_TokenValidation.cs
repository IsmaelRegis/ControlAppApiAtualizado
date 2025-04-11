using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControlApp.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class TokenValidation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "USER_TOKENS",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    USER_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TOKEN = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    DEVICE_INFO = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USER_TOKENS", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_USER_TOKENS_TOKEN",
                table: "USER_TOKENS",
                column: "TOKEN");

            migrationBuilder.CreateIndex(
                name: "IX_USER_TOKENS_USER_ID",
                table: "USER_TOKENS",
                column: "USER_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "USER_TOKENS");
        }
    }
}
