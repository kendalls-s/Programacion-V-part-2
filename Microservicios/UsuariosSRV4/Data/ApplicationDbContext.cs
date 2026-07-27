using Microsoft.EntityFrameworkCore;
using UsuariosSRV4.Entities;

namespace UsuariosSRV4.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<TipoUsuario> TiposUsuario { get; set; }
        public DbSet<TipoIdentificacion> TiposIdentificacion { get; set; }
        public DbSet<EstadoUsuario> EstadosUsuario { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<UsuarioTelefono> UsuariosTelefonos { get; set; }
        public DbSet<UsuarioArea> UsuariosAreas { get; set; }
        public DbSet<UsuarioCarrera> UsuariosCarreras { get; set; }
        public DbSet<UsuarioInstitucion> UsuariosInstituciones { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("Usuario");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("Id");
                entity.Property(e => e.Email).HasColumnName("Email");
                entity.Property(e => e.Contrasena).HasColumnName("Contrasena");
                entity.Property(e => e.TipoUsuarioId).HasColumnName("TipoUsuarioId");
                entity.Property(e => e.EstadoId).HasColumnName("EstadoId");
                entity.Property(e => e.NombreCompleto).HasColumnName("NombreCompleto");
                entity.Property(e => e.TipoIdentificacionId).HasColumnName("TipoIdentificacionId");
                entity.Property(e => e.NumeroIdentificacion).HasColumnName("NumeroIdentificacion");
                entity.Property(e => e.RolId).HasColumnName("RolId");
                entity.Property(e => e.Fotografia).HasColumnName("Fotografia");
                entity.Property(e => e.Confirmado).HasColumnName("Confirmado");
                entity.Property(e => e.FechaCreacion).HasColumnName("FechaCreacion");

                // ✅ NUEVAS COLUMNAS - DEBEN ESTAR MAPEADAS
                entity.Property(e => e.IntentosFallidos).HasColumnName("IntentosFallidos");
                entity.Property(e => e.Bloqueado).HasColumnName("Bloqueado");
                entity.Property(e => e.FechaBloqueo).HasColumnName("FechaBloqueo");
            });
        }
    }
}