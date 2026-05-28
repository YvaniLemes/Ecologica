using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecologica.Models
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }
        public string? Nome { get; set; }
        public string? Email { get; set; }
        public string? Senha { get; set; }
        public int? Pontos { get; set; }
        public int? QuantidadeArvores { get; set; }
        public int? PosicaoNoMapa { get; set; } // O banco diz "PosicaoNoMapa"
    }
}
