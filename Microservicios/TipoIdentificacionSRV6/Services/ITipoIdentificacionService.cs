using TipoIdentificacionSRV6.DTOs;
using TipoIdentificacionSRV6.Entities;

namespace TipoIdentificacionSRV6.Services
{
    public interface ITipoIdentificacionService
    {
        Task<List<TipoIdentificacionDto>> ObtenerTodosAsync();
        Task<TipoIdentificacionDto?> ObtenerPorIdAsync(int id);
        Task<(bool ok, string? error, int id)> CrearAsync(TipoIdentificacionCreateDto dto);
        Task<(bool ok, string? error)> ActualizarAsync(int id, TipoIdentificacionUpdateDto dto);
        Task<(bool ok, string? error)> EliminarAsync(int id);
        Task<bool> ExisteAsync(int id);
        Task<bool> ExisteNombreAsync(string nombre, int? excludeId = null);
    }
}