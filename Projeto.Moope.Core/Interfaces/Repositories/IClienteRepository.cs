using Projeto.Moope.Core.Interfaces.Repositories.Base;
using Projeto.Moope.Core.Models;

namespace Projeto.Moope.Core.Interfaces.Repositories
{
    public interface IClienteRepository : IRepository<Cliente>
    {
        Task<Cliente> BuscarPorIdAsNotrackingAsync(Guid id);
        Task<Cliente> BuscarPorEmailAsync(string email);
    }
}