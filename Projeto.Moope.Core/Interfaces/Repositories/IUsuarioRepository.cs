using Projeto.Moope.Core.Interfaces.Repositories.Base;
using Projeto.Moope.Core.Models;

namespace Projeto.Moope.Core.Interfaces.Repositories
{
    public interface IUsuarioRepository : IRepository<Usuario>
    {
        Task<Usuario> BuscarPorIdAsNotrackingAsync(Guid id);
    }
}