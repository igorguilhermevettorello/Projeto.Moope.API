using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Projeto.Moope.Core.Enums;
using Projeto.Moope.Core.Interfaces.Identity;
using Projeto.Moope.Core.Interfaces.Notifications;
using Projeto.Moope.Core.Notifications;

namespace Projeto.Moope.API.Controllers.Base
{
    public class MainController : ControllerBase
    {
        private readonly INotificador _notificador;
        public readonly IUser AppUser;
        protected Guid UsuarioId { get; set; }
        protected bool UsuarioAutenticado { get; set; }

        protected MainController(INotificador notificador, IUser appUser)
        {
            _notificador = notificador;
            AppUser = appUser;

            if (appUser.IsAuthenticated())
            {
                UsuarioId = appUser.GetUserId();
                UsuarioAutenticado = true;
            }
        }
        
        protected async Task<bool> IsAdmin()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value == TipoUsuario.Administrador.ToString();
        }

        protected bool OperacaoValida()
        {
            return !_notificador.TemNotificacao();
        }

        protected ActionResult CustomResponse(object result = null)
        {
            if (OperacaoValida())
            {
                return Ok(result);
            }

            return BadRequest(_notificador.ObterNotificacoes().Select(n => n));
        }

        protected ActionResult CustomResponse(ModelStateDictionary modelState)
        {
            if (!modelState.IsValid) NotificarErroModelInvalida(modelState);
            return CustomResponse();
        }

        protected void NotificarErroModelInvalida(ModelStateDictionary modelState)
        {
            var erros = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => new
                {
                    Campo = ModelState
                        .FirstOrDefault(m => m.Value.Errors.Contains(e))
                        .Key,
                    Mensagem = e.ErrorMessage
                }).ToList();

            foreach (var erro in erros)
            {
                NotificarErro(erro.Campo, erro.Mensagem);
            }
        }

        protected void NotificarErro(string campo, string mensagem)
        {
            _notificador.Handle(new Notificacao
            {
                Campo = campo,
                Mensagem = mensagem
            });
        }
    }
}
