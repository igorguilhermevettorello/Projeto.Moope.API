using Microsoft.Extensions.Logging;
using Projeto.Moope.API.DTOs;
using Projeto.Moope.Core.Commands.Base;
using Projeto.Moope.Core.DTOs.Pagamentos;
using Projeto.Moope.Core.Enums;
using Projeto.Moope.Core.Interfaces.Notifications;
using Projeto.Moope.Core.Interfaces.Pagamentos;
using Projeto.Moope.Core.Interfaces.Repositories;
using Projeto.Moope.Core.Interfaces.UnitOfWork;
using Projeto.Moope.Core.Models;
using Projeto.Moope.Core.Models.Validators.Base;
using Projeto.Moope.Core.Notifications;
using Projeto.Moope.Core.Utils;

namespace Projeto.Moope.Core.Commands.Vendas
{
    public class ProcessarVendaCommandHandler : ICommandHandler<ProcessarVendaCommand, Result<Pedido>>
    {
        private readonly IPaymentGatewayStrategy _paymentGateway;
        private readonly IClienteRepository _clienteRepository;
        private readonly IVendedorRepository _vendedorRepository;
        private readonly IPedidoRepository _pedidoRepository;
        private readonly ITransacaoRepository _transacaoRepository;
        private readonly IPlanoRepository _planoRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificador _notificador;
        private readonly ILogger<ProcessarVendaCommandHandler> _logger;

        public ProcessarVendaCommandHandler(
            IPaymentGatewayStrategy paymentGateway,
            IClienteRepository clienteRepository,
            IVendedorRepository vendedorRepository,
            IPedidoRepository pedidoRepository,
            ITransacaoRepository transacaoRepository,
            IPlanoRepository planoRepository,
            IUnitOfWork unitOfWork,
            INotificador notificador,
            ILogger<ProcessarVendaCommandHandler> logger)
        {
            _paymentGateway = paymentGateway;
            _clienteRepository = clienteRepository;
            _vendedorRepository = vendedorRepository;
            _pedidoRepository = pedidoRepository;
            _transacaoRepository = transacaoRepository;
            _planoRepository = planoRepository;
            _unitOfWork = unitOfWork;
            _notificador = notificador;
            _logger = logger;
        }

        public async Task<Result<Pedido>> Handle(ProcessarVendaCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Validar se o vendedor existe
                if (request.VendedorId != Guid.Empty)
                {
                    var vendedor = await _vendedorRepository.BuscarPorIdAsNotrackingAsync(request.VendedorId);
                    if (vendedor == null)
                    {
                        _notificador.Handle(new Notificacao()
                        {
                            Campo = "Mensagem",
                            Mensagem = "Vendedor não encontrado"
                        });

                        return new Result<Pedido>
                        {
                            Status = false,
                            Mensagem = "Vendedor não encontrado"
                        };
                    }
                }
                
                // Validar se o plano existe
                var plano = await _planoRepository.BuscarPorIdAsNotrackingAsync(request.PlanoId);
                if (plano == null)
                {
                    _notificador.Handle(new Notificacao()
                    {
                        Campo = "Mensagem",
                        Mensagem = "Plano não encontrado"
                    });
                    return new Result<Pedido> 
                    { 
                        Status = false, 
                        Mensagem = "Plano não encontrado" 
                    };
                }

                // Validar se o plano está ativo
                if (!plano.Status)
                {
                    _notificador.Handle(new Notificacao()
                    {
                        Campo = "Mensagem",
                        Mensagem = "Plano inativo"
                    });
                    return new Result<Pedido> 
                    { 
                        Status = false, 
                        Mensagem = "Plano inativo" 
                    };
                }

                // Calcular o valor total baseado no plano e quantidade
                var totalCalculado = plano.Valor * request.Quantidade;
                request.Valor = totalCalculado; // Atualizar o valor no request

                var clienteId = request.ClienteId;
                
                // Criar pedido com snapshot do plano
                var pedido = new Pedido
                {
                    ClienteId = (Guid)clienteId,
                    VendedorId = (request.VendedorId != Guid.Empty) ? request.VendedorId : null,
                    PlanoId = request.PlanoId,
                    Quantidade = request.Quantidade,
                    
                    // Snapshot do plano no momento da venda
                    PlanoValor = plano.Valor,
                    PlanoDescricao = plano.Descricao,
                    PlanoCodigo = plano.Codigo,
                    
                    Total = totalCalculado,
                    StatusAssinatura = StatusAssinatura.WaitingPayment,
                    Status = "PENDENTE",
                    StatusDescricao = "Aguardando criação da subscription",
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow
                };

                await _pedidoRepository.SalvarAsync(pedido);

                // Verificar se o cliente já existe no gateway de pagamento
                var clienteExisteNoGateway = await _paymentGateway.BuscarClientePorEmailAsync(request.Email);
                string? customerId = null;

                if (clienteExisteNoGateway.Type && clienteExisteNoGateway.HasCustomers)
                {
                    // Cliente já existe no gateway
                    customerId = clienteExisteNoGateway.FirstCustomer?.GalaxPayId.ToString();
                    _logger.LogInformation("Cliente encontrado no gateway CelPay. CustomerId: {CustomerId}", customerId);
                }
                else
                {
                    // Cliente não existe, criar no gateway
                    var customerDto = new CelPayCustomerRequestDto
                    {
                        MyId = request.ClienteId?.ToString(),
                        Name = request.NomeCliente,
                        Document = request.CpfCnpj,
                        Emails = new List<string> { request.Email },
                        Phones = new List<string> { request.Telefone }
                    };

                    var resultadoCriacaoCliente = await _paymentGateway.CriarClienteAsync(customerDto);
                    
                    if (resultadoCriacaoCliente.Type && resultadoCriacaoCliente.HasCustomers)
                    {
                        customerId = resultadoCriacaoCliente.FirstCustomer?.GalaxPayId.ToString();
                        _logger.LogInformation("Cliente criado no gateway CelPay. CustomerId: {CustomerId}", customerId);
                    }
                    else
                    {
                        _notificador.Handle(new Notificacao()
                        {
                            Campo = "Mensagem",
                            Mensagem = "Erro ao criar cliente no gateway de pagamento"
                        });
                        return new Result<Pedido> 
                        { 
                            Status = false, 
                            Mensagem = "Erro ao criar cliente no gateway de pagamento"
                        };
                    }
                }

                // Processar subscription com plano via gateway
                var subscriptionDto = new CelPaySubscriptionRequestDto
                {
                    ExternalId = pedido.Id.ToString(),
                    PlanId = plano.Codigo,
                    FirstPayDayDate = DateTime.Now.ToString("yyyy-MM-dd"),
                    Periodicity = Periodicidade.Monthly,
                    MainPaymentMethodId = MetodoPagamento.CreditCard,
                    Value = (int) (plano.Valor * request.Quantidade) * 100,
                    Card = new CardInfo
                    {
                        Number = request.NumeroCartao,
                        ExpMonth = ExtrairMesValidade(request.DataValidade),
                        ExpYear = ExtrairAnoValidade(request.DataValidade),
                        Cvv = request.Cvv,
                        HolderName = request.NomeCliente
                    },
                    Customer = new CustomerInfo
                    {
                        GalaxPayId = customerId,
                        Name = request.NomeCliente,
                        Emails = new string[] { request.Email },
                    },
                    Description = request.Descricao ?? $"Assinatura {plano.Descricao} - {request.NomeCliente}",
                    StartDate = DateTime.UtcNow,
                    Metadata = new SubscriptionMetadata
                    {
                        ClienteId = request.ClienteId?.ToString(),
                        VendedorId = request.VendedorId != Guid.Empty ? request.VendedorId.ToString() : null,
                        Observacoes = $"Pedido: {pedido.Id}"
                    }
                };

                // requisição para a CelPay
                var resultadoPagamento = await _paymentGateway.CriarSubscriptionComPlanoAsync(subscriptionDto);

                // Mapear status da assinatura
                // var statusAssinatura = StatusMapper.MapearStatusAssinatura(resultadoPagamento.Status, resultadoPagamento.ErrorMessage ?? "");
                // var statusAssinaturaDescricao = StatusMapper.ObterDescricaoEnum(statusAssinatura);

                var dadosStatus = EnumHelper.GetEnumInfo<StatusAssinatura>(resultadoPagamento.Status);
                pedido.StatusAssinatura = dadosStatus.Value.EnumValue;
                pedido.Status = resultadoPagamento.Status;
                pedido.StatusDescricao = dadosStatus?.Description;
                pedido.GalaxPayId = int.TryParse(resultadoPagamento.GalaxPayId, out var galaxPayId) ? galaxPayId : null;
                pedido.Updated = DateTime.UtcNow;

                // Processar transações retornadas pela plataforma
                foreach (var transaction in resultadoPagamento.Transactions)
                {
                    // var statusPagamento = StatusMapper.MapearStatusPagamento(transaction.Status, transaction.StatusDescription);
                    // var statusPagamentoDescricao = StatusMapper.ObterDescricaoEnum(statusPagamento);
                
                    var transactionStatus = EnumHelper.GetEnumInfo<StatusPagamento>(transaction.Status);
                    
                    var transacao = new Transacao
                    {
                        PedidoId = pedido.Id,
                        Valor = transaction.Value / 100m, // Converter de centavos para reais
                        DataPagamento = DateTime.TryParse(transaction.PaydayDate, out var paydayDate) ? paydayDate : DateTime.UtcNow,
                        StatusPagamento = transactionStatus.Value.EnumValue,
                        Status = transaction.Status,
                        StatusDescricao = transactionStatus?.Description,
                        GalaxPayId = transaction.GalaxPayId,
                        MetodoPagamento = "SUBSCRIPTION",
                        Created = DateTime.TryParse(transaction.CreatedAt, out var createdAt) ? createdAt : DateTime.UtcNow,
                        Updated = DateTime.UtcNow
                    };

                    await _transacaoRepository.SalvarAsync(transacao);
                }

                // Verificar se a subscription foi criada com sucesso
                if (dadosStatus.Value.EnumValue == StatusAssinatura.Active || dadosStatus.Value.EnumValue == StatusAssinatura.WaitingPayment)      
                {
                    return new Result<Pedido> 
                    { 
                        Status = true, 
                        Mensagem = "Pedido criado com sucesso!",
                        Dados = pedido
                    };
                }
                else
                {
                    _notificador.Handle(new Notificacao()
                    {
                        Campo = "Mensagem",
                        Mensagem = resultadoPagamento.ErrorMessage ?? "Pagamento rejeitado. Verifique os dados e tente novamente.",
                    });

                    return new Result<Pedido> 
                    { 
                        Status = false, 
                        Mensagem = resultadoPagamento.ErrorMessage ?? "Pagamento rejeitado. Verifique os dados e tente novamente."
                    };
                }
            }
            catch (Exception ex)
            {
                _notificador.Handle(new Notificacao()
                {
                    Campo = "Mensagem",
                    Mensagem = $"Erro ao processar venda: {ex.Message}"
                });
                return new Result<Pedido>
                {
                    Status = false,
                    Mensagem = $"Erro ao processar venda: {ex.Message}"
                };
            }
        }

        private string ExtrairMesValidade(string dataValidade)
        {
            var partes = dataValidade.Split('/');
            if (partes.Length == 2)
            {
                return partes[0];
            }
            
            throw new ArgumentException("Formato de data de validade inválido. Use MM/YY");
        }
        
        private string ExtrairAnoValidade(string dataValidade)
        {
            var partes = dataValidade.Split('/');
            if (partes.Length == 2)
            {
                return "20" + partes[1]; // Assumindo formato MM/YY
            }
            
            throw new ArgumentException("Formato de data de validade inválido. Use MM/YY");
        }


    }
}
