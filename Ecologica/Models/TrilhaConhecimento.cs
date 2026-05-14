namespace Ecologica.Models
{
    public class TrilhaConhecimento
    {
        public int Id { get; set; }
        public string? Titulo { get; set; }
        public string? Descricao { get; set; }
        public double Progresso { get; set; } // De 0.0 a 1.0
        public bool EstaBloqueado { get; set; }

        // Lógica recomendada (Opção B): Ciclo de vida visual completo
        public string ObterImagemProgresso()
        {
            // 1. Se o progresso é 1.0, o usuário já concluiu. É uma Árvore.
            if (Progresso >= 1.0) return "tree.png"; 

            // 2. Se NÃO está bloqueado mas não é 1.0, é a fase atual. É um Broto.
            if (!EstaBloqueado) return "sprout.png"; 

            // 3. Se está bloqueado, ainda é uma Semente (noz).
            return "nuts.png"; 
        }
    }
}