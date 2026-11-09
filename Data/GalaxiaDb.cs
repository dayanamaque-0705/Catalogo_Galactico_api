using System;
using System.Collections.Generic;
using System.Linq;
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
        // === Mapeo de Personaje ===
        modelBuilder.Entity<Personaje>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Nombre).IsRequired().HasMaxLength(100);
            e.Property(p => p.Especie).HasMaxLength(80);
            e.Property(p => p.Afiliacion).HasMaxLength(100);
            e.Property(p => p.Faccion).HasConversion<string>();
            e.Property(p => p.Estado).HasConversion<string>();

            // Reglas de Fluent API para el sistema de ascenso
            e.Property(p => p.Rango).HasMaxLength(50).HasDefaultValue("Recluta");
            e.Property(p => p.XP).HasDefaultValue(0);
        });

        // Seed Data: Personajes iniciales usando el catálogo estático
        modelBuilder.Entity<Personaje>().HasData(Catalogo.Personajes);

        // === Mapeo de CardPersonaje ===
        modelBuilder.Entity<CardPersonaje>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.HabilidadEspecial).HasMaxLength(150);
            e.Property(c => c.Arma).HasMaxLength(150);
            e.Property(c => c.ImagenUrl).HasMaxLength(255);
        });

        // Seed Data: Cartas iniciales
        modelBuilder.Entity<CardPersonaje>().HasData(Catalogo.Cartas);

        // === Mapeo de Evento ===
        modelBuilder.Entity<Evento>(e =>
        {
            e.HasKey(ev => ev.Id);
            e.Property(ev => ev.Nombre).IsRequired().HasMaxLength(120);
            e.Property(ev => ev.Ubicacion).HasMaxLength(100);
            e.Property(ev => ev.ResultadoGanador).HasMaxLength(80);

            // Convierte List<int> a string (ej. "1,2,3") para guardarlo en una sola columna en MySQL
            e.Property(ev => ev.Participantes)
             .HasConversion(
                 v => string.Join(',', v),
                 v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList()
             );
        });

        // Seed Data: Eventos iniciales
        modelBuilder.Entity<Evento>().HasData(Catalogo.Eventos);
    }
}