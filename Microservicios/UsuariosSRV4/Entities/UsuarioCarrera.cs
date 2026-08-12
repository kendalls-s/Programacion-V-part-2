using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace UsuariosSRV4.Entities
{
    [Table("UsuarioCarrera")]
    [PrimaryKey(nameof(UsuarioId), nameof(CarreraId))]
    public class UsuarioCarrera
    {
        public int UsuarioId { get; set; }
        public string CarreraId { get; set; } = string.Empty;

        [ForeignKey("UsuarioId")]
        public virtual Usuario? Usuario { get; set; }
    }
}