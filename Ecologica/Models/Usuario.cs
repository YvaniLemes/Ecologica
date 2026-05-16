using System.ComponentModel.DataAnnotations;

namespace Ecologica.Models
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório")]
        public string? Nome { get; set; }

        [Required(ErrorMessage = "O e-mail é obrigatório")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "A senha é obrigatória")]
        public string? Senha { get; set; }

        public int Pontos { get; set; } = 0;

        // 🌳 NOVA PROPRIEDADE: Guarda a quantidade de árvores vitalícias plantadas!
        public int QuantidadeArvores { get; set; } = 0;
    }
}