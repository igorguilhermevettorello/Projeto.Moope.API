using FluentValidation;
using Projeto.Moope.API.DTOs.Clientes;

namespace Projeto.Moope.API.DTOs.Validators
{
    public class AlterarSenhaAdminDtoValidator : AbstractValidator<AlterarSenhaAdminDto>
    {
        public AlterarSenhaAdminDtoValidator()
        {
            RuleFor(x => x.ClienteId)
                .NotEmpty()
                .WithMessage("O ID do cliente é obrigatório");

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
