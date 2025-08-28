using Projeto.Moope.Core.Interfaces.Repositories.Base;
using Projeto.Moope.Core.Models;

namespace Projeto.Moope.Core.Interfaces.Repositories
{
    public interface IVendedorRepository : IRepository<Vendedor>
    {
        Task<Vendedor> BuscarPorIdAsNotrackingAsync(Guid id);
        Task<IEnumerable<Vendedor>> BuscarTodosComDadosRelacionadosAsync();
        Task<IEnumerable<T>> BuscarVendedoresComDadosAsync<T>();
        Task<T?> BuscarVendedorPorIdComDadosAsync<T>(Guid id);
    }
}