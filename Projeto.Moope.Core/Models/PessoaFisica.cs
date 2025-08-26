using System.ComponentModel.DataAnnotations.Schema;
using Projeto.Moope.Core.Models.Base;

namespace Projeto.Moope.Core.Models
{
    [Table("PessoaFisica")]
    public class PessoaFisica : Entity
    {
        public string Nome { get; set; }
        public string Cpf { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
} 