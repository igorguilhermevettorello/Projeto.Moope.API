using FluentValidation;
using Projeto.Moope.API.DTOs.Clientes;

namespace Projeto.Moope.API.DTOs.Validators
{
    public class CreateClienteDtoValidator : AbstractValidator<CreateClienteDto>
    {
        public CreateClienteDtoValidator()
        {
            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("O nome é obrigatório.")
                .Length(2, 100).WithMessage("O nome deve ter entre 2 e 100 caracteres.")
                .Matches(@"^[a-zA-ZÀ-ÿ\s]+$").WithMessage("O nome deve conter apenas letras e espaços.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("O email é obrigatório.")
                .EmailAddress().WithMessage("Email deve ter um formato válido.")
                .Length(5, 100).WithMessage("O email deve ter entre 5 e 100 caracteres.");

            RuleFor(x => x.CpfCnpj)
                .NotEmpty().WithMessage("O CPF é obrigatório.")
                .Length(11, 14).WithMessage("CPF deve ter formato válido.")
                .Matches(@"^\d{3}\.?\d{3}\.?\d{3}-?\d{2}$").WithMessage("CPF deve ter formato válido (000.000.000-00 ou 00000000000).");

            RuleFor(x => x.Telefone)
                .NotEmpty().WithMessage("O Telefone é obrigatório.")
                .Matches(@"^\(\d{2}\)\s?\d{4,5}-?\d{4}$").WithMessage("Telefone deve ter formato válido ((00) 00000-0000 ou (00) 0000-0000).");

            RuleFor(x => x.TipoPessoa)
                .IsInEnum().WithMessage("Tipo de pessoa inválido.");

            RuleFor(x => x.Endereco)
                .NotNull().WithMessage("O endereço é obrigatório.");

            When(x => x.Endereco != null, () =>
            {
                RuleFor(x => x.Endereco.Cep)
                    .NotEmpty().WithMessage("O CEP é obrigatório.")
                    .Matches(@"^\d{5}-?\d{3}$").WithMessage("CEP deve ter formato válido (00000-000 ou 00000000).");

                RuleFor(x => x.Endereco.Logradouro)
                    .NotEmpty().WithMessage("O logradouro é obrigatório.")
                    .Length(2, 100).WithMessage("O logradouro deve ter entre 2 e 100 caracteres.");

                RuleFor(x => x.Endereco.Numero)
                    .NotEmpty().WithMessage("O número é obrigatório.")
                    .Length(1, 10).WithMessage("O número deve ter entre 1 e 10 caracteres.");

                RuleFor(x => x.Endereco.Bairro)
                    .NotEmpty().WithMessage("O bairro é obrigatório.")
                    .Length(2, 50).WithMessage("O bairro deve ter entre 2 e 50 caracteres.");

                RuleFor(x => x.Endereco.Cidade)
                    .NotEmpty().WithMessage("A cidade é obrigatória.")
                    .Length(2, 50).WithMessage("A cidade deve ter entre 2 e 50 caracteres.");

                RuleFor(x => x.Endereco.Estado)
                    .NotEmpty().WithMessage("O estado é obrigatório.")
                    .Length(2, 2).WithMessage("O estado deve ter 2 caracteres (UF).")
                    .Matches(@"^[A-Z]{2}$").WithMessage("Estado deve ser uma UF válida (ex: SP, RJ).");

                RuleFor(x => x.Endereco.Complemento)
                    .MaximumLength(100).WithMessage("O complemento deve ter no máximo 100 caracteres.");
            });

            RuleFor(x => x.Senha)
                .NotEmpty().WithMessage("A senha é obrigatória.")
                .MinimumLength(6).WithMessage("A senha deve ter no mínimo 6 caracteres.")
                .MaximumLength(50).WithMessage("A senha deve ter no máximo 50 caracteres.")
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)").WithMessage("A senha deve conter pelo menos uma letra minúscula, uma maiúscula e um número.");

            RuleFor(x => x.Confirmacao)
                .NotEmpty().WithMessage("A confirmação da senha é obrigatória.")
                .Equal(x => x.Senha).WithMessage("A confirmação deve ser igual à senha.");
        }
    }
}