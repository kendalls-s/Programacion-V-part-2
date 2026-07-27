namespace CarnetDigitalWeb.Models
{
    public class Rol
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public List<PantallaRol> Pantallas { get; set; } = new();
    }
}