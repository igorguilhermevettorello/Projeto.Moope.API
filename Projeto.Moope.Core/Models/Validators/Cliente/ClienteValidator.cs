using FluentValidation;
using ClienteModel = Projeto.Moope.Core.Models.Cliente;
using Projeto.Moope.Core.Enums;
using Projeto.Moope.Core.Validation;

namespace Projeto.Moope.Core.Models.Validators.Cliente
{
    public class ClienteValidator : AbstractValidator<ClienteModel>
    {
        public ClienteValidator()
        {
            RuleFor(x => x.TipoPessoa)
                .IsInEnum().WithMessage("O campo {PropertyName} deve ser um valor válido")
                .NotEmpty().WithMessage("O campo {PropertyName} precisa ser fornecido")
                .OverridePropertyName("TipoPessoa");

            When(x => x.TipoPessoa == TipoPessoa.JURIDICA, () =>
            {
                RuleFor(x => x.CpfCnpj)
                    .Must(Documentos.IsValidCnpj)
                    .WithMessage("O CNPJ informado não é válido")
                    .OverridePropertyName("CpfCnpj");
            });
            
            When(x => x.TipoPessoa == TipoPessoa.FISICA, () =>
            {
                RuleFor(x => x.CpfCnpj)
                    .Must(Documentos.IsValidCpf)
                    .WithMessage("O CPF informado não é válido")
                    .OverridePropertyName("CpfCnpj");
            });
        }
    }
}