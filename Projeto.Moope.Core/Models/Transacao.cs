using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Projeto.Moope.Core.Enums;

namespace Projeto.Moope.Core.Models
{
    [Table("Transacao")]
    public class Transacao
    {
        [Key]
        public Guid Id { get; set; }
        public Guid PedidoId { get; set; }
        public decimal Valor { get; set; }
        public DateTime DataPagamento { get; set; }
        public StatusPagamento StatusPagamento { get; set; }
        public string? Status { get; set; }
        public string? StatusDescricao { get; set; }
        public int? GalaxPayId { get; set; }
        public string MetodoPagamento { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public Pedido Pedido { get; set; }
    }
} 