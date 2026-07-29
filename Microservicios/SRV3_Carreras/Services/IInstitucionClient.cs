using System.Text.Json.Serialization;

namespace SRV3_Carreras.Services;

public interface IInstitucionClient
{
    Task<List<InstitucionDto>> GetAllInstituciones();
    Task<InstitucionDto?> GetInstitucionById(int id, string token);
    Task<bool> ValidateInstitucionExists(int id, string token);
}

public class InstitucionDto
{
    [JsonPropertyName("id")]
    public int ID { get; set; }

    [JsonPropertyName("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("telefono")]
    public string Telefono { get; set; } = string.Empty;

    [JsonPropertyName("dominios")]
    public string Dominios { get; set; } = string.Empty;
}

public class InstitucionResponse
{
    public int Codigo { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public InstitucionDto? Data { get; set; }
}

public class InstitucionesResponse
{
    public int Codigo { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public List<InstitucionDto> Data { get; set; } = new List<InstitucionDto>();
}