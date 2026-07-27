using Microsoft.EntityFrameworkCore;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using TiposUsuarioSRV5.Data;
using TiposUsuarioSRV5.DTOs;
using TiposUsuarioSRV5.Entities;

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
    // Rutas públicas (no requieren token)
    var path = context.Request.Path.Value ?? "";
    var publicRoutes = new[] { "/health", "/api/health" };

    if (publicRoutes.Any(r => path.StartsWith(r, StringComparison.OrdinalIgnoreCase)))
    {
        await next();
        return;
    }

    // Si la ruta es Swagger o documentación, pasar
    if (path.Contains("/swagger"))
    {
        await next();
        return;
    }

    // Obtener token del header Authorization
    var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

    if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
    {
        context.Response.StatusCode = 401;
        await context.Response.WriteAsync("Token no proporcionado");
        return;
    }

    var token = authHeader.Substring("Bearer ".Length).Trim();

    // Validar token
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

// GET: /api/TipoUsuario
app.MapGet("/api/TipoUsuario", async (ApplicationDbContext db) =>
{
    var tipos = await db.TiposUsuario
        .OrderBy(t => t.Nombre)
        .ToListAsync();

    return Results.Ok(tipos.Select(t => new TipoUsuarioDto
    {
        Id = t.Id,
        Nombre = t.Nombre
    }));
});

// GET: /api/TipoUsuario/{id}
app.MapGet("/api/TipoUsuario/{id}", async (int id, ApplicationDbContext db) =>
{
    var tipo = await db.TiposUsuario.FindAsync(id);
    if (tipo == null)
        return Results.NotFound(new { message = "Tipo de usuario no encontrado" });

    return Results.Ok(new TipoUsuarioDto
    {
        Id = tipo.Id,
        Nombre = tipo.Nombre
    });
});

// POST: /api/TipoUsuario
app.MapPost("/api/TipoUsuario", async (TipoUsuarioCreateDto dto, ApplicationDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(dto.Nombre))
        return Results.BadRequest(new { error = "El nombre es requerido" });

    var exists = await db.TiposUsuario.AnyAsync(t => t.Nombre == dto.Nombre.Trim());
    if (exists)
        return Results.BadRequest(new { error = "Ya existe un tipo de usuario con ese nombre" });

    var tipo = new TipoUsuario { Nombre = dto.Nombre.Trim() };
    db.TiposUsuario.Add(tipo);
    await db.SaveChangesAsync();

    return Results.Created($"/api/TipoUsuario/{tipo.Id}", new TipoUsuarioDto
    {
        Id = tipo.Id,
        Nombre = tipo.Nombre
    });
});

// PUT: /api/TipoUsuario/{id}
app.MapPut("/api/TipoUsuario/{id}", async (int id, TipoUsuarioUpdateDto dto, ApplicationDbContext db) =>
{
    var tipo = await db.TiposUsuario.FindAsync(id);
    if (tipo == null)
        return Results.NotFound(new { message = "Tipo de usuario no encontrado" });

    if (string.IsNullOrWhiteSpace(dto.Nombre))
        return Results.BadRequest(new { error = "El nombre es requerido" });

    var exists = await db.TiposUsuario.AnyAsync(t => t.Nombre == dto.Nombre.Trim() && t.Id != id);
    if (exists)
        return Results.BadRequest(new { error = "Ya existe otro tipo de usuario con ese nombre" });

    tipo.Nombre = dto.Nombre.Trim();
    await db.SaveChangesAsync();

    return Results.Ok(new TipoUsuarioDto
    {
        Id = tipo.Id,
        Nombre = tipo.Nombre
    });
});

// DELETE: /api/TipoUsuario/{id}
app.MapDelete("/api/TipoUsuario/{id}", async (int id, ApplicationDbContext db) =>
{
    var tipo = await db.TiposUsuario.FindAsync(id);
    if (tipo == null)
        return Results.NotFound(new { message = "Tipo de usuario no encontrado" });

    db.TiposUsuario.Remove(tipo);
    await db.SaveChangesAsync();

    return Results.NoContent();
});

// GET: /api/TipoUsuario/exists/{id}
app.MapGet("/api/TipoUsuario/exists/{id}", async (int id, ApplicationDbContext db) =>
{
    var exists = await db.TiposUsuario.AnyAsync(t => t.Id == id);
    return Results.Ok(new { exists });
});

// GET: /api/TipoUsuario/exists/nombre/{nombre}
app.MapGet("/api/TipoUsuario/exists/nombre/{nombre}", async (string nombre, int? excludeId, ApplicationDbContext db) =>
{
    var query = db.TiposUsuario.Where(t => t.Nombre == nombre);
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