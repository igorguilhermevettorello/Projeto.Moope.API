using Projeto.Moope.Core.Models;
using Projeto.Moope.Core.Models.Validators.Base;

namespace Projeto.Moope.Core.Interfaces.Services
{
    public interface ITransacaoService
    {
        Task<Transacao> BuscarPorIdAsync(Guid id);
        Task<IEnumerable<Transacao>> BuscarTodosAsync();
        Task<Result<Transacao>> SalvarAsync(Transacao transacao);
        Task<Result<Transacao>> AtualizarAsync(Transacao transacao);
        Task<bool> RemoverAsync(Guid id);
    }
}