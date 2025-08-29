using Projeto.Moope.Core.Models;
using Projeto.Moope.Core.Interfaces.Repositories.Base;

namespace Projeto.Moope.Core.Interfaces.Repositories
{
    public interface IPlanoRepository : IRepository<Plano>
    {
        Task<Plano> BuscarPorIdAsNotrackingAsync(Guid id);
        Task<Plano> BuscarPorPlanoSelecionadoAsync(string codigo);
        
    }
} 