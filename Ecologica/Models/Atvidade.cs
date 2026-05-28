using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecologica.Models
{
    [Table("atividades")]
    public class Atividade
    {
        [Key]
        [Column("id_atividade")]
        public int Id { get; set; }

        [Column("nome")]
        public string? Nome { get; set; }   // aceita NULL

        [Column("fator_emissao")]
        public double? FatorEmissao { get; set; }   // aceita NULL

        [Column("unidade_medida")]
        public string? UnidadeMedida { get; set; }   // aceita NULL
    }
}

