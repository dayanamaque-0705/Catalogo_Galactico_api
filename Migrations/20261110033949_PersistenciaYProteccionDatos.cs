using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalogo_Galactico_api.Migrations
{
    /// <inheritdoc />
    public partial class PersistenciaYProteccionDatos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdentidadSecretaCifrada",
                table: "Personajes",
                type: "varchar(1200)",
                maxLength: 1200,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Correo = table.Column<string>(type: "varchar(254)", maxLength: 254, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PasswordHash = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreadoEn = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Personajes",
                keyColumn: "Id",
                keyValue: 1,
                column: "IdentidadSecretaCifrada",
                value: null);

            migrationBuilder.UpdateData(
                table: "Personajes",
                keyColumn: "Id",
                keyValue: 2,
                column: "IdentidadSecretaCifrada",
                value: null);

            migrationBuilder.UpdateData(
                table: "Personajes",
                keyColumn: "Id",
                keyValue: 3,
                column: "IdentidadSecretaCifrada",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_Cartas_PersonajeId",
                table: "Cartas",
                column: "PersonajeId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Correo",
                table: "Usuarios",
                column: "Correo",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Cartas_Personajes_PersonajeId",
                table: "Cartas",
                column: "PersonajeId",
                principalTable: "Personajes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cartas_Personajes_PersonajeId",
                table: "Cartas");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Cartas_PersonajeId",
                table: "Cartas");

            migrationBuilder.DropColumn(
                name: "IdentidadSecretaCifrada",
                table: "Personajes");
        }
    }
}
