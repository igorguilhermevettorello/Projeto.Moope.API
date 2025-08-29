using System.ComponentModel.DataAnnotations;
using Projeto.Moope.Core.Commands.Base;
using Projeto.Moope.Core.Enums;
using Projeto.Moope.Core.Models.Validators.Base;

namespace Projeto.Moope.Core.Commands.Emails
{
    /// <summary>
    /// Command para salvar email de confirmação de compra
    /// </summary>
    public class SalvarEmailConfirmacaoCompraCommand : ICommand<Result<Guid>>
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
        public DadosClienteNovoCommand? DadosClienteNovo { get; set; }
    }

    /// <summary>
    /// Dados de cliente novo para o comando
    /// </summary>
    public class DadosClienteNovoCommand
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
        public EnderecoClienteCommand? Endereco { get; set; }
    }

    /// <summary>
    /// Endereço do cliente para o comando
    /// </summary>
    public class EnderecoClienteCommand
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
