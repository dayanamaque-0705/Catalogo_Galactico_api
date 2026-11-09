using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Models;
using Data;

namespace Services
{
    public class CatalogoService
    {
        private readonly GalaxiaDb? _db;

        public CatalogoService(GalaxiaDb? db = null)
        {
            _db = db;
        }

        // ==================== MÉTODOS EXISTENTES ====================

        public List<Evento> ObtenerEventosPorPersonaje(int personajeId)
        {
            return Catalogo.Eventos
                .Where(e => e.Participantes.Contains(personajeId))
                .ToList();
        }

        public List<CardPersonaje> ObtenerRankingPorPoder()
        {
            return Catalogo.Cartas
                .OrderByDescending(c => c.Poder)
                .ToList();
        }

        public Personaje? ObtenerMvpDeEvento(int eventoId)
        {
            var evento = Catalogo.Eventos.FirstOrDefault(e => e.Id == eventoId);
         
            if (evento == null || !evento.Participantes.Any()) return null;

            var cartaMvp = Catalogo.Cartas
                .Where(c => evento.Participantes.Contains(c.PersonajeId))
                .OrderByDescending(c => c.Poder)
                .FirstOrDefault();

            if (cartaMvp == null) return null;

            return Catalogo.Personajes.FirstOrDefault(p => p.Id == cartaMvp.PersonajeId);
        }

        public void RegistrarMuertePersonaje(int personajeId)
        {
            var index = Catalogo.Personajes.FindIndex(p => p.Id == personajeId);
            if (index != -1)
            {
                var personajeOriginal = Catalogo.Personajes[index];
                
                Catalogo.Personajes[index] = personajeOriginal with { Estado = Estado.Muerto };
            }
        }

        public ResultadoSimulacion SimularBatalla(int eventoId)
        {
            var evento = Catalogo.Eventos.FirstOrDefault(e => e.Id == eventoId);
            
            if (evento == null || evento.Participantes.Count < 2)
            {
                return new ResultadoSimulacion { Exito = false, MensajeError = "Evento no encontrado o no tiene participantes suficientes." };
            }

            var personajesParticipantes = Catalogo.Personajes
                .Where(p => evento.Participantes.Contains(p.Id))
                .ToList();

            var cartasParticipantes = Catalogo.Cartas
                .Where(c => evento.Participantes.Contains(c.PersonajeId))
                .ToList();

            if (personajesParticipantes.Count != evento.Participantes.Count || cartasParticipantes.Count != evento.Participantes.Count)
            {
                return new ResultadoSimulacion { Exito = false, MensajeError = "Faltan cartas para algunos de los participantes del evento." };
            }

            var poderPorFaccion = new Dictionary<string, int>();
            var random = new Random();

            foreach (var personaje in personajesParticipantes)
            {
                var carta = cartasParticipantes.First(c => c.PersonajeId == personaje.Id);
                string nombreFaccion = personaje.Faccion.ToString();

                if (!poderPorFaccion.ContainsKey(nombreFaccion))
                    poderPorFaccion[nombreFaccion] = 0;

                poderPorFaccion[nombreFaccion] += carta.Poder;
            }

            foreach (var faccion in poderPorFaccion.Keys.ToList())
            {
                int factorAleatorio = random.Next(-10, 11);
                poderPorFaccion[faccion] += factorAleatorio;
            }

            var bandoGanador = poderPorFaccion.OrderByDescending(x => x.Value).First();

            return new ResultadoSimulacion 
            { 
                Exito = true,
                FaccionGanadora = bandoGanador.Key,
                PoderCalculado = poderPorFaccion,
                Criterio = "Suma del poder base por bando más un factor aleatorio de suerte (-10 a +10) aplicado a la fuerza total."
            };
        }

        // ==================== NUEVOS MÉTODOS: SISTEMA DE XP Y ASCENSO ====================

        /// <summary>
        /// Devuelve los personajes ordenados por su XP acumulada.
        /// </summary>
        public async Task<List<Personaje>> ObtenerRankingExperienciaAsync()
        {
            if (_db != null)
            {
                return await _db.Personajes.OrderByDescending(p => p.XP).ToListAsync();
            }

            return await Task.FromResult(Catalogo.Personajes.OrderByDescending(p => p.XP).ToList());
        }
        public async Task<Personaje?> OtorgarXPAsync(int id, int puntosXP)
        {
            if (_db != null)
            {
                var personajeDb = await _db.Personajes.FindAsync(id);
                if (personajeDb == null) return null;

                int xpTotalDb = personajeDb.XP + puntosXP;
                string rangoDb = personajeDb.Rango;

                if (xpTotalDb >= 300) rangoDb = "Leyenda";
                else if (xpTotalDb >= 200) rangoDb = "Líder";
                else if (xpTotalDb >= 100) rangoDb = "Comandante";
                else rangoDb = "Recluta";

                var nuevoPersonajeDb = personajeDb with { XP = xpTotalDb, Rango = rangoDb };
                _db.Entry(personajeDb).CurrentValues.SetValues(nuevoPersonajeDb);

                if (rangoDb != personajeDb.Rango)
                {
                    var cartaDb = await _db.Cartas.FirstOrDefaultAsync(c => c.PersonajeId == id);
                    if (cartaDb != null)
                    {
                        var nuevaCartaDb = cartaDb with { NivelPeligrosidad = Math.Min(10, cartaDb.NivelPeligrosidad + 2) };
                        _db.Entry(cartaDb).CurrentValues.SetValues(nuevaCartaDb);
                    }
                }

                await _db.SaveChangesAsync();
                return nuevoPersonajeDb;
            }

            // Fallback en memoria si _db no está inyectado
            var index = Catalogo.Personajes.FindIndex(p => p.Id == id);
            if (index == -1) return null;

            var personaje = Catalogo.Personajes[index];
            int nuevaXP = personaje.XP + puntosXP;

            string nuevoRango = personaje.Rango;
            if (nuevaXP >= 300) nuevoRango = "Leyenda";
            else if (nuevaXP >= 200) nuevoRango = "Líder";
            else if (nuevaXP >= 100) nuevoRango = "Comandante";
            else nuevoRango = "Recluta";

            bool ascendio = nuevoRango != personaje.Rango;
            var personajeActualizado = personaje with { XP = nuevaXP, Rango = nuevoRango };
            Catalogo.Personajes[index] = personajeActualizado;

            if (ascendio)
            {
                var indexCarta = Catalogo.Cartas.FindIndex(c => c.PersonajeId == id);
                if (indexCarta != -1)
                {
                    var cartaOriginal = Catalogo.Cartas[indexCarta];
                    Catalogo.Cartas[indexCarta] = cartaOriginal with { NivelPeligrosidad = Math.Min(10, cartaOriginal.NivelPeligrosidad + 2) };
                }
            }

            return await Task.FromResult(personajeActualizado);
        }

        /// <summary>
        /// Reinicia la experiencia de un personaje a 0 y asigna el rango solicitado (por defecto Recluta).
        /// </summary>
        public async Task<Personaje?> ResetearExperienciaAsync(int id, string? nuevoRango = "Recluta")
        {
            string rangoFinal = string.IsNullOrWhiteSpace(nuevoRango) ? "Recluta" : nuevoRango;

            if (_db != null)
            {
                var personajeDb = await _db.Personajes.FindAsync(id);
                if (personajeDb == null) return null;

                var personajeReseteadoDb = personajeDb with { XP = 0, Rango = rangoFinal };
                _db.Entry(personajeDb).CurrentValues.SetValues(personajeReseteadoDb);

                await _db.SaveChangesAsync();
                return personajeReseteadoDb;
            }

            // Fallback en memoria
            var index = Catalogo.Personajes.FindIndex(p => p.Id == id);
            if (index == -1) return null;

            var personajeReseteado = Catalogo.Personajes[index] with { XP = 0, Rango = rangoFinal };
            Catalogo.Personajes[index] = personajeReseteado;

            return await Task.FromResult(personajeReseteado);
        }
    }

    public class ResultadoSimulacion
    {
        public bool Exito { get; set; }
        public string? MensajeError { get; set; }
        public string? FaccionGanadora { get; set; }
        public Dictionary<string, int>? PoderCalculado { get; set; }
        public string? Criterio { get; set; }
    }
}