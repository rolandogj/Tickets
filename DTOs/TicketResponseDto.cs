using System.ComponentModel.DataAnnotations;

namespace GlobalTech.DTOs
{
    public class TicketResponseDto
    {
        public int IdTicket { get; set; }
        public string UsuarioSolicitante { get; set; } = string.Empty;
        public string? TecnicoAsignado { get; set; }
        public string Departamento { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Prioridad { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
    }
}
