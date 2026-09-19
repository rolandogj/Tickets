using System.ComponentModel.DataAnnotations;

namespace GlobalTech.DTOs
{
    public class TicketCreateDto
    {
        [Required(ErrorMessage = "El ID del usuario solicitante es obligatorio")]
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "La descripción del problema es obligatoria")]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "La prioridad es obligatoria")]
        [RegularExpression("^(Baja|Media|Alta|Critica)$", ErrorMessage = "La prioridad debe ser Baja, Media, Alta o Critica")]
        public string Prioridad { get; set; } = string.Empty;
    }
}
