using Projeto.Moope.Core.Models;
using Projeto.Moope.Core.Models.Validators.Base;

namespace Projeto.Moope.Core.Interfaces.Services
{
    public interface IUsuarioService
    {
        Task<Usuario> BuscarPorIdAsync(Guid id);
        Task<IEnumerable<Usuario>> BuscarTodosAsync();
        Task<Result<Usuario>> SalvarAsync(Usuario usuario);
        Task<Result<Usuario>> AtualizarAsync(Usuario usuario);
        Task<bool> RemoverAsync(Guid id);
        Task<Usuario> BuscarPorIdAsNotrackingAsync(Guid id);
    }
}