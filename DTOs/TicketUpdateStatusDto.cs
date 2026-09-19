using System.ComponentModel.DataAnnotations;

namespace GlobalTech.DTOs
{
    public class TicketUpdateStatusDto
    {
        [Required(ErrorMessage = "El ID del usuario es requerido")]
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "El nuevo estado es obligatorio")]
        [RegularExpression("^(Abierto|En Proceso|Resuelto|Cerrado)$", ErrorMessage = "El estado del ticket no es válido")]
        public string NuevoEstado { get; set; } = string.Empty;

        public string DetalleCambio { get; set; } = string.Empty;
    }
}
