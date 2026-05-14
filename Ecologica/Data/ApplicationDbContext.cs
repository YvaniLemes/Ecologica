using Microsoft.EntityFrameworkCore;
using Ecologica.Models;

namespace Ecologica.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Atividade> Atividades { get; set; }
        public DbSet<RegistroCarbonoModel> RegistrosCarbono { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Conquista> Conquistas { get; set; }
        
        // MUDANÇA AQUI: Alterei para trilha_progresso para bater com o banco e sumir as ondinhas
        public DbSet<TrilhaConhecimento> trilha_progresso { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Garante que o EF procure a tabela com este nome no SQLite
            modelBuilder.Entity<TrilhaConhecimento>().ToTable("trilha_progresso");

            modelBuilder.Entity<TrilhaConhecimento>().HasData(
                new TrilhaConhecimento { Id = 1, Titulo = "Introdução ao CO2", Descricao = "Aprenda o básico", Progresso = 1.0, EstaBloqueado = false },
                new TrilhaConhecimento { Id = 2, Titulo = "Cálculo de Emissões", Descricao = "Hora de praticar", Progresso = 0.4, EstaBloqueado = false },
                new TrilhaConhecimento { Id = 3, Titulo = "Desafio Final", Descricao = "Mestre da Ecologia", Progresso = 0.0, EstaBloqueado = true }
            );
        }
    }
}