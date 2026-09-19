using Microsoft.EntityFrameworkCore;
using GlobalTech.Models;

namespace GlobalTech.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Definición de las tablas (DbSet) para cada modelo
        public DbSet<Departamento> Departamentos { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Bitacora> Bitacora { get; set; }
    }
}
