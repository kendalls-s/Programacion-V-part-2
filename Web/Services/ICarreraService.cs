using CarnetDigitalWeb.Models;

namespace CarnetDigitalWeb.Services
{
    public interface ICarreraService
    {
        Task<List<Models.Carrera>> GetAllAsync();
        Task<Models.Carrera?> GetByIdAsync(int id);
        Task<(bool success, string message, int? id)> CreateAsync(CreateCarreraRequest request);
        Task<(bool success, string message)> UpdateAsync(UpdateCarreraRequest request);
        Task<(bool success, string message)> DeleteAsync(int id);
    }
}