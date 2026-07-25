using Microsoft.EntityFrameworkCore;
using TiposUsuarioSRV5.Data;
using TiposUsuarioSRV5.DTOs;
using TiposUsuarioSRV5.Entities;
using TiposUsuarioSRV5.Security;

namespace TiposUsuarioSRV5.Endpoints
{
    public static class TipoUsuarioEndpoints
    {
        public static void MapTipoUsuarioEndpoints(this WebApplication app)
        {
            // Todas las operaciones de /tiposusuario requieren un token válido (validado contra LoginSRV1 /validate)
            var group = app.MapGroup("/tiposusuario")
                .AddEndpointFilter<TokenValidationFilter>();

            // GET: /tiposusuario -> obtener todos
            group.MapGet("/", GetTiposUsuario);

            // GET: /tiposusuario/{id} -> obtener por llave primaria
            group.MapGet("/{id:int}", GetTipoUsuarioById);

            // POST: /tiposusuario -> crear
            group.MapPost("/", CreateTipoUsuario);

            // PUT: /tiposusuario/{id} -> modificar
            group.MapPut("/{id:int}", UpdateTipoUsuario);

            // DELETE: /tiposusuario/{id} -> eliminar
            group.MapDelete("/{id:int}", DeleteTipoUsuario);
        }

        // ========================================
        // HANDLERS
        // ========================================

        private static async Task<IResult> GetTiposUsuario(ApplicationDbContext db)
        {
            var tipos = await db.TiposUsuario
                .OrderBy(t => t.Nombre)
                .Select(t => new TipoUsuarioDto
                {
                    Id = t.Id,
                    Nombre = t.Nombre
                })
                .ToListAsync();

            return Results.Ok(tipos);
        }

        private static async Task<IResult> GetTipoUsuarioById(int id, ApplicationDbContext db)
        {
            var tipo = await db.TiposUsuario.FindAsync(id);
            if (tipo == null)
                return Results.NotFound(new { message = "Tipo de usuario no encontrado" });

            return Results.Ok(new TipoUsuarioDto
            {
                Id = tipo.Id,
                Nombre = tipo.Nombre
            });
        }

        private static async Task<IResult> CreateTipoUsuario(TipoUsuarioCreateDto dto, ApplicationDbContext db)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre))
                return Results.BadRequest(new { message = "El nombre es requerido y no puede estar en blanco" });

            var nombre = dto.Nombre.Trim();

            var exists = await db.TiposUsuario.AnyAsync(t => t.Nombre == nombre);
            if (exists)
                return Results.Conflict(new { message = "Ya existe un tipo de usuario con ese nombre" });

            var tipo = new TipoUsuario
            {
                Nombre = nombre
            };

            db.TiposUsuario.Add(tipo);
            await db.SaveChangesAsync();

            return Results.Created($"/tiposusuario/{tipo.Id}", new TipoUsuarioDto
            {
                Id = tipo.Id,
                Nombre = tipo.Nombre
            });
        }

        private static async Task<IResult> UpdateTipoUsuario(int id, TipoUsuarioUpdateDto dto, ApplicationDbContext db)
        {
            var tipo = await db.TiposUsuario.FindAsync(id);
            if (tipo == null)
                return Results.NotFound(new { message = "Tipo de usuario no encontrado" });

            if (string.IsNullOrWhiteSpace(dto.Nombre))
                return Results.BadRequest(new { message = "El nombre es requerido y no puede estar en blanco" });

            var nombre = dto.Nombre.Trim();

            var exists = await db.TiposUsuario.AnyAsync(t => t.Nombre == nombre && t.Id != id);
            if (exists)
                return Results.Conflict(new { message = "Ya existe otro tipo de usuario con ese nombre" });

            tipo.Nombre = nombre;
            await db.SaveChangesAsync();

            return Results.Ok(new TipoUsuarioDto
            {
                Id = tipo.Id,
                Nombre = tipo.Nombre
            });
        }

        private static async Task<IResult> DeleteTipoUsuario(int id, ApplicationDbContext db)
        {
            var tipo = await db.TiposUsuario.FindAsync(id);
            if (tipo == null)
                return Results.NotFound(new { message = "Tipo de usuario no encontrado" });

            db.TiposUsuario.Remove(tipo);
            await db.SaveChangesAsync();

            return Results.NoContent();
        }
    }
}
