using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecologica.Models
{
    [Table("conquistas")]
    public class Conquista
    {
        [Key]
        public int Id { get; set; }

        [Column("nome")]
        public string? Nome { get; set; }

        [Column("descricao")]
        public string? Descricao { get; set; }

        [Column("data_aquisicao")]
        public DateTime? DataAquisicao { get; set; }

        // --- É AQUI QUE VOCÊ COLOCA ---
        [Column("usuario_id")]
        public int? UsuarioId { get; set; }
        // ------------------------------

        [ForeignKey("UsuarioId")]
        public virtual Usuario? Usuario { get; set; }
    }
}