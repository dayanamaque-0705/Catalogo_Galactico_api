using Data;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace Controllers;

[ApiController]
[Route("cartas")]
[Produces("application/json")]
public sealed class CartasController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<CardPersonaje>), StatusCodes.Status200OK)]
    public ActionResult<List<CardPersonaje>> ObtenerTodas() =>
        Ok(Catalogo.Cartas);

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CardPersonaje), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<CardPersonaje> ObtenerPorId(int id)
    {
        var carta = Catalogo.Cartas.FirstOrDefault(carta => carta.Id == id);
        return carta is null
            ? NotFound(new { mensaje = "Carta no encontrada" })
            : Ok(carta);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CardPersonaje), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<CardPersonaje> Crear(CreateCardDto dto)
    {
        var nuevoId = Catalogo.Cartas.Count > 0
            ? Catalogo.Cartas.Max(carta => carta.Id) + 1
            : 1;
        var carta = new CardPersonaje(
            nuevoId, dto.PersonajeId, dto.Poder, dto.HabilidadEspecial,
            dto.Arma, dto.NivelPeligrosidad, dto.ImagenUrl);
        Catalogo.Cartas.Add(carta);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = nuevoId }, carta);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Actualizar(int id, CreateCardDto dto)
    {
        var index = Catalogo.Cartas.FindIndex(carta => carta.Id == id);
        if (index < 0)
        {
            return NotFound(new { mensaje = "Carta no encontrada" });
        }

        Catalogo.Cartas[index] = new CardPersonaje(
            id, dto.PersonajeId, dto.Poder, dto.HabilidadEspecial,
            dto.Arma, dto.NivelPeligrosidad, dto.ImagenUrl);
        return NoContent();
    }
}
