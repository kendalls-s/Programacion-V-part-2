using SRV4_Areas.Entities;

namespace SRV4_Areas.Services;

public interface IAreaService
{
    Task<IEnumerable<AreaTrabajo>> GetAll();

    Task<AreaTrabajo?> GetById(int id);

    Task<(bool success, string message, int? id)> Create(
        CreateAreaRequest request,
        string token);

    Task<(bool success, string message)> Update(
        UpdateAreaRequest request,
        string token);

    Task<(bool success, string message)> Delete(int id);
}

public class CreateAreaRequest
{
    public string Nombre { get; set; } = string.Empty;

    public int InstitucionID { get; set; }
}

public class UpdateAreaRequest
{
    public int ID { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public int InstitucionID { get; set; }
}