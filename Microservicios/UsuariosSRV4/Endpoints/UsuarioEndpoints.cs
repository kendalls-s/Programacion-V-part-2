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

            // ✅ ENDPOINT DE VALIDACIÓN - ESTE ES EL QUE FALTA
            group.MapPost("/validar-credenciales", ValidarCredencialesAsync);

            // ✅ OTROS ENDPOINTS
            group.MapGet("/", GetAllAsync);
            group.MapGet("/{id}", GetByIdAsync);
            group.MapPost("/", CreateAsync);
            group.MapPut("/{id}", UpdateAsync);
            group.MapDelete("/{id}", DeleteAsync);
        }

        // ✅ POST: /api/Usuarios/validar-credenciales
        private static async Task<IResult> ValidarCredencialesAsync(
            [FromBody] ValidarCredencialesRequest request,
            IUsuarioService service,
            ILogger<Program> logger)
        {
            logger.LogInformation($"=== VALIDAR CREDENCIALES ===");
            logger.LogInformation($"Email: {request.Email}");
            logger.LogInformation($"Password: {request.Password}");
            logger.LogInformation($"Tipo: {request.Tipo}");

            try
            {
                var (ok, error, data) = await service.ValidarCredencialesAsync(
                    request.Email,
                    request.Password,
                    request.Tipo);

                logger.LogInformation($"Resultado: ok={ok}, error={error}");

                if (!ok)
                {
                    return Results.BadRequest(new { message = error ?? "Credenciales inválidas" });
                }

                if (data == null)
                {
                    return Results.BadRequest(new { message = "Error al obtener datos del usuario" });
                }

                logger.LogInformation($"Validación exitosa para: {request.Email}");
                return Results.Ok(data);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error en ValidarCredencialesAsync: {request.Email}");
                return Results.BadRequest(new { message = $"Error: {ex.Message}" });
            }
        }

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