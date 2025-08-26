using Projeto.Moope.Core.DTOs.Clientes;
using Projeto.Moope.Core.Models;
using Projeto.Moope.Core.Models.Validators.Base;

namespace Projeto.Moope.Core.Interfaces.Services
{
    public interface IClienteService
    {
        Task<Cliente> BuscarPorIdAsNotrackingAsync(Guid id);
        Task<Cliente> BuscarPorIdAsync(Guid id);
        Task<Cliente> BuscarPorEmailAsync(string email);
        Task<IEnumerable<Cliente>> BuscarTodosAsync();
        Task<Result<Cliente>> SalvarAsync(Cliente cliente);
        Task<Result<Cliente>> SalvarAsync(Cliente cliente, PessoaFisica pessoaFisica, PessoaJuridica pessoaJuridica);
        Task<Result<Cliente>> AtualizarAsync(Cliente cliente, PessoaFisica pessoaFisica, PessoaJuridica pessoaJuridica);
        Task<Result<Cliente>> AtualizarAsync(Cliente cliente);
        Task<bool> RemoverAsync(Guid id);
    }
}