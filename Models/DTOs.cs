using System.Collections.Generic;

namespace Models
{
    // para la creación de personajes, cartas y eventos(sin Id)
    public record CreatePersonajeDto(
        string Nombre,
       string url,
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
        int Fecha, 
        string Ubicacion,
        string Descripcion,
        List<int> Participantes,
        string ResultadoGanador
    );
}