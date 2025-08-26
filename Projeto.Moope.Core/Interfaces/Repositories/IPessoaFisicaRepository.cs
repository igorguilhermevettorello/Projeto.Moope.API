using Projeto.Moope.Core.Interfaces.Repositories.Base;
using Projeto.Moope.Core.Models;

namespace Projeto.Moope.Core.Interfaces.Repositories
{
    public interface IPessoaFisicaRepository : IRepository<PessoaFisica>
    {
        Task<PessoaFisica> BuscarPorCpfAsync(string cpf);
        Task<PessoaFisica> BuscarPorIdAsNotrackingAsync(Guid id);
    }
}