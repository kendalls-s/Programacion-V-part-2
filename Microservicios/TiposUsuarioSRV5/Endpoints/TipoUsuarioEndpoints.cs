using Microsoft.AspNetCore.Mvc;
using TiposUsuarioSRV5.DTOs;
using TiposUsuarioSRV5.Services;

namespace TiposUsuarioSRV5.Endpoints
{
    public static class TipoUsuarioEndpoints
    {
        public static void MapTipoUsuarioEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/TipoUsuario");

            // ✅ GET: /api/TipoUsuario (para combo box y lista)
            group.MapGet("/", GetAllAsync);
            group.MapGet("/{id}", GetByIdAsync);
            group.MapPost("/", CreateAsync);
            group.MapPut("/{id}", UpdateAsync);
            group.MapDelete("/{id}", DeleteAsync);
        }

        // ✅ GET: /api/TipoUsuario - DEVUELVE LISTA PARA COMBO BOX
        private static async Task<IResult> GetAllAsync(ITipoUsuarioService service)
        {
            try
            {
                var (ok, error, data) = await service.GetAllAsync();
                if (!ok) return Results.BadRequest(new { error });
                return Results.Ok(data);  // ✅ Devuelve [{id, nombre}, ...]
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }

        // ✅ GET: /api/TipoUsuario/{id}
        private static async Task<IResult> GetByIdAsync(int id, ITipoUsuarioService service)
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

        // ✅ POST: /api/TipoUsuario
        private static async Task<IResult> CreateAsync([FromBody] TipoUsuarioCreateDto dto, ITipoUsuarioService service)
        {
            try
            {
                var (ok, error, data) = await service.CreateAsync(dto);
                if (!ok) return Results.BadRequest(new { error });
                return Results.Created($"/api/TipoUsuario/{data.Id}", data);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }

        // ✅ PUT: /api/TipoUsuario/{id}
        // ✅ PUT: /api/TipoUsuario/{id}
        private static async Task<IResult> UpdateAsync(
            int id,
            [FromBody] TipoUsuarioUpdateDto dto,
            ITipoUsuarioService service)
        {
            try
            {
                // ✅ Validar que el ID de la URL coincida con el ID del DTO
                if (id != dto.Id)
                {
                    return Results.BadRequest(new { error = "El ID no coincide" });
                }

                var (ok, error, data) = await service.UpdateAsync(id, dto);
                if (!ok) return Results.BadRequest(new { error });
                return Results.Ok(data);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }

        // ✅ DELETE: /api/TipoUsuario/{id}
        private static async Task<IResult> DeleteAsync(int id, ITipoUsuarioService service)
        {
            try
            {
                var (ok, error) = await service.DeleteAsync(id);
                if (!ok) return Results.BadRequest(new { error });
                return Results.Ok(new { message = "Tipo de usuario eliminado correctamente" });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }
    }
}