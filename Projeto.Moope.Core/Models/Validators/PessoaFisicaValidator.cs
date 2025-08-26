using FluentValidation;
using Projeto.Moope.Core.Validation;

namespace Projeto.Moope.Core.Models.Validators
{
    public class PessoaFisicaValidator : AbstractValidator<PessoaFisica>
    {
        public PessoaFisicaValidator()
        {
            RuleFor(c => c.Nome)
                .NotEmpty().WithMessage("O campo {PropertyName} precisa ser fornecido")
                .Length(2, 200).WithMessage("O campo {PropertyName} precisa ter entre {MinLength} e {MaxLength} caracteres")
                .Matches(@"^[a-zA-ZÀ-ÿ0-9\s\-\.]+$").WithMessage("O campo {PropertyName} deve conter apenas letras, números, espaços, hífens e pontos")
                .OverridePropertyName("Nome");
            
            RuleFor(x => x.Cpf)
                .Must(Documentos.IsValidCnpj)
                .WithMessage("O CNPJ informado não é válido")
                .OverridePropertyName("CpfCnpj");
        }
    }
}