using Data;
using System.Text.Json.Serialization;
using Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Models;
using Services;

var builder = WebApplication.CreateBuilder(args);

// Configuración de MySQL con Entity Framework Core
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Falta ConnectionStrings:DefaultConnection en User Secrets.");
}

builder.Services.AddDbContext<GalaxiaDb>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddScoped<ICatalogoService, CatalogoService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();
builder.Services.AddSingleton<SensitiveDataEncryptionService>();
builder.Services.AddScoped<AdminApiKeyFilter>();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();