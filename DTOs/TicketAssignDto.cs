using System.ComponentModel.DataAnnotations;

namespace GlobalTech.DTOs
{
    public class TicketAssignDto
    {
        [Required(ErrorMessage = "El ID del técnico a asignar es obligatorio.")]
        public int IdTecnico { get; set; }

        [Required(ErrorMessage = "El ID del usuario que realiza la asignación es obligatorio.")]
        public int IdUsuarioAsignador { get; set; }
    }
}
