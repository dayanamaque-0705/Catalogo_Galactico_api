using System.ComponentModel.DataAnnotations;

namespace Models;

/// <summary>Datos para crear o actualizar un personaje.</summary>
public sealed record CreatePersonajeDto
{
    [Required, StringLength(100)]
    public required string Nombre { get; init; }

    [Required, StringLength(80)]
    public required string Especie { get; init; }

    public required Faccion Faccion { get; init; }

    [Required, StringLength(100)]
    public required string Afiliacion { get; init; }

    public required Estado Estado { get; init; }
    public required bool FuerzaSensitivo { get; init; }

    [StringLength(200)]
    public string? IdentidadSecreta { get; init; }
}

/// <summary>Datos públicos de un personaje.</summary>
public sealed record PersonajeResponse(
    int Id,
    string Nombre,
    string Especie,
    Faccion Faccion,
    string Afiliacion,
    Estado Estado,
    bool FuerzaSensitivo,
    int XP,
    string Rango,
    string? IdentidadSecreta);

/// <summary>Datos para crear o actualizar una carta de personaje.</summary>
public sealed record CreateCardDto
{
    [Range(1, int.MaxValue)]
    public int PersonajeId { get; init; }

    [Range(0, 100000)]
    public int Poder { get; init; }

    [Required, StringLength(150)]
    public required string HabilidadEspecial { get; init; }

    [Required, StringLength(150)]
    public required string Arma { get; init; }

    [Range(0, 10)]
    public int NivelPeligrosidad { get; init; }

    [Required, StringLength(255)]
    public required string ImagenUrl { get; init; }
}

/// <summary>Datos públicos de una carta.</summary>
public sealed record CardResponse(
    int Id,
    int PersonajeId,
    int Poder,
    string HabilidadEspecial,
    string Arma,
    int NivelPeligrosidad,
    string ImagenUrl);

/// <summary>Datos para crear o actualizar un evento.</summary>
public sealed record CreateEventoDto
{
    [Required, StringLength(120)]
    public required string Nombre { get; init; }

    public int Fecha { get; init; }

    [Required, StringLength(100)]
    public required string Ubicacion { get; init; }

    [Required]
    public required string Descripcion { get; init; }

    [Required, MinLength(1)]
    public required List<int> Participantes { get; init; }

    [Required, StringLength(80)]
    public required string ResultadoGanador { get; init; }
}

/// <summary>Datos públicos de un evento.</summary>
public sealed record EventoResponse(
    int Id,
    string Nombre,
    int Fecha,
    string Ubicacion,
    string Descripcion,
    IReadOnlyList<int> Participantes,
    string ResultadoGanador);

/// <summary>XP que se otorga a un personaje.</summary>
public sealed record OtorgarXpRequest
{
    [Range(1, 100000)]
    public int PuntosXP { get; init; }
}

/// <summary>Rango que se asignará al restablecer la experiencia.</summary>
public sealed record ResetearExperienciaRequest
{
    [Required]
    public required string Rango { get; init; }
}

/// <summary>Datos necesarios para crear una cuenta local.</summary>
public sealed record CrearUsuarioRequest
{
    [Required, EmailAddress, StringLength(254)]
    public required string Correo { get; init; }

    [Required, MinLength(12), StringLength(128)]
    public required string Contrasena { get; init; }
}

/// <summary>Datos públicos de una cuenta, sin incluir su hash de contraseña.</summary>
public sealed record UsuarioResponse(int Id, string Correo, DateTimeOffset CreadoEn);
