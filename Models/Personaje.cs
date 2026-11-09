namespace Models;

public record Personaje(
    int Id,
    string Nombre,
    string Especie,
    Faccion Faccion,
    string Afiliacion,
    Estado Estado,
    bool FuerzaSensitivo
)
{
    // Propiedades adicionales para el juego
    public int XP { get; init; } = 0;
    public string Rango { get; init; } = "Recluta";// Nivel de rango inicial
}