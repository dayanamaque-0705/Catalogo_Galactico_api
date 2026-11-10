using System.Security.Cryptography;
using System.Text;
using Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Models;
using MySqlConnector;

namespace Services;

public interface ICatalogoService
{
    Task<IReadOnlyList<PersonajeResponse>> ObtenerPersonajesAsync(Faccion? faccion, bool? fuerzaSensitivo, CancellationToken ct);
    Task<PersonajeResponse?> ObtenerPersonajeAsync(int id, CancellationToken ct);
    Task<PersonajeResponse> CrearPersonajeAsync(CreatePersonajeDto dto, CancellationToken ct);
    Task<PersonajeResponse?> ActualizarPersonajeAsync(int id, CreatePersonajeDto dto, CancellationToken ct);
    Task<EliminarResultado> EliminarPersonajeAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<CardResponse>> ObtenerCartasAsync(CancellationToken ct);
    Task<CardResponse?> ObtenerCartaAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<CardResponse>> ObtenerRankingPoderAsync(CancellationToken ct);
    Task<CardResponse?> CrearCartaAsync(CreateCardDto dto, CancellationToken ct);
    Task<CardResponse?> ActualizarCartaAsync(int id, CreateCardDto dto, CancellationToken ct);
    Task<bool> EliminarCartaAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<EventoResponse>> ObtenerEventosAsync(CancellationToken ct);
    Task<EventoResponse?> ObtenerEventoAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<EventoResponse>?> ObtenerEventosPorPersonajeAsync(int id, CancellationToken ct);
    Task<EventoRespuestaEscritura> CrearEventoAsync(CreateEventoDto dto, CancellationToken ct);
    Task<EventoRespuestaEscritura> ActualizarEventoAsync(int id, CreateEventoDto dto, CancellationToken ct);
    Task<bool> EliminarEventoAsync(int id, CancellationToken ct);
    Task<PersonajeResponse?> ObtenerMvpAsync(int eventoId, CancellationToken ct);
    Task<ResultadoSimulacion?> SimularBatallaAsync(int eventoId, CancellationToken ct);
    Task<bool?> RegistrarMuerteAsync(int eventoId, int personajeId, CancellationToken ct);
    Task<PersonajeResponse?> OtorgarXpAsync(int id, int puntosXp, CancellationToken ct);
    Task<PersonajeResponse?> ResetearExperienciaAsync(int id, string rango, CancellationToken ct);
}

public enum EliminarResultado
{
    Eliminado,
    NoEncontrado,
    TieneRelaciones
}

public sealed record EventoRespuestaEscritura(EventoResponse? Evento, string? Error, bool NoEncontrado = false);

public sealed class CatalogoService(
    GalaxiaDb db,
    SensitiveDataEncryptionService cifrado) : ICatalogoService
{
    private static readonly string[] Rangos = ["Recluta", "Comandante", "Líder", "Leyenda"];

    public async Task<IReadOnlyList<PersonajeResponse>> ObtenerPersonajesAsync(
        Faccion? faccion, bool? fuerzaSensitivo, CancellationToken ct)
    {
        var query = db.Personajes.AsNoTracking().AsQueryable();
        if (faccion.HasValue)
            query = query.Where(p => p.Faccion == faccion.Value);
        if (fuerzaSensitivo.HasValue)
            query = query.Where(p => p.FuerzaSensitivo == fuerzaSensitivo.Value);

        return (await query.OrderBy(p => p.Id).ToListAsync(ct)).Select(Map).ToList();
    }

    public async Task<PersonajeResponse?> ObtenerPersonajeAsync(int id, CancellationToken ct)
    {
        var personaje = await db.Personajes.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);
        return personaje is null ? null : Map(personaje);
    }

    public async Task<PersonajeResponse> CrearPersonajeAsync(CreatePersonajeDto dto, CancellationToken ct)
    {
        var personaje = new Personaje(
            0, dto.Nombre.Trim(), dto.Especie.Trim(), dto.Faccion,
            dto.Afiliacion.Trim(), dto.Estado, dto.FuerzaSensitivo)
        {
            IdentidadSecretaCifrada = cifrado.Cifrar(dto.IdentidadSecreta)
        };
        db.Personajes.Add(personaje);
        await db.SaveChangesAsync(ct);
        return Map(personaje);
    }

    public async Task<PersonajeResponse?> ActualizarPersonajeAsync(
        int id, CreatePersonajeDto dto, CancellationToken ct)
    {
        var personaje = await db.Personajes.FindAsync([id], ct);
        if (personaje is null)
            return null;

        var actualizado = personaje with
        {
            Nombre = dto.Nombre.Trim(),
            Especie = dto.Especie.Trim(),
            Faccion = dto.Faccion,
            Afiliacion = dto.Afiliacion.Trim(),
            Estado = dto.Estado,
            FuerzaSensitivo = dto.FuerzaSensitivo,
            IdentidadSecretaCifrada = dto.IdentidadSecreta is null
                ? personaje.IdentidadSecretaCifrada
                : cifrado.Cifrar(dto.IdentidadSecreta)
        };
        db.Entry(personaje).CurrentValues.SetValues(actualizado);
        await db.SaveChangesAsync(ct);
        return Map(actualizado);
    }

    public async Task<EliminarResultado> EliminarPersonajeAsync(int id, CancellationToken ct)
    {
        var personaje = await db.Personajes.FindAsync([id], ct);
        if (personaje is null)
            return EliminarResultado.NoEncontrado;

        var tieneCartas = await db.Cartas.AnyAsync(c => c.PersonajeId == id, ct);
        var eventos = await db.Eventos.AsNoTracking().ToListAsync(ct);
        if (tieneCartas || eventos.Any(e => e.Participantes.Contains(id)))
            return EliminarResultado.TieneRelaciones;

        db.Personajes.Remove(personaje);
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException exception) when (exception.InnerException is MySqlException { Number: 1451 })
        {
            db.Entry(personaje).State = EntityState.Detached;
            return EliminarResultado.TieneRelaciones;
        }

        return EliminarResultado.Eliminado;
    }

    public async Task<IReadOnlyList<CardResponse>> ObtenerCartasAsync(CancellationToken ct) =>
        (await db.Cartas.AsNoTracking().OrderBy(c => c.Id).ToListAsync(ct)).Select(Map).ToList();

    public async Task<CardResponse?> ObtenerCartaAsync(int id, CancellationToken ct)
    {
        var carta = await db.Cartas.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);
        return carta is null ? null : Map(carta);
    }

    public async Task<IReadOnlyList<CardResponse>> ObtenerRankingPoderAsync(CancellationToken ct) =>
        (await db.Cartas.AsNoTracking().OrderByDescending(c => c.Poder).ToListAsync(ct))
        .Select(Map).ToList();

    public async Task<CardResponse?> CrearCartaAsync(CreateCardDto dto, CancellationToken ct)
    {
        if (!await db.Personajes.AnyAsync(p => p.Id == dto.PersonajeId, ct))
            return null;

        var carta = new CardPersonaje(
            0, dto.PersonajeId, dto.Poder, dto.HabilidadEspecial.Trim(),
            dto.Arma.Trim(), dto.NivelPeligrosidad, dto.ImagenUrl.Trim());
        db.Cartas.Add(carta);
        await db.SaveChangesAsync(ct);
        return Map(carta);
    }

    public async Task<CardResponse?> ActualizarCartaAsync(int id, CreateCardDto dto, CancellationToken ct)
    {
        if (!await db.Personajes.AnyAsync(p => p.Id == dto.PersonajeId, ct))
            return null;

        var carta = await db.Cartas.FindAsync([id], ct);
        if (carta is null)
            return null;

        var actualizada = new CardPersonaje(
            id, dto.PersonajeId, dto.Poder, dto.HabilidadEspecial.Trim(),
            dto.Arma.Trim(), dto.NivelPeligrosidad, dto.ImagenUrl.Trim());
        db.Entry(carta).CurrentValues.SetValues(actualizada);
        await db.SaveChangesAsync(ct);
        return Map(actualizada);
    }

    public async Task<bool> EliminarCartaAsync(int id, CancellationToken ct)
    {
        var carta = await db.Cartas.FindAsync([id], ct);
        if (carta is null)
            return false;
        db.Cartas.Remove(carta);
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<IReadOnlyList<EventoResponse>> ObtenerEventosAsync(CancellationToken ct) =>
        (await db.Eventos.AsNoTracking().OrderBy(e => e.Id).ToListAsync(ct)).Select(Map).ToList();

    public async Task<EventoResponse?> ObtenerEventoAsync(int id, CancellationToken ct)
    {
        var evento = await db.Eventos.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct);
        return evento is null ? null : Map(evento);
    }

    public async Task<IReadOnlyList<EventoResponse>?> ObtenerEventosPorPersonajeAsync(int id, CancellationToken ct)
    {
        if (!await db.Personajes.AnyAsync(p => p.Id == id, ct))
            return null;
        return (await db.Eventos.AsNoTracking().ToListAsync(ct))
            .Where(e => e.Participantes.Contains(id)).Select(Map).ToList();
    }

    public async Task<EventoRespuestaEscritura> CrearEventoAsync(CreateEventoDto dto, CancellationToken ct)
    {
        var error = await ValidarParticipantesAsync(dto.Participantes, ct);
        if (error is not null)
            return new(null, error);

        var evento = CrearEntidadEvento(0, dto);
        db.Eventos.Add(evento);
        await OtorgarExperienciaEventoAsync(evento, ct);
        await db.SaveChangesAsync(ct);
        return new(Map(evento), null);
    }

    public async Task<EventoRespuestaEscritura> ActualizarEventoAsync(
        int id, CreateEventoDto dto, CancellationToken ct)
    {
        var evento = await db.Eventos.FindAsync([id], ct);
        if (evento is null)
            return new(null, null, true);

        var error = await ValidarParticipantesAsync(dto.Participantes, ct);
        if (error is not null)
            return new(null, error);

        var actualizado = CrearEntidadEvento(id, dto);
        db.Entry(evento).CurrentValues.SetValues(actualizado);
        await db.SaveChangesAsync(ct);
        return new(Map(actualizado), null);
    }

    public async Task<bool> EliminarEventoAsync(int id, CancellationToken ct)
    {
        var evento = await db.Eventos.FindAsync([id], ct);
        if (evento is null)
            return false;
        db.Eventos.Remove(evento);
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<PersonajeResponse?> ObtenerMvpAsync(int eventoId, CancellationToken ct)
    {
        var evento = await db.Eventos.AsNoTracking().FirstOrDefaultAsync(e => e.Id == eventoId, ct);
        if (evento is null || evento.Participantes.Count == 0)
            return null;

        var carta = await db.Cartas.AsNoTracking()
            .Where(c => evento.Participantes.Contains(c.PersonajeId))
            .OrderByDescending(c => c.Poder)
            .FirstOrDefaultAsync(ct);
        if (carta is null)
            return null;
        var personaje = await db.Personajes.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == carta.PersonajeId, ct);
        return personaje is null ? null : Map(personaje);
    }

    public async Task<ResultadoSimulacion?> SimularBatallaAsync(int eventoId, CancellationToken ct)
    {
        var evento = await db.Eventos.AsNoTracking().FirstOrDefaultAsync(e => e.Id == eventoId, ct);
        if (evento is null || evento.Participantes.Count < 2)
            return null;

        var personajes = await db.Personajes.AsNoTracking()
            .Where(p => evento.Participantes.Contains(p.Id)).ToListAsync(ct);
        var cartas = await db.Cartas.AsNoTracking()
            .Where(c => evento.Participantes.Contains(c.PersonajeId)).ToListAsync(ct);
        if (personajes.Count != evento.Participantes.Count ||
            personajes.Any(p => cartas.All(c => c.PersonajeId != p.Id)))
            return null;

        var poderPorFaccion = personajes.GroupBy(p => p.Faccion.ToString())
            .ToDictionary(
                g => g.Key,
                g => g.Sum(p => cartas.Where(c => c.PersonajeId == p.Id).Sum(c => c.Poder)),
                StringComparer.Ordinal);
        foreach (var faccion in poderPorFaccion.Keys.ToList())
            poderPorFaccion[faccion] += Random.Shared.Next(-10, 11);

        var ganador = poderPorFaccion.OrderByDescending(x => x.Value).First();
        return new ResultadoSimulacion
        {
            Exito = true,
            FaccionGanadora = ganador.Key,
            PoderCalculado = poderPorFaccion,
            Criterio = "Suma del poder base por facción con un factor aleatorio de -10 a +10."
        };
    }

    public async Task<bool?> RegistrarMuerteAsync(int eventoId, int personajeId, CancellationToken ct)
    {
        var evento = await db.Eventos.AsNoTracking().FirstOrDefaultAsync(e => e.Id == eventoId, ct);
        if (evento is null)
            return null;
        if (!evento.Participantes.Contains(personajeId))
            return false;
        var personaje = await db.Personajes.FindAsync([personajeId], ct);
        if (personaje is null)
            return false;

        db.Entry(personaje).CurrentValues.SetValues(personaje with { Estado = Estado.Muerto });
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<PersonajeResponse?> OtorgarXpAsync(int id, int puntosXp, CancellationToken ct)
    {
        var personaje = await db.Personajes.FindAsync([id], ct);
        if (personaje is null)
            return null;
        if (puntosXp <= 0 || personaje.XP > int.MaxValue - puntosXp)
            throw new ArgumentOutOfRangeException(nameof(puntosXp), "La cantidad de XP no es válida.");

        await ActualizarXpYRangoAsync(personaje, personaje.XP + puntosXp, null, ct);
        await db.SaveChangesAsync(ct);
        return Map(personaje);
    }

    public async Task<PersonajeResponse?> ResetearExperienciaAsync(int id, string rango, CancellationToken ct)
    {
        var rangoNormalizado = Rangos.FirstOrDefault(r =>
            string.Equals(r, rango, StringComparison.OrdinalIgnoreCase));
        if (rangoNormalizado is null)
            throw new ArgumentException("Rango inválido. Use Recluta, Comandante, Líder o Leyenda.", nameof(rango));

        var personaje = await db.Personajes.FindAsync([id], ct);
        if (personaje is null)
            return null;
        await ActualizarXpYRangoAsync(personaje, 0, rangoNormalizado, ct);
        await db.SaveChangesAsync(ct);
        return Map(personaje);
    }

    private async Task<string?> ValidarParticipantesAsync(IReadOnlyCollection<int> ids, CancellationToken ct)
    {
        if (ids.Count == 0)
            return "El evento debe tener al menos un participante.";
        if (ids.Distinct().Count() != ids.Count)
            return "No se puede repetir un participante.";

        var personajes = await db.Personajes.Where(p => ids.Contains(p.Id)).ToListAsync(ct);
        if (personajes.Count != ids.Count)
            return "Uno o más personajes no existen.";
        var muertos = personajes.Where(p => p.Estado == Estado.Muerto).Select(p => p.Nombre).ToList();
        return muertos.Count == 0
            ? null
            : $"Los personajes muertos no pueden participar: {string.Join(", ", muertos)}.";
    }

    private async Task OtorgarExperienciaEventoAsync(Evento evento, CancellationToken ct)
    {
        var personajes = await db.Personajes.Where(p => evento.Participantes.Contains(p.Id)).ToListAsync(ct);
        var faccionGanadora = evento.ResultadoGanador.Equals("Rebeldes", StringComparison.OrdinalIgnoreCase)
            ? nameof(Faccion.Rebelde)
            : evento.ResultadoGanador;

        foreach (var personaje in personajes)
        {
            var puntos = string.Equals(personaje.Faccion.ToString(), faccionGanadora, StringComparison.OrdinalIgnoreCase)
                ? 25
                : 10;
            var xp = (int)Math.Min(int.MaxValue, (long)personaje.XP + puntos);
            await ActualizarXpYRangoAsync(personaje, xp, null, ct);
        }
    }

    private async Task ActualizarXpYRangoAsync(Personaje personaje, int xp, string? rangoForzado, CancellationToken ct)
    {
        var anterior = IndiceRango(personaje.Rango);
        var rango = rangoForzado ?? (xp >= 300 ? "Leyenda" : xp >= 200 ? "Líder" : xp >= 100 ? "Comandante" : "Recluta");
        var actualizado = personaje with { XP = xp, Rango = rango };
        db.Entry(personaje).CurrentValues.SetValues(actualizado);

        var diferencia = IndiceRango(rango) - anterior;
        if (diferencia == 0)
            return;
        var carta = await db.Cartas.FirstOrDefaultAsync(c => c.PersonajeId == personaje.Id, ct);
        if (carta is not null)
        {
            var peligro = Math.Clamp(carta.NivelPeligrosidad + diferencia * 2, 0, 10);
            db.Entry(carta).CurrentValues.SetValues(carta with { NivelPeligrosidad = peligro });
        }
    }

    private static int IndiceRango(string rango)
    {
        var indice = Array.FindIndex(Rangos, r => string.Equals(r, rango, StringComparison.OrdinalIgnoreCase));
        return indice < 0 ? 0 : indice;
    }

    private static Evento CrearEntidadEvento(int id, CreateEventoDto dto) =>
        new(id, dto.Nombre.Trim(), dto.Fecha, dto.Ubicacion.Trim(), dto.Descripcion.Trim(),
            dto.Participantes.Distinct().ToList(), dto.ResultadoGanador.Trim());

    private PersonajeResponse Map(Personaje p) => new(
        p.Id, p.Nombre, p.Especie, p.Faccion, p.Afiliacion, p.Estado, p.FuerzaSensitivo,
        p.XP, p.Rango, cifrado.Descifrar(p.IdentidadSecretaCifrada));

    private static CardResponse Map(CardPersonaje c) =>
        new(c.Id, c.PersonajeId, c.Poder, c.HabilidadEspecial, c.Arma, c.NivelPeligrosidad, c.ImagenUrl);

    private static EventoResponse Map(Evento e) =>
        new(e.Id, e.Nombre, e.Fecha, e.Ubicacion, e.Descripcion, e.Participantes, e.ResultadoGanador);
}

public sealed class ResultadoSimulacion
{
    public bool Exito { get; set; }
    public string? MensajeError { get; set; }
    public string? FaccionGanadora { get; set; }
    public Dictionary<string, int>? PoderCalculado { get; set; }
    public string? Criterio { get; set; }
}

public sealed class SensitiveDataEncryptionService(IConfiguration configuration)
{
    private const int NonceLength = 12;
    private const int TagLength = 16;
    private readonly byte[] _key = ReadKey(configuration["Cifrado:Clave"]);

    public string? Cifrar(string? plaintext)
    {
        if (plaintext is null)
            return null;
        var nonce = RandomNumberGenerator.GetBytes(NonceLength);
        var ciphertext = new byte[Encoding.UTF8.GetByteCount(plaintext)];
        var tag = new byte[TagLength];
        using var aes = new AesGcm(_key, TagLength);
        aes.Encrypt(nonce, Encoding.UTF8.GetBytes(plaintext), ciphertext, tag);
        return $"v1:{Convert.ToBase64String(nonce)}:{Convert.ToBase64String(tag)}:{Convert.ToBase64String(ciphertext)}";
    }

    public string? Descifrar(string? value)
    {
        if (value is null)
            return null;
        var parts = value.Split(':');
        if (parts.Length != 4 || parts[0] != "v1")
            throw new CryptographicException("El formato del dato cifrado no es válido.");

        var nonce = Convert.FromBase64String(parts[1]);
        var tag = Convert.FromBase64String(parts[2]);
        var ciphertext = Convert.FromBase64String(parts[3]);
        if (nonce.Length != NonceLength || tag.Length != TagLength)
            throw new CryptographicException("El dato cifrado tiene longitudes no válidas.");
        var plaintext = new byte[ciphertext.Length];
        using var aes = new AesGcm(_key, TagLength);
        aes.Decrypt(nonce, ciphertext, tag, plaintext);
        return Encoding.UTF8.GetString(plaintext);
    }

    private static byte[] ReadKey(string? base64Key)
    {
        if (string.IsNullOrWhiteSpace(base64Key))
            throw new InvalidOperationException("Falta configurar Cifrado:Clave en User Secrets.");
        byte[] key;
        try
        {
            key = Convert.FromBase64String(base64Key);
        }
        catch (FormatException exception)
        {
            throw new InvalidOperationException("Cifrado:Clave debe ser Base64 válido.", exception);
        }
        if (key.Length != 32)
            throw new InvalidOperationException("Cifrado:Clave debe contener exactamente 32 bytes.");
        return key;
    }
}
