using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TipoIdentificacionSRV6.Entities
{
    [Table("TipoIdentificacion", Schema = "dbo")]  // ✅ Cambiar a dbo
    public class TipoIdentificacion
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("Nombre")]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;
    }
}