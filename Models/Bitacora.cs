using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GlobalTech.Models
{
    [Table("bitacora")]
    public class Bitacora
    {
        [Key]
        [Column("id_bitacora")]
        public int IdBitacora { get; set; }

        [Column("id_ticket")]
        public int IdTicket { get; set; }

        [ForeignKey("IdTicket")]
        public Ticket? Ticket { get; set; }

        [Column("id_usuario")]
        public int IdUsuario { get; set; }

        [ForeignKey("IdUsuario")]
        public Usuario? Usuario { get; set; }

        [Required]
        [Column("comentario")]
        public string Comentario { get; set; } = string.Empty;

        [Column("estado_anterior")]
        public string? EstadoAnterior { get; set; }

        [Required]
        [Column("estado_nuevo")]
        public string EstadoNuevo { get; set; } = string.Empty;

        [Required]
        [Column("fecha_registro")]
        public DateTime Fecha { get; set; }
    }
}
