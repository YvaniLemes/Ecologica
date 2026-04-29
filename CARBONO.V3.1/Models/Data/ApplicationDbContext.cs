using Microsoft.EntityFrameworkCore;
using CARBONO.V3._1.Models;

namespace CARBONO.V3._1.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Atividade> Atividades { get; set; }
        public DbSet<RegistroCarbono> RegistrosCarbono { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
    }
}