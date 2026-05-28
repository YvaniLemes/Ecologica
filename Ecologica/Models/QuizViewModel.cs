namespace Ecologica.Models
{
    public class QuizViewModel
    {
        public int Id { get; set; }   // Identificador único da questão

        public string? Pergunta { get; set; }   // aceita NULL

        public List<string>? Opcoes { get; set; }   // aceita NULL

        public int? RespostaCorretaIndice { get; set; }   // aceita NULL

        public string? Explicacao { get; set; }   // aceita NULL
    }
}
