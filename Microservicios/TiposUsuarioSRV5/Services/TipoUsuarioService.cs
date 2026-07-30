using Microsoft.EntityFrameworkCore;
using TiposUsuarioSRV5.Data;
using TiposUsuarioSRV5.DTOs;
using TiposUsuarioSRV5.Entities;

namespace TiposUsuarioSRV5.Services
{
    public class TipoUsuarioService : ITipoUsuarioService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<TipoUsuarioService> _logger;

        public TipoUsuarioService(ApplicationDbContext db, ILogger<TipoUsuarioService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<(bool ok, string? error, List<TipoUsuarioDto>? data)> GetAllAsync()
        {
            try
            {
                var tipos = await _db.TiposUsuario
                    .OrderBy(t => t.Nombre)
                    .Select(t => new TipoUsuarioDto
                    {
                        Id = t.Id,
                        Nombre = t.Nombre
                    })
                    .ToListAsync();

                return (true, null, tipos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetAllAsync");
                return (false, ex.Message, null);
            }
        }

        public async Task<(bool ok, string? error, TipoUsuarioDto? data)> GetByIdAsync(int id)
        {
            try
            {
                var tipo = await _db.TiposUsuario.FindAsync(id);
                if (tipo == null)
                {
                    return (false, "Tipo de usuario no encontrado", null);
                }

                return (true, null, new TipoUsuarioDto
                {
                    Id = tipo.Id,
                    Nombre = tipo.Nombre
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error en GetByIdAsync: {id}");
                return (false, ex.Message, null);
            }
        }

        public async Task<(bool ok, string? error, TipoUsuarioDto? data)> CreateAsync(TipoUsuarioCreateDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Nombre))
                {
                    return (false, "El nombre es requerido", null);
                }

                var exists = await _db.TiposUsuario.AnyAsync(t => t.Nombre == dto.Nombre.Trim());
                if (exists)
                {
                    return (false, "Ya existe un tipo de usuario con ese nombre", null);
                }

                var tipo = new TipoUsuario { Nombre = dto.Nombre.Trim() };
                _db.TiposUsuario.Add(tipo);
                await _db.SaveChangesAsync();

                return (true, null, new TipoUsuarioDto
                {
                    Id = tipo.Id,
                    Nombre = tipo.Nombre
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en CreateAsync");
                return (false, ex.Message, null);
            }
        }

        public async Task<(bool ok, string? error, TipoUsuarioDto? data)> UpdateAsync(int id, TipoUsuarioUpdateDto dto)
        {
            try
            {
                // ✅ Validar que el ID de la URL coincida con el ID del DTO
                if (id != dto.Id)
                {
                    return (false, "El ID no coincide", null);
                }

                var tipo = await _db.TiposUsuario.FindAsync(id);
                if (tipo == null)
                {
                    return (false, "Tipo de usuario no encontrado", null);
                }

                if (string.IsNullOrWhiteSpace(dto.Nombre))
                {
                    return (false, "El nombre es requerido", null);
                }

                // ✅ Verificar que no exista otro tipo con el mismo nombre
                var exists = await _db.TiposUsuario.AnyAsync(t => t.Nombre == dto.Nombre.Trim() && t.Id != id);
                if (exists)
                {
                    return (false, "Ya existe otro tipo de usuario con ese nombre", null);
                }

                tipo.Nombre = dto.Nombre.Trim();
                await _db.SaveChangesAsync();

                return (true, null, new TipoUsuarioDto
                {
                    Id = tipo.Id,
                    Nombre = tipo.Nombre
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error en UpdateAsync: {id}");
                return (false, ex.Message, null);
            }
        }

        public async Task<(bool ok, string? error)> DeleteAsync(int id)
        {
            try
            {
                var tipo = await _db.TiposUsuario.FindAsync(id);
                if (tipo == null)
                {
                    return (false, "Tipo de usuario no encontrado");
                }

                _db.TiposUsuario.Remove(tipo);
                await _db.SaveChangesAsync();

                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error en DeleteAsync: {id}");
                return (false, ex.Message);
            }
        }
    }
}