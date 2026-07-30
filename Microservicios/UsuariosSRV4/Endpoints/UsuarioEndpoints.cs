using Microsoft.AspNetCore.Mvc;
using UsuariosSRV4.DTOs;
using UsuariosSRV4.Services;

namespace UsuariosSRV4.Endpoints
{
    public static class UsuarioEndpoints
    {
        public static void MapUsuarioEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/Usuarios");

            // ✅ ENDPOINT DE VALIDACIÓN DE CREDENCIALES (con validación de tipo)
            group.MapPost("/validar-credenciales", ValidarCredencialesAsync);

            // ✅ OTROS ENDPOINTS
            group.MapGet("/", GetAllAsync);
            group.MapGet("/{id}", GetByIdAsync);
            group.MapPost("/", CreateAsync);
            group.MapPut("/{id}", UpdateAsync);
            group.MapDelete("/{id}", DeleteAsync);
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
                // ✅ Validar credenciales incluyendo el tipo de usuario
                var (ok, error, data) = await service.ValidarCredencialesAsync(
                    request.Email,
                    request.Password,
                    request.Tipo);

                logger.LogInformation($"Resultado: ok={ok}, error={error}");

                // ✅ Si no es exitoso, devolver mensaje genérico
                if (!ok)
                {
                    return Results.BadRequest(new { message = error ?? "Usuario y/o contraseña incorrectos" });
                }

                // ✅ Si no hay datos, error genérico
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
}