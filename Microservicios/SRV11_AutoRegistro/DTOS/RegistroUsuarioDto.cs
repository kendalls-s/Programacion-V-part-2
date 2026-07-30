namespace SRV11_AutoRegistro.DTOs;

public class RegistroUsuarioDto
{
    public int TipoUsuarioId { get; set; }

    public int TipoIdentificacionId { get; set; }

    public int RolId { get; set; }

    public string NumeroIdentificacion { get; set; } = string.Empty;

    public string NombreCompleto { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Contrasena { get; set; } = string.Empty;

    public List<int> Instituciones { get; set; } = [];

    public List<int> CarrerasAsociadas { get; set; } = [];

    public List<int> AreasAsociadas { get; set; } = [];

    public List<string> Telefonos { get; set; } = [];
}