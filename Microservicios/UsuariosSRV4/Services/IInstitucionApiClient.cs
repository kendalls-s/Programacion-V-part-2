using UsuariosSRV4.DTOs;

namespace UsuariosSRV4.Services
{
    public interface IInstitucionApiClient
    {
        Task<List<InstitucionDto>> GetAllAsync(CancellationToken ct = default);
        Task<InstitucionDto?> GetByIdAsync(int id, CancellationToken ct = default);
    }
}
