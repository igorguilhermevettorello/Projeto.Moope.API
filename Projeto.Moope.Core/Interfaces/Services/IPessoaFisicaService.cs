using Projeto.Moope.Core.Models;
using Projeto.Moope.Core.Models.Validators.Base;

namespace Projeto.Moope.Core.Interfaces.Services
{
    public interface IPessoaFisicaService
    {
        Task<PessoaFisica> BuscarPorIdAsync(Guid id);
        Task<IEnumerable<PessoaFisica>> BuscarTodosAsync();
        Task<Result<PessoaFisica>> SalvarAsync(PessoaFisica pessoaFisica);
        Task<Result<PessoaFisica>> AtualizarAsync(PessoaFisica pessoaFisica);
        Task<bool> RemoverAsync(Guid id);
    }
}