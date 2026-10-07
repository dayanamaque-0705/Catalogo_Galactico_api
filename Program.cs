using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Endpoints; 
using Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddCors(options =>
{
 options.AddDefaultPolicy(policy=>
    policy.WithOrigins("http://localhost:5173")
    .AllowAnyHeader()
    .AllowAnyMethod());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<CatalogoService>();

var app = builder.Build();
app.UseCors();

//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

var catalogoService = new CatalogoService();

app.MapCatalogoEndpoints(catalogoService);

app.Run();