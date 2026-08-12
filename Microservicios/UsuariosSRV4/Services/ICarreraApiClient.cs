using UsuariosSRV4.DTOs;

namespace UsuariosSRV4.Services
{
    public interface ICarreraApiClient
    {
        Task<List<CarreraDto>> GetAllAsync(CancellationToken ct = default);
        Task<CarreraDto?> GetByIdAsync(int id, CancellationToken ct = default);
    }
}
