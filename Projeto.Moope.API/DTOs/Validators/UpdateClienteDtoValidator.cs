using FluentValidation;
using Projeto.Moope.API.DTOs.Clientes;

namespace Projeto.Moope.API.DTOs.Validators
{
    public class UpdateClienteDtoValidator : AbstractValidator<UpdateClienteDto>
    {
        public UpdateClienteDtoValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("O Id é obrigatório.");

            // RuleFor(x => x.PapelId)
            //     .NotEmpty().WithMessage("O PapelId é obrigatório.");

            RuleFor(x => x.TipoPessoa)
                .IsInEnum().WithMessage("Tipo de pessoa inválido.");
        }
    }
}