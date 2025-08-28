using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Projeto.Moope.API.Controllers.Base;
using Projeto.Moope.API.DTOs.Clientes;
using Projeto.Moope.Core.Commands.Clientes.Atualizar;
using Projeto.Moope.Core.Commands.Clientes.Criar;
using Projeto.Moope.Core.Enums;
using Projeto.Moope.Core.Interfaces.Identity;
using Projeto.Moope.Core.Interfaces.Notifications;
using Projeto.Moope.Core.Interfaces.Services;
using Projeto.Moope.Core.Interfaces.UnitOfWork;
using Projeto.Moope.Core.Models;
using Projeto.Moope.Infrastructure.Helpers;

namespace Projeto.Moope.API.Controllers
{
    [ApiController]
    [Route("api/cliente")]
    [Authorize]
    public class ClienteController : MainController
    {
        private readonly IClienteService _clienteService;
        private readonly IUsuarioService _usuarioService;
        private readonly IEnderecoService _enderecoService;
        private readonly IIdentityUserService _identityUserService;
        private readonly IPapelService _papelService;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMediator _mediator;

        public ClienteController(
            IClienteService clienteService,
            IUsuarioService usuarioService,
            IEnderecoService enderecoService,
            IIdentityUserService identityUserService,
            IPapelService papelService,
            IMapper mapper,
            IUnitOfWork unitOfWork,
            IMediator mediator,
            INotificador notificador,
            IUser user) : base(notificador, user)
        {
            _clienteService = clienteService;
            _usuarioService = usuarioService;
            _enderecoService = enderecoService;
            _identityUserService = identityUserService;
            _papelService = papelService;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _mediator = mediator;
        }

        [HttpGet]
        [Authorize(Roles = nameof(TipoUsuario.Administrador))]
        [ProducesResponseType(typeof(IEnumerable<ClienteQueryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> BuscarTodosAsync()
        {
            var clientes = await _clienteService.BuscarClientesComDadosAsync<ClienteQueryDto>();
            return Ok(clientes);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = nameof(TipoUsuario.Administrador))]
        [ProducesResponseType(typeof(ClienteDetalheDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> BuscarPorIdAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                ModelState.AddModelError("Id", "ID do cliente é obrigatório");
                return CustomResponse(ModelState);
            }

            var cliente = await _clienteService.BuscarClientePorIdComDadosAsync<ClienteDetalheDto>(id);
            
            if (cliente == null)
            {
                return NotFound("Cliente não encontrado");
            }

            return Ok(cliente);
        }

        [HttpGet("email/{email}")]
        [Authorize(Roles = nameof(TipoUsuario.Administrador))]
        [ProducesResponseType(typeof(ListClienteDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> BuscarPorEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError("Email", "Email é obrigatório");
                return CustomResponse(ModelState);
            }

            var cliente = await _clienteService.BuscarPorEmailAsync(email);
            if (cliente == null) 
                return NotFound("Cliente não encontrado");
            
            return Ok(_mapper.Map<ListClienteDto>(cliente));
        }

        [HttpPost]
        [Authorize(Roles = $"{nameof(TipoUsuario.Vendedor)},{nameof(TipoUsuario.Administrador)}")]
        [ProducesResponseType(typeof(CreateClienteDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CriarAsync([FromBody] CreateClienteDto createClienteDto)
        {
            if (createClienteDto == null)
            {
                NotificarErro("Mensagem", "As informações do cliente não foram carregadas. Tente novamente.");
                return CustomResponse();
            }
            
            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            try
            {
                var command = _mapper.Map<CriarClienteCommand>(createClienteDto);
                
                // Se não for admin, definir vendedor como o usuário logado
                if (!await IsAdmin())
                {
                    command.VendedorId = (UsuarioId == Guid.Empty) ? null : UsuarioId;
                }
                
                var resultado = await _mediator.Send(command);
                
                if (!resultado.Status)
                    return CustomResponse();

                return Created(string.Empty, new { id = resultado.Dados });
            }
            catch (Exception ex)
            {
                NotificarErro("Mensagem", ex.Message);
                return CustomResponse();
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = nameof(TipoUsuario.Administrador))]
        [ProducesResponseType(typeof(UpdateClienteDto), StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AtualizarAsync(Guid id, [FromBody] UpdateClienteDto updateClienteDto)
        {
            if (id == Guid.Empty || updateClienteDto.Id == Guid.Empty)
            {
                ModelState.AddModelError("Id", "Campo Id está inválido.");
                return CustomResponse(ModelState);
            }

            if (id != updateClienteDto.Id)
            {
                ModelState.AddModelError("Id", "Campo Id do parâmetro não confere com o Id solicitado.");
                return CustomResponse(ModelState);
            }

            if (!ModelState.IsValid) 
                return CustomResponse(ModelState);

            try
            {
                var command = _mapper.Map<AtualizarClienteCommand>(updateClienteDto);
                var resultado = await _mediator.Send(command);
                
                if (!resultado.Status)
                    return CustomResponse();

                return NoContent();
            }
            catch (Exception ex)
            {
                NotificarErro("Mensagem", ex.Message);
                return CustomResponse();
            }
        }


        [HttpPut("ativar/{id}")]
        [Authorize(Roles = nameof(TipoUsuario.Administrador))]
        [ProducesResponseType(typeof(UpdateClienteDto), StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AtivarAsync(Guid id)
        {
            var cliente = await _clienteService.BuscarPorIdAsync(id);
            if (cliente == null) 
                return NotFound("Cliente não encontrado");

            // cliente.Ativo = true;
            cliente.Updated = DateTime.UtcNow;

            var result = await _clienteService.AtualizarAsync(cliente);
            
            if (!result.Status) 
                return CustomResponse();

            return NoContent();
        }

        [HttpPut("inativar/{id}")]
        [Authorize(Roles = nameof(TipoUsuario.Administrador))]
        [ProducesResponseType(typeof(UpdateClienteDto), StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> InativarAsync(Guid id)
        {
            var cliente = await _clienteService.BuscarPorIdAsync(id);
            if (cliente == null) 
                return NotFound("Cliente não encontrado");

            // cliente.Ativo = false;
            cliente.Updated = DateTime.UtcNow;

            var result = await _clienteService.AtualizarAsync(cliente);
            
            if (!result.Status) 
                return CustomResponse();

            return NoContent();
        }

        [HttpGet("tipo-pessoa")]
        [ProducesResponseType(typeof(UpdateClienteDto), StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> BuscarTipoPessoasAsync()
        {
            var lista = EnumHelper.GetEnumAsList<TipoPessoa>();
            return Ok(lista);
        }

        [HttpPost("alterar-senha")]
        [Authorize(Roles = nameof(TipoUsuario.Cliente))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AlterarSenhaAsync([FromBody] AlterarSenhaClienteDto alterarSenhaDto)
        {
            if (alterarSenhaDto == null)
            {
                NotificarErro("Mensagem", "As informações da alteração de senha não foram carregadas. Tente novamente.");
                return CustomResponse();
            }

            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            try
            {
                // O cliente só pode alterar sua própria senha
                var clienteId = UsuarioId;
                
                if (clienteId == Guid.Empty)
                {
                    NotificarErro("Usuário", "Usuário não identificado");
                    return CustomResponse();
                }

                var resultado = await _clienteService.AlterarSenhaClienteAsync(
                    clienteId, 
                    alterarSenhaDto.SenhaAtual, 
                    alterarSenhaDto.NovaSenha);

                if (!resultado.Status)
                    return CustomResponse();

                return NoContent();
            }
            catch (Exception ex)
            {
                NotificarErro("Mensagem", ex.Message);
                return CustomResponse();
            }
        }

        [HttpPost("alterar-senha-admin")]
        [Authorize(Roles = $"{nameof(TipoUsuario.Administrador)},{nameof(TipoUsuario.Vendedor)}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AlterarSenhaAdminAsync([FromBody] AlterarSenhaAdminDto alterarSenhaDto)
        {
            if (alterarSenhaDto == null)
            {
                NotificarErro("Mensagem", "As informações da alteração de senha não foram carregadas. Tente novamente.");
                return CustomResponse();
            }

            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            try
            {
                var resultado = await _clienteService.AlterarSenhaAdminAsync(
                    alterarSenhaDto.ClienteId, 
                    alterarSenhaDto.NovaSenha);

                if (!resultado.Status)
                    return CustomResponse();

                return NoContent();
            }
            catch (Exception ex)
            {
                NotificarErro("Mensagem", ex.Message);
                return CustomResponse();
            }
        }

    }
}