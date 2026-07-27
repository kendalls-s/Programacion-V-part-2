// IInstitucionService.cs
using CarnetDigitalWeb.Models;

namespace CarnetDigitalWeb.Services
{
    public interface IInstitucionService
    {
        Task<List<Institucion>> GetAllAsync();
        Task<Institucion?> GetByIdAsync(int id);
        Task<(bool success, string message, int? id)> CreateAsync(InstitucionCreateRequest request);
        Task<(bool success, string message)> UpdateAsync(InstitucionUpdateRequest request);
        Task<(bool success, string message)> DeleteAsync(int id);
    }
}