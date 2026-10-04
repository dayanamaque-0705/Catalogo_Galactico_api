using System.Collections.Generic;
using System.Linq;
using Models;
using Data;

namespace Services
{
    public class CatalogoService
    {
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