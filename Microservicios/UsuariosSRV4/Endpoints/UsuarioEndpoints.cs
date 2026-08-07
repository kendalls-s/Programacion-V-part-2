using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UsuariosSRV4.Data;
using UsuariosSRV4.DTOs;
using UsuariosSRV4.Entities;
using UsuariosSRV4.Services;

namespace UsuariosSRV4.Endpoints
{
    public static class UsuarioEndpoints
    {
        public static void MapUsuarioEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/Usuarios");

            // ============================================================
            // ✅ ENDPOINTS PARA LOGINSRV1
            // ============================================================

            // GET: /api/Usuarios/por-email-tipo
            group.MapGet("/por-email-tipo", ObtenerPorEmailTipoAsync);

            // GET: /api/Usuarios/por-email
            group.MapGet("/por-email", ObtenerPorEmailAsync);

            // GET: /api/Usuarios/existe
            group.MapGet("/existe", VerificarExistenciaAsync);

            // GET: /api/Usuarios/intentos-fallidos
            group.MapGet("/intentos-fallidos", ObtenerIntentosFallidosAsync);

            // POST: /api/Usuarios/incrementar-intentos
            group.MapPost("/incrementar-intentos", IncrementarIntentosFallidosAsync);

            // POST: /api/Usuarios/resetear-intentos
            group.MapPost("/resetear-intentos", ResetearIntentosFallidosAsync);

            // POST: /api/Usuarios/bloquear
            group.MapPost("/bloquear", BloquearUsuarioAsync);

            // POST: /api/Usuarios/crear
            group.MapPost("/crear", CrearUsuarioDesdeLoginAsync);

            // POST: /api/Usuarios/guardar-refresh-token
            group.MapPost("/guardar-refresh-token", GuardarRefreshTokenAsync);

            // GET: /api/Usuarios/validar-refresh-token
            group.MapGet("/validar-refresh-token", ValidarRefreshTokenAsync);

            // PUT: /api/Usuarios/actualizar-refresh-token
            group.MapPut("/actualizar-refresh-token", ActualizarRefreshTokenAsync);

            // DELETE: /api/Usuarios/eliminar-refresh-token
            group.MapDelete("/eliminar-refresh-token", EliminarRefreshTokenAsync);

            // GET: /api/Usuarios/debug-hash
            group.MapGet("/debug-hash", DebugHashAsync);

            // ============================================================
            // ✅ ENDPOINTS CRUD ORIGINALES
            // ============================================================

            // POST: /api/Usuarios/validar-credenciales
            group.MapPost("/validar-credenciales", ValidarCredencialesAsync);

            // GET: /api/Usuarios
            group.MapGet("/", GetAllAsync);

            // GET: /api/Usuarios/{id}
            group.MapGet("/{id}", GetByIdAsync);

            // POST: /api/Usuarios
            group.MapPost("/", CreateAsync);

            // PUT: /api/Usuarios/{id}
            group.MapPut("/{id}", UpdateAsync);

            // DELETE: /api/Usuarios/{id}
            group.MapDelete("/{id}", DeleteAsync);
        }

        // ============================================================
        // ✅ GET: /api/Usuarios/por-email-tipo
        // ============================================================
        private static async Task<IResult> ObtenerPorEmailTipoAsync(
            string email,
            string tipo,
            ApplicationDbContext db,
            ILogger<Program> logger)
        {
            logger.LogInformation($"=== USUARIOSSRV4 - BUSCANDO USUARIO ===");
            logger.LogInformation($"Email: {email}");
            logger.LogInformation($"Tipo: {tipo}");

            var usuario = await db.Usuarios
                .Include(u => u.TipoUsuario)
                .FirstOrDefaultAsync(u => u.Email == email && u.TipoUsuario.Nombre == tipo);

            if (usuario == null)
            {
                logger.LogWarning($"❌ Usuario no encontrado: {email}");
                return Results.NotFound(new { message = "Usuario no encontrado" });
            }

            logger.LogInformation($"✅ Usuario encontrado: {email}");
            logger.LogInformation($"Hash en BD (Contrasena): '{usuario.Contrasena}'");
            logger.LogInformation($"Longitud: {usuario.Contrasena?.Length ?? 0}");
            logger.LogInformation($"¿Empieza con $2a$? {usuario.Contrasena?.StartsWith("$2a$") ?? false}");

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
                usuario.IntentosFallidos,
                usuario.Fotografia
            });
        }

        // ============================================================
        // ✅ GET: /api/Usuarios/por-email
        // ============================================================
        private static async Task<IResult> ObtenerPorEmailAsync(
            string email,
            ApplicationDbContext db,
            ILogger<Program> logger)
        {
            logger.LogInformation($"=== USUARIOSSRV4 - BUSCANDO POR EMAIL ===");
            logger.LogInformation($"Email: {email}");

            var usuario = await db.Usuarios
                .Include(u => u.TipoUsuario)
                .FirstOrDefaultAsync(u => u.Email == email);

            if (usuario == null)
            {
                logger.LogWarning($"❌ Usuario no encontrado: {email}");
                return Results.NotFound(new { message = "Usuario no encontrado" });
            }

            logger.LogInformation($"✅ Usuario encontrado: {email}");

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
        }

        // ============================================================
        // ✅ GET: /api/Usuarios/existe
        // ============================================================
        private static async Task<IResult> VerificarExistenciaAsync(
            string email,
            ApplicationDbContext db)
        {
            var existe = await db.Usuarios.AnyAsync(u => u.Email == email);
            return Results.Ok(new { Existe = existe });
        }

        // ============================================================
        // ✅ GET: /api/Usuarios/intentos-fallidos
        // ============================================================
        private static async Task<IResult> ObtenerIntentosFallidosAsync(
            string email,
            ApplicationDbContext db)
        {
            var usuario = await db.Usuarios
                .FirstOrDefaultAsync(u => u.Email == email);

            if (usuario == null)
                return Results.NotFound(new { message = "Usuario no encontrado" });

            return Results.Ok(new { IntentosFallidos = usuario.IntentosFallidos });
        }

        // ============================================================
        // ✅ POST: /api/Usuarios/incrementar-intentos
        // ============================================================
        private static async Task<IResult> IncrementarIntentosFallidosAsync(
            string email,
            ApplicationDbContext db,
            ILogger<Program> logger)
        {
            logger.LogInformation($"=== INCREMENTAR INTENTOS ===");
            logger.LogInformation($"Email: {email}");

            var usuario = await db.Usuarios
                .FirstOrDefaultAsync(u => u.Email == email);

            if (usuario == null)
                return Results.NotFound(new { message = "Usuario no encontrado" });

            usuario.IntentosFallidos++;
            logger.LogInformation($"Intentos fallidos: {usuario.IntentosFallidos}");

            if (usuario.IntentosFallidos >= 3)
            {
                usuario.Bloqueado = true;
                usuario.FechaBloqueo = DateTime.UtcNow;
                logger.LogWarning($"⚠️ Usuario bloqueado: {email}");
            }

            await db.SaveChangesAsync();

            return Results.Ok(new
            {
                IntentosFallidos = usuario.IntentosFallidos,
                Bloqueado = usuario.Bloqueado
            });
        }

        // ============================================================
        // ✅ POST: /api/Usuarios/resetear-intentos
        // ============================================================
        private static async Task<IResult> ResetearIntentosFallidosAsync(
            string email,
            ApplicationDbContext db,
            ILogger<Program> logger)
        {
            logger.LogInformation($"=== RESETEAR INTENTOS ===");
            logger.LogInformation($"Email: {email}");

            var usuario = await db.Usuarios
                .FirstOrDefaultAsync(u => u.Email == email);

            if (usuario == null)
                return Results.NotFound(new { message = "Usuario no encontrado" });

            usuario.IntentosFallidos = 0;
            await db.SaveChangesAsync();

            logger.LogInformation($"✅ Intentos reseteados para: {email}");

            return Results.Ok(new { message = "Intentos reseteados correctamente" });
        }

        // ============================================================
        // ✅ POST: /api/Usuarios/bloquear
        // ============================================================
        private static async Task<IResult> BloquearUsuarioAsync(
            string email,
            ApplicationDbContext db,
            ILogger<Program> logger)
        {
            logger.LogInformation($"=== BLOQUEAR USUARIO ===");
            logger.LogInformation($"Email: {email}");

            var usuario = await db.Usuarios
                .FirstOrDefaultAsync(u => u.Email == email);

            if (usuario == null)
                return Results.NotFound(new { message = "Usuario no encontrado" });

            usuario.Bloqueado = true;
            usuario.FechaBloqueo = DateTime.UtcNow;
            await db.SaveChangesAsync();

            logger.LogWarning($"⚠️ Usuario bloqueado: {email}");

            return Results.Ok(new { message = "Usuario bloqueado correctamente" });
        }

        // ============================================================
        // ✅ POST: /api/Usuarios/crear
        // ============================================================
        private static async Task<IResult> CrearUsuarioDesdeLoginAsync(
            [FromBody] CrearUsuarioRequest request,
            ApplicationDbContext db,
            ILogger<Program> logger)
        {
            logger.LogInformation($"=== CREAR USUARIO DESDE LOGIN ===");
            logger.LogInformation($"Email: {request.Email}");
            logger.LogInformation($"PasswordHash: {request.PasswordHash?.Substring(0, Math.Min(20, request.PasswordHash?.Length ?? 0))}...");

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

            logger.LogInformation($"✅ Usuario creado: {request.Email} (ID: {usuario.Id})");

            return Results.Ok(new
            {
                success = true,
                usuario.Id,
                usuario.Email,
                usuario.NombreCompleto
            });
        }

        // ============================================================
        // ✅ POST: /api/Usuarios/guardar-refresh-token
        // ============================================================
        private static async Task<IResult> GuardarRefreshTokenAsync(
            [FromBody] RefreshTokenRequest request,
            ApplicationDbContext db,
            ILogger<Program> logger)
        {
            logger.LogInformation($"=== GUARDAR REFRESH TOKEN ===");
            logger.LogInformation($"UsuarioId: {request.UsuarioId}");

            var usuario = await db.Usuarios
                .FirstOrDefaultAsync(u => u.Id == request.UsuarioId);

            if (usuario == null)
                return Results.NotFound(new { message = "Usuario no encontrado" });

            usuario.RefreshToken = request.RefreshToken;
            usuario.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1);
            await db.SaveChangesAsync();

            logger.LogInformation($"✅ Refresh token guardado para usuario: {usuario.Email}");

            return Results.Ok(new { message = "Refresh token guardado correctamente" });
        }

        // ============================================================
        // ✅ GET: /api/Usuarios/validar-refresh-token
        // ============================================================
        private static async Task<IResult> ValidarRefreshTokenAsync(
            string refreshToken,
            ApplicationDbContext db,
            ILogger<Program> logger)
        {
            logger.LogInformation($"=== VALIDAR REFRESH TOKEN ===");

            var usuario = await db.Usuarios
                .Include(u => u.TipoUsuario)
                .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

            if (usuario == null)
            {
                logger.LogWarning($"❌ Refresh token inválido");
                return Results.NotFound(new { message = "Refresh token inválido" });
            }

            if (usuario.RefreshTokenExpiryTime < DateTime.UtcNow)
            {
                logger.LogWarning($"❌ Refresh token expirado para: {usuario.Email}");
                return Results.BadRequest(new { message = "Refresh token expirado" });
            }

            logger.LogInformation($"✅ Refresh token válido para: {usuario.Email}");

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
        }

        // ============================================================
        // ✅ PUT: /api/Usuarios/actualizar-refresh-token
        // ============================================================
        private static async Task<IResult> ActualizarRefreshTokenAsync(
            [FromBody] RefreshTokenRequest request,
            ApplicationDbContext db,
            ILogger<Program> logger)
        {
            logger.LogInformation($"=== ACTUALIZAR REFRESH TOKEN ===");
            logger.LogInformation($"UsuarioId: {request.UsuarioId}");

            var usuario = await db.Usuarios
                .FirstOrDefaultAsync(u => u.Id == request.UsuarioId);

            if (usuario == null)
                return Results.NotFound(new { message = "Usuario no encontrado" });

            usuario.RefreshToken = request.RefreshToken;
            usuario.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1);
            await db.SaveChangesAsync();

            logger.LogInformation($"✅ Refresh token actualizado para: {usuario.Email}");

            return Results.Ok(new { message = "Refresh token actualizado correctamente" });
        }

        // ============================================================
        // ✅ DELETE: /api/Usuarios/eliminar-refresh-token
        // ============================================================
        private static async Task<IResult> EliminarRefreshTokenAsync(
            string refreshToken,
            ApplicationDbContext db,
            ILogger<Program> logger)
        {
            logger.LogInformation($"=== ELIMINAR REFRESH TOKEN ===");

            var usuario = await db.Usuarios
                .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

            if (usuario == null)
                return Results.NotFound(new { message = "Refresh token no encontrado" });

            usuario.RefreshToken = null;
            usuario.RefreshTokenExpiryTime = null;
            await db.SaveChangesAsync();

            logger.LogInformation($"✅ Refresh token eliminado para: {usuario.Email}");

            return Results.Ok(new { message = "Refresh token eliminado correctamente" });
        }

        // ============================================================
        // ✅ GET: /api/Usuarios/debug-hash
        // ============================================================
        private static async Task<IResult> DebugHashAsync(
            string email,
            ApplicationDbContext db)
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
        }

        // ============================================================
        // ✅ POST: /api/Usuarios/validar-credenciales
        // ============================================================
        private static async Task<IResult> ValidarCredencialesAsync(
            [FromBody] ValidarCredencialesRequest request,
            IUsuarioService service,
            ILogger<Program> logger)
        {
            logger.LogInformation($"=== VALIDAR CREDENCIALES ===");
            logger.LogInformation($"Email: {request.Email}");
            logger.LogInformation($"Tipo seleccionado: {request.Tipo}");

            try
            {
                var (ok, error, data) = await service.ValidarCredencialesAsync(
                    request.Email,
                    request.Password,
                    request.Tipo);

                logger.LogInformation($"Resultado: ok={ok}, error={error}");

                if (!ok)
                {
                    return Results.BadRequest(new { message = error ?? "Usuario y/o contraseña incorrectos" });
                }

                if (data == null)
                {
                    return Results.BadRequest(new { message = "Usuario y/o contraseña incorrectos" });
                }

                logger.LogInformation($"✅ Validación exitosa para: {request.Email} - Tipo: {data.TipoUsuario}");
                return Results.Ok(data);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error en ValidarCredencialesAsync: {request.Email}");
                return Results.BadRequest(new { message = "Usuario y/o contraseña incorrectos" });
            }
        }

        // ============================================================
        // ✅ GET: /api/Usuarios
        // ============================================================
        private static async Task<IResult> GetAllAsync(IUsuarioService service)
        {
            try
            {
                var (ok, error, data) = await service.GetAllAsync();
                if (!ok) return Results.BadRequest(new { error });
                return Results.Ok(data);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }

        // ============================================================
        // ✅ GET: /api/Usuarios/{id}
        // ============================================================
        private static async Task<IResult> GetByIdAsync(int id, IUsuarioService service)
        {
            try
            {
                var (ok, error, data) = await service.GetByIdAsync(id);
                if (!ok) return Results.NotFound(new { error });
                return Results.Ok(data);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }

        // ============================================================
        // ✅ POST: /api/Usuarios
        // ============================================================
        private static async Task<IResult> CreateAsync([FromBody] CrearUsuarioDto dto, IUsuarioService service)
        {
            try
            {
                var (ok, error, data) = await service.CreateAsync(dto);
                if (!ok) return Results.BadRequest(new { error });
                return Results.Created($"/api/Usuarios/{data.Id}", data);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }

        // ============================================================
        // ✅ PUT: /api/Usuarios/{id}
        // ============================================================
        private static async Task<IResult> UpdateAsync(int id, [FromBody] ActualizarUsuarioDto dto, IUsuarioService service)
        {
            try
            {
                if (id != dto.Id) return Results.BadRequest(new { error = "El ID no coincide" });
                var (ok, error, data) = await service.UpdateAsync(id, dto);
                if (!ok) return Results.BadRequest(new { error });
                return Results.Ok(data);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }

        // ============================================================
        // ✅ DELETE: /api/Usuarios/{id}
        // ============================================================
        private static async Task<IResult> DeleteAsync(int id, IUsuarioService service)
        {
            try
            {
                var (ok, error) = await service.DeleteAsync(id);
                if (!ok) return Results.BadRequest(new { error });
                return Results.Ok(new { message = "Usuario eliminado correctamente" });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }
    }

    // ============================================================
    // ✅ DTOs PARA REQUESTS
    // ============================================================

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
}