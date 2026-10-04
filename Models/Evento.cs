
namespace Models
{  //una lista de participantes,solo id del personaje
    public record Evento(
        int Id,
        string Nombre,
        int Fecha, 
        string Ubicacion,
        string Descripcion,
        List<int> Participantes, 
        string ResultadoGanador
    );
}