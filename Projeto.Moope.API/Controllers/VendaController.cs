using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Projeto.Moope.API.Controllers.Base;
using Projeto.Moope.API.DTOs.Clientes;
using Projeto.Moope.API.DTOs.Vendas;
using Projeto.Moope.Core.Commands.Clientes.Criar;
using Projeto.Moope.Core.Commands.Emails;
using Projeto.Moope.Core.Commands.Vendas;
using Projeto.Moope.Core.Interfaces.Identity;
using Projeto.Moope.Core.Interfaces.Notifications;
using Projeto.Moope.Core.Interfaces.Services;

namespace Projeto.Moope.API.Controllers
{
    [ApiController]
    [Route("api/venda")]
    [Authorize]
    public class VendaController : MainController
    {
        private readonly IVendaService _vendaService;
        private readonly IClienteService _clienteService;
        private readonly IIdentityUserService _identityUserService;
        private readonly IVendedorService _vendedorService;
        private readonly IPlanoService _planoService;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        
        public VendaController(
            IVendaService vendaService,
            IClienteService clienteService,
            IIdentityUserService identityUserService,
            IVendedorService vendedorService,
            IPlanoService planoService,
            IMapper mapper,
            IMediator mediator,
            INotificador notificador,
            IUser user) : base(notificador, user)
        {
            _vendaService = vendaService;
            _clienteService = clienteService;
            _identityUserService = identityUserService;
            _vendedorService = vendedorService;
            _planoService = planoService;
            _mapper = mapper;
            _mediator = mediator;
        }
        
        [AllowAnonymous]
        [HttpPost("processar")]
        [ProducesResponseType(typeof(CreateVendaDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ProcessarVenda([FromBody] CreateVendaDto vendaDto)
        {
            if (!ModelState.IsValid)
                return CustomResponse(ModelState);
            
            try
            {
                var command = _mapper.Map<ProcessarVendaCommand>(vendaDto);
                var clienteExiste = await _clienteService.BuscarPorEmailAsync(vendaDto.Email);
                if (clienteExiste == null)
                {
                    var cliente = _mapper.Map<CriarClienteCommand>(vendaDto);
                    if (!await IsAdmin())
                    {
                        cliente.VendedorId = (UsuarioId == Guid.Empty) ? null : UsuarioId;
                    }

                    var rsCliente = await _mediator.Send(cliente);
                    if (!rsCliente.Status)
                        return CustomResponse();
                    
                    command.ClienteId = rsCliente.Dados;
                }
                else
                {
                    command.ClienteId = clienteExiste.Id;
                }
                
                var rsVenda = await _mediator.Send(command);
                
                if (!rsVenda.Status) return CustomResponse();

                // Salvar email de confirmação de compra após sucesso da venda
                if (rsVenda.Dados != null)
                {
                    await SalvarEmailConfirmacaoCompra(vendaDto, rsVenda.Dados);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                NotificarErro("Mensagem", ex.Message);
                return CustomResponse();
            }
        }

        
        [HttpGet("{vendaId:guid}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ConsultarVenda(Guid vendaId)
        {
            try
            {
                var venda = await _vendaService.ConsultarVendaAsync(vendaId);
                
                if (venda.Id == Guid.Empty)
                {
                    return NotFound(new { error = "Venda não encontrada" });
                }

                return Ok(venda);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Erro interno ao consultar venda", details = ex.Message });
            }
        }

        [HttpGet("vendedor/{vendedorId:guid}")]
        public async Task<IActionResult> ListarVendasPorVendedor(Guid vendedorId)
        {
            try
            {
                var vendas = await _vendaService.ListarVendasPorVendedorAsync(vendedorId);
                return Ok(vendas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Erro interno ao listar vendas", details = ex.Message });
            }
        }

        [HttpGet("cliente/{email}")]
        public async Task<IActionResult> ListarVendasPorCliente(string email)
        {
            try
            {
                var vendas = await _vendaService.ListarVendasPorClienteAsync(email);
                return Ok(vendas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Erro interno ao listar vendas do cliente", details = ex.Message });
            }
        }
        
        [HttpGet("health")]
        [AllowAnonymous]
        public IActionResult HealthCheck()
        {
            return Ok(new { 
                status = "OK", 
                message = "API de Vendas funcionando normalmente",
                timestamp = DateTime.UtcNow,
                gateway = "CelPay"
            });
        }

        /// <summary>
        /// Endpoint para testar o salvamento de email de confirmação de compra
        /// </summary>
        [HttpPost("testar-email-confirmacao")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> TestarEmailConfirmacao([FromBody] CreateVendaDto vendaDto)
        {
            try
            {
                // Simular dados para teste
                var emailCommand = new Projeto.Moope.Core.Commands.Emails.SalvarEmailConfirmacaoCompraCommand
                {
                    ClienteId = Guid.NewGuid(),
                    EmailCliente = vendaDto.Email,
                    NomeCliente = vendaDto.NomeCliente,
                    PedidoId = Guid.NewGuid(),
                    PlanoId = vendaDto.PlanoId ?? Guid.NewGuid(),
                    NomePlano = "Plano Teste",
                    DescricaoPlano = "Plano de teste para validação",
                    ValorPlano = 99.90m,
                    Quantidade = vendaDto.Quantidade,
                    ValorTotal = 99.90m * vendaDto.Quantidade,
                    StatusAssinatura = Projeto.Moope.Core.Enums.StatusAssinatura.Active,
                    StatusPagamento = Projeto.Moope.Core.Enums.StatusPagamento.Captured,
                    ClienteNovo = true,
                    VendedorId = vendaDto.VendedorId,
                    NomeVendedor = "Vendedor Teste",
                    DataCompra = DateTime.UtcNow,
                    DadosClienteNovo = new Projeto.Moope.Core.Commands.Emails.DadosClienteNovoCommand
                    {
                        Telefone = vendaDto.Telefone,
                        TipoPessoa = vendaDto.TipoPessoa,
                        CpfCnpj = vendaDto.CpfCnpj,
                        Endereco = null
                    }
                };

                var resultado = await _mediator.Send(emailCommand);

                if (resultado.Status)
                {
                    return Ok(new { 
                        message = "Email de confirmação salvo com sucesso!",
                        emailId = resultado.Dados,
                        timestamp = DateTime.UtcNow
                    });
                }
                else
                {
                    return BadRequest(new { 
                        message = resultado.Mensagem,
                        timestamp = DateTime.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    error = "Erro interno ao testar email de confirmação", 
                    details = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Salva email de confirmação de compra
        /// </summary>
        private async Task SalvarEmailConfirmacaoCompra(CreateVendaDto vendaDto, Projeto.Moope.Core.Models.Pedido pedido)
        {
            try
            {
                // Buscar dados do cliente
                var cliente = await _clienteService.BuscarPorIdAsNotrackingAsync(pedido.ClienteId);
                if (cliente == null) return;

                // Buscar dados do plano
                var plano = await _planoService.BuscarPorIdAsync(pedido.PlanoId);
                if (plano == null) return;

                // Buscar dados do vendedor se informado
                var nomeVendedor = string.Empty;
                if (pedido.VendedorId.HasValue)
                {
                    var vendedor = await _vendedorService.BuscarPorIdAsNotrackingAsync(pedido.VendedorId.Value);
                    nomeVendedor = vendedor?.Usuario?.Nome ?? string.Empty;
                }

                // Determinar se é cliente novo (baseado na data de criação)
                var clienteNovo = cliente.Created.Date == DateTime.UtcNow.Date;

                // Criar comando para salvar email
                var emailCommand = new SalvarEmailConfirmacaoCompraCommand
                {
                    ClienteId = pedido.ClienteId,
                    EmailCliente = vendaDto.Email,
                    NomeCliente = vendaDto.NomeCliente,
                    PedidoId = pedido.Id,
                    PlanoId = plano.Id,
                    NomePlano = plano.Descricao,
                    DescricaoPlano = plano.Descricao,
                    ValorPlano = plano.Valor,
                    Quantidade = vendaDto.Quantidade,
                    ValorTotal = pedido.Total,
                    StatusAssinatura = pedido.StatusAssinatura,
                    StatusPagamento = null, // Será preenchido se houver transações
                    ClienteNovo = clienteNovo,
                    VendedorId = pedido.VendedorId,
                    NomeVendedor = nomeVendedor,
                    DataCompra = pedido.Created,
                    DadosClienteNovo = clienteNovo ? new DadosClienteNovoCommand
                    {
                        Telefone = vendaDto.Telefone,
                        TipoPessoa = vendaDto.TipoPessoa,
                        CpfCnpj = vendaDto.CpfCnpj,
                        Endereco = null // Pode ser expandido futuramente se necessário
                    } : null
                };

                // Enviar comando via MediatR
                await _mediator.Send(emailCommand);
            }
            catch (Exception ex)
            {
                // Log do erro mas não falha o processamento da venda
                NotificarErro("Email", $"Erro ao salvar email de confirmação: {ex.Message}");
            }
        }
    }
}
