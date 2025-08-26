using Projeto.Moope.Core.Models.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projeto.Moope.Core.Models
{
    [Table("PessoaJuridica")]
    public class PessoaJuridica : Entity
    {
        public string Cnpj { get; set; }
        public string RazaoSocial { get; set; }
        public string NomeFantasia { get; set; }
        public string InscricaoEstadual { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
} 