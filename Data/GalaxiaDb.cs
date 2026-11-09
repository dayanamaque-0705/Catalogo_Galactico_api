using Microsoft.EntityFrameworkCore;
using Models;

namespace Data;

public class GalaxiaDb : DbContext
{
    public GalaxiaDb(DbContextOptions<GalaxiaDb> options) : base(options)
    {
    }

    public DbSet<Personaje> Personajes => Set<Personaje>();
    public DbSet<CardPersonaje> Cartas => Set<CardPersonaje>();
    public DbSet<Evento> Eventos => Set<Evento>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configuración Personaje
        modelBuilder.Entity<Personaje>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Nombre).IsRequired().HasMaxLength(100);
            e.Property(p => p.Especie).HasMaxLength(80);
            e.Property(p => p.Afiliacion).HasMaxLength(100);
            e.Property(p => p.Faccion).HasConversion<string>();
            e.Property(p => p.Estado).HasConversion<string>();
        });


        // Configuración CardPersonaje
        modelBuilder.Entity<CardPersonaje>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.HabilidadEspecial).HasMaxLength(150);
            e.Property(c => c.Arma).HasMaxLength(150);
            e.Property(c => c.ImagenUrl).HasMaxLength(255);
        });

        modelBuilder.Entity<Evento>(e =>
        {
            e.HasKey(ev => ev.Id);
            e.Property(ev => ev.Nombre).IsRequired().HasMaxLength(120);
            e.Property(ev => ev.Ubicacion).HasMaxLength(100);
            e.Property(ev => ev.ResultadoGanador).HasMaxLength(80);

            e.Property(ev => ev.Participantes)
             .HasConversion(
                 v => string.Join(',', v),
                 v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList()
             );
        });

    }
}