using Projeto.Moope.Core.Models;
using Projeto.Moope.Core.Models.Validators.Base;

namespace Projeto.Moope.Core.Interfaces.Services
{
    public interface IVendedorService
    {
        Task<Vendedor> BuscarPorIdAsNotrackingAsync(Guid id);
        Task<Vendedor> BuscarPorIdAsync(Guid id);
        Task<IEnumerable<Vendedor>> BuscarTodosAsync();
        Task<IEnumerable<Vendedor>> BuscarTodosComDadosRelacionadosAsync();
        Task<IEnumerable<T>> BuscarVendedoresComDadosAsync<T>();
        Task<T?> BuscarVendedorPorIdComDadosAsync<T>(Guid id);
        Task<Result<Vendedor>> SalvarAsync(Vendedor vendedor);
        Task<Result<Vendedor>> SalvarAsync(Vendedor vendedor, PessoaFisica pessoaFisica, PessoaJuridica pessoaJuridica);
        Task<Result<Vendedor>> AtualizarAsync(Vendedor vendedor, PessoaFisica pessoaFisica, PessoaJuridica pessoaJuridica);
        Task<bool> RemoverAsync(Guid id);
    }
}