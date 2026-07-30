namespace CarnetDigitalWeb.Models
{
    public class Carrera
    {
        public int ID { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Director { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public int InstitucionID { get; set; }
        public string InstitucionNombre { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaModificacion { get; set; }
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
}