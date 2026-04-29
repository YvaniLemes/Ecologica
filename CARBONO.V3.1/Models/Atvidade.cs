using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CARBONO.V3._1.Models
{
    [Table("atividades")]
    public class Atividade
    {
        [Key]
        [Column("id_atividade")]
        public int Id { get; set; }

        [Column("nome")]
        public string Nome { get; set; } = string.Empty;

        [Column("fator_emissao")]
        public double FatorEmissao { get; set; }
    }
}