using Microsoft.EntityFrameworkCore;
using UsuariosSRV4.Data;
using UsuariosSRV4.Entities;

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

// ============================================================
// ✅ ENDPOINTS DE USUARIOS
// ============================================================

// 1️⃣ OBTENER USUARIO POR EMAIL Y TIPO
app.MapGet("/api/Usuarios/por-email-tipo", async (string email, string tipo, ApplicationDbContext db) =>
{
    Console.WriteLine($"=== USUARIO ENCONTRADO ===");
    Console.WriteLine($"Email: {email}");
    Console.WriteLine($"Tipo: {tipo}");

    var usuario = await db.Usuarios
        .Include(u => u.TipoUsuario)
        .FirstOrDefaultAsync(u => u.Email == email && u.TipoUsuario.Nombre == tipo);

    if (usuario == null)
    {
        Console.WriteLine($"❌ Usuario no encontrado: {email}");
        return Results.NotFound(new { message = "Usuario no encontrado" });
    }

    Console.WriteLine($"✅ Usuario encontrado: {email}");
    Console.WriteLine($"Contrasena (hash): '{usuario.Contrasena}'");
    Console.WriteLine($"Longitud: {usuario.Contrasena?.Length ?? 0}");
    Console.WriteLine($"¿Empieza con $2a$? {usuario.Contrasena?.StartsWith("$2a$") ?? false}");

    return Results.Ok(new
    {
        usuario.Id,
        usuario.Email,
        PasswordHash = usuario.Contrasena,
        usuario.NombreCompleto,
        Tipo = usuario.TipoUsuario?.Nombre ?? string.Empty,
        usuario.TipoUsuarioId,
        usuario.RolId,
        Activo = usuario.EstadoId == 1,
        usuario.Bloqueado,
        usuario.IntentosFallidos
    });
});

// 2️⃣ OBTENER USUARIO POR EMAIL
app.MapGet("/api/Usuarios/por-email", async (string email, ApplicationDbContext db) =>
{
    var usuario = await db.Usuarios
        .Include(u => u.TipoUsuario)
        .FirstOrDefaultAsync(u => u.Email == email);

    if (usuario == null)
        return Results.NotFound(new { message = "Usuario no encontrado" });

    return Results.Ok(new
    {
        usuario.Id,
        usuario.Email,
        PasswordHash = usuario.Contrasena,
        usuario.NombreCompleto,
        Tipo = usuario.TipoUsuario?.Nombre ?? string.Empty,
        usuario.TipoUsuarioId,
        usuario.RolId,
        Activo = usuario.EstadoId == 1,
        usuario.Bloqueado,
        usuario.IntentosFallidos
    });
});

// 3️⃣ VERIFICAR SI USUARIO EXISTE
app.MapGet("/api/Usuarios/existe", async (string email, ApplicationDbContext db) =>
{
    var existe = await db.Usuarios
        .AnyAsync(u => u.Email == email);

    return Results.Ok(new { Existe = existe });
});

// 4️⃣ OBTENER INTENTOS FALLIDOS
app.MapGet("/api/Usuarios/intentos-fallidos", async (string email, ApplicationDbContext db) =>
{
    var usuario = await db.Usuarios
        .FirstOrDefaultAsync(u => u.Email == email);

    if (usuario == null)
        return Results.NotFound(new { message = "Usuario no encontrado" });

    return Results.Ok(new { IntentosFallidos = usuario.IntentosFallidos });
});

// 5️⃣ INCREMENTAR INTENTOS FALLIDOS
app.MapPost("/api/Usuarios/incrementar-intentos", async (string email, ApplicationDbContext db) =>
{
    var usuario = await db.Usuarios
        .FirstOrDefaultAsync(u => u.Email == email);

    if (usuario == null)
        return Results.NotFound(new { message = "Usuario no encontrado" });

    usuario.IntentosFallidos++;

    if (usuario.IntentosFallidos >= 3)
    {
        usuario.Bloqueado = true;
        usuario.FechaBloqueo = DateTime.UtcNow;
    }

    await db.SaveChangesAsync();

    return Results.Ok(new
    {
        IntentosFallidos = usuario.IntentosFallidos,
        Bloqueado = usuario.Bloqueado
    });
});

// 6️⃣ RESETEAR INTENTOS FALLIDOS
app.MapPost("/api/Usuarios/resetear-intentos", async (string email, ApplicationDbContext db) =>
{
    var usuario = await db.Usuarios
        .FirstOrDefaultAsync(u => u.Email == email);

    if (usuario == null)
        return Results.NotFound(new { message = "Usuario no encontrado" });

    usuario.IntentosFallidos = 0;
    await db.SaveChangesAsync();

    return Results.Ok(new { message = "Intentos reseteados correctamente" });
});

// 7️⃣ BLOQUEAR USUARIO
app.MapPost("/api/Usuarios/bloquear", async (string email, ApplicationDbContext db) =>
{
    var usuario = await db.Usuarios
        .FirstOrDefaultAsync(u => u.Email == email);

    if (usuario == null)
        return Results.NotFound(new { message = "Usuario no encontrado" });

    usuario.Bloqueado = true;
    usuario.FechaBloqueo = DateTime.UtcNow;
    await db.SaveChangesAsync();

    return Results.Ok(new { message = "Usuario bloqueado correctamente" });
});

// 8️⃣ CREAR USUARIO
app.MapPost("/api/Usuarios/crear", async (CrearUsuarioRequest request, ApplicationDbContext db) =>
{
    var usuario = new Usuario
    {
        Email = request.Email,
        Contrasena = request.PasswordHash,
        NombreCompleto = request.NombreCompleto,
        TipoUsuarioId = request.TipoUsuarioId ?? 0,
        TipoIdentificacionId = request.TipoIdentificacionId ?? 0,
        RolId = request.RolId ?? 0,
        EstadoId = 1,
        Confirmado = false,
        IntentosFallidos = 0,
        Bloqueado = false,
        FechaCreacion = DateTime.UtcNow
    };

    db.Usuarios.Add(usuario);
    await db.SaveChangesAsync();

    return Results.Ok(new
    {
        success = true,
        usuario.Id,
        usuario.Email,
        usuario.NombreCompleto
    });
});

// 9️⃣ GUARDAR REFRESH TOKEN
app.MapPost("/api/Usuarios/guardar-refresh-token", async (RefreshTokenRequest request, ApplicationDbContext db) =>
{
    var usuario = await db.Usuarios
        .FirstOrDefaultAsync(u => u.Id == request.UsuarioId);

    if (usuario == null)
        return Results.NotFound(new { message = "Usuario no encontrado" });

    usuario.RefreshToken = request.RefreshToken;
    usuario.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1);
    await db.SaveChangesAsync();

    return Results.Ok(new { message = "Refresh token guardado correctamente" });
});

// 🔟 VALIDAR REFRESH TOKEN
app.MapGet("/api/Usuarios/validar-refresh-token", async (string refreshToken, ApplicationDbContext db) =>
{
    var usuario = await db.Usuarios
        .Include(u => u.TipoUsuario)
        .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

    if (usuario == null)
        return Results.NotFound(new { message = "Refresh token inválido" });

    if (usuario.RefreshTokenExpiryTime < DateTime.UtcNow)
        return Results.BadRequest(new { message = "Refresh token expirado" });

    return Results.Ok(new
    {
        usuario.Id,
        usuario.Email,
        PasswordHash = usuario.Contrasena,
        usuario.NombreCompleto,
        Tipo = usuario.TipoUsuario?.Nombre ?? string.Empty,
        usuario.TipoUsuarioId,
        usuario.RolId,
        Activo = usuario.EstadoId == 1,
        usuario.Bloqueado,
        usuario.IntentosFallidos
    });
});

// 1️⃣1️⃣ ACTUALIZAR REFRESH TOKEN
app.MapPut("/api/Usuarios/actualizar-refresh-token", async (RefreshTokenRequest request, ApplicationDbContext db) =>
{
    var usuario = await db.Usuarios
        .FirstOrDefaultAsync(u => u.Id == request.UsuarioId);

    if (usuario == null)
        return Results.NotFound(new { message = "Usuario no encontrado" });

    usuario.RefreshToken = request.RefreshToken;
    usuario.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1);
    await db.SaveChangesAsync();

    return Results.Ok(new { message = "Refresh token actualizado correctamente" });
});

// 1️⃣2️⃣ ELIMINAR REFRESH TOKEN
app.MapDelete("/api/Usuarios/eliminar-refresh-token", async (string refreshToken, ApplicationDbContext db) =>
{
    var usuario = await db.Usuarios
        .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

    if (usuario == null)
        return Results.NotFound(new { message = "Refresh token no encontrado" });

    usuario.RefreshToken = null;
    usuario.RefreshTokenExpiryTime = null;
    await db.SaveChangesAsync();

    return Results.Ok(new { message = "Refresh token eliminado correctamente" });
});

// 1️⃣3️⃣ DEBUG HASH
app.MapGet("/api/Usuarios/debug-hash", async (string email, ApplicationDbContext db) =>
{
    var usuario = await db.Usuarios
        .FirstOrDefaultAsync(u => u.Email == email);

    if (usuario == null)
        return Results.NotFound(new { message = "Usuario no encontrado" });

    return Results.Ok(new
    {
        usuario.Email,
        HashAlmacenado = usuario.Contrasena,
        LongitudHash = usuario.Contrasena?.Length ?? 0,
        EmpiezaConDolar = usuario.Contrasena?.StartsWith("$2a$") ?? false,
        IntentosFallidos = usuario.IntentosFallidos,
        Bloqueado = usuario.Bloqueado
    });
});

// ============================================================
// ✅ MODELOS PARA REQUESTS
// ============================================================

app.Run();

public class CrearUsuarioRequest
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public int? TipoUsuarioId { get; set; }
    public int? TipoIdentificacionId { get; set; }
    public int? RolId { get; set; }
}

public class RefreshTokenRequest
{
    public int UsuarioId { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
}