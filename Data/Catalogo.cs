// data/CatalogoContext.cs
using System.Collections.Generic;
using Models;
namespace Data
{
    public static class Catalogo
    {
        // Lista Personajes 
        public static List<Personaje> Personajes = new List<Personaje>
        {
            new Personaje(1, "Luke Skywalker", "Humano", Faccion.Rebelde, "Alianza Rebelde", Estado.Vivo, true),
            new Personaje(2, "Darth Vader", "Humano/Cyborg", Faccion.Imperio, "Sith", Estado.Vivo, true),
            new Personaje(3, "Han Solo", "Humano", Faccion.Rebelde, "Contrabandista", Estado.Vivo, false)
        };

        // Lista Cartas 
        public static List<CardPersonaje> Cartas = new List<CardPersonaje>
        {
            new CardPersonaje(1, 1, 85, "Reflejos Jedi", "Sable de luz verde", 8, "url_luke.jpg"),
            new CardPersonaje(2, 2, 95, "Estrangulamiento con la Fuerza", "Sable de luz rojo", 10, "url_vader.jpg"),
            new CardPersonaje(3, 3, 60, "Disparo rápido", "Blaster pesado", 6, "url_han.jpg")
        };

        // Lista de everntos 
        public static List<Evento> Eventos = new List<Evento>
        {
            
            new Evento(1, "Batalla de Yavin", 0, "Yavin 4", "Destrucción de la primera Estrella de la Muerte", new List<int> { 1, 2, 3 }, "Rebeldes"), 
            new Evento(2, "Misión en Lothal", -5, "Lothal", "Incursión temprana del Imperio", new List<int> { 2 }, "Imperio"),
            new Evento(3, "Batalla de Endor", 4, "Luna de Endor", "Caída del Imperio", new List<int> { 1, 2, 3 }, "Rebeldes")
        };
    }
}