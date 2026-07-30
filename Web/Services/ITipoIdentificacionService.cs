using CarnetDigitalWeb.Models;

namespace CarnetDigitalWeb.Services
{
    public interface ITipoIdentificacionService
    {
        Task<List<TipoIdentificacion>> GetAllAsync();
        Task<TipoIdentificacion?> GetByIdAsync(int id);
        Task<TipoIdentificacion?> CreateAsync(TipoIdentificacion tipo);
        Task<bool> UpdateAsync(TipoIdentificacion tipo);
        Task<bool> DeleteAsync(int id);
    }
}