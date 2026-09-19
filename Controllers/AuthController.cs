using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using GlobalTech.Data;
using GlobalTech.DTOs;
using System.Reflection.Metadata.Ecma335;
using GlobalTech.Services;

namespace GlobalTech.Controllers
{
    [ApiController]
    [Route("api/[controller]")] 
    
    public class AuthController :ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IJwtService _jwtService;

        public AuthController(ApplicationDbContext context, IJwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Busca usuario e incluye sus relaciones de rol y departamento
            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .Include(u => u.Departamento)
                .FirstOrDefaultAsync(u => u.NombreUsuario == loginDto.NombreUsuario);

            // verificar si el usuario existe y si la contraseña es correcta
            if (usuario == null || usuario.PasswordHash != loginDto.Password)
            { 
                return Unauthorized(new { message = "Nombre de usuario o contraseña incorrectos" });
            }

            // Generar token JWT
            var token = _jwtService.GenerateToken(usuario.IdUsuario, usuario.Rol?.NombreRol);
            var fechaExpiracion = DateTime.UtcNow.AddHours(1); // Ejemplo: el token expira en 1 hora

            // Mapear la entidad de dominio a un DTO seguro (sin incluir la contraseña)
            var respuesta = new LoginResponseDto
            {
                IdUsuario = usuario.IdUsuario,
                NombreCompleto = $"{usuario.Nombre} {usuario.Apellido}",
                NombreUsuario = usuario.NombreUsuario,
                Correo = usuario.Correo,
                Rol = usuario.Rol?.NombreRol ?? "Sin Rol",
                Departamento = usuario.Departamento?.NombreDepartamento ?? "Sin Departamento",
                Mensaje = "Inicio de sesión exitoso",
                Token = token,
                Expiracion = fechaExpiracion
            };

            return Ok(respuesta);
        }
    }
}
