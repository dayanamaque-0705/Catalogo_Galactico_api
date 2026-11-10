using System.ComponentModel.DataAnnotations;
using Data;
using Microsoft.AspNetCore.Mvc;
using Models;
using Services;

namespace Controllers;

[ApiController]
[Route("personajes")]
[Produces("application/json")]
public sealed class PersonajesController(CatalogoService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<Personaje>), StatusCodes.Status200OK)]
    public ActionResult<List<Personaje>> ObtenerTodos(
        [FromQuery] string? faccion,
        [FromQuery] bool? fuerzaSensitivo)
    {
        var query = Catalogo.Personajes.AsQueryable();
        if (!string.IsNullOrEmpty(faccion) &&
            Enum.TryParse<Faccion>(faccion, true, out var faccionEnum))
        {
            query = query.Where(personaje => personaje.Faccion == faccionEnum);
        }

        if (fuerzaSensitivo.HasValue)
        {
            query = query.Where(personaje => personaje.FuerzaSensitivo == fuerzaSensitivo.Value);
        }

        return Ok(query.ToList());
    }

    [HttpGet("ranking")]
    [ProducesResponseType(typeof(List<CardPersonaje>), StatusCodes.Status200OK)]
    public ActionResult<List<CardPersonaje>> ObtenerRankingPorPoder() =>
        Ok(service.ObtenerRankingPorPoder());

    [HttpGet("ranking-experiencia")]
    [ProducesResponseType(typeof(List<Personaje>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<Personaje>>> ObtenerRankingExperiencia(CancellationToken cancellationToken) =>
        Ok(await service.ObtenerRankingExperienciaAsync(cancellationToken));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Personaje), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<Personaje> ObtenerPorId(int id)
    {
        var personaje = Catalogo.Personajes.FirstOrDefault(personaje => personaje.Id == id);
        return personaje is null
            ? NotFound(new { mensaje = "Personaje no encontrado" })
            : Ok(personaje);
    }

    [HttpGet("{id:int}/eventos")]
    [ProducesResponseType(typeof(List<Evento>), StatusCodes.Status200OK)]
    public ActionResult<List<Evento>> ObtenerEventos(int id) =>
        Ok(service.ObtenerEventosPorPersonaje(id));

    [HttpPost]
    [ProducesResponseType(typeof(Personaje), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<Personaje> Crear(CreatePersonajeDto dto)
    {
        var nuevoId = Catalogo.Personajes.Count > 0
            ? Catalogo.Personajes.Max(personaje => personaje.Id) + 1
            : 1;
        var personaje = new Personaje(
            nuevoId, dto.Nombre, dto.Especie, dto.Faccion, dto.Afiliacion, dto.Estado, dto.FuerzaSensitivo);
        Catalogo.Personajes.Add(personaje);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = nuevoId }, personaje);
    }

    [HttpPost("{id:int}/xp")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> OtorgarXp(
        int id,
        [FromBody, Range(1, int.MaxValue)] int puntosXP,
        CancellationToken cancellationToken)
    {
        var personaje = await service.OtorgarXPAsync(id, puntosXP, cancellationToken);
        if (personaje is null)
        {
            return NotFound(new { mensaje = "Personaje no encontrado" });
        }

        return Ok(new
        {
            mensaje = $"XP otorgada a {personaje.Nombre}",
            xpTotal = personaje.XP,
            rangoActual = personaje.Rango
        });
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public ActionResult Actualizar(int id, CreatePersonajeDto dto)
    {
        var index = Catalogo.Personajes.FindIndex(personaje => personaje.Id == id);
        if (index < 0)
        {
            return NotFound(new { mensaje = "Personaje no encontrado" });
        }

        Catalogo.Personajes[index] = new Personaje(
            id, dto.Nombre, dto.Especie, dto.Faccion, dto.Afiliacion, dto.Estado, dto.FuerzaSensitivo);
        return NoContent();
    }

    [HttpPut("{id:int}/reseteo")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResetearExperiencia(
        int id,
        [FromQuery] string? rango = "Recluta",
        CancellationToken cancellationToken = default)
    {
        var personaje = await service.ResetearExperienciaAsync(id, rango, cancellationToken);
        if (personaje is null)
        {
            return NotFound(new { mensaje = "Personaje no encontrado" });
        }

        return Ok(new
        {
            mensaje = $"Experiencia y rango reseteados para {personaje.Nombre}",
            xp = personaje.XP,
            rango = personaje.Rango
        });
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult Eliminar(int id)
    {
        var personaje = Catalogo.Personajes.FirstOrDefault(personaje => personaje.Id == id);
        if (personaje is null)
        {
            return NotFound(new { mensaje = "Personaje no encontrado" });
        }

        if (Catalogo.Cartas.Any(carta => carta.PersonajeId == id) ||
            Catalogo.Eventos.Any(evento => evento.Participantes.Contains(id)))
        {
            return Conflict(new { mensaje = "No se puede eliminar un personaje asociado a cartas o eventos." });
        }

        Catalogo.Personajes.Remove(personaje);
        return NoContent();
    }
}
