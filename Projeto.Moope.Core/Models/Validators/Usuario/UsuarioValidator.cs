using FluentValidation;
using UsuarioModel = Projeto.Moope.Core.Models.Usuario;
using System.Text.RegularExpressions;
using Projeto.Moope.Core.Enums;

namespace Projeto.Moope.Core.Models.Validators.Usuario
{
    public class UsuarioValidator : AbstractValidator<UsuarioModel>
    {
        public UsuarioValidator()
        {
            RuleFor(c => c.Nome)
                .NotEmpty().WithMessage("O campo {PropertyName} precisa ser fornecido")
                .Length(2, 200).WithMessage("O campo {PropertyName} precisa ter entre {MinLength} e {MaxLength} caracteres")
                .OverridePropertyName("Nome");

            RuleFor(x => x.TipoUsuario)
                .NotNull().WithMessage("Informe o tipo de usuário.")
                .IsInEnum().WithMessage("Tipo de usuário inválido.")
                .OverridePropertyName("TipoUsuario");
        }
    }
}