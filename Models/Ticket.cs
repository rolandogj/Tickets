using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GlobalTech.Models
{
    [Table("tickets")]
    public class Ticket
    {
        [Key]
        [Column("id_ticket")]
        public int IdTicket { get; set; }

        [Column("id_usuario")]
        public int IdUsuario { get; set; }

        [ForeignKey("IdUsuario")]
        public Usuario? Usuario { get; set; } = null;

        [Column("id_tecnico")]
        public int? IdTecnico { get; set; }

        [ForeignKey("IdTecnico")]
        public Usuario? Tecnico { get; set; }

        [Required]
        [Column("fecha_creacion")]
        public DateTime FechaCreacion { get; set; }

        [Required]
        [Column("descripcion")]
        public string Descripcion { get; set; } = string.Empty;

        [Required]
        [RegularExpression("^(Baja|Media|Alta|Critica)$", ErrorMessage = "La prioridad debe ser: Baja, Media, Alta o Critica.")]
        [Column("prioridad")]
        public string Prioridad { get; set; } = string.Empty;

        [Required]
        [RegularExpression("^(Abierto|En Progreso|Resuelto)$", ErrorMessage = "El estado debe ser: Abierto, En Progreso o Resuelto.")]
        [Column("estado")]
        public string Estado { get; set; } = string.Empty;
    }
}
