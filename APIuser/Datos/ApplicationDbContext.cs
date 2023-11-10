using APIuser.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace APIuser.Datos
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options):base(options)
        {
 
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Acceso> Accesos { get; set; }
    }
}
