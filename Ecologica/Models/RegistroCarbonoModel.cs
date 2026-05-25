using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecologica.Models
{
    [Table("registros_carbono")]
    public class RegistroCarbonoModel
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

        // ADICIONE ESTA PARTE QUE ESTÁ FALTANDO:
        [ForeignKey("IdAtividade")]
        public virtual Atividade? AtividadeRelacionada { get; set; }

        [ForeignKey("IdUsuario")]
        public virtual Usuario? Usuario { get; set; }
    }
}