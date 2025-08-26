using System.ComponentModel.DataAnnotations;
using Projeto.Moope.API.Attributes;
using Projeto.Moope.Core.Enums;

namespace Projeto.Moope.API.DTOs.Vendas
{
    public class CreateVendaDto
    {
        [Required(ErrorMessage = "Nome do cliente é obrigatório")]
        public string NomeCliente { get; set; }
        
        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; }
        
        [Required(ErrorMessage = "O campo Telefone é obrigatório")]
        public string Telefone { get; set; }
        
        [Required(ErrorMessage = "O campo TipoPessoa é obrigatório")]
        public TipoPessoa TipoPessoa { get; set; }
        
        [Documento("TipoPessoa")]
        public string CpfCnpj { get; set; }
        
        public Guid? VendedorId { get; set; }
        
        [Required(ErrorMessage = "ID do plano é obrigatório")]
        public Guid? PlanoId { get; set; }
        
        [Required(ErrorMessage = "Quantidade é obrigatória")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantidade deve ser maior que zero")]
        public int Quantidade { get; set; }
        
        // [Required(ErrorMessage = "Valor da venda é obrigatório")]
        // [Range(0.01, double.MaxValue, ErrorMessage = "Valor deve ser maior que zero")]
        // public decimal Valor { get; set; }
        
        [Required(ErrorMessage = "Nome do cartão de crédito é obrigatório")]
        public string NomeCartao { get; set; }
        
        [Required(ErrorMessage = "Número do cartão é obrigatório")]
        [RegularExpression(@"^\d{13,19}$", ErrorMessage = "Número do cartão deve ter entre 13 e 19 dígitos")]
        public string NumeroCartao { get; set; }
        
        [Required(ErrorMessage = "CVV é obrigatório")]
        [RegularExpression(@"^\d{3,4}$", ErrorMessage = "CVV deve ter 3 ou 4 dígitos")]
        public string Cvv { get; set; }
        
        [Required(ErrorMessage = "Data de validade é obrigatória")]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/([0-9]{2})$", ErrorMessage = "Data de validade deve estar no formato MM/YY")]
        public string DataValidade { get; set; }
    }    
}

