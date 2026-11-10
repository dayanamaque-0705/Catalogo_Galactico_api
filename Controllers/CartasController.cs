using Microsoft.AspNetCore.Mvc;
using Models;
using Services;

namespace Controllers;

[ApiController]
[Route("cartas")]
[Produces("application/json")]
public sealed class CartasController(ICatalogoService catalogo) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CardResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CardResponse>>> ObtenerTodas(CancellationToken ct) =>
        Ok(await catalogo.ObtenerCartasAsync(ct));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CardResponse>> ObtenerPorId(int id, CancellationToken ct)
    {
        var carta = await catalogo.ObtenerCartaAsync(id, ct);
        return carta is null
            ? NotFound(new ProblemDetails { Status = StatusCodes.Status404NotFound, Title = "Carta no encontrada." })
            : Ok(carta);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CardResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CardResponse>> Crear(CreateCardDto dto, CancellationToken ct)
    {
        var carta = await catalogo.CrearCartaAsync(dto, ct);
        if (carta is null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "El personaje asociado no existe."
            });
        }

        return CreatedAtAction(nameof(ObtenerPorId), new { id = carta.Id }, carta);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Actualizar(int id, CreateCardDto dto, CancellationToken ct)
    {
        var carta = await catalogo.ActualizarCartaAsync(id, dto, ct);
        return carta is null
            ? NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "No se encontró la carta o el personaje asociado."
            })
            : NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(int id, CancellationToken ct) =>
        await catalogo.EliminarCartaAsync(id, ct)
            ? NoContent()
            : NotFound(new ProblemDetails { Status = StatusCodes.Status404NotFound, Title = "Carta no encontrada." });
}
