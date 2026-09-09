using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GlobalTech.Models
{
    [Table("departamentos")]
    public class Departamento
    {
        [Key]
        [Column("id_departamento")]
        public int IdDepartamento { get; set; }

        [Required]
        [Column("nombre_departamento")]
        public string NombreDepartamento { get; set; } = string.Empty;
    }
}
