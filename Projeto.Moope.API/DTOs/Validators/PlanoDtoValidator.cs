using FluentValidation;
using Projeto.Moope.API.DTOs.Planos;

namespace Projeto.Moope.API.DTOs.Validators
{
    public class PlanoDtoValidator : AbstractValidator<CreatePlanoDto>
    {
        public PlanoDtoValidator()
        {
            RuleFor(x => x.Codigo)
                .NotEmpty().WithMessage("O código é obrigatório.")
                .MaximumLength(50).WithMessage("O código deve ter no máximo 50 caracteres.");

            RuleFor(x => x.Descricao)
                .NotEmpty().WithMessage("A descrição é obrigatória.")
                .Length(2, 100).WithMessage("A descrição deve ter entre 2 e 100 caracteres.");

            RuleFor(x => x.Valor)
                .GreaterThan(0).WithMessage("O valor deve ser maior que zero.");
        }
    }
} 