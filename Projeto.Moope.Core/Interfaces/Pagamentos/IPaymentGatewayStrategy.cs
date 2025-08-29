using Projeto.Moope.API.DTOs;
using Projeto.Moope.Core.DTOs.Pagamentos;
using Projeto.Moope.Core.DTOs.Vendas;

namespace Projeto.Moope.Core.Interfaces.Pagamentos
{
    public interface IPaymentGatewayStrategy
    {
        Task<CelPayResponseDto> ProcessarPagamentoAsync(VendaStoreDto vendaDto);
        Task<CelPayResponseDto> ConsultarTransacaoAsync(string transactionId);
        
        // Métodos para subscription
        Task<CelPaySubscriptionResponseDto> CriarSubscriptionComPlanoAsync(CelPaySubscriptionRequestDto subscriptionDto);
        Task<CelPaySubscriptionResponseDto> ConsultarSubscriptionAsync(string subscriptionId);
        Task<CelPaySubscriptionResponseDto> CancelarSubscriptionAsync(CelPayCancelSubscriptionDto cancelDto);
        Task<CelPaySubscriptionResponseDto> AtualizarSubscriptionAsync(CelPayUpdateSubscriptionDto updateDto);
        
        // Métodos para clientes
        Task<CelPayCustomerResponseDto> BuscarClientePorEmailAsync(string email);
        Task<CelPayCustomerResponseDto> CriarClienteAsync(CelPayCustomerRequestDto customerDto);
        
        string NomeGateway { get; }
    }
}
