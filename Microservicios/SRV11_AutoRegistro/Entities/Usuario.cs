namespace SRV11_AutoRegistro.Entities;

public class Usuario
{
    public int Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string Contrasena { get; set; } = string.Empty;

    public int TipoUsuarioId { get; set; }

    public int EstadoId { get; set; } = 1;

    public bool Confirmado { get; set; }

    public DateTime FechaCreacion { get; set; }

    public string NombreCompleto { get; set; } = string.Empty;

    public int TipoIdentificacionId { get; set; }

    public string NumeroIdentificacion { get; set; } = string.Empty;

    public int RolId { get; set; }

    public byte[]? Fotografia { get; set; }

    public int IntentosFallidos { get; set; }

    public bool Bloqueado { get; set; }

    public DateTime? FechaBloqueo { get; set; }

    public string? TokenConfirmacion { get; set; }

    public DateTime? FechaExpiracion { get; set; }

    public List<int> Instituciones { get; set; } = [];

    public List<int> CarrerasAsociadas { get; set; } = [];

    public List<int> AreasAsociadas { get; set; } = [];

    public List<string> Telefonos { get; set; } = [];
}