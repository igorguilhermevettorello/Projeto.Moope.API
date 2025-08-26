using Projeto.Moope.Core.Enums;
using Projeto.Moope.Core.Models.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projeto.Moope.Core.Models
{
    [Table("Usuario")]
    public class Usuario : Entity
    {
        [Required]
        [StringLength(255)]
        public string Nome { get; set; }

        // [Required]
        // [StringLength(255)]
        // public string Email { get; set; }

        // [StringLength(20)]
        // public string Telefone { get; set; }

        [NotMapped]
        [Required]
        public TipoUsuario TipoUsuario { get; set; }

        // [Required]
        // public bool Ativo { get; set; }

        public Guid? EnderecoId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }

        // [Required]
        // [ForeignKey("IdentityUserId")]
        // public string IdentityUserId { get; set; }
        public Endereco Endereco { get; set; }
    }
} 