using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Cargar configuración de Ocelot
builder.Configuration.AddJsonFile(
    "ocelot.json",
    optional: false,
    reloadOnChange: true
);

// Registrar Ocelot
builder.Services.AddOcelot(builder.Configuration);

// CORS para que después React pueda consumir el Gateway
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactApp", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("ReactApp");

// Activar Ocelot
await app.UseOcelot();

app.Run();