using System.ComponentModel.DataAnnotations;
using Projeto.Moope.API.Attributes;
using Projeto.Moope.API.DTOs.Enderecos;
using Projeto.Moope.Core.Enums;

namespace Projeto.Moope.API.DTOs.Revendedor
{
    public class UpdateVendedorDto
    {
        [Required(ErrorMessage = "O campo Id é obrigatório")]
        public Guid Id { get; set; }
        
        [Required(ErrorMessage = "O campo Nome é obrigatório")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "O campo Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email deve ter um formato válido")]
        public string Email { get; set; }

        [Documento("TipoPessoa")]
        public string CpfCnpj { get; set; }

        [Required(ErrorMessage = "O campo Telefone é obrigatório")]
        public string Telefone { get; set; }

        [Required(ErrorMessage = "O campo TipoPessoa é obrigatório")]
        public TipoPessoa TipoPessoa { get; set; }

        public bool Ativo { get; set; } = true;

        [Required(ErrorMessage = "O campo Endereco é obrigatório")]
        public CreateEnderecoDto? Endereco { get; set; }

        public string NomeFantasia { get; set; }
        public string InscricaoEstadual { get; set; }
        public decimal PercentualComissao { get; set; }
        public string ChavePix { get; set; }
    }    
}

