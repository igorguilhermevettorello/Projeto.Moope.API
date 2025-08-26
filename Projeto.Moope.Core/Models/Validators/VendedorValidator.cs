using FluentValidation;
using Projeto.Moope.Core.Enums;

namespace Projeto.Moope.Core.Models.Validators
{
    public class VendedorValidator : AbstractValidator<Vendedor>
    {
        public VendedorValidator()
        {
            RuleFor(f => f.TipoPessoa)
                .IsInEnum().WithMessage("Tipo de Pessoa inválido.")
                .Equal(TipoPessoa.JURIDICA).WithMessage("Para Vendedor, o Tipo de Pessoa deve ser Jurídica.")
                .OverridePropertyName("TipoPessoa");
        }
    }
}