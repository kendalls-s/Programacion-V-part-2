using CarnetDigitalWeb.Models;

namespace CarnetDigitalWeb.Services
{
    public interface ITipoUsuarioService
    {
        Task<List<TipoUsuario>> GetAllAsync();
        Task<TipoUsuario?> GetByIdAsync(int id);
        Task<TipoUsuario?> CreateAsync(TipoUsuario tipo);
        Task<bool> UpdateAsync(TipoUsuario tipo);
        Task<bool> DeleteAsync(int id);
    }
}