using Projeto.Moope.API.DTOs;
using Projeto.Moope.Core.DTOs.Vendas;
using Projeto.Moope.Core.Interfaces.Notifications;
using Projeto.Moope.Core.Interfaces.Pagamentos;
using Projeto.Moope.Core.Interfaces.Repositories;
using Projeto.Moope.Core.Interfaces.Services;
using Projeto.Moope.Core.Interfaces.UnitOfWork;
using Projeto.Moope.Core.Models;
using Projeto.Moope.Core.Models.Validators.Base;
using Projeto.Moope.Core.Services.Base;

namespace Projeto.Moope.Core.Services
{
    public class VendaService : BaseService, IVendaService
    {
        private readonly IPaymentGatewayStrategy _paymentGateway;
        private readonly IClienteRepository _clienteRepository;
        private readonly IVendedorRepository _vendedorRepository;
        private readonly IPedidoRepository _pedidoRepository;
        private readonly ITransacaoRepository _transacaoRepository;
        private readonly IPlanoRepository _planoRepository;
        private readonly IUnitOfWork _unitOfWork;

        public VendaService(
            IPaymentGatewayStrategy paymentGateway,
            IClienteRepository clienteRepository,
            IVendedorRepository vendedorRepository,
            IPedidoRepository pedidoRepository,
            ITransacaoRepository transacaoRepository,
            IPlanoRepository planoRepository,
            IUnitOfWork unitOfWork,
            INotificador notificador) : base(notificador)
        {
            _paymentGateway = paymentGateway;
            _clienteRepository = clienteRepository;
            _vendedorRepository = vendedorRepository;
            _pedidoRepository = pedidoRepository;
            _transacaoRepository = transacaoRepository;
            _planoRepository = planoRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Pedido>> ProcessarVendaAsync(VendaStoreDto vendaDto)
        {
            try
            {
                // Validar se o vendedor existe
                var vendedor = await _vendedorRepository.BuscarPorIdAsNotrackingAsync(vendaDto.VendedorId);
                if (vendedor != null)
                    vendaDto.VendedorId = vendedor.Id;
                
                // Validar se o plano existe
                var plano = await _planoRepository.BuscarPorIdAsNotrackingAsync(vendaDto.PlanoId);
                if (plano == null)
                {
                    Notificar("Mensagem","Plano não encontrado");
                    return new Result<Pedido>() { Status = false, Mensagem = "Dados do plano são inválidos" };
                }

                // Validar se o plano está ativo
                if (!plano.Status)
                {
                    Notificar("Mensagem","Plano inativo");
                    return new Result<Pedido>() { Status = false, Mensagem = "Dados do plano são inválidos" };
                }

                // Criar ou buscar cliente
                var cliente = await CriarOuBuscarClienteAsync(vendaDto);
                if (cliente == null)
                {
                    Notificar("NomeCliente", "Falha ao processar dados do cliente");
                    return new Result<Pedido>() { Status = false, Mensagem = "Falha ao processar dados do cliente" };
                }

                var totalCalculado = plano.Valor * vendaDto.Quantidade;

                // Validar se o valor informado corresponde ao calculado
                if (vendaDto.Valor != totalCalculado)
                {
                    Notificar("Mensagem","Valor informado não corresponde ao valor calculado do plano");
                    return new Result<Pedido>() { Status = false, Mensagem = "Falha Valor informado não corresponde ao valor calculado do plano" };
                }

                // Criar pedido com snapshot do plano
                var pedido = new Pedido
                {
                    ClienteId = cliente.Id,
                    VendedorId = vendaDto.VendedorId,
                    PlanoId = vendaDto.PlanoId,
                    Quantidade = vendaDto.Quantidade,
                    
                    // Snapshot do plano no momento da venda
                    PlanoValor = plano.Valor,
                    PlanoDescricao = plano.Descricao,
                    PlanoCodigo = plano.Codigo,
                    
                    Total = totalCalculado,
                    Status = "PENDENTE",
                    StatusDescricao = "Aguardando processamento do pagamento",
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow
                };

                await _pedidoRepository.SalvarAsync(pedido);
                //await _unitOfWork.SaveChangesAsync();

                // Processar pagamento via gateway
                var resultadoPagamento = await _paymentGateway.ProcessarPagamentoAsync(vendaDto);

                // Atualizar status do pedido e criar transação
                if (resultadoPagamento.Status == "APPROVED" || resultadoPagamento.Status == "SUCCESS")
                {
                    pedido.Status = "APROVADO";
                    pedido.StatusDescricao = "Pagamento aprovado com sucesso";
                    pedido.Updated = DateTime.UtcNow;

                    var transacao = new Transacao
                    {
                        PedidoId = pedido.Id,
                        Valor = vendaDto.Valor,
                        DataPagamento = DateTime.UtcNow,
                        Status = "APROVADA",
                        StatusDescricao = "Pagamento aprovado com sucesso",
                        MetodoPagamento = "CARTAO_CREDITO",
                        Created = DateTime.UtcNow,
                        Updated = DateTime.UtcNow
                    };

                    await _transacaoRepository.SalvarAsync(transacao);
                    //await _unitOfWork.SaveChangesAsync();

                    return new Result<Pedido>() { 
                        Status = false, 
                        Mensagem = "Compra efetuada com sucesso",
                        Dados = pedido
                    };

                    //return new VendaResponseDto
                    //{
                    //    Id = pedido.Id,
                    //    Status = "APROVADA",
                    //    Mensagem = "Venda processada com sucesso",
                    //    CodigoTransacao = resultadoPagamento.Id,
                    //    DataProcessamento = DateTime.UtcNow,
                    //    Valor = totalCalculado,
                    //    NomeCliente = vendaDto.NomeCliente,
                    //    Email = vendaDto.Email,
                    //    VendedorId = vendaDto.VendedorId,
                    //    PlanoId = vendaDto.PlanoId,
                    //    NomePlano = pedido.PlanoDescricao,
                    //    CodigoPlano = pedido.PlanoCodigo,
                    //    ValorUnitarioPlano = pedido.PlanoValor,
                    //    Quantidade = vendaDto.Quantidade,
                    //    Sucesso = true
                    //};
                }
                else
                {
                    pedido.Status = "REJEITADO";
                    pedido.StatusDescricao = "Pagamento rejeitado";
                    pedido.Updated = DateTime.UtcNow;
                    //await _unitOfWork.SaveChangesAsync();

                    Notificar("Mensagem", resultadoPagamento.ErrorMessage ?? "Pagamento rejeitado");
                    return new Result<Pedido>() { 
                        Status = false, 
                        Mensagem = resultadoPagamento.ErrorMessage ?? "Pagamento rejeitado"
                    };

                    //return new VendaResponseDto
                    //{
                    //    Id = pedido.Id,
                    //    Status = "REJEITADA",
                    //    Mensagem = resultadoPagamento.ErrorMessage ?? "Pagamento rejeitado",
                    //    DataProcessamento = DateTime.UtcNow,
                    //    Valor = totalCalculado,
                    //    NomeCliente = vendaDto.NomeCliente,
                    //    Email = vendaDto.Email,
                    //    VendedorId = vendaDto.VendedorId,
                    //    PlanoId = vendaDto.PlanoId,
                    //    NomePlano = pedido.PlanoDescricao,
                    //    CodigoPlano = pedido.PlanoCodigo,
                    //    ValorUnitarioPlano = pedido.PlanoValor,
                    //    Quantidade = vendaDto.Quantidade,
                    //    Sucesso = false
                    //};
                }
            }
            catch (Exception ex)
            {
                Notificar("Mensagem",$"Erro ao processar venda: {ex.Message}");
                //return CriarRespostaErro($"Erro interno: {ex.Message}");

                return new Result<Pedido>()
                {
                    Status = false,
                    Mensagem = $"Erro ao processar venda: {ex.Message}"
                };
            }
        }

        public async Task<VendaResponseDto> ConsultarVendaAsync(Guid vendaId)
        {
            try
            {
                var pedido = await _pedidoRepository.BuscarPorIdAsync(vendaId);
                if (pedido == null)
                {
                    return CriarRespostaErro("Venda não encontrada");
                }

                var transacao = await _transacaoRepository.BuscarPorPedidoIdAsync(pedido.Id);
                
                return new VendaResponseDto
                {
                    Id = pedido.Id,
                    Status = pedido.Status,
                    Mensagem = transacao != null ? "Venda processada" : "Venda pendente",
                    CodigoTransacao = transacao?.Id.ToString(),
                    DataProcessamento = pedido.Created,
                    Valor = pedido.Total,
                    NomeCliente = pedido.Cliente?.ToString() ?? "N/A",
                    Email = "N/A", // Seria necessário adicionar email ao modelo Cliente
                    VendedorId = pedido.VendedorId ?? Guid.Empty,
                    PlanoId = pedido.PlanoId,
                    NomePlano = pedido.PlanoDescricao ?? "N/A",
                    CodigoPlano = pedido.PlanoCodigo ?? "N/A",
                    ValorUnitarioPlano = pedido.PlanoValor,
                    Quantidade = pedido.Quantidade,
                    Sucesso = pedido.Status == "APROVADO"
                };
            }
            catch (Exception ex)
            {
                Notificar("Mensagem",$"Erro ao consultar venda: {ex.Message}");
                return CriarRespostaErro($"Erro interno: {ex.Message}");
            }
        }

        public async Task<IEnumerable<VendaResponseDto>> ListarVendasPorVendedorAsync(Guid vendedorId)
        {
            try
            {
                var pedidos = await _pedidoRepository.BuscarPorVendedorIdAsync(vendedorId);
                var vendas = new List<VendaResponseDto>();

                foreach (var pedido in pedidos)
                {
                    var transacao = await _transacaoRepository.BuscarPorPedidoIdAsync(pedido.Id);
                    
                    vendas.Add(new VendaResponseDto
                    {
                        Id = pedido.Id,
                        Status = pedido.Status,
                        Mensagem = transacao != null ? "Venda processada" : "Venda pendente",
                        CodigoTransacao = transacao?.Id.ToString(),
                        DataProcessamento = pedido.Created,
                        Valor = pedido.Total,
                        NomeCliente = pedido.Cliente?.ToString() ?? "N/A",
                        Email = "N/A",
                        VendedorId = pedido.VendedorId ?? Guid.Empty,
                        PlanoId = pedido.PlanoId,
                        NomePlano = pedido.PlanoDescricao ?? "N/A",
                        CodigoPlano = pedido.PlanoCodigo ?? "N/A",
                        ValorUnitarioPlano = pedido.PlanoValor,
                        Quantidade = pedido.Quantidade,
                        Sucesso = pedido.Status == "APROVADO"
                    });
                }

                return vendas;
            }
            catch (Exception ex)
            {
                Notificar("Mensagem",$"Erro ao listar vendas: {ex.Message}");
                return Enumerable.Empty<VendaResponseDto>();
            }
        }

        public async Task<IEnumerable<VendaResponseDto>> ListarVendasPorClienteAsync(string email)
        {
            try
            {
                var cliente = await _clienteRepository.BuscarPorEmailAsync(email);
                if (cliente == null)
                {
                    return Enumerable.Empty<VendaResponseDto>();
                }

                var pedidos = await _pedidoRepository.BuscarPorClienteIdAsync(cliente.Id);
                var vendas = new List<VendaResponseDto>();

                foreach (var pedido in pedidos)
                {
                    var transacao = await _transacaoRepository.BuscarPorPedidoIdAsync(pedido.Id);
                    
                    vendas.Add(new VendaResponseDto
                    {
                        Id = pedido.Id,
                        Status = pedido.Status,
                        Mensagem = transacao != null ? "Venda processada" : "Venda pendente",
                        CodigoTransacao = transacao?.Id.ToString(),
                        DataProcessamento = pedido.Created,
                        Valor = pedido.Total,
                        NomeCliente = pedido.Cliente?.ToString() ?? "N/A",
                        Email = email,
                        VendedorId = pedido.VendedorId ?? Guid.Empty,
                        PlanoId = pedido.PlanoId,
                        NomePlano = pedido.PlanoDescricao ?? "N/A",
                        CodigoPlano = pedido.PlanoCodigo ?? "N/A",
                        ValorUnitarioPlano = pedido.PlanoValor,
                        Quantidade = pedido.Quantidade,
                        Sucesso = pedido.Status == "APROVADO"
                    });
                }

                return vendas;
            }
            catch (Exception ex)
            {
                Notificar("Mensagem",$"Erro ao listar vendas do cliente: {ex.Message}");
                return Enumerable.Empty<VendaResponseDto>();
            }
        }

        private async Task<Cliente?> CriarOuBuscarClienteAsync(VendaStoreDto vendaDto)
        {
            try
            {
                // Tentar buscar cliente existente por email
                var clienteExistente = await _clienteRepository.BuscarPorIdAsNotrackingAsync(vendaDto.ClienteId ?? Guid.Empty);
                if (clienteExistente != null)
                {
                    return clienteExistente;
                }

                // Criar novo cliente
                var novoCliente = new Cliente
                {
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow,
                    VendedorId = vendaDto.VendedorId
                };

                await _clienteRepository.SalvarAsync(novoCliente);
                //await _unitOfWork.SaveChangesAsync();

                return novoCliente;
            }
            catch
            {
                return null;
            }
        }

        private VendaResponseDto CriarRespostaErro(string mensagem)
        {
            return new VendaResponseDto
            {
                Id = Guid.Empty,
                Status = "ERRO",
                Mensagem = mensagem,
                DataProcessamento = DateTime.UtcNow,
                Valor = 0,
                NomeCliente = string.Empty,
                Email = string.Empty,
                VendedorId = Guid.Empty,
                Sucesso = false
            };
        }
    }
}
