using System.ComponentModel.DataAnnotations;

namespace GlobalTech.DTOs
{
    public class TicketResolveDto
    {
        [Required(ErrorMessage = "El ID del usuario es obligatorio.")]
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "El estado final es obligatorio.")]
        [RegularExpression("^(Resuelto|Cerrado)$", ErrorMessage = "El estado debe ser 'Resuelto' o 'Cerrado'.")]
        public string EstadoFinal { get; set; } = string.Empty;

        [Required(ErrorMessage = "La solución o comentario final es obligatorio.")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "La solución debe tener entre 10 y 500 caracteres.")]
        public string Solucion { get; set; } = string.Empty;
    }
}
