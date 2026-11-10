using Microsoft.AspNetCore.Mvc;
using Models;
using Services;

namespace Controllers;

[ApiController]
[Route("usuarios")]
[Produces("application/json")]
public sealed class UsuariosController(IUsuarioService usuarios) : ControllerBase
{
    /// <summary>Obtiene una cuenta sin revelar su hash de contraseña.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UsuarioResponse>> ObtenerPorId(int id, CancellationToken cancellationToken)
    {
        var usuario = await usuarios.ObtenerPorIdAsync(id, cancellationToken);
        return usuario is null
            ? NotFound(new ProblemDetails { Status = StatusCodes.Status404NotFound, Title = "Cuenta no encontrada." })
            : Ok(usuario);
    }

    /// <summary>Crea una cuenta y guarda su contraseña como hash.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UsuarioResponse>> Crear(
        CrearUsuarioRequest request, CancellationToken cancellationToken)
    {
        var usuario = await usuarios.CrearAsync(request, cancellationToken);
        if (usuario is null)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Ya existe una cuenta con ese correo."
            });
        }

        return CreatedAtAction(nameof(ObtenerPorId), new { id = usuario.Id }, usuario);
    }
}
