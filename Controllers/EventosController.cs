using Microsoft.AspNetCore.Mvc;
using Models;
using Services;

namespace Controllers;

[ApiController]
[Route("eventos")]
[Produces("application/json")]
public sealed class EventosController(ICatalogoService catalogo) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<EventoResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<EventoResponse>>> ObtenerTodos(CancellationToken ct) =>
        Ok(await catalogo.ObtenerEventosAsync(ct));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(EventoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventoResponse>> ObtenerPorId(int id, CancellationToken ct)
    {
        var evento = await catalogo.ObtenerEventoAsync(id, ct);
        return evento is null
            ? NotFound(new ProblemDetails { Status = StatusCodes.Status404NotFound, Title = "Evento no encontrado." })
            : Ok(evento);
    }

    [HttpGet("{id:int}/mvp")]
    [ProducesResponseType(typeof(PersonajeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PersonajeResponse>> ObtenerMvp(int id, CancellationToken ct)
    {
        var mvp = await catalogo.ObtenerMvpAsync(id, ct);
        return mvp is null
            ? NotFound(new ProblemDetails { Status = StatusCodes.Status404NotFound, Title = "No se encontró MVP para el evento." })
            : Ok(mvp);
    }

    [HttpPost]
    [ProducesResponseType(typeof(EventoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EventoResponse>> Crear(CreateEventoDto dto, CancellationToken ct)
    {
        var result = await catalogo.CrearEventoAsync(dto, ct);
        if (result.Error is not null)
            return BadRequest(new ProblemDetails { Status = StatusCodes.Status400BadRequest, Title = result.Error });

        return CreatedAtAction(nameof(ObtenerPorId), new { id = result.Evento!.Id }, result.Evento);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Actualizar(int id, CreateEventoDto dto, CancellationToken ct)
    {
        var result = await catalogo.ActualizarEventoAsync(id, dto, ct);
        if (result.NoEncontrado)
            return NotFound(new ProblemDetails { Status = StatusCodes.Status404NotFound, Title = "Evento no encontrado." });
        if (result.Error is not null)
            return BadRequest(new ProblemDetails { Status = StatusCodes.Status400BadRequest, Title = result.Error });
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(int id, CancellationToken ct) =>
        await catalogo.EliminarEventoAsync(id, ct)
            ? NoContent()
            : NotFound(new ProblemDetails { Status = StatusCodes.Status404NotFound, Title = "Evento no encontrado." });

    [HttpPost("{id:int}/simular")]
    [ProducesResponseType(typeof(ResultadoSimulacion), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ResultadoSimulacion>> Simular(int id, CancellationToken ct)
    {
        var resultado = await catalogo.SimularBatallaAsync(id, ct);
        return resultado is null
            ? BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "El evento no existe o no tiene participantes suficientes con cartas."
            })
            : Ok(resultado);
    }

    [HttpPost("{id:int}/registrar-muerte/{personajeId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RegistrarMuerte(int id, int personajeId, CancellationToken ct)
    {
        var result = await catalogo.RegistrarMuerteAsync(id, personajeId, ct);
        return result switch
        {
            null => NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Evento no encontrado."
            }),
            false => BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "El personaje no participó en este evento o no existe."
            }),
            _ => Ok(new { mensaje = $"El personaje {personajeId} fue marcado como muerto." })
        };
    }
}
