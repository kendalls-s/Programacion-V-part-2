namespace UsuariosSRV4.DTOs
{
    public class UsuarioDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string TipoUsuario { get; set; } = string.Empty;
        public string TipoIdentificacion { get; set; } = string.Empty;
        public string NumeroIdentificacion { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public bool Bloqueado { get; set; }
        public int IntentosFallidos { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string? FotografiaBase64 { get; set; }
        public List<string> Telefonos { get; set; } = new();
        public bool Confirmado { get; set; }
        public int? RolId { get; set; }
        public int? EstadoId { get; set; }
    }

    public class CrearUsuarioDto
    {
        public string Email { get; set; } = string.Empty;
        public string Contrasena { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public int TipoUsuarioId { get; set; }
        public int TipoIdentificacionId { get; set; }
        public string NumeroIdentificacion { get; set; } = string.Empty;
        public int? RolId { get; set; }
        public List<string> Telefonos { get; set; } = new();
        public bool Confirmado { get; set; } = true;
    }

    public class ActualizarUsuarioDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Contrasena { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public int TipoUsuarioId { get; set; }
        public int TipoIdentificacionId { get; set; }
        public string NumeroIdentificacion { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public List<string> Telefonos { get; set; } = new();
        public bool? Confirmado { get; set; }
        public int? RolId { get; set; }
        public int? EstadoId { get; set; }
    }

    // ✅ FiltroUsuarioDto - AGREGADO
    public class FiltroUsuarioDto
    {
        public string? Email { get; set; }
        public string? NombreCompleto { get; set; }
        public int? TipoUsuarioId { get; set; }
        public int? EstadoId { get; set; }
        public bool? Activo { get; set; }
        public bool? Bloqueado { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public int? RolId { get; set; }
        public int? TipoIdentificacionId { get; set; }
        public string? NumeroIdentificacion { get; set; }
        public bool? Confirmado { get; set; }
    }
}