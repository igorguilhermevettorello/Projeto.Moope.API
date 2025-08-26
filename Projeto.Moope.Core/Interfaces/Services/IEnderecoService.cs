using Projeto.Moope.Core.Models;
using Projeto.Moope.Core.Models.Validators.Base;

namespace Projeto.Moope.Core.Interfaces.Services
{
    public interface IEnderecoService
    {
        Task<Endereco> BuscarPorIdAsNotrackingAsync(Guid id);
        Task<Endereco> BuscarPorIdAsync(Guid id);
        Task<IEnumerable<Endereco>> BuscarTodosAsync();
        Task<Result<Endereco>> SalvarAsync(Endereco endereco);
        Task<Result<Endereco>> AtualizarAsync(Endereco endereco);
        Task<bool> RemoverAsync(Guid id);
    }
}