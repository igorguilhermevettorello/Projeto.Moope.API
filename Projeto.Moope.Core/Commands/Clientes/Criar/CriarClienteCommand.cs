using System.ComponentModel.DataAnnotations;
using Projeto.Moope.Core.Commands.Base;
using Projeto.Moope.Core.Enums;
using Projeto.Moope.Core.Models.Validators.Base;

namespace Projeto.Moope.Core.Commands.Clientes.Criar
{
    public class CriarClienteCommand : ICommand<Result<Guid>>
    {
        [Required(ErrorMessage = "O campo Nome é obrigatório")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email deve ter um formato válido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo CpfCnpj é obrigatório")]
        public string CpfCnpj { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Telefone é obrigatório")]
        public string Telefone { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo TipoPessoa é obrigatório")]
        public TipoPessoa TipoPessoa { get; set; }

        public bool Ativo { get; set; } = true;

        [Required(ErrorMessage = "O campo Senha é obrigatório")]
        [MinLength(6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres")]
        public string Senha { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Confirmação é obrigatório")]
        [Compare("Senha", ErrorMessage = "A confirmação deve ser igual à senha")]
        public string Confirmacao { get; set; } = string.Empty;

        public string NomeFantasia { get; set; } = string.Empty;
        public string InscricaoEstadual { get; set; } = string.Empty;
        public Guid? VendedorId { get; set; }

        // Dados do Endereço (Opcionais)
        public string? Logradouro { get; set; }
        public string? Numero { get; set; }
        public string? Complemento { get; set; }
        public string? Bairro { get; set; }
        public string? Cidade { get; set; }
        public string? Estado { get; set; }
        public string? Cep { get; set; }
    }
}
