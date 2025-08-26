using FluentValidation;
using EnderecoModel = Projeto.Moope.Core.Models.Endereco;
using System.Text.RegularExpressions;

namespace Projeto.Moope.Core.Models.Validators.Endereco
{
    public class EnderecoValidator : AbstractValidator<EnderecoModel>
    {
        public EnderecoValidator()
        {
            RuleFor(c => c.Logradouro)
                .NotEmpty().WithMessage("O campo {PropertyName} precisa ser fornecido")
                .Length(2, 200).WithMessage("O campo {PropertyName} precisa ter entre {MinLength} e {MaxLength} caracteres")
                .Matches(@"^[a-zA-ZÀ-ÿ0-9\s\-\.]+$").WithMessage("O campo {PropertyName} deve conter apenas letras, números, espaços, hífens e pontos")
                .OverridePropertyName("Logradouro");

            RuleFor(c => c.Bairro)
                .NotEmpty().WithMessage("O campo {PropertyName} precisa ser fornecido")
                .Length(2, 100).WithMessage("O campo {PropertyName} precisa ter entre {MinLength} e {MaxLength} caracteres")
                .Matches(@"^[a-zA-ZÀ-ÿ0-9\s\-]+$").WithMessage("O campo {PropertyName} deve conter apenas letras, números, espaços e hífens")
                .OverridePropertyName("Bairro");

            RuleFor(c => c.Cep)
                .NotEmpty().WithMessage("O campo {PropertyName} precisa ser fornecido")
                .Matches(@"^\d{8}$").WithMessage("O campo {PropertyName} deve conter exatamente 8 dígitos numéricos")
                .OverridePropertyName("CEP");

            RuleFor(c => c.Cidade)
                .NotEmpty().WithMessage("O campo {PropertyName} precisa ser fornecido")
                .Length(2, 100).WithMessage("O campo {PropertyName} precisa ter entre {MinLength} e {MaxLength} caracteres")
                .OverridePropertyName("Cidade");

            RuleFor(c => c.Estado)
                .NotEmpty().WithMessage("O campo {PropertyName} precisa ser fornecido")
                .Length(2, 2).WithMessage("O campo {PropertyName} precisa ter entre {MinLength} e {MaxLength} caracteres")
                .OverridePropertyName("Estado");

            RuleFor(c => c.Numero)
                .NotEmpty().WithMessage("O campo {PropertyName} precisa ser fornecido")
                .Length(1, 50).WithMessage("O campo {PropertyName} precisa ter entre {MinLength} e {MaxLength} caracteres")
                .Matches(@"^[a-zA-Z0-9\-\/]+$").WithMessage("O campo {PropertyName} deve conter apenas letras, números, hífens e barras")
                .OverridePropertyName("Numero");

            // Validação para Complemento quando fornecido
            When(c => !string.IsNullOrEmpty(c.Complemento), () =>
            {
                RuleFor(c => c.Complemento)
                    .MaximumLength(100).WithMessage("O campo {PropertyName} deve ter no máximo {MaxLength} caracteres")
                    .Matches(@"^[a-zA-ZÀ-ÿ0-9\s\-\.\/]+$").WithMessage("O campo {PropertyName} deve conter apenas letras, números, espaços, hífens, pontos e barras")
                    .OverridePropertyName("Complemento");
            });
        }
    }
}
