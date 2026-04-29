using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CARBONO.V3._1.Models
{
    [Table("registros_carbono")] // Nome exato da tabela no seu SQL
    public class RegistroCarbono
    {
        [Key]
        [Column("id_registro")]
        public int Id { get; set; }

        [Column("id_usuario")]
        public int IdUsuario { get; set; }

        [Column("id_atividade")]
        public int IdAtividade { get; set; }

        [Column("quantidade")]
        public double Quantidade { get; set; }

        [Column("emissao_total")]
        public double EmissaoTotal { get; set; }

        [Column("data_registro")]
        public DateTime DataRegistro { get; set; } = DateTime.Now;

        // Propriedades de navegação (para facilitar consultas)
        [ForeignKey("IdAtividade")]
        public virtual Atividade? AtividadeRelacionada { get; set; }
        
        [ForeignKey("IdUsuario")]
        public virtual Usuario? Usuario { get; set; }
    }
}