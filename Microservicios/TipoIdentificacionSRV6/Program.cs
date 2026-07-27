using Microsoft.EntityFrameworkCore;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using TipoIdentificacionSRV6.Data;
using TipoIdentificacionSRV6.DTOs;
using TipoIdentificacionSRV6.Entities;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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
// ✅ MIDDLEWARE: VALIDAR TOKEN
// ========================================

app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value ?? "";
    var publicRoutes = new[] { "/health", "/api/health" };

    if (publicRoutes.Any(r => path.StartsWith(r, StringComparison.OrdinalIgnoreCase)))
    {
        await next();
        return;
    }

    if (path.Contains("/swagger"))
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

    if (!ValidateToken(token))
    {
        context.Response.StatusCode = 401;
        await context.Response.WriteAsync("Token inválido o expirado");
        return;
    }

    await next();
});

// ========================================
// ✅ ENDPOINTS MINIMAL API
// ========================================

// GET: /api/TipoIdentificacion
app.MapGet("/api/TipoIdentificacion", async (ApplicationDbContext db) =>
{
    var tipos = await db.TiposIdentificacion
        .OrderBy(t => t.Nombre)
        .ToListAsync();

    return Results.Ok(tipos.Select(t => new TipoIdentificacionDto
    {
        Id = t.Id,
        Nombre = t.Nombre
    }));
});

// GET: /api/TipoIdentificacion/{id}
app.MapGet("/api/TipoIdentificacion/{id}", async (int id, ApplicationDbContext db) =>
{
    var tipo = await db.TiposIdentificacion.FindAsync(id);
    if (tipo == null)
        return Results.NotFound(new { message = "Tipo de identificación no encontrado" });

    return Results.Ok(new TipoIdentificacionDto
    {
        Id = tipo.Id,
        Nombre = tipo.Nombre
    });
});

// POST: /api/TipoIdentificacion
app.MapPost("/api/TipoIdentificacion", async (TipoIdentificacionCreateDto dto, ApplicationDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(dto.Nombre))
        return Results.BadRequest(new { error = "El nombre es requerido" });

    var exists = await db.TiposIdentificacion.AnyAsync(t => t.Nombre == dto.Nombre.Trim());
    if (exists)
        return Results.BadRequest(new { error = "Ya existe un tipo de identificación con ese nombre" });

    var tipo = new TipoIdentificacion { Nombre = dto.Nombre.Trim() };
    db.TiposIdentificacion.Add(tipo);
    await db.SaveChangesAsync();

    return Results.Created($"/api/TipoIdentificacion/{tipo.Id}", new TipoIdentificacionDto
    {
        Id = tipo.Id,
        Nombre = tipo.Nombre
    });
});

// PUT: /api/TipoIdentificacion/{id}
app.MapPut("/api/TipoIdentificacion/{id}", async (int id, TipoIdentificacionUpdateDto dto, ApplicationDbContext db) =>
{
    var tipo = await db.TiposIdentificacion.FindAsync(id);
    if (tipo == null)
        return Results.NotFound(new { message = "Tipo de identificación no encontrado" });

    if (string.IsNullOrWhiteSpace(dto.Nombre))
        return Results.BadRequest(new { error = "El nombre es requerido" });

    var exists = await db.TiposIdentificacion.AnyAsync(t => t.Nombre == dto.Nombre.Trim() && t.Id != id);
    if (exists)
        return Results.BadRequest(new { error = "Ya existe otro tipo de identificación con ese nombre" });

    tipo.Nombre = dto.Nombre.Trim();
    await db.SaveChangesAsync();

    return Results.Ok(new TipoIdentificacionDto
    {
        Id = tipo.Id,
        Nombre = tipo.Nombre
    });
});

// DELETE: /api/TipoIdentificacion/{id}
app.MapDelete("/api/TipoIdentificacion/{id}", async (int id, ApplicationDbContext db) =>
{
    var tipo = await db.TiposIdentificacion.FindAsync(id);
    if (tipo == null)
        return Results.NotFound(new { message = "Tipo de identificación no encontrado" });

    db.TiposIdentificacion.Remove(tipo);
    await db.SaveChangesAsync();

    return Results.NoContent();
});

// GET: /api/TipoIdentificacion/exists/{id}
app.MapGet("/api/TipoIdentificacion/exists/{id}", async (int id, ApplicationDbContext db) =>
{
    var exists = await db.TiposIdentificacion.AnyAsync(t => t.Id == id);
    return Results.Ok(new { exists });
});

// GET: /api/TipoIdentificacion/exists/nombre/{nombre}
app.MapGet("/api/TipoIdentificacion/exists/nombre/{nombre}", async (string nombre, int? excludeId, ApplicationDbContext db) =>
{
    var query = db.TiposIdentificacion.Where(t => t.Nombre == nombre);
    if (excludeId.HasValue)
    {
        query = query.Where(t => t.Id != excludeId.Value);
    }
    var exists = await query.AnyAsync();
    return Results.Ok(new { exists });
});

app.Run();

// ========================================
// ✅ FUNCIÓN PARA VALIDAR TOKEN
// ========================================

bool ValidateToken(string token)
{
    try
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes("TuSuperSecretKeyLarga123456789012345678901234567890");

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = "CUC",
            ValidateAudience = true,
            ValidAudience = "CUCApp",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
        return principal != null;
    }
    catch
    {
        return false;
    }
}