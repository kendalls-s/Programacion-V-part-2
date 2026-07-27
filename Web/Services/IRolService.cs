using CarnetDigitalWeb.Models;

namespace CarnetDigitalWeb.Services
{
    public interface IRolService
    {
        Task<List<Rol>> GetAllAsync();

        Task<Rol?> GetByIdAsync(int id);

        Task<bool> CreateAsync(RolRequest request);

        Task<bool> UpdateAsync(int id, RolRequest request);

        Task<bool> DeleteAsync(int id);
    }
}