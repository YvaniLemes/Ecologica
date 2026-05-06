using System.ComponentModel.DataAnnotations;


namespace Ecologica.Models
{
    public class Conquista
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string? Nome { get; set; } // Ex: "Eco Iniciante", "Mestre da Reciclagem"

        public string? Descricao { get; set; }

        public DateTime DataAquisicao { get; set; } = DateTime.Now;

        // Relacionamento com o usuário
        public int UsuarioId { get; set; }
    }
}