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
using Projeto.Moope.Core.Models;

namespace Projeto.Moope.API.Controllers
{
    [ApiController]
    [Route("api/plano")]
    [Authorize]
    public class PlanoController : MainController
    {
        private readonly IPlanoService _planoService;
        private readonly IValidator<PlanoDto> _validator;
        private readonly IMapper _mapper;

        public PlanoController(
            IPlanoService planoService, 
            IValidator<PlanoDto> validator, 
            IMapper mapper,
            INotificador notificador,
            IUser user) : base(notificador, user)
        {
            _planoService = planoService;
            _validator = validator;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> BuscarTodosAsync()
        {
            var planos = await _planoService.BuscarTodosAsync();
            return Ok(_mapper.Map<IEnumerable<PlanoDto>>(planos));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> BuscarPorIdAsync(Guid id)
        {
            var plano = await _planoService.BuscarPorIdAsync(id);
            if (plano == null) return NotFound();
            return Ok(_mapper.Map<PlanoDto>(plano));
        }

        [HttpPost]
        [Authorize(Roles = nameof(TipoUsuario.Administrador))]
        [ProducesResponseType(typeof(PlanoDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> SalvarAsync(PlanoDto planoDto)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);
            var plano = _mapper.Map<Plano>(planoDto);
            var result = await _planoService.SalvarAsync(plano);
            if (!result.Status) return CustomResponse();
            return CreatedAtAction(nameof(result.Dados.Id), new { id = result.Dados.Codigo }, _mapper.Map<PlanoDto>(result.Dados));
        }
        
        [HttpPut("{codigo}")]
        [Authorize(Roles = nameof(TipoUsuario.Administrador))]
        [ProducesResponseType(typeof(PlanoDto), StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AtualizarAsync(string codigo, [FromBody] PlanoDto planoDto)
        {
            if (codigo != planoDto.Codigo) return BadRequest();
            var validation = await _validator.ValidateAsync(planoDto);
            if (!validation.IsValid) return BadRequest(validation.Errors);
            var plano = _mapper.Map<Plano>(planoDto);
            var result = await _planoService.AtualizarAsync(plano);
            if (!result.Status) return CustomResponse();
            return NoContent();
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
            if (!isValid) return BadRequest();
            var plano = await _planoService.BuscarPorIdAsync(id);
            if (plano == null) return NotFound();
            var result = await _planoService.AtivarInativarAsync(plano, false);
            if (!result.Status) return CustomResponse();
            return NoContent();
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
            if (!isValid) return BadRequest();
            var plano = await _planoService.BuscarPorIdAsync(id);
            if (plano == null) return NotFound();
            var result = await _planoService.AtivarInativarAsync(plano, true);
            if (!result.Status) return CustomResponse();
            return NoContent();
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