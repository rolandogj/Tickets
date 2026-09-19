namespace GlobalTech.Services
{
    public interface IJwtService
    {
        string GenerateToken(int idUsuario, string rol);
    }
}
