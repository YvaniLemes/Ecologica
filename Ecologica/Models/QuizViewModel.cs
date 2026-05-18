namespace Ecologica.Models
{
    public class QuizViewModel

    {
        public int Id { get; set; } // Identificador único da questão
        public string Pergunta { get; set; } = null!;
        public List<string> Opcoes { get; set; } = null!;
        public int RespostaCorretaIndice { get; set; }
        public string? Explicacao { get; set; }
    }
}