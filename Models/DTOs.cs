
namespace Models
{
    // para la creación de personajes, cartas y eventos(sin Id)
        public record CreatePersonajeDto(
        string Nombre,
        string Especie,
        Faccion Faccion,
        string Afiliacion,
        Estado Estado,
        bool FuerzaSensitivo
    );

    public record CreateCardDto(
        int PersonajeId,
        int Poder,
        string HabilidadEspecial,
        string Arma,
        int NivelPeligrosidad,
        string ImagenUrl
    );
    public record CreateEventoDto(
        string Nombre,
        string Descripcion,
        string Fecha,
        string Ubicacion
    );
}