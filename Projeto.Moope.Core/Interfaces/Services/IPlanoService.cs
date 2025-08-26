using Projeto.Moope.Core.Models;
using Projeto.Moope.Core.Models.Validators.Base;

namespace Projeto.Moope.Core.Interfaces.Services
{
    public interface IPlanoService
    {
        Task<Plano> BuscarPorIdAsync(Guid id);
        Task<IEnumerable<Plano>> BuscarTodosAsync();
        Task<Result<Plano>> SalvarAsync(Plano plano);
        Task<Result<Plano>> AtualizarAsync(Plano plano);
        Task<Result<Plano>> AtivarInativarAsync(Plano plano, bool status);
        Task<bool> RemoverAsync(Guid id);
    }
} 