using FluentValidation;
using Projeto.Moope.API.DTOs;
using Projeto.Moope.Core.DTOs.Vendas;

namespace Projeto.Moope.Core.DTOs.Validators
{
    public class CreateVendaDtoValidator : AbstractValidator<VendaStoreDto>
    {
        public CreateVendaDtoValidator()
        {
            RuleFor(x => x.NomeCliente)
                .NotEmpty().WithMessage("Nome do cliente é obrigatório")
                .MaximumLength(100).WithMessage("Nome do cliente deve ter no máximo 100 caracteres");

            RuleFor(x => x.NumeroCartao)
                .NotEmpty().WithMessage("Número do cartão é obrigatório")
                .Matches(@"^\d{13,19}$").WithMessage("Número do cartão deve ter entre 13 e 19 dígitos")
                .Must(ValidarLuhn).WithMessage("Número do cartão inválido");

            RuleFor(x => x.Cvv)
                .NotEmpty().WithMessage("CVV é obrigatório")
                .Matches(@"^\d{3,4}$").WithMessage("CVV deve ter 3 ou 4 dígitos");

            RuleFor(x => x.DataValidade)
                .NotEmpty().WithMessage("Data de validade é obrigatória")
                .Matches(@"^(0[1-9]|1[0-2])\/([0-9]{2})$").WithMessage("Data de validade deve estar no formato MM/YY")
                .Must(ValidarDataValidade).WithMessage("Data de validade deve ser futura");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email é obrigatório")
                .EmailAddress().WithMessage("Email inválido")
                .MaximumLength(100).WithMessage("Email deve ter no máximo 100 caracteres");

            RuleFor(x => x.Telefone)
                .NotEmpty().WithMessage("Telefone é obrigatório")
                .MaximumLength(20).WithMessage("Telefone deve ter no máximo 20 caracteres");

            RuleFor(x => x.Valor)
                .GreaterThan(0).WithMessage("Valor deve ser maior que zero")
                .LessThanOrEqualTo(999999.99m).WithMessage("Valor máximo permitido é R$ 999.999,99");

            RuleFor(x => x.VendedorId)
                .NotEmpty().WithMessage("ID do vendedor é obrigatório");

            RuleFor(x => x.PlanoId)
                .NotEmpty().WithMessage("ID do plano é obrigatório");

            RuleFor(x => x.Quantidade)
                .GreaterThan(0).WithMessage("Quantidade deve ser maior que zero")
                .LessThanOrEqualTo(1000).WithMessage("Quantidade máxima permitida é 1000");

            RuleFor(x => x.Descricao)
                .MaximumLength(500).WithMessage("Descrição deve ter no máximo 500 caracteres");
        }

        private bool ValidarLuhn(string numeroCartao)
        {
            if (string.IsNullOrEmpty(numeroCartao))
                return false;

            var soma = 0;
            var alternar = false;

            // Percorrer o número do cartão da direita para a esquerda
            for (int i = numeroCartao.Length - 1; i >= 0; i--)
            {
                var digito = numeroCartao[i] - '0';

                if (alternar)
                {
                    digito *= 2;
                    if (digito > 9)
                        digito = (digito % 10) + 1;
                }

                soma += digito;
                alternar = !alternar;
            }

            return soma % 10 == 0;
        }

        private bool ValidarDataValidade(string dataValidade)
        {
            if (string.IsNullOrEmpty(dataValidade))
                return false;

            try
            {
                var partes = dataValidade.Split('/');
                if (partes.Length != 2)
                    return false;

                var mes = int.Parse(partes[0]);
                var ano = int.Parse(partes[1]);

                if (mes < 1 || mes > 12)
                    return false;

                var anoCompleto = 2000 + ano;
                var dataValidadeCompleta = new DateTime(anoCompleto, mes, 1).AddMonths(1).AddDays(-1);
                var hoje = DateTime.Today;

                return dataValidadeCompleta > hoje;
            }
            catch
            {
                return false;
            }
        }
    }
}
