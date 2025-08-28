using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Projeto.Moope.API.Controllers.Base;
using Projeto.Moope.API.DTOs.Clientes;
using Projeto.Moope.API.DTOs.Revendedor;
using Projeto.Moope.API.DTOs.Vendedor;
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
    [Route("api/vendedor")]
    [Authorize]
    public class VendedorController : MainController
    {
        private readonly IVendedorService _vendedorService;
        private readonly IUsuarioService _usuarioService;
        private readonly IEnderecoService _enderecoService;
        private readonly IIdentityUserService _identityUserService;
        private readonly IPapelService _papelService;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public VendedorController(
            IVendedorService vendedorService,
            IUsuarioService usuarioService,
            IEnderecoService enderecoService,
            IIdentityUserService identityUserService,
            IPapelService papelService,
            IMapper mapper,
            IUnitOfWork unitOfWork,
            INotificador notificador,
            IUser user) : base(notificador, user)
        {
            _vendedorService = vendedorService;
            _usuarioService = usuarioService;
            _enderecoService = enderecoService;
            _identityUserService = identityUserService;
            _papelService = papelService;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        [Authorize(Roles = nameof(TipoUsuario.Administrador))]
        [ProducesResponseType(typeof(IEnumerable<VendedorQueryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> BuscarTodosAsync()
        {
            var vendedores = await _vendedorService.BuscarVendedoresComDadosAsync<VendedorQueryDto>();
            return Ok(vendedores);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = nameof(TipoUsuario.Administrador))]
        [ProducesResponseType(typeof(VendedorDetalheDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> BuscarVendedorPorIdAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                ModelState.AddModelError("Id", "ID do vendedor é obrigatório");
                return CustomResponse(ModelState);
            }

            var vendedor = await _vendedorService.BuscarVendedorPorIdComDadosAsync<VendedorDetalheDto>(id);
            
            if (vendedor == null)
            {
                return NotFound("Vendedor não encontrado");
            }

            return Ok(vendedor);
        }

        [HttpPost]
        [Authorize(Roles = $"{nameof(TipoUsuario.Vendedor)},{nameof(TipoUsuario.Administrador)}")]
        [ProducesResponseType(typeof(CreateVendedorDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CriarAsync([FromBody] CreateVendedorDto createVendedorDto)
        {
            var vendedorId = Guid.NewGuid();
            var usuarioExistente = false;
            if (createVendedorDto == null)
            {
                NotificarErro("Mensagem", "As informações do cliente não foram carregadas. Tente novamente.");
                return CustomResponse();
            }
            
            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            var identityUser = new IdentityUser<Guid>();

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var vendedor = _mapper.Map<Vendedor>(createVendedorDto);
                var endereco = _mapper.Map<Endereco>(createVendedorDto.Endereco);
                var usuario =  _mapper.Map<Usuario>(createVendedorDto);
                var pessoaFisica = _mapper.Map<PessoaFisica>(createVendedorDto);
                var pessoaJuridica = _mapper.Map<PessoaJuridica>(createVendedorDto);
                
                var rsIdentity = await _identityUserService.CriarUsuarioAsync(
                    createVendedorDto.Email, 
                    createVendedorDto.Senha, 
                    telefone: createVendedorDto.Telefone, 
                    tipoUsuario: TipoUsuario.Vendedor);

                usuarioExistente = rsIdentity.UsuarioExiste;
                
                if (!rsIdentity.Status) 
                    throw new Exception(rsIdentity.Mensagem);

                identityUser = (IdentityUser<Guid>)rsIdentity.Dados;

                if (rsIdentity.UsuarioExiste)
                {
                    vendedor.Id = identityUser.Id;
                    vendedorId = vendedor.Id;
                    var rsPapel = await _papelService.SalvarAsync(new Papel()
                    {
                        UsuarioId = identityUser.Id,
                        TipoUsuario = TipoUsuario.Vendedor,
                        Created = DateTime.UtcNow
                    });
                    
                    if (!rsPapel.Status) 
                        throw new Exception(rsPapel.Mensagem);
                    
                    if (!await IsAdmin())
                    {
                        vendedor.VendedorId = UsuarioId;
                    }
                    var rsVendedor = await _vendedorService.SalvarAsync(vendedor);
                    if (!rsVendedor.Status) 
                        throw new Exception(rsVendedor.Mensagem);
                }
                else
                {
                    var rsEndereco = await _enderecoService.SalvarAsync(endereco);
                    if (!rsEndereco.Status) 
                        throw new Exception(rsEndereco.Mensagem);
                
                    usuario.Id =  rsIdentity.Dados.Id;
                    usuario.EnderecoId = rsEndereco.Dados.Id;
                    usuario.TipoUsuario = TipoUsuario.Vendedor;
                
                    var rsUsuario = await _usuarioService.SalvarAsync(usuario);
                    if (!rsUsuario.Status) 
                        throw new Exception(rsUsuario.Mensagem);
                
                    vendedor.Id = rsUsuario.Dados.Id;
                    pessoaFisica.Id = rsUsuario.Dados.Id;
                    pessoaJuridica.Id = rsUsuario.Dados.Id;
                    vendedorId = vendedor.Id;
                    if (!await IsAdmin())
                    {
                        vendedor.VendedorId = UsuarioId;
                    }
                    var rsVendedor = await _vendedorService.SalvarAsync(vendedor, pessoaFisica, pessoaJuridica);
                    if (!rsVendedor.Status) 
                        throw new Exception(rsVendedor.Mensagem);    
                }

                await _unitOfWork.CommitAsync();
                
                return Created(string.Empty, new { id = vendedorId });
            }
            catch (Exception ex)
            {
                if (!usuarioExistente)
                    _identityUserService.RemoverAoFalharAsync(identityUser);
                
                NotificarErro("Mensagem",  ex.Message);
                await _unitOfWork.RollbackAsync();
                return CustomResponse();
            }
        }
        
        [HttpPut("{id}")]
        [Authorize(Roles = $"{nameof(TipoUsuario.Vendedor)},{nameof(TipoUsuario.Administrador)}")]
        [ProducesResponseType(typeof(UpdateVendedorDto), StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AtualizarAsync(Guid id, [FromBody] UpdateVendedorDto updateVendedorDto)
        {
            if (id == Guid.Empty || updateVendedorDto.Id == Guid.Empty)
            {
                ModelState.AddModelError("Id", "Campo Id está inválido.");
                return CustomResponse(ModelState);
            }

            if (id != updateVendedorDto.Id)
            {
                ModelState.AddModelError("Id", "Campo Id do parâmetro não confere com o Id solicitado.");
                return CustomResponse(ModelState);
            }

            if (!ModelState.IsValid) 
                return CustomResponse(ModelState);

            var vendedorExistente = await _vendedorService.BuscarPorIdAsNotrackingAsync(id);
            if (vendedorExistente == null)
            {
                ModelState.AddModelError("Mensagem", "Vendedor não encontrado.");
                return CustomResponse(ModelState);
            }
            
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var vendedor = _mapper.Map<Vendedor>(updateVendedorDto);
                var endereco = _mapper.Map<Endereco>(updateVendedorDto.Endereco);
                var usuario =  _mapper.Map<Usuario>(updateVendedorDto);
                var pessoaFisica = _mapper.Map<PessoaFisica>(updateVendedorDto);
                var pessoaJuridica = _mapper.Map<PessoaJuridica>(updateVendedorDto);

                var rsIdentity = await _identityUserService.AlterarUsuarioAsync(
                    vendedorExistente.Id,
                    updateVendedorDto.Email, 
                    telefone: updateVendedorDto.Telefone);
                
                if (!rsIdentity.Status) 
                    throw new Exception(rsIdentity.Mensagem);

                var usuarioAuxiliar = await _usuarioService.BuscarPorIdAsNotrackingAsync(id);
                endereco.Id = (Guid) usuarioAuxiliar.EnderecoId;
                var rsEndereco = await _enderecoService.AtualizarAsync(endereco);
                if (!rsEndereco.Status) 
                    throw new Exception(rsEndereco.Mensagem);

                usuario.Id = vendedorExistente.Id;
                usuario.TipoUsuario = TipoUsuario.Vendedor;
                var rsUsuario = await _usuarioService.AtualizarAsync(usuario);
                if (!rsUsuario.Status) 
                    throw new Exception(rsUsuario.Mensagem);
                
                vendedor.Id = vendedorExistente.Id;
                var rsCliente = await _vendedorService.AtualizarAsync(vendedor, pessoaFisica, pessoaJuridica);
                if (!rsCliente.Status) 
                    throw new Exception(rsCliente.Mensagem);

                await _unitOfWork.CommitAsync();
                
                return NoContent();
            }
            catch (Exception ex)
            {
                NotificarErro("Mensagem", ex.Message);
                await _unitOfWork.RollbackAsync();
                return CustomResponse();
            }
        }
        
        [HttpGet("tipo-pessoa")]
        [ProducesResponseType(typeof(UpdateClienteDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult BuscarTipoPessoasAsync()
        {
            // Retorna apenas PessoaJuridica
            var lista = new List<object>
            {
                new { value = (int)TipoPessoa.JURIDICA, label = "Pessoa Jurídica" }
            };
            
            return Ok(lista);
        }
    }
}

