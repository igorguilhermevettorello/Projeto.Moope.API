using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Projeto.Moope.Core.Enums;
using Projeto.Moope.Core.Models.Base;

namespace Projeto.Moope.Core.Models
{
    [Table("Papel")]
    public class Papel : Entity
    {
        public Guid? UsuarioId { get; set; }
        public Usuario Usuario { get; set; }
        [Required]
        public TipoUsuario TipoUsuario { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}

