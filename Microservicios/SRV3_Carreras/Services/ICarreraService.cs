using SRV3_Carreras.Entities;

namespace SRV3_Carreras.Services;

public interface ICarreraService
{
    Task<IEnumerable<Carrera>> GetAll();
    Task<Carrera?> GetById(int id);
    Task<(bool success, string message, int? id)> Create(CreateCarreraRequest request, string token);
    Task<(bool success, string message)> Update(UpdateCarreraRequest request, string token);
    Task<(bool success, string message)> Delete(int id);
}

public class CreateCarreraRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string Director { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public int InstitucionID { get; set; }
}

public class UpdateCarreraRequest
{
    public int ID { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Director { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public int InstitucionID { get; set; }
}