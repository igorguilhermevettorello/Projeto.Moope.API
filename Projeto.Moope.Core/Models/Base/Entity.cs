using System.ComponentModel.DataAnnotations;

namespace Projeto.Moope.Core.Models.Base
{
    public abstract class Entity
    {
        protected Entity()
        {
            Id = Guid.NewGuid();
        }

        [Key]
        public Guid Id { get; set; }
    }
}
