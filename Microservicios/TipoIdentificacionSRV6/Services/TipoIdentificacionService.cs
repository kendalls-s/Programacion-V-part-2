using Microsoft.EntityFrameworkCore;
using TipoIdentificacionSRV6.Data;
using TipoIdentificacionSRV6.DTOs;
using TipoIdentificacionSRV6.Entities;

namespace TipoIdentificacionSRV6.Services
{
    public class TipoIdentificacionService : ITipoIdentificacionService
    {
        private readonly ApplicationDbContext _db;

        public TipoIdentificacionService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<TipoIdentificacionDto>> ObtenerTodosAsync()
        {
            return await _db.TiposIdentificacion
                .OrderBy(t => t.Nombre)
                .Select(t => new TipoIdentificacionDto
                {
                    Id = t.Id,
                    Nombre = t.Nombre
                })
                .ToListAsync();
        }

        public async Task<TipoIdentificacionDto?> ObtenerPorIdAsync(int id)
        {
            var tipo = await _db.TiposIdentificacion.FindAsync(id);
            if (tipo == null) return null;

            return new TipoIdentificacionDto
            {
                Id = tipo.Id,
                Nombre = tipo.Nombre
            };
        }

        public async Task<(bool ok, string? error, int id)> CrearAsync(TipoIdentificacionCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre))
                return (false, "El nombre es requerido", 0);

            var exists = await _db.TiposIdentificacion.AnyAsync(t => t.Nombre == dto.Nombre.Trim());
            if (exists)
                return (false, "Ya existe un tipo de identificación con ese nombre", 0);

            var tipo = new TipoIdentificacion { Nombre = dto.Nombre.Trim() };
            _db.TiposIdentificacion.Add(tipo);
            await _db.SaveChangesAsync();

            return (true, null, tipo.Id);
        }

        public async Task<(bool ok, string? error)> ActualizarAsync(int id, TipoIdentificacionUpdateDto dto)
        {
            var tipo = await _db.TiposIdentificacion.FindAsync(id);
            if (tipo == null)
                return (false, "Tipo de identificación no encontrado");

            if (string.IsNullOrWhiteSpace(dto.Nombre))
                return (false, "El nombre es requerido");

            var exists = await _db.TiposIdentificacion.AnyAsync(t => t.Nombre == dto.Nombre.Trim() && t.Id != id);
            if (exists)
                return (false, "Ya existe otro tipo de identificación con ese nombre");

            tipo.Nombre = dto.Nombre.Trim();
            await _db.SaveChangesAsync();

            return (true, null);
        }

        public async Task<(bool ok, string? error)> EliminarAsync(int id)
        {
            var tipo = await _db.TiposIdentificacion.FindAsync(id);
            if (tipo == null)
                return (false, "Tipo de identificación no encontrado");

            _db.TiposIdentificacion.Remove(tipo);
            await _db.SaveChangesAsync();

            return (true, null);
        }

        public async Task<bool> ExisteAsync(int id)
        {
            return await _db.TiposIdentificacion.AnyAsync(t => t.Id == id);
        }

        public async Task<bool> ExisteNombreAsync(string nombre, int? excludeId = null)
        {
            var query = _db.TiposIdentificacion.Where(t => t.Nombre == nombre);
            if (excludeId.HasValue)
            {
                query = query.Where(t => t.Id != excludeId.Value);
            }
            return await query.AnyAsync();
        }
    }
}