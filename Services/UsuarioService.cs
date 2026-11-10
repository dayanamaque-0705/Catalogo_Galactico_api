using Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Models;
using MySqlConnector;

namespace Services;

public interface IUsuarioService
{
    Task<UsuarioResponse?> CrearAsync(CrearUsuarioRequest request, CancellationToken ct);
    Task<UsuarioResponse?> ObtenerPorIdAsync(int id, CancellationToken ct);
}

public sealed class UsuarioService(GalaxiaDb db, IPasswordHasher<Usuario> passwordHasher) : IUsuarioService
{
    public async Task<UsuarioResponse?> ObtenerPorIdAsync(int id, CancellationToken ct)
    {
        var usuario = await db.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, ct);
        return usuario is null ? null : new UsuarioResponse(usuario.Id, usuario.Correo, usuario.CreadoEn);
    }

    public async Task<UsuarioResponse?> CrearAsync(CrearUsuarioRequest request, CancellationToken ct)
    {
        var correo = request.Correo.Trim().ToLowerInvariant();
        if (await db.Usuarios.AnyAsync(u => u.Correo == correo, ct))
            return null;

        var usuario = new Usuario
        {
            Correo = correo,
            PasswordHash = string.Empty,
            CreadoEn = DateTimeOffset.UtcNow
        };
        usuario.PasswordHash = passwordHasher.HashPassword(usuario, request.Contrasena);
        db.Usuarios.Add(usuario);
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException exception) when (exception.InnerException is MySqlException { Number: 1062 })
        {
            db.Entry(usuario).State = EntityState.Detached;
            return null;
        }

        return new UsuarioResponse(usuario.Id, usuario.Correo, usuario.CreadoEn);
    }
}
