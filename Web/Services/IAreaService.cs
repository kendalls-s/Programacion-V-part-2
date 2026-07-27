// Services/IAreaService.cs
using CarnetDigitalWeb.Models;

namespace CarnetDigitalWeb.Services
{
    public interface IAreaService
    {
        Task<List<Area>> GetAllAsync();
        Task<Area?> GetByIdAsync(int id);
        Task<(bool success, string message, int? id)> CreateAsync(Area area);
        Task<(bool success, string message)> UpdateAsync(int id, Area area);
        Task<(bool success, string message)> DeleteAsync(int id);
    }
}