using Microsoft.EntityFrameworkCore;
using TipoIdentificacionSRV6.Data;
using TipoIdentificacionSRV6.Endpoints;
using TipoIdentificacionSRV6.Services;
using TipoIdentificacionSRV6.Auth;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Servicios
builder.Services.AddHttpClient<IBitacoraClient, BitacoraClient>();
builder.Services.AddScoped<ITokenValidator, TokenValidator>();  // ✅ REGISTRAR VALIDADOR
builder.Services.AddScoped<ITipoIdentificacionService, TipoIdentificacionService>();

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

// ========================================
// ✅ MIDDLEWARE DE SEGURIDAD - AQUÍ VA
// ========================================
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value ?? "";
    var method = context.Request.Method;

    // GET y OPTIONS NO REQUIEREN TOKEN
    if (method == "GET" || method == "OPTIONS")
    {
        await next();
        return;
    }

    // Validar token para POST, PUT, DELETE
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

    await next();
});

// ========================================
// ✅ REGISTRAR ENDPOINTS - DESPUÉS DEL MIDDLEWARE
// ========================================
app.MapTipoIdentificacionEndpoints();

app.Run();