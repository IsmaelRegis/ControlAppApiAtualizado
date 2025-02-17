using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControlApp.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "USUARIOS",
                columns: table => new
                {
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NOME = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EMAIL = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    SENHA = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ROLE = table.Column<int>(type: "int", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ATIVO = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    TipoUsuario = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: false),
                    Cpf = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HoraEntrada = table.Column<TimeSpan>(type: "time", nullable: true),
                    HoraSaida = table.Column<TimeSpan>(type: "time", nullable: true),
                    HoraAlmocoInicio = table.Column<TimeSpan>(type: "time", nullable: true),
                    HoraAlmocoFim = table.Column<TimeSpan>(type: "time", nullable: true),
                    FotoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsOnline = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USUARIOS", x => x.UsuarioId);
                });

            migrationBuilder.CreateTable(
                name: "PONTOS",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    INICIO_EXPEDIENTE = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FIM_EXPEDIENTE = table.Column<DateTime>(type: "datetime2", nullable: true),
                    INICIO_PAUSA = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RETORNO_PAUSA = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HORAS_TRABALHADAS = table.Column<TimeSpan>(type: "TIME", nullable: false),
                    HORAS_EXTRAS = table.Column<TimeSpan>(type: "TIME", nullable: false),
                    HORAS_DEVIDAS = table.Column<TimeSpan>(type: "TIME", nullable: false),
                    LATITUDE = table.Column<double>(type: "FLOAT", nullable: true),
                    LONGITUDE = table.Column<double>(type: "FLOAT", nullable: true),
                    OBSERVACOES = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TIPO_PONTO = table.Column<int>(type: "int", nullable: false),
                    USUARIO_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ATIVO = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    FOTO_INICIO_EXPEDIENTE = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FOTO_FIM_EXPEDIENTE = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PONTOS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PONTOS_USUARIOS_USUARIO_ID",
                        column: x => x.USUARIO_ID,
                        principalTable: "USUARIOS",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TRAJETOS",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DATA = table.Column<DateTime>(type: "datetime2", nullable: false),
                    USUARIO_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DISTANCIA_TOTAL_KM = table.Column<decimal>(type: "DECIMAL(10,2)", nullable: false),
                    DURACAO_TOTAL = table.Column<TimeSpan>(type: "TIME", nullable: false, defaultValueSql: "CAST('00:00:00' AS TIME)"),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TRAJETOS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TRAJETOS_USUARIOS_USUARIO_ID",
                        column: x => x.USUARIO_ID,
                        principalTable: "USUARIOS",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LOCALIZACOES",
                columns: table => new
                {
                    LocalizacaoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LATITUDE = table.Column<double>(type: "FLOAT", nullable: true),
                    LONGITUDE = table.Column<double>(type: "FLOAT", nullable: true),
                    DATA_HORA = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PRECISAO = table.Column<double>(type: "float", nullable: false),
                    PontoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrajetoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LOCALIZACOES", x => x.LocalizacaoId);
                    table.ForeignKey(
                        name: "FK_LOCALIZACOES_PONTOS_PontoId",
                        column: x => x.PontoId,
                        principalTable: "PONTOS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LOCALIZACOES_TRAJETOS_TrajetoId",
                        column: x => x.TrajetoId,
                        principalTable: "TRAJETOS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LOCALIZACOES_PontoId",
                table: "LOCALIZACOES",
                column: "PontoId");

            migrationBuilder.CreateIndex(
                name: "IX_LOCALIZACOES_TrajetoId",
                table: "LOCALIZACOES",
                column: "TrajetoId");

            migrationBuilder.CreateIndex(
                name: "IX_PONTOS_USUARIO_ID",
                table: "PONTOS",
                column: "USUARIO_ID");

            migrationBuilder.CreateIndex(
                name: "IX_TRAJETOS_USUARIO_ID",
                table: "TRAJETOS",
                column: "USUARIO_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LOCALIZACOES");

            migrationBuilder.DropTable(
                name: "PONTOS");

            migrationBuilder.DropTable(
                name: "TRAJETOS");

            migrationBuilder.DropTable(
                name: "USUARIOS");
        }
    }
}
