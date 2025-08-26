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

        public ProcessarVendaCommandHandler(
            IPaymentGatewayStrategy paymentGateway,
            IClienteRepository clienteRepository,
            IVendedorRepository vendedorRepository,
            IPedidoRepository pedidoRepository,
            ITransacaoRepository transacaoRepository,
            IPlanoRepository planoRepository,
            IUnitOfWork unitOfWork,
            INotificador notificador)
        {
            _paymentGateway = paymentGateway;
            _clienteRepository = clienteRepository;
            _vendedorRepository = vendedorRepository;
            _pedidoRepository = pedidoRepository;
            _transacaoRepository = transacaoRepository;
            _planoRepository = planoRepository;
            _unitOfWork = unitOfWork;
            _notificador = notificador;
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
                            Campo = "Vendedor",
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
                        Campo = "Plano",
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
                        Campo = "Plano",
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
                    Status = "PENDENTE",
                    StatusDescricao = "Aguardando criação da subscription",
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow
                };

                await _pedidoRepository.SalvarAsync(pedido);

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

                var resultadoPagamento = await _paymentGateway.CriarSubscriptionComPlanoAsync(subscriptionDto);

                // Atualizar status do pedido e criar transação
                if (resultadoPagamento.Status == "ACTIVE" || resultadoPagamento.Status == "PENDING")
                {
                    pedido.Status = "APROVADO";
                    pedido.StatusDescricao = "Subscription criada com sucesso";
                    pedido.Updated = DateTime.UtcNow;

                    var transacao = new Transacao
                    {
                        PedidoId = pedido.Id,
                        Valor = request.Valor,
                        DataPagamento = DateTime.UtcNow,
                        Status = "APROVADA",
                        StatusDescricao = "Subscription criada com sucesso",
                        MetodoPagamento = "SUBSCRIPTION",
                        Created = DateTime.UtcNow,
                        Updated = DateTime.UtcNow
                    };

                    await _transacaoRepository.SalvarAsync(transacao);

                    return new Result<Pedido> 
                    { 
                        Status = true, 
                        Mensagem = "Subscription criada com sucesso",
                        Dados = pedido
                    };
                }
                else
                {
                    pedido.Status = "REJEITADO";
                    pedido.StatusDescricao = "Falha na criação da subscription";
                    pedido.Updated = DateTime.UtcNow;

                    _notificador.Handle(new Notificacao()
                    {
                        Campo = "Subscription",
                        Mensagem = resultadoPagamento.ErrorMessage ?? "Subscription rejeitada"
                    });

                    return new Result<Pedido> 
                    { 
                        Status = false, 
                        Mensagem = resultadoPagamento.ErrorMessage ?? "Subscription rejeitada"
                    };
                }
            }
            catch (Exception ex)
            {
                _notificador.Handle(new Notificacao()
                {
                    Campo = "Erro",
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
