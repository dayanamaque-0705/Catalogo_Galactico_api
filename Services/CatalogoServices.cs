using System.Collections.Generic;
using System.Linq;
using Models;
using Data;

namespace Services
{
    public class CatalogoService
    {
        // GET /personajes/{id}/eventos: devuelve los eventos relacionados con un personaje.
        public List<Evento> ObtenerEventosPorPersonaje(int personajeId)
        {
            return CatalogoContext.Eventos
                .Where(e => e.Participantes.Contains(personajeId))
                .ToList();
        }

        // GET /personajes/ranking?por=poder: ordena personajes según el poder de su carta[cite: 2].
        public List<CardPersonaje> ObtenerRankingPorPoder()
        {
            return CatalogoContext.Cartas
                .OrderByDescending(c => c.Poder)
                .ToList();
        }

        // GET /eventos/{id}/mvp: devuelve el participante con mayor poder dentro del evento[cite: 2].
        public Personaje? ObtenerMvpDeEvento(int eventoId)
        {
            var evento = CatalogoContext.Eventos.FirstOrDefault(e => e.Id == eventoId);
            // Si el evento no existe o no tiene participantes, retornamos nulo
            if (evento == null || !evento.Participantes.Any()) return null;

            // Buscamos las cartas de los participantes y elegimos la de mayor poder
            var cartaMvp = CatalogoContext.Cartas
                .Where(c => evento.Participantes.Contains(c.PersonajeId))
                .OrderByDescending(c => c.Poder)
                .FirstOrDefault();

            if (cartaMvp == null) return null;

            return CatalogoContext.Personajes.FirstOrDefault(p => p.Id == cartaMvp.PersonajeId);
        }

        // Regla de negocio: Actualización automática de estado a muerto[cite: 2, 3]
        public void RegistrarMuertePersonaje(int personajeId)
        {
            var index = CatalogoContext.Personajes.FindIndex(p => p.Id == personajeId);
            if (index != -1)
            {
                var personajeOriginal = CatalogoContext.Personajes[index];
                
                // Como usamos 'record', creamos una copia inmutable con el campo cambiado y sustituimos en la lista[cite: 2, 3].
                CatalogoContext.Personajes[index] = personajeOriginal with { Estado = Estado.Muerto };
            }
        }
        // POST /eventos/{id}/simular: Simulación de batalla
        public ResultadoSimulacion SimularBatalla(int eventoId)
        {
            var evento = CatalogoContext.Eventos.FirstOrDefault(e => e.Id == eventoId);
            
            // Validación: Error claro si no hay suficientes participantes[cite: 2]
            if (evento == null || evento.Participantes.Count < 2)
            {
                return new ResultadoSimulacion { Exito = false, MensajeError = "Evento no encontrado o no tiene participantes suficientes." };
            }

            var personajesParticipantes = CatalogoContext.Personajes
                .Where(p => evento.Participantes.Contains(p.Id))
                .ToList();

            var cartasParticipantes = CatalogoContext.Cartas
                .Where(c => evento.Participantes.Contains(c.PersonajeId))
                .ToList();

            // Validación: Error claro si faltan cartas[cite: 2]
            if (personajesParticipantes.Count != evento.Participantes.Count || cartasParticipantes.Count != evento.Participantes.Count)
            {
                return new ResultadoSimulacion { Exito = false, MensajeError = "Faltan cartas para algunos de los participantes del evento." };
            }

            // Agrupar poder por bando (Facción)[cite: 2]
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

            // Aplicar factor aleatorio acotado (-10 a +10 de poder extra por facción)[cite: 2]
            foreach (var faccion in poderPorFaccion.Keys.ToList())
            {
                int factorAleatorio = random.Next(-10, 11);
                poderPorFaccion[faccion] += factorAleatorio;
            }

            // Determinar ganador
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

    // Clase auxiliar en el mismo archivo para empaquetar la respuesta
    public class ResultadoSimulacion
    {
        public bool Exito { get; set; }
        public string? MensajeError { get; set; }
        public string? FaccionGanadora { get; set; }
        public Dictionary<string, int>? PoderCalculado { get; set; }
        public string? Criterio { get; set; }
    }
 }
