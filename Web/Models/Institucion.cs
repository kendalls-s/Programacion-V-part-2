// InstitucionModels.cs
namespace CarnetDigitalWeb.Models
{
    // Modelo completo de Institución
    public class Institucion
    {
        public int ID { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Dominios { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
    }

    // Request para crear una institución
    public class InstitucionCreateRequest
    {
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Dominios { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;
    }

    // Request para actualizar una institución
    public class InstitucionUpdateRequest
    {
        public int ID { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Dominios { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }

    // Respuesta genérica de la API
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }

    // Respuesta de la API solo con mensaje (sin datos)
    public class ApiResponseMessage
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}