using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GlobalTech.Models
{
    [Table("rol")]
    public class Rol
    {
        [Key]
        [Column("id_rol")]
        public int IdRol { get; set; }

        [Required]
        [Column("nombre_rol")]
        public string NombreRol { get; set; } = string.Empty;
    }
}
