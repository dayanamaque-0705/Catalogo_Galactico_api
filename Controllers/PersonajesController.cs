using Filters;
using Microsoft.AspNetCore.Mvc;
using Models;
using Services;

namespace Controllers;

[ApiController]
[Route("personajes")]
[Produces("application/json")]
public sealed class PersonajesController(ICatalogoService catalogo) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PersonajeResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PersonajeResponse>>> ObtenerTodos(
        [FromQuery] Faccion? faccion,
        [FromQuery] bool? fuerzaSensitivo,
        CancellationToken ct) =>
        Ok(await catalogo.ObtenerPersonajesAsync(faccion, fuerzaSensitivo, ct));

    [HttpGet("ranking")]
    [ProducesResponseType(typeof(IReadOnlyList<CardResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CardResponse>>> ObtenerRankingPoder(CancellationToken ct) =>
        Ok(await catalogo.ObtenerRankingPoderAsync(ct));

    [HttpGet("ranking-experiencia")]
    [ProducesResponseType(typeof(IReadOnlyList<PersonajeResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PersonajeResponse>>> ObtenerRankingExperiencia(CancellationToken ct)
    {
        var personajes = await catalogo.ObtenerPersonajesAsync(null, null, ct);
        return Ok(personajes.OrderByDescending(p => p.XP).ThenBy(p => p.Id));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PersonajeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PersonajeResponse>> ObtenerPorId(int id, CancellationToken ct)
    {
        var personaje = await catalogo.ObtenerPersonajeAsync(id, ct);
        return personaje is null
            ? NotFound(new ProblemDetails { Status = StatusCodes.Status404NotFound, Title = "Personaje no encontrado." })
            : Ok(personaje);
    }

    [HttpGet("{id:int}/eventos")]
    [ProducesResponseType(typeof(IReadOnlyList<EventoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<EventoResponse>>> ObtenerEventos(int id, CancellationToken ct)
    {
        var eventos = await catalogo.ObtenerEventosPorPersonajeAsync(id, ct);
        return eventos is null
            ? NotFound(new ProblemDetails { Status = StatusCodes.Status404NotFound, Title = "Personaje no encontrado." })
            : Ok(eventos);
    }

    [HttpPost]
    [ProducesResponseType(typeof(PersonajeResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PersonajeResponse>> Crear(CreatePersonajeDto dto, CancellationToken ct)
    {
        var personaje = await catalogo.CrearPersonajeAsync(dto, ct);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = personaje.Id }, personaje);
    }

    [HttpPost("{id:int}/xp")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> OtorgarXp(
        int id,
        [FromBody] OtorgarXpRequest request,
        CancellationToken ct)
    {
        var personaje = await catalogo.OtorgarXpAsync(id, request.PuntosXP, ct);
        return personaje is null
            ? NotFound(new ProblemDetails { Status = StatusCodes.Status404NotFound, Title = "Personaje no encontrado." })
            : Ok(personaje);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Actualizar(int id, CreatePersonajeDto dto, CancellationToken ct)
    {
        var personaje = await catalogo.ActualizarPersonajeAsync(id, dto, ct);
        return personaje is null
            ? NotFound(new ProblemDetails { Status = StatusCodes.Status404NotFound, Title = "Personaje no encontrado." })
            : NoContent();
    }

    [HttpPut("{id:int}/reseteo")]
    [ServiceFilter(typeof(AdminApiKeyFilter))]
    [ProducesResponseType(typeof(PersonajeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PersonajeResponse>> ResetearExperiencia(
        int id,
        [FromBody] ResetearExperienciaRequest request,
        CancellationToken ct)
    {
        var personaje = await catalogo.ResetearExperienciaAsync(id, request.Rango, ct);
        return personaje is null
            ? NotFound(new ProblemDetails { Status = StatusCodes.Status404NotFound, Title = "Personaje no encontrado." })
            : Ok(personaje);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Eliminar(int id, CancellationToken ct)
    {
        var result = await catalogo.EliminarPersonajeAsync(id, ct);
        return result switch
        {
            EliminarResultado.NoEncontrado => NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Personaje no encontrado."
            }),
            EliminarResultado.TieneRelaciones => Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "No se puede eliminar un personaje asociado a cartas o eventos."
            }),
            _ => NoContent()
        };
    }
}
