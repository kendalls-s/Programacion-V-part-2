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

        // ============================================================
        // ✅ DATOS ENRIQUECIDOS: Instituciones / Areas / Carreras
        // Se llenan consultando los microservicios respectivos a través
        // de los ApiClients (ver Services/*ApiClient.cs). Si algún
        // servicio remoto no responde, estas listas simplemente quedan
        // vacías y no se rompe la respuesta del usuario.
        // ============================================================
        public List<InstitucionDto> Instituciones { get; set; } = new();
        public List<AreaDto> Areas { get; set; } = new();
        public List<CarreraDto> Carreras { get; set; } = new();

        // Campos de conveniencia para pantallas tipo "Carnet" (HU USR2/GRD2)
        // que solo necesitan mostrar UN nombre, no la lista completa.
        public string? InstitucionNombre { get; set; }
        public string? AreaNombre { get; set; }
        public string? CarreraNombre { get; set; }

        // Se conservan por compatibilidad con el front actual (IDs crudos)
        public string? CarreraId { get; set; }
        public string? AreaId { get; set; }
        public string? InstitucionId { get; set; }
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

        // Relaciones opcionales al crear el usuario
        public string? InstitucionId { get; set; }
        public string? AreaId { get; set; }
        public string? CarreraId { get; set; }
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

        // Relaciones opcionales al actualizar el usuario
        public string? InstitucionId { get; set; }
        public string? AreaId { get; set; }
        public string? CarreraId { get; set; }
    }

    // ✅ FiltroUsuarioDto
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
