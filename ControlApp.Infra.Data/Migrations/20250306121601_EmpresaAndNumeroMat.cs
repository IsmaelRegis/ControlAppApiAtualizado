using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControlApp.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class EmpresaAndNumeroMat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserName",
                table: "USUARIOS",
                newName: "USERNAME");

            migrationBuilder.RenameColumn(
                name: "Cpf",
                table: "USUARIOS",
                newName: "CPF");

            migrationBuilder.RenameColumn(
                name: "LongitutdeAtual",
                table: "USUARIOS",
                newName: "LONGITUDE_ATUAL");

            migrationBuilder.RenameColumn(
                name: "LatitudeAtual",
                table: "USUARIOS",
                newName: "LATITUDE_ATUAL");

            migrationBuilder.RenameColumn(
                name: "IsOnline",
                table: "USUARIOS",
                newName: "IS_ONLINE");

            migrationBuilder.RenameColumn(
                name: "HoraSaida",
                table: "USUARIOS",
                newName: "HORA_SAIDA");

            migrationBuilder.RenameColumn(
                name: "HoraEntrada",
                table: "USUARIOS",
                newName: "HORA_ENTRADA");

            migrationBuilder.RenameColumn(
                name: "HoraAlmocoInicio",
                table: "USUARIOS",
                newName: "HORA_ALMOCO_INICIO");

            migrationBuilder.RenameColumn(
                name: "HoraAlmocoFim",
                table: "USUARIOS",
                newName: "HORA_ALMOCO_FIM");

            migrationBuilder.RenameColumn(
                name: "FOTOURL",
                table: "USUARIOS",
                newName: "FOTO_URL");

            migrationBuilder.RenameColumn(
                name: "DATAHORAULTIMAAUTENTICACAO",
                table: "USUARIOS",
                newName: "DATA_HORA_ULTIMA_AUTENTICACAO");

            migrationBuilder.AlterColumn<string>(
                name: "USERNAME",
                table: "USUARIOS",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CPF",
                table: "USUARIOS",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LONGITUDE_ATUAL",
                table: "USUARIOS",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LATITUDE_ATUAL",
                table: "USUARIOS",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IS_ONLINE",
                table: "USUARIOS",
                type: "bit",
                nullable: true,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true);

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "HORA_SAIDA",
                table: "USUARIOS",
                type: "TIME",
                nullable: true,
                defaultValueSql: "CAST('00:00:00' AS TIME)",
                oldClrType: typeof(TimeSpan),
                oldType: "time",
                oldNullable: true);

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "HORA_ENTRADA",
                table: "USUARIOS",
                type: "TIME",
                nullable: true,
                defaultValueSql: "CAST('00:00:00' AS TIME)",
                oldClrType: typeof(TimeSpan),
                oldType: "time",
                oldNullable: true);

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "HORA_ALMOCO_INICIO",
                table: "USUARIOS",
                type: "TIME",
                nullable: true,
                defaultValueSql: "CAST('00:00:00' AS TIME)",
                oldClrType: typeof(TimeSpan),
                oldType: "time",
                oldNullable: true);

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "HORA_ALMOCO_FIM",
                table: "USUARIOS",
                type: "TIME",
                nullable: true,
                defaultValueSql: "CAST('00:00:00' AS TIME)",
                oldClrType: typeof(TimeSpan),
                oldType: "time",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EMPRESA_ID",
                table: "USUARIOS",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NUMERO_MATRICULA",
                table: "USUARIOS",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EMPRESAS",
                columns: table => new
                {
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ATIVO = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    NOME_EMPRESA = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Endereco_EnderecoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CEP = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    LOGRADOURO = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    BAIRRO = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CIDADE = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ESTADO = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    COMPLEMENTO = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    NUMERO = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EMPRESAS", x => x.EmpresaId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_USUARIOS_CPF",
                table: "USUARIOS",
                column: "CPF",
                unique: true,
                filter: "[CPF] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_USUARIOS_EMPRESA_ID",
                table: "USUARIOS",
                column: "EMPRESA_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_USUARIOS_EMPRESAS_EMPRESA_ID",
                table: "USUARIOS",
                column: "EMPRESA_ID",
                principalTable: "EMPRESAS",
                principalColumn: "EmpresaId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_USUARIOS_EMPRESAS_EMPRESA_ID",
                table: "USUARIOS");

            migrationBuilder.DropTable(
                name: "EMPRESAS");

            migrationBuilder.DropIndex(
                name: "IX_USUARIOS_CPF",
                table: "USUARIOS");

            migrationBuilder.DropIndex(
                name: "IX_USUARIOS_EMPRESA_ID",
                table: "USUARIOS");

            migrationBuilder.DropColumn(
                name: "EMPRESA_ID",
                table: "USUARIOS");

            migrationBuilder.DropColumn(
                name: "NUMERO_MATRICULA",
                table: "USUARIOS");

            migrationBuilder.RenameColumn(
                name: "USERNAME",
                table: "USUARIOS",
                newName: "UserName");

            migrationBuilder.RenameColumn(
                name: "CPF",
                table: "USUARIOS",
                newName: "Cpf");

            migrationBuilder.RenameColumn(
                name: "LONGITUDE_ATUAL",
                table: "USUARIOS",
                newName: "LongitutdeAtual");

            migrationBuilder.RenameColumn(
                name: "LATITUDE_ATUAL",
                table: "USUARIOS",
                newName: "LatitudeAtual");

            migrationBuilder.RenameColumn(
                name: "IS_ONLINE",
                table: "USUARIOS",
                newName: "IsOnline");

            migrationBuilder.RenameColumn(
                name: "HORA_SAIDA",
                table: "USUARIOS",
                newName: "HoraSaida");

            migrationBuilder.RenameColumn(
                name: "HORA_ENTRADA",
                table: "USUARIOS",
                newName: "HoraEntrada");

            migrationBuilder.RenameColumn(
                name: "HORA_ALMOCO_INICIO",
                table: "USUARIOS",
                newName: "HoraAlmocoInicio");

            migrationBuilder.RenameColumn(
                name: "HORA_ALMOCO_FIM",
                table: "USUARIOS",
                newName: "HoraAlmocoFim");

            migrationBuilder.RenameColumn(
                name: "FOTO_URL",
                table: "USUARIOS",
                newName: "FOTOURL");

            migrationBuilder.RenameColumn(
                name: "DATA_HORA_ULTIMA_AUTENTICACAO",
                table: "USUARIOS",
                newName: "DATAHORAULTIMAAUTENTICACAO");

            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                table: "USUARIOS",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Cpf",
                table: "USUARIOS",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LongitutdeAtual",
                table: "USUARIOS",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LatitudeAtual",
                table: "USUARIOS",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsOnline",
                table: "USUARIOS",
                type: "bit",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true,
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "HoraSaida",
                table: "USUARIOS",
                type: "time",
                nullable: true,
                oldClrType: typeof(TimeSpan),
                oldType: "TIME",
                oldNullable: true,
                oldDefaultValueSql: "CAST('00:00:00' AS TIME)");

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "HoraEntrada",
                table: "USUARIOS",
                type: "time",
                nullable: true,
                oldClrType: typeof(TimeSpan),
                oldType: "TIME",
                oldNullable: true,
                oldDefaultValueSql: "CAST('00:00:00' AS TIME)");

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "HoraAlmocoInicio",
                table: "USUARIOS",
                type: "time",
                nullable: true,
                oldClrType: typeof(TimeSpan),
                oldType: "TIME",
                oldNullable: true,
                oldDefaultValueSql: "CAST('00:00:00' AS TIME)");

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "HoraAlmocoFim",
                table: "USUARIOS",
                type: "time",
                nullable: true,
                oldClrType: typeof(TimeSpan),
                oldType: "TIME",
                oldNullable: true,
                oldDefaultValueSql: "CAST('00:00:00' AS TIME)");
        }
    }
}
