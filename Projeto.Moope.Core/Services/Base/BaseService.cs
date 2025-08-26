using FluentValidation;
using Projeto.Moope.Core.Interfaces.Notifications;
using Projeto.Moope.Core.Models.Base;
using Projeto.Moope.Core.Notifications;
using FluentValidation.Results;

namespace Projeto.Moope.Core.Services.Base
{
    public abstract class BaseService
    {
        private readonly INotificador _notificador;

        protected BaseService(INotificador notificador)
        {
            _notificador = notificador;
        }

        protected void Notificar(ValidationResult validationResult)
        {
            var erros = validationResult.Errors.Select(e => new
            {
                Campo = e.PropertyName,
                Mensagem = e.ErrorMessage
            }).ToList();

            foreach (var error in erros)
            {
                Notificar(error.Campo, error.Mensagem);
            }
        }

        protected void Notificar(string campo, string mensagem)
        {
            _notificador.Handle(new Notificacao
            {
                Campo = campo,
                Mensagem = mensagem
            });
        }

        protected bool ExecutarValidacao<TV, TE>(TV validacao, TE entidade) where TV : AbstractValidator<TE> where TE : Entity
        {
            var validator = validacao.Validate(entidade);

            if (validator.IsValid) return true;

            Notificar(validator);

            return false;
        }
        
        
    }
}
