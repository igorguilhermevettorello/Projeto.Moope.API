using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Projeto.Moope.API.Controllers.Base;
using Projeto.Moope.API.DTOs.Clientes;
using Projeto.Moope.API.DTOs.Planos;
using Projeto.Moope.Core.Enums;
using Projeto.Moope.Core.Interfaces.Identity;
using Projeto.Moope.Core.Interfaces.Notifications;
using Projeto.Moope.Core.Interfaces.Services;
using Projeto.Moope.Core.Interfaces.UnitOfWork;
using Projeto.Moope.Core.Models;

namespace Projeto.Moope.API.Controllers
{
    [ApiController]
    [Route("api/plano")]
    [Authorize]
    public class PlanoController : MainController
    {
        private readonly IPlanoService _planoService;
        private readonly IValidator<CreatePlanoDto> _validator;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public PlanoController(
            IPlanoService planoService, 
            IValidator<CreatePlanoDto> validator, 
            IMapper mapper,
            IUnitOfWork unitOfWork,
            INotificador notificador,
            IUser user) : base(notificador, user)
        {
            _planoService = planoService;
            _validator = validator;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        [Authorize(Roles = nameof(TipoUsuario.Administrador))]
        [ProducesResponseType(typeof(CreatePlanoDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> BuscarTodosAsync()
        {
            var planos = await _planoService.BuscarTodosAsync();
            return Ok(_mapper.Map<IEnumerable<ListPlanoDto>>(planos));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> BuscarPorIdAsync(Guid id)
        {
            var plano = await _planoService.BuscarPorIdAsync(id);
            if (plano == null) return NotFound();
            return Ok(_mapper.Map<CreatePlanoDto>(plano));
        }

        [HttpPost]
        [Authorize(Roles = nameof(TipoUsuario.Administrador))]
        [ProducesResponseType(typeof(CreatePlanoDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> SalvarAsync(CreatePlanoDto planoDto)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var plano = _mapper.Map<Plano>(planoDto);
                var result = await _planoService.SalvarAsync(plano);
                if (!result.Status) throw new Exception(result.Mensagem);
                await _unitOfWork.CommitAsync();
                return Created(string.Empty, new { id = result.Dados.Id });
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                NotificarErro("Mensagem", ex.Message);
                return CustomResponse(); 
            }
        }
        
        [HttpPut("{id}")]
        [Authorize(Roles = nameof(TipoUsuario.Administrador))]
        [ProducesResponseType(typeof(UpdatePlanoDto), StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AtualizarAsync(Guid id, [FromBody] UpdatePlanoDto planoDto)
        {
            if (id == Guid.Empty || planoDto.Id == Guid.Empty)
            {
                ModelState.AddModelError("Id", "Campo Id está inválido.");
                return CustomResponse(ModelState);
            }
            
            if (id != planoDto.Id)
            {
                ModelState.AddModelError("Id", "Campo Id está inválido.");
                return CustomResponse(ModelState);
            }
            
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var plano = _mapper.Map<Plano>(planoDto);
                var result = await _planoService.AtualizarAsync(plano);
                if (!result.Status) throw new Exception(result.Mensagem);
                await _unitOfWork.CommitAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                NotificarErro("Mensagem", ex.Message);
                return CustomResponse(); 
            }
        }

        [HttpPut("inativar/{codigo}")]
        [Authorize(Roles = nameof(TipoUsuario.Administrador))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> InativarAsync(string codigo)
        {
            bool isValid = Guid.TryParse(codigo, out Guid id);
            if (!isValid)
            {
                ModelState.AddModelError("Id", "Campo Id está inválido.");
                return CustomResponse(ModelState);
            }
            
            var plano = await _planoService.BuscarPorIdAsync(id);
            if (plano == null) 
                return NotFound();
            
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var result = await _planoService.AtivarInativarAsync(plano, false);
                if (!result.Status) throw new Exception(result.Mensagem);
                await _unitOfWork.CommitAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                NotificarErro("Mensagem", ex.Message);
                return CustomResponse(); 
            }
        }

        [HttpPut("ativar/{codigo}")]
        [Authorize(Roles = nameof(TipoUsuario.Administrador))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AtivarAsync(string codigo)
        {
            bool isValid = Guid.TryParse(codigo, out Guid id);
            if (!isValid)
            {
                ModelState.AddModelError("Id", "Campo Id está inválido.");
                return CustomResponse(ModelState);
            }
            
            var plano = await _planoService.BuscarPorIdAsync(id);
            if (plano == null) 
                return NotFound();
            
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var result = await _planoService.AtivarInativarAsync(plano, true);
                if (!result.Status) throw new Exception(result.Mensagem);
                await _unitOfWork.CommitAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                NotificarErro("Mensagem", ex.Message);
                return CustomResponse(); 
            }
        }

        [HttpDelete("{codigo}")]
        [Authorize(Roles = nameof(TipoUsuario.Administrador))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Delete(string codigo)
        {
            // Supondo que o método DeleteAsync aceite Guid, será necessário buscar o plano por código antes
            return BadRequest("Delete por código não implementado");
        }
    }
} 