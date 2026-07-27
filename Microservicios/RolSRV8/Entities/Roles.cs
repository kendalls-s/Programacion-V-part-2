namespace RolSRV8.Entities;

public class Rol
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public List<Pantalla> Pantallas { get; set; } = new();
}