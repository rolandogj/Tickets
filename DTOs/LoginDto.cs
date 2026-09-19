using System.ComponentModel.DataAnnotations;

namespace GlobalTech.DTOs
{
    public class LoginDto
    {
        [Required(ErrorMessage = "El nombre del usuario es obligatorio")]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        public string Password { get; set; } = string.Empty;
    }
}
