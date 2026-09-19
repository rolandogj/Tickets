using System.ComponentModel.DataAnnotations;

namespace GlobalTech.DTOs
{
    public class BitacoraResponseDto
    {
        public int IdBitacora { get; set; }
        public int IdTicket { get; set; }
        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string? Comentario { get; set; }
        public string EstadoAnterior { get; set; } = string.Empty;
        public string EstadoNuevo { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
    }
}
