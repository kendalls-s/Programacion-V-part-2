using Microsoft.EntityFrameworkCore;
using TiposUsuarioSRV5.Entities;

namespace TiposUsuarioSRV5.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<TipoUsuario> TiposUsuario { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TipoUsuario>(entity =>
            {
                entity.ToTable("TipoUsuario", "dbo");  // ✅ Cambiar a dbo
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("Id");
                entity.Property(e => e.Nombre).HasColumnName("Nombre").HasMaxLength(100);
            });
        }
    }
}