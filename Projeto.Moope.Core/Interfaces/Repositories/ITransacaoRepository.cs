using Projeto.Moope.Core.Interfaces.Repositories.Base;
using Projeto.Moope.Core.Models;

namespace Projeto.Moope.Core.Interfaces.Repositories
{
    public interface ITransacaoRepository : IRepository<Transacao>
    {
        Task<Transacao> BuscarPorPedidoIdAsync(Guid pedidoId);
    }
}