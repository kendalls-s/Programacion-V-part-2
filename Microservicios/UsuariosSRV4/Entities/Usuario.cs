using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UsuariosSRV4.Entities
{
    [Table("Usuario")]
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        public string Email { get; set; } = string.Empty;
        public string Contrasena { get; set; } = string.Empty;
        public int TipoUsuarioId { get; set; }
        public int EstadoId { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public int TipoIdentificacionId { get; set; }
        public string NumeroIdentificacion { get; set; } = string.Empty;
        public int RolId { get; set; }
        public string? Fotografia { get; set; }
        public bool Confirmado { get; set; }
        public DateTime FechaCreacion { get; set; }

        // ✅ PROPIEDADES PARA BLOQUEO
        public int IntentosFallidos { get; set; } = 0;
        public bool Bloqueado { get; set; } = false;
        public DateTime? FechaBloqueo { get; set; }

        // ✅ PROPIEDADES PARA REFRESH TOKEN (AGREGAR ESTO)
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }

        // Propiedades de navegación
        [ForeignKey("TipoUsuarioId")]
        public virtual TipoUsuario? TipoUsuario { get; set; }

        [ForeignKey("EstadoId")]
        public virtual EstadoUsuario? Estado { get; set; }

        [ForeignKey("TipoIdentificacionId")]
        public virtual TipoIdentificacion? TipoIdentificacion { get; set; }

        [ForeignKey("RolId")]
        public virtual Rol? Rol { get; set; }

        public virtual ICollection<UsuarioTelefono> Telefonos { get; set; } = new List<UsuarioTelefono>();
        public virtual ICollection<UsuarioArea> Areas { get; set; } = new List<UsuarioArea>();
        public virtual ICollection<UsuarioCarrera> Carreras { get; set; } = new List<UsuarioCarrera>();
        public virtual ICollection<UsuarioInstitucion> Instituciones { get; set; } = new List<UsuarioInstitucion>();
    }
}