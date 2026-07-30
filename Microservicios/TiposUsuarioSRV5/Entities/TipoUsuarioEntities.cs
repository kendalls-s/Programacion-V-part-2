using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TiposUsuarioSRV5.Entities
{
    [Table("TipoUsuario", Schema = "dbo")]  // ✅ Cambiar a dbo
    public class TipoUsuario
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("Nombre")]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;
    }
}