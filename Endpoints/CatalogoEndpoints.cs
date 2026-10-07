using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;
using Models;
using Data;
using Services;
using System.Linq;
using System.Collections.Generic;

namespace Endpoints
{
    public static class CatalogoEndpoints
    {
        public static void MapCatalogoEndpoints(this IEndpointRouteBuilder app, CatalogoService service)
        {
            var grupoPersonajes = app.MapGroup("/personajes").WithTags("Personajes");
            var grupoCartas = app.MapGroup("/cartas").WithTags("Cartas");
            var grupoEventos = app.MapGroup("/eventos").WithTags("Eventos");

            /*grupoPersonajes.MapGet("/", ([FromQuery] string? faccion, [FromQuery] bool? fuerzaSensitivo) =>
            {
                var query = Catalogo.Personajes.AsQueryable();
                
                if (!string.IsNullOrEmpty(faccion) && System.Enum.TryParse<Faccion>(faccion, true, out var faccionEnum))
                {
                    query = query.Where(p => p.Faccion == faccionEnum);
                }
                
                if (fuerzaSensitivo.HasValue)
                {
                    query = query.Where(p => p.FuerzaSensitivo == fuerzaSensitivo.Value);
                }

                return Results.Ok(query.ToList());
            })
            .WithSummary("Obtiene la lista de personajes con filtros opcionales")
            .Produces<List<Personaje>>(StatusCodes.Status200OK);
            */
            //lista de personajes 
            grupoPersonajes.MapGet("/",()=>Results.Ok(Catalogo.Personajes))
            .WithSummary("Obtiene la lista simple  de personajes")
            .Produces<List<Personaje>>(StatusCodes.Status200OK);
             //obtener personajes con card
             /*grupoPersonajes.MapGet("/{id}/con-card",(int id)=> Results.Ok(service.ObtenerCartasPorPersonaje(id)))
            .WithSummary("Obtiene las cartas en los que participó un personaje")
            .Produces<List<CardPersonaje>>(StatusCodes.Status200OK);
*/
            grupoPersonajes.MapGet("/ranking", () => Results.Ok(service.ObtenerRankingPorPoder()))
            .WithSummary("Obtiene el ranking de personajes ordenados por su poder")
            .Produces<List<CardPersonaje>>(StatusCodes.Status200OK);

            grupoPersonajes.MapGet("/{id}", (int id) =>
            {
                var personaje = Catalogo.Personajes.FirstOrDefault(p => p.Id == id);
                return personaje is not null ? Results.Ok(personaje) : Results.NotFound();
            })
            .WithSummary("Busca un personaje específico por su ID")
            .Produces<Personaje>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

            grupoPersonajes.MapGet("/{id}/eventos", (int id) => Results.Ok(service.ObtenerEventosPorPersonaje(id)))
            .WithSummary("Obtiene los eventos en los que participó un personaje")
            .Produces<List<Evento>>(StatusCodes.Status200OK);

            grupoPersonajes.MapPost("/", (CreatePersonajeDto dto) =>
            {
                var nuevoId = Catalogo.Personajes.Any() ? Catalogo.Personajes.Max(p => p.Id) + 1 : 1;
                var nuevoPersonaje = new Personaje(nuevoId, dto.Nombre,dto.url, dto.Especie, dto.Faccion, dto.Afiliacion, dto.Estado, dto.FuerzaSensitivo);
                
                Catalogo.Personajes.Add(nuevoPersonaje);
                
                return Results.Created($"/personajes/{nuevoId}", nuevoPersonaje);
            })
            .WithSummary("Crea un nuevo personaje")
            .Produces<Personaje>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

            grupoPersonajes.MapPut("/{id}", (int id, CreatePersonajeDto dto) =>
            {
                var index = Catalogo.Personajes.FindIndex(p => p.Id == id);
                if (index == -1) return Results.NotFound();

                Catalogo.Personajes[index] = new Personaje(id, dto.Nombre,dto.url, dto.Especie, dto.Faccion, dto.Afiliacion, dto.Estado, dto.FuerzaSensitivo);
                
                return Results.NoContent();
            })
            .WithSummary("Actualiza los datos de un personaje existente")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

            grupoPersonajes.MapDelete("/{id}", (int id) =>
            {
                var personajesRemovidos = Catalogo.Personajes.RemoveAll(p => p.Id == id);
                return personajesRemovidos > 0 ? Results.NoContent() : Results.NotFound();
            })
            .WithSummary("Elimina un personaje por su ID")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

            grupoCartas.MapGet("/", () => Results.Ok(Catalogo.Cartas))
            .WithSummary("Obtiene el catálogo completo de cartas")
            .Produces<List<CardPersonaje>>(StatusCodes.Status200OK);
            
            grupoCartas.MapGet("/{id}", (int id) =>
            {
                var carta = Catalogo.Cartas.FirstOrDefault(c => c.Id == id);
                return carta is not null ? Results.Ok(carta) : Results.NotFound();
            })
            .WithSummary("Busca una carta específica por su ID")
            .Produces<CardPersonaje>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
            // POST /cartas
            grupoCartas.MapPost("/", (CreateCardDto dto) =>
            {
                var nuevoId = Catalogo.Cartas.Any() ? Catalogo.Cartas.Max(c => c.Id) + 1 : 1;
                var nuevaCarta = new CardPersonaje(nuevoId, dto.PersonajeId, dto.Poder, dto.HabilidadEspecial, dto.Arma, dto.NivelPeligrosidad, dto.ImagenUrl);
                
                Catalogo.Cartas.Add(nuevaCarta);
                
                return Results.Created($"/cartas/{nuevoId}", nuevaCarta);
            })
            .WithSummary("Crea una nueva carta para un personaje")
            .Produces<CardPersonaje>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

            grupoCartas.MapPut("/{id}", (int id, CreateCardDto dto) =>
            {
                var index = Catalogo.Cartas.FindIndex(c => c.Id == id);
                if (index == -1) return Results.NotFound();

                Catalogo.Cartas[index] = new CardPersonaje(id, dto.PersonajeId, dto.Poder, dto.HabilidadEspecial, dto.Arma, dto.NivelPeligrosidad, dto.ImagenUrl);
                
                return Results.NoContent();
            })
            .WithSummary("Actualiza los datos de una carta existente")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

            grupoEventos.MapGet("/", () => Results.Ok(Catalogo.Eventos))
            .WithSummary("Obtiene el registro de todos los eventos galácticos")
            .Produces<List<Evento>>(StatusCodes.Status200OK);
            
            grupoEventos.MapGet("/{id}", (int id) =>
            {
                var evento = Catalogo.Eventos.FirstOrDefault(e => e.Id == id);
                return evento is not null ? Results.Ok(evento) : Results.NotFound();
            })
            .WithSummary("Busca un evento específico por su ID")
            .Produces<Evento>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
            grupoEventos.MapPost("/", (CreateEventoDto dto) =>
            {
                var personajesMuertos = Catalogo.Personajes
                    .Where(p => dto.Participantes.Contains(p.Id) && p.Estado == Estado.Muerto)
                    .Select(p => p.Nombre)
                    .ToList();

                if (personajesMuertos.Any())
                {
                    return Results.BadRequest($"Violación de regla temporal: No se puede registrar el evento. Los siguientes personajes ya están muertos: {string.Join(", ", personajesMuertos)}");
                }

                var nuevoId = Catalogo.Eventos.Any() ? Catalogo.Eventos.Max(e => e.Id) + 1 : 1;
                var nuevoEvento = new Evento(nuevoId, dto.Nombre, dto.Fecha, dto.Ubicacion, dto.Descripcion, dto.Participantes, dto.ResultadoGanador);
                
                Catalogo.Eventos.Add(nuevoEvento);
                
                return Results.Created($"/eventos/{nuevoId}", nuevoEvento);
            })
            .WithSummary("Crea un nuevo evento validando que los participantes estén vivos")
            .Produces<Evento>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

            // PUT /eventos/{id}
            grupoEventos.MapPut("/{id}", (int id, CreateEventoDto dto) =>
            {
                var index = Catalogo.Eventos.FindIndex(e => e.Id == id);
                if (index == -1) return Results.NotFound();

                var personajesMuertos = Catalogo.Personajes
                    .Where(p => dto.Participantes.Contains(p.Id) && p.Estado == Estado.Muerto)
                    .Select(p => p.Nombre)
                    .ToList();

                if (personajesMuertos.Any())
                {
                    return Results.BadRequest($"Violación de regla temporal: Los siguientes personajes están muertos y no pueden participar: {string.Join(", ", personajesMuertos)}");
                }

                Catalogo.Eventos[index] = new Evento(id, dto.Nombre, dto.Fecha, dto.Ubicacion, dto.Descripcion, dto.Participantes, dto.ResultadoGanador);
                
                return Results.NoContent();
            })
            .WithSummary("Actualiza un evento existente validando el estado temporal de los participantes")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

            grupoEventos.MapGet("/{id}/mvp", (int id) =>
            {
                var mvp = service.ObtenerMvpDeEvento(id);
                return mvp is not null ? Results.Ok(mvp) : Results.NotFound("No se encontró MVP para este evento.");
            })
            .WithSummary("Calcula el personaje más valioso (MVP) de un evento")
            .Produces<Personaje>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

            grupoEventos.MapPost("/{id}/simular", (int id) =>
            {
                var resultado = service.SimularBatalla(id);
                return resultado.Exito ? Results.Ok(resultado) : Results.BadRequest(resultado.MensajeError);
            })
            .WithSummary("Ejecuta la simulación de batalla para un evento")
            .Produces<ResultadoSimulacion>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);
            // POST
            grupoEventos.MapPost("/{id}/registrar-muerte/{personajeId}", (int id, int personajeId) =>
            {
                
                var evento = Catalogo.Eventos.FirstOrDefault(e => e.Id == id);
                if (evento == null) return Results.NotFound("Evento no encontrado.");
                
                if (!evento.Participantes.Contains(personajeId))
                    return Results.BadRequest("El personaje no participó en este evento, no puede morir aquí.");

                service.RegistrarMuertePersonaje(personajeId);
                
                return Results.Ok(new { Mensaje = $"El personaje con ID {personajeId} ha sido registrado como muerto en el evento {id}." });
            })
            .WithSummary("Registra la muerte de un participante en un evento y actualiza su estado automáticamente")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);
            
        }
    }
}