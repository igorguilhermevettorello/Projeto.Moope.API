using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Projeto.Moope.API.Controllers.Base;
using Projeto.Moope.API.DTOs.Clientes;
using Projeto.Moope.API.DTOs.Vendas;
using Projeto.Moope.Core.Commands.Clientes.Criar;
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
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        
        public VendaController(
            IVendaService vendaService,
            IClienteService clienteService,
            IIdentityUserService identityUserService,
            IMapper mapper,
            IMediator mediator,
            INotificador notificador,
            IUser user) : base(notificador, user)
        {
            _vendaService = vendaService;
            _clienteService = clienteService;
            _identityUserService = identityUserService;
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
                
                if (!rsVenda.Status)
                    return CustomResponse();

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
    }
}
