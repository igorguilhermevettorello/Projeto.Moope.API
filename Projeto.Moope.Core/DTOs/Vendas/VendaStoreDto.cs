using System.ComponentModel.DataAnnotations;

namespace Projeto.Moope.API.DTOs
{
    public class VendaStoreDto
    {
        [Required(ErrorMessage = "Nome do cliente é obrigatório")]
        public string NomeCliente { get; set; }
        
        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; }
        
        [Required(ErrorMessage = "O campo Telefone é obrigatório")]
        public string Telefone { get; set; }
        
        [Required(ErrorMessage = "Número do cartão é obrigatório")]
        [RegularExpression(@"^\d{13,19}$", ErrorMessage = "Número do cartão deve ter entre 13 e 19 dígitos")]
        public string NumeroCartao { get; set; }
        
        [Required(ErrorMessage = "CVV é obrigatório")]
        [RegularExpression(@"^\d{3,4}$", ErrorMessage = "CVV deve ter 3 ou 4 dígitos")]
        public string Cvv { get; set; }
        
        [Required(ErrorMessage = "Data de validade é obrigatória")]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/([0-9]{2})$", ErrorMessage = "Data de validade deve estar no formato MM/YY")]
        public string DataValidade { get; set; }
        
        
        
        [Required(ErrorMessage = "Valor da venda é obrigatório")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Valor deve ser maior que zero")]
        public decimal Valor { get; set; }
        
        [Required(ErrorMessage = "ID do vendedor é obrigatório")]
        public Guid VendedorId { get; set; }
        
        [Required(ErrorMessage = "ID do plano é obrigatório")]
        public Guid PlanoId { get; set; }
        
        [Required(ErrorMessage = "Quantidade é obrigatória")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantidade deve ser maior que zero")]
        public int Quantidade { get; set; }
        
        public string? Descricao { get; set; }
        public Guid? ClienteId { get; set; }
    }
}
