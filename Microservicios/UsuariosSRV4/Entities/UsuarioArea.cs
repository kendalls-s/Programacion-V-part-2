using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace UsuariosSRV4.Entities
{
    [Table("UsuarioArea", Schema = "dbo")]
    [PrimaryKey(nameof(UsuarioId), nameof(AreaId))]
    public class UsuarioArea
    {
        [Column("UsuarioId")]
        public int UsuarioId { get; set; }

        [Column("AreaId")]
        public string AreaId { get; set; } = string.Empty;

        [ForeignKey("UsuarioId")]
        public virtual Usuario? Usuario { get; set; }
    }
}