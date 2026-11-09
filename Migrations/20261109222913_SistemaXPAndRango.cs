using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Catalogo_Galactico_api.Migrations
{
    /// <inheritdoc />
    public partial class SistemaXPAndRango : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Rango",
                table: "Personajes",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Recluta")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "XP",
                table: "Personajes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.InsertData(
                table: "Cartas",
                columns: new[] { "Id", "Arma", "HabilidadEspecial", "ImagenUrl", "NivelPeligrosidad", "PersonajeId", "Poder" },
                values: new object[,]
                {
                    { 1, "Sable de luz verde", "Reflejos Jedi", "url_luke.jpg", 8, 1, 85 },
                    { 2, "Sable de luz rojo", "Estrangulamiento con la Fuerza", "url_vader.jpg", 10, 2, 95 },
                    { 3, "Blaster pesado", "Disparo rápido", "url_han.jpg", 6, 3, 60 }
                });

            migrationBuilder.InsertData(
                table: "Eventos",
                columns: new[] { "Id", "Descripcion", "Fecha", "Nombre", "Participantes", "ResultadoGanador", "Ubicacion" },
                values: new object[,]
                {
                    { 1, "Destrucción de la primera Estrella de la Muerte", 0, "Batalla de Yavin", "1,2,3", "Rebeldes", "Yavin 4" },
                    { 2, "Incursión temprana del Imperio", -5, "Misión en Lothal", "2", "Imperio", "Lothal" },
                    { 3, "Caída del Imperio", 4, "Batalla de Endor", "1,2,3", "Rebeldes", "Luna de Endor" }
                });

            migrationBuilder.InsertData(
                table: "Personajes",
                columns: new[] { "Id", "Afiliacion", "Especie", "Estado", "Faccion", "FuerzaSensitivo", "Nombre", "Rango" },
                values: new object[,]
                {
                    { 1, "Alianza Rebelde", "Humano", "Vivo", "Rebelde", true, "Luke Skywalker", "Recluta" },
                    { 2, "Sith", "Humano/Cyborg", "Vivo", "Imperio", true, "Darth Vader", "Recluta" },
                    { 3, "Contrabandista", "Humano", "Vivo", "Rebelde", false, "Han Solo", "Recluta" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Cartas",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Cartas",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Cartas",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Eventos",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Eventos",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Eventos",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Personajes",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Personajes",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Personajes",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DropColumn(
                name: "Rango",
                table: "Personajes");

            migrationBuilder.DropColumn(
                name: "XP",
                table: "Personajes");
        }
    }
}
