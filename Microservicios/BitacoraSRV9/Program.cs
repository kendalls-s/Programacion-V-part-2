using BitacoraSRV9;
using BitacoraSRV9.Auth;
using BitacoraSRV9.Repository;
using BitacoraSRV9.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<
    IDbConnectionFactory,
    DbConnectionFactory>();

builder.Services.AddScoped<
    BitacoraRepository>();

builder.Services.AddScoped<
    IBitacoraService,
    BitacoraService>();

builder.Services.AddHttpClient<
    ITokenValidator,
    TokenValidator>();

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new
{
    servicio = "BitacoraSRV9",
    estado = "Activo"
}));

app.MapBitacoraEndpoints();

app.Run();