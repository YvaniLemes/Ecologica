namespace Ecologica.Models
{
    public class TrilhaConhecimento
    {
        public int Id { get; set; }

        public string? Titulo { get; set; }   // aceita NULL
        public string? Descricao { get; set; }   // aceita NULL

        public double? Progresso { get; set; }   // aceita NULL
        public bool? EstaBloqueado { get; set; }   // aceita NULL

        // Método auxiliar para retornar imagem conforme progresso
        public string ObterImagemProgresso()
        {
            if (Progresso >= 1.0) return "tree.png";
            if (EstaBloqueado == false) return "sprout.png";
            return "nuts.png";
        }
    }
}
