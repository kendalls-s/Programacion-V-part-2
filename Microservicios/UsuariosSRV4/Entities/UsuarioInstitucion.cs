using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace UsuariosSRV4.Entities
{
    [Table("UsuarioInstitucion")]
    [PrimaryKey(nameof(UsuarioId), nameof(InstitucionId))]
    public class UsuarioInstitucion
    {
        public int UsuarioId { get; set; }
        public string InstitucionId { get; set; } = string.Empty;

        [ForeignKey("UsuarioId")]
        public virtual Usuario? Usuario { get; set; }
    }
}