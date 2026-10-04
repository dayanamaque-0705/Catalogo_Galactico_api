namespace Models
{   //información de los personajes
    public record Personaje(
        int Id,
        string Nombre,
        string Especie,
        Faccion Faccion,
        string Afiliacion,
        Estado Estado,
        bool FuerzaSensitivo
    );
}