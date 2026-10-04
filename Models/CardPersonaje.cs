namespace Models
{   //atributos de los personajes y sus cartas
    public record CardPersonaje(
        int Id,
        int PersonajeId,
        int Poder,
        string HabilidadEspecial,
        string Arma,
        int NivelPeligrosidad,
        string ImagenUrl
    );
}