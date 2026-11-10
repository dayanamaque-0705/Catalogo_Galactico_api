using Data;
using Microsoft.AspNetCore.Mvc;
using Models;
using Services;

namespace Controllers;

[ApiController]
[Route("eventos")]
[Produces("application/json")]
public sealed class EventosController(CatalogoService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<Evento>), StatusCodes.Status200OK)]
    public ActionResult<List<Evento>> ObtenerTodos() =>
        Ok(Catalogo.Eventos);

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Evento), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<Evento> ObtenerPorId(int id)
    {
        var evento = Catalogo.Eventos.FirstOrDefault(evento => evento.Id == id);
        return evento is null
            ? NotFound(new { mensaje = "Evento no encontrado" })
            : Ok(evento);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Evento), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<Evento> Crear(CreateEventoDto dto)
    {
        var errorParticipantes = ValidarParticipantesVivos(dto.Participantes);
        if (errorParticipantes is not null)
        {
            return BadRequest(new { mensaje = errorParticipantes });
        }

        var nuevoId = Catalogo.Eventos.Count > 0
            ? Catalogo.Eventos.Max(evento => evento.Id) + 1
            : 1;
        var evento = new Evento(
            nuevoId, dto.Nombre, dto.Fecha, dto.Ubicacion,
            dto.Descripcion, dto.Participantes, dto.ResultadoGanador);
        Catalogo.Eventos.Add(evento);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = nuevoId }, evento);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Actualizar(int id, CreateEventoDto dto)
    {
        var index = Catalogo.Eventos.FindIndex(evento => evento.Id == id);
        if (index < 0)
        {
            return NotFound(new { mensaje = "Evento no encontrado" });
        }

        var errorParticipantes = ValidarParticipantesVivos(dto.Participantes);
        if (errorParticipantes is not null)
        {
            return BadRequest(new { mensaje = errorParticipantes });
        }

        Catalogo.Eventos[index] = new Evento(
            id, dto.Nombre, dto.Fecha, dto.Ubicacion,
            dto.Descripcion, dto.Participantes, dto.ResultadoGanador);
        return NoContent();
    }

    [HttpGet("{id:int}/mvp")]
    [ProducesResponseType(typeof(Personaje), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<Personaje> ObtenerMvp(int id)
    {
        if (!Catalogo.Eventos.Any(evento => evento.Id == id))
        {
            return NotFound(new { mensaje = "Evento no encontrado" });
        }

        var mvp = service.ObtenerMvpDeEvento(id);
        return mvp is null
            ? NotFound(new { mensaje = "No se encontró MVP para este evento." })
            : Ok(mvp);
    }

    [HttpPost("{id:int}/simular")]
    [ProducesResponseType(typeof(ResultadoSimulacion), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<ResultadoSimulacion> Simular(int id)
    {
        var resultado = service.SimularBatalla(id);
        return resultado.Exito
            ? Ok(resultado)
            : BadRequest(new { mensaje = resultado.MensajeError });
    }

    [HttpPost("{id:int}/registrar-muerte/{personajeId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult RegistrarMuerte(int id, int personajeId)
    {
        var evento = Catalogo.Eventos.FirstOrDefault(evento => evento.Id == id);
        if (evento is null)
        {
            return NotFound(new { mensaje = "Evento no encontrado." });
        }

        if (!evento.Participantes.Contains(personajeId))
        {
            return BadRequest(new { mensaje = "El personaje no participó en este evento, no puede morir aquí." });
        }

        service.RegistrarMuertePersonaje(personajeId);
        return Ok(new
        {
            mensaje = $"El personaje con ID {personajeId} ha sido registrado como muerto en el evento {id}."
        });
    }

    private static string? ValidarParticipantesVivos(IEnumerable<int> participantes)
    {
        var personajesMuertos = Catalogo.Personajes
            .Where(personaje =>
                participantes.Contains(personaje.Id) && personaje.Estado == Estado.Muerto)
            .Select(personaje => personaje.Nombre)
            .ToList();

        return personajesMuertos.Count == 0
            ? null
            : $"Los siguientes personajes están muertos y no pueden participar: {string.Join(", ", personajesMuertos)}";
    }
}
