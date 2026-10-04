using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MiniProyecto.Services; // Solo importamos tus servicios

var builder = WebApplication.CreateBuilder(args);

// Configuración de Swagger/OpenAPI[cite: 3]
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Registramos la clase del servicio que acabas de crear
builder.Services.AddScoped<CatalogoService>();

var app = builder.Build();

// Habilitar Swagger[cite: 3]
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//TEMPORAL
app.MapGet("/prueba-ranking", (CatalogoService service) => 
{
    return service.ObtenerRankingPorPoder();
});

app.MapGet("/prueba-simulacion/{eventoId}", (int eventoId, CatalogoService service) => 
{
    return service.SimularBatalla(eventoId);
});

app.Run();