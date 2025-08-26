using System.ComponentModel.DataAnnotations.Schema;
using Projeto.Moope.Core.Enums;
using Projeto.Moope.Core.Models.Base;

namespace Projeto.Moope.Core.Models
{
    [Table("Cliente")]
    public class Cliente : Entity
    {
        [NotMapped]
        public TipoPessoa TipoPessoa { get; set; }
        
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        [NotMapped]
        public string CpfCnpj { get; set; }
        public Guid? VendedorId { get; set; }
        public Vendedor? Vendedor { get; set; }
    }
} 