using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GlobalTech.Models
{
    [Table("usuarios")]
    public class Usuario
    {
        
        [Key]
        [Column("id_usuario")]
        public int IdUsuario { get; set; }

        [Required]
        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [Column("apellido")]
        public string Apellido { get; set; } = string.Empty;

        [Required]
        [Column("correo")]
        public string Correo { get; set; } = string.Empty;

        [Required]
        [Column("nombre_usuario")]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required]
        [Column("password_hash")]
        public string PasswordHash { get; set; } = string.Empty;

        [Column("id_departamento")]
        public int IdDepartamento { get; set; }

        [ForeignKey("IdDepartamento")]
        public Departamento? Departamento { get; set; }

        [Column("id_rol")]
        public int IdRol { get; set; }

        [ForeignKey("IdRol")]
        public Rol? Rol { get; set; }
    }
}
