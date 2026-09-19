namespace GlobalTech.DTOs
{
    public class RedAlertReportDto
    {
        public int IdDepartamento { get; set; }
        public string NombreDepartamento { get; set; } = string.Empty;
        public int TotalTicketsCriticos { get; set; }
        public string NivelRiesgo { get; set; } = "Alerta Roja";
    }
}
