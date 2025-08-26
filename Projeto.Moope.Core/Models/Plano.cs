using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Projeto.Moope.Core.Models.Base;

namespace Projeto.Moope.Core.Models
{
    [Table("Plano")]
    public class Plano : Entity
    {
        [Required]
        public string Codigo { get; set; }
        [Required]
        public string Descricao { get; set; }
        [Required]
        [Column(TypeName = "numeric(15,2)")]
        public decimal Valor { get; set; }
        public bool Status { get; set; }
    }
} 