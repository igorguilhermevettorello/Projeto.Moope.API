using System.ComponentModel.DataAnnotations;
using Projeto.Moope.Core.Enums;

namespace Projeto.Moope.API.DTOs.Emails
{
    /// <summary>
    /// DTO para dados do email de confirmação de compra
    /// </summary>
    public class EmailConfirmacaoCompraDto
    {
        /// <summary>
        /// ID do cliente
        /// </summary>
        [Required(ErrorMessage = "ID do cliente é obrigatório")]
        public Guid ClienteId { get; set; }

        /// <summary>
        /// Email do cliente
        /// </summary>
        [Required(ErrorMessage = "Email do cliente é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string EmailCliente { get; set; } = string.Empty;

        /// <summary>
        /// Nome do cliente
        /// </summary>
        [Required(ErrorMessage = "Nome do cliente é obrigatório")]
        public string NomeCliente { get; set; } = string.Empty;

        /// <summary>
        /// ID do pedido
        /// </summary>
        [Required(ErrorMessage = "ID do pedido é obrigatório")]
        public Guid PedidoId { get; set; }

        /// <summary>
        /// ID do plano
        /// </summary>
        [Required(ErrorMessage = "ID do plano é obrigatório")]
        public Guid PlanoId { get; set; }

        /// <summary>
        /// Nome do plano
        /// </summary>
        [Required(ErrorMessage = "Nome do plano é obrigatório")]
        public string NomePlano { get; set; } = string.Empty;

        /// <summary>
        /// Descrição do plano
        /// </summary>
        public string? DescricaoPlano { get; set; }

        /// <summary>
        /// Valor do plano
        /// </summary>
        [Required(ErrorMessage = "Valor do plano é obrigatório")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Valor deve ser maior que zero")]
        public decimal ValorPlano { get; set; }

        /// <summary>
        /// Quantidade comprada
        /// </summary>
        [Required(ErrorMessage = "Quantidade é obrigatória")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantidade deve ser maior que zero")]
        public int Quantidade { get; set; }

        /// <summary>
        /// Valor total da compra
        /// </summary>
        [Required(ErrorMessage = "Valor total é obrigatório")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Valor total deve ser maior que zero")]
        public decimal ValorTotal { get; set; }

        /// <summary>
        /// Status da assinatura
        /// </summary>
        [Required(ErrorMessage = "Status da assinatura é obrigatório")]
        public StatusAssinatura StatusAssinatura { get; set; }

        /// <summary>
        /// Status do pagamento
        /// </summary>
        public StatusPagamento? StatusPagamento { get; set; }

        /// <summary>
        /// Indica se é um cliente novo (true) ou existente (false)
        /// </summary>
        [Required(ErrorMessage = "Indicador de cliente novo é obrigatório")]
        public bool ClienteNovo { get; set; }

        /// <summary>
        /// ID do vendedor (opcional)
        /// </summary>
        public Guid? VendedorId { get; set; }

        /// <summary>
        /// Nome do vendedor (opcional)
        /// </summary>
        public string? NomeVendedor { get; set; }

        /// <summary>
        /// Data da compra
        /// </summary>
        [Required(ErrorMessage = "Data da compra é obrigatória")]
        public DateTime DataCompra { get; set; }

        /// <summary>
        /// Dados adicionais do cliente (para clientes novos)
        /// </summary>
        public DadosClienteNovoDto? DadosClienteNovo { get; set; }
    }

    /// <summary>
    /// DTO para dados de cliente novo
    /// </summary>
    public class DadosClienteNovoDto
    {
        /// <summary>
        /// Telefone do cliente
        /// </summary>
        public string? Telefone { get; set; }

        /// <summary>
        /// Tipo de pessoa
        /// </summary>
        public TipoPessoa? TipoPessoa { get; set; }

        /// <summary>
        /// CPF/CNPJ do cliente
        /// </summary>
        public string? CpfCnpj { get; set; }

        /// <summary>
        /// Endereço do cliente (opcional)
        /// </summary>
        public EnderecoClienteDto? Endereco { get; set; }
    }

    /// <summary>
    /// DTO para endereço do cliente
    /// </summary>
    public class EnderecoClienteDto
    {
        /// <summary>
        /// CEP
        /// </summary>
        public string? Cep { get; set; }

        /// <summary>
        /// Logradouro
        /// </summary>
        public string? Logradouro { get; set; }

        /// <summary>
        /// Número
        /// </summary>
        public string? Numero { get; set; }

        /// <summary>
        /// Complemento
        /// </summary>
        public string? Complemento { get; set; }

        /// <summary>
        /// Bairro
        /// </summary>
        public string? Bairro { get; set; }

        /// <summary>
        /// Cidade
        /// </summary>
        public string? Cidade { get; set; }

        /// <summary>
        /// Estado
        /// </summary>
        public string? Estado { get; set; }
    }
}
