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
        public int? IdUsuario { get; set; }   // aceita NULL

        [Column("id_atividade")]
        public int? IdAtividade { get; set; }   // aceita NULL

        [Column("quantidade")]
        public double? Quantidade { get; set; }   // aceita NULL

        [Column("emissao_total")]
        public double? EmissaoTotal { get; set; }   // aceita NULL

        [Column("data_registro")]
        public DateTime? DataRegistro { get; set; }   // aceita NULL

        [ForeignKey("IdAtividade")]
        public virtual Atividade? AtividadeRelacionada { get; set; }

        [ForeignKey("IdUsuario")]
        public virtual Usuario? Usuario { get; set; }
    }
}
