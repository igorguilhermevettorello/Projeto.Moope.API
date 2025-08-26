using Projeto.Moope.Core.Models;
using Projeto.Moope.Core.Models.Validators.Base;

namespace Projeto.Moope.Core.Interfaces.Services
{
    public interface IPapelService
    {
        Task<Papel> BuscarPorIdAsync(Guid id);
        Task<IEnumerable<Papel>> BuscarTodosAsync();
        Task<Result<Papel>> SalvarAsync(Papel papel);
        Task<Result<Papel>> AtualizarAsync(Papel papel);
        Task<bool> RemoverAsync(Guid id);
    }
}