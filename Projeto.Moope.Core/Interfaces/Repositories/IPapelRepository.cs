using Projeto.Moope.Core.Interfaces.Repositories.Base;
using Projeto.Moope.Core.Models;

namespace Projeto.Moope.Core.Interfaces.Repositories
{
    public interface IPapelRepository : IRepository<Papel>
    {
        Task<IEnumerable<Papel>> BuscarPorUsuarioIdAsync(Guid usuarioId);
    }
}