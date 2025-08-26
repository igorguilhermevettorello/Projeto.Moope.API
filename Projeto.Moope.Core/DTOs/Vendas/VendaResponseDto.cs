namespace Projeto.Moope.Core.DTOs.Vendas
{
    public class VendaResponseDto
    {
        public Guid Id { get; set; }
        public string Status { get; set; }
        public string Mensagem { get; set; }
        public string? CodigoTransacao { get; set; }
        public DateTime DataProcessamento { get; set; }
        public decimal Valor { get; set; }
        public string NomeCliente { get; set; }
        public string Email { get; set; }
        public Guid VendedorId { get; set; }
        public Guid PlanoId { get; set; }
        public string NomePlano { get; set; }
        public string CodigoPlano { get; set; }
        public decimal ValorUnitarioPlano { get; set; }
        public int Quantidade { get; set; }
        public bool Sucesso { get; set; }
    }
}
