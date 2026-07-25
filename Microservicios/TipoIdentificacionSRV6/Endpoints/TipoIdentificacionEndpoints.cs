using Microsoft.EntityFrameworkCore;
using TipoIdentificacionSRV6.Data;
using TipoIdentificacionSRV6.DTOs;
using TipoIdentificacionSRV6.Entities;
using TipoIdentificacionSRV6.Security;

namespace TipoIdentificacionSRV6.Endpoints
{
    public static class TipoIdentificacionEndpoints
    {
        public static void MapTipoIdentificacionEndpoints(this WebApplication app)
        {
            // Todas las operaciones de /tiposidentificacion requieren un token válido (validado contra LoginSRV1 /validate)
            var group = app.MapGroup("/tiposidentificacion")
                .AddEndpointFilter<TokenValidationFilter>();

            // GET: /tiposidentificacion -> obtener todos
            group.MapGet("/", GetTiposIdentificacion);

            // GET: /tiposidentificacion/{id} -> obtener por llave primaria
            group.MapGet("/{id:int}", GetTipoIdentificacionById);

            // POST: /tiposidentificacion -> crear
            group.MapPost("/", CreateTipoIdentificacion);

            // PUT: /tiposidentificacion/{id} -> modificar
            group.MapPut("/{id:int}", UpdateTipoIdentificacion);

            // DELETE: /tiposidentificacion/{id} -> eliminar
            group.MapDelete("/{id:int}", DeleteTipoIdentificacion);
        }

        // ========================================
        // HANDLERS
        // ========================================

        private static async Task<IResult> GetTiposIdentificacion(ApplicationDbContext db)
        {
            var tipos = await db.TiposIdentificacion
                .OrderBy(t => t.Nombre)
                .Select(t => new TipoIdentificacionDto
                {
                    Id = t.Id,
                    Nombre = t.Nombre
                })
                .ToListAsync();

            return Results.Ok(tipos);
        }

        private static async Task<IResult> GetTipoIdentificacionById(int id, ApplicationDbContext db)
        {
            var tipo = await db.TiposIdentificacion.FindAsync(id);
            if (tipo == null)
                return Results.NotFound(new { message = "Tipo de identificación no encontrado" });

            return Results.Ok(new TipoIdentificacionDto
            {
                Id = tipo.Id,
                Nombre = tipo.Nombre
            });
        }

        private static async Task<IResult> CreateTipoIdentificacion(TipoIdentificacionCreateDto dto, ApplicationDbContext db)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre))
                return Results.BadRequest(new { message = "El nombre es requerido y no puede estar en blanco" });

            var nombre = dto.Nombre.Trim();

            var exists = await db.TiposIdentificacion.AnyAsync(t => t.Nombre == nombre);
            if (exists)
                return Results.Conflict(new { message = "Ya existe un tipo de identificación con ese nombre" });

            var tipo = new TipoIdentificacion
            {
                Nombre = nombre
            };

            db.TiposIdentificacion.Add(tipo);
            await db.SaveChangesAsync();

            return Results.Created($"/tiposidentificacion/{tipo.Id}", new TipoIdentificacionDto
            {
                Id = tipo.Id,
                Nombre = tipo.Nombre
            });
        }

        private static async Task<IResult> UpdateTipoIdentificacion(int id, TipoIdentificacionUpdateDto dto, ApplicationDbContext db)
        {
            var tipo = await db.TiposIdentificacion.FindAsync(id);
            if (tipo == null)
                return Results.NotFound(new { message = "Tipo de identificación no encontrado" });

            if (string.IsNullOrWhiteSpace(dto.Nombre))
                return Results.BadRequest(new { message = "El nombre es requerido y no puede estar en blanco" });

            var nombre = dto.Nombre.Trim();

            var exists = await db.TiposIdentificacion.AnyAsync(t => t.Nombre == nombre && t.Id != id);
            if (exists)
                return Results.Conflict(new { message = "Ya existe otro tipo de identificación con ese nombre" });

            tipo.Nombre = nombre;
            await db.SaveChangesAsync();

            return Results.Ok(new TipoIdentificacionDto
            {
                Id = tipo.Id,
                Nombre = tipo.Nombre
            });
        }

        private static async Task<IResult> DeleteTipoIdentificacion(int id, ApplicationDbContext db)
        {
            var tipo = await db.TiposIdentificacion.FindAsync(id);
            if (tipo == null)
                return Results.NotFound(new { message = "Tipo de identificación no encontrado" });

            db.TiposIdentificacion.Remove(tipo);
            await db.SaveChangesAsync();

            return Results.NoContent();
        }
    }
}
