using TiposUsuarioSRV5.DTOs;

namespace TiposUsuarioSRV5.Services
{
    public interface ITipoUsuarioService
    {
        Task<(bool ok, string? error, List<TipoUsuarioDto>? data)> GetAllAsync();
        Task<(bool ok, string? error, TipoUsuarioDto? data)> GetByIdAsync(int id);
        Task<(bool ok, string? error, TipoUsuarioDto? data)> CreateAsync(TipoUsuarioCreateDto dto);
        Task<(bool ok, string? error, TipoUsuarioDto? data)> UpdateAsync(int id, TipoUsuarioUpdateDto dto);
        Task<(bool ok, string? error)> DeleteAsync(int id);
    }
}