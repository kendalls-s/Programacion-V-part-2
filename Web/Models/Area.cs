// Area.cs
namespace CarnetDigitalWeb.Models
{
    public class Area
    {
        public int ID { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int InstitucionID { get; set; }
        public string InstitucionNombre { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
    }

    // Request para crear un área
    public class AreaCreateRequest
    {
        public string Nombre { get; set; } = string.Empty;
        public int InstitucionID { get; set; }
        public bool Activo { get; set; } = true;
    }

    // Request para actualizar un área
    public class AreaUpdateRequest
    {
        public int ID { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int InstitucionID { get; set; }
        public bool Activo { get; set; }
    }

    // Si tu código está usando específicamente "AreaRequest", agrega esta clase
    public class AreaRequest
    {
        public int ID { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int InstitucionID { get; set; }
        public bool Activo { get; set; }
    }

    public class AreaApiResponse<T>
    {
        public int Codigo { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public T? Data { get; set; }
    }
}
