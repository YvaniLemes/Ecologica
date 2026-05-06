using Microsoft.EntityFrameworkCore;
using Ecologica.Models;

namespace Ecologica.Models.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // Mapeamento das tabelas do banco de dados
        public DbSet<Atividade> Atividades { get; set; }
        
        // Aqui usamos o nome da classe correta (RegistroCarbonoModel) 
        // mas a tabela no banco continua se chamando RegistrosCarbono
        public DbSet<RegistroCarbonoModel> RegistrosCarbono { get; set; }
        
        public DbSet<Usuario> Usuarios { get; set; }
        
        public DbSet<Conquista> Conquistas { get; set; }
    }
}