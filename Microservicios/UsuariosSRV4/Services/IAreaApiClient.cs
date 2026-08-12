using UsuariosSRV4.DTOs;

namespace UsuariosSRV4.Services
{
    public interface IAreaApiClient
    {
        Task<List<AreaDto>> GetAllAsync(CancellationToken ct = default);
        Task<AreaDto?> GetByIdAsync(int id, CancellationToken ct = default);
    }
}
