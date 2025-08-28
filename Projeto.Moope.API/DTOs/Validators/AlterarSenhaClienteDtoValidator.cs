using FluentValidation;
using Projeto.Moope.API.DTOs.Clientes;

namespace Projeto.Moope.API.DTOs.Validators
{
    public class AlterarSenhaClienteDtoValidator : AbstractValidator<AlterarSenhaClienteDto>
    {
        public AlterarSenhaClienteDtoValidator()
        {
            RuleFor(x => x.SenhaAtual)
                .NotEmpty()
                .WithMessage("A senha atual é obrigatória");

            RuleFor(x => x.NovaSenha)
                .NotEmpty()
                .WithMessage("A nova senha é obrigatória")
                .MinimumLength(6)
                .WithMessage("A nova senha deve ter pelo menos 6 caracteres")
                .Matches("[A-Z]")
                .WithMessage("A nova senha deve conter pelo menos uma letra maiúscula")
                .Matches("[a-z]")
                .WithMessage("A nova senha deve conter pelo menos uma letra minúscula")
                .Matches("[0-9]")
                .WithMessage("A nova senha deve conter pelo menos um dígito")
                .Matches("[^a-zA-Z0-9]")
                .WithMessage("A nova senha deve conter pelo menos um caractere especial");

            RuleFor(x => x.ConfirmarNovaSenha)
                .NotEmpty()
                .WithMessage("A confirmação da nova senha é obrigatória")
                .Equal(x => x.NovaSenha)
                .WithMessage("A confirmação da senha não confere com a nova senha");
        }
    }
}
