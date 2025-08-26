using Projeto.Moope.Core.Models.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projeto.Moope.Core.Models
{
    [Table("Pedido")]
    public class Pedido : Entity
    {
        public Guid ClienteId { get; set; }
        public Guid? VendedorId { get; set; }
        public Guid PlanoId { get; set; }
        public int Quantidade { get; set; }
        
        // Snapshot do plano no momento da venda
        public decimal PlanoValor { get; set; }
        public string PlanoDescricao { get; set; }
        public string PlanoCodigo { get; set; }
        
        public decimal Total { get; set; }
        public string? Status { get; set; }
        public string? StatusDescricao { get; set; }
        public int? GalaxPayId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        
        // Navegações
        public Cliente Cliente { get; set; }
        public Vendedor? Vendedor { get; set; }
    }
} 