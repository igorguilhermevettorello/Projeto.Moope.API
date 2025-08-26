using FluentValidation;

namespace Projeto.Moope.Core.Models.Validators
{
    public class PlanoValidator : AbstractValidator<Plano>
    {
        public PlanoValidator()
        {
            RuleFor(x => x.Codigo)
                .NotEmpty().WithMessage("O Código é obrigatório.")
                .OverridePropertyName("Codigo");

            RuleFor(x => x.Descricao)
                .NotEmpty().WithMessage("A Descrição é obrigatória.")
                .Length(2, 100).WithMessage("O campo Descrição precisa ter entre {MinLength} e {MaxLength} caracteres")
                .OverridePropertyName("Descricao");

            RuleFor(x => x.Valor)
                .GreaterThan(0).WithMessage("O valor deve ser maior que zero.")
                .OverridePropertyName("Valor");
        }
    }
} 