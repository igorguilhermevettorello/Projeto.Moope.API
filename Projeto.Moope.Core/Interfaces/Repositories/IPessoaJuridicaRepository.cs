using Projeto.Moope.Core.Enums;
using Projeto.Moope.Core.Interfaces.Repositories.Base;
using Projeto.Moope.Core.Models;

namespace Projeto.Moope.Core.Interfaces.Repositories
{
    public interface IPessoaJuridicaRepository : IRepository<PessoaJuridica>
    {
        Task<PessoaJuridica> BuscarPorCnpjAsync(string cnpj);
        Task<PessoaJuridica> BuscarPorIdAsNotrackingAsync(Guid id);
    }
}