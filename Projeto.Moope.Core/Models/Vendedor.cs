using Projeto.Moope.Core.Models.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Projeto.Moope.Core.Enums;

namespace Projeto.Moope.Core.Models
{
    [Table("Vendedor")]
    public class Vendedor : Entity
    {
        [NotMapped]
        public TipoPessoa TipoPessoa { get; set; }
        [NotMapped]
        public string CpfCnpj { get; set; }
        public decimal PercentualComissao { get; set; }
        public string ChavePix { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public Guid? VendedorId { get; set; }
        public Vendedor VendedorPai { get; set; }
        public ICollection<Vendedor> VendedoresFilhos { get; set; }
        [NotMapped]
        public Usuario Usuario { get; set; }
    }
} 