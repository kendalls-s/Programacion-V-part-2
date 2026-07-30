using Microsoft.EntityFrameworkCore;
using TiposUsuarioSRV5.Data;
using TiposUsuarioSRV5.Endpoints;
using TiposUsuarioSRV5.Services;
using TiposUsuarioSRV5.Auth;

var builder = WebApplication.CreateBuilder(args);

// ✅ Database CON EnableRetryOnFailure
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        }));

// Servicios
builder.Services.AddHttpClient<IBitacoraClient, BitacoraClient>();
builder.Services.AddScoped<ITokenValidator, TokenValidator>();
builder.Services.AddScoped<ITipoUsuarioService, TipoUsuarioService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors("AllowAll");
app.UseHttpsRedirection();

// ✅ MIDDLEWARE DE SEGURIDAD
// ✅ MIDDLEWARE DE SEGURIDAD
app.Use(async (context, next) =>
{
    var method = context.Request.Method;

    if (method == "GET" || method == "OPTIONS")
    {
        await next();
        return;
    }

    var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

    if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
    {
        context.Response.StatusCode = 401;
        await context.Response.WriteAsync("Token no proporcionado");
        return;
    }

    var token = authHeader.Substring("Bearer ".Length).Trim();

    var validator = context.RequestServices.GetRequiredService<ITokenValidator>();
    var isValid = await validator.ValidateAsync(token);

    if (!isValid)
    {
        context.Response.StatusCode = 401;
        await context.Response.WriteAsync("Token inválido o expirado");
        return;
    }

    Console.WriteLine($"✅ Token válido para: {context.Request.Path}");
    await next();
});

// Registrar endpoints
app.MapTipoUsuarioEndpoints();

app.Run();