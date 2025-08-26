using Projeto.Moope.Core.Models;
using Projeto.Moope.Core.Models.Validators.Base;

namespace Projeto.Moope.Core.Interfaces.Services
{
    public interface IPedidoService
    {
        Task<Pedido> BuscarPorIdAsync(Guid id);
        Task<IEnumerable<Pedido>> BuscarTodosAsync();
        Task<Result<Pedido>> SalvarAsync(Pedido pedido);
        Task<Result<Pedido>> AtualizarAsync(Pedido pedido);
        Task<bool> RemoverAsync(Guid id);
    }
}