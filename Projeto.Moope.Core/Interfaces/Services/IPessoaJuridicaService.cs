using Projeto.Moope.Core.Models;
using Projeto.Moope.Core.Models.Validators.Base;

namespace Projeto.Moope.Core.Interfaces.Services
{
    public interface IPessoaJuridicaService
    {
        Task<PessoaJuridica> BuscarPorIdAsync(Guid id);
        Task<IEnumerable<PessoaJuridica>> BuscarTodosAsync();
        Task<Result<PessoaJuridica>> SalvarAsync(PessoaJuridica pessoaJuridica);
        Task<Result<PessoaJuridica>> AtualizarAsync(PessoaJuridica pessoaJuridica);
        Task<bool> RemoverAsync(Guid id);
    }
}