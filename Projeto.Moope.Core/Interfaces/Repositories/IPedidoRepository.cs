using Projeto.Moope.Core.Interfaces.Repositories.Base;
using Projeto.Moope.Core.Models;

namespace Projeto.Moope.Core.Interfaces.Repositories
{
    public interface IPedidoRepository : IRepository<Pedido>
    {
        Task<IEnumerable<Pedido>> BuscarPorVendedorIdAsync(Guid vendedorId);
        Task<IEnumerable<Pedido>> BuscarPorClienteIdAsync(Guid clienteId);
    }
}