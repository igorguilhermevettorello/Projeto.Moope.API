using FluentValidation;

namespace Projeto.Moope.Core.Models.Validators
{
    public class TransacaoValidator : AbstractValidator<Transacao>
    {
        public TransacaoValidator()
        {
            RuleFor(x => x.PedidoId).NotEmpty();
            RuleFor(x => x.Valor).GreaterThanOrEqualTo(0);
            RuleFor(x => x.DataPagamento).NotEmpty();
            RuleFor(x => x.Status).NotEmpty().MaximumLength(50);
            RuleFor(x => x.StatusDescricao).NotEmpty().MaximumLength(200);
            RuleFor(x => x.MetodoPagamento).NotEmpty().MaximumLength(50);
        }
    }
}