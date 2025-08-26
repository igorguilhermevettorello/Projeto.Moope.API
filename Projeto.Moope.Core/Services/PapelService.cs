using Projeto.Moope.Core.Interfaces.Notifications;
using Projeto.Moope.Core.Interfaces.Repositories;
using Projeto.Moope.Core.Interfaces.Services;
using Projeto.Moope.Core.Models;
using Projeto.Moope.Core.Models.Validators.Base;
using Projeto.Moope.Core.Services.Base;

namespace Projeto.Moope.Core.Services
{
    public class PapelService  : BaseService, IPapelService
    {
        private readonly IPapelRepository _papelRepository;

        public PapelService(
            IPapelRepository papelRepository,
            INotificador notificador) : base(notificador)
        {
            _papelRepository = papelRepository;
        }

        public async Task<Papel> BuscarPorIdAsync(Guid id)
        {
            return await _papelRepository.BuscarPorIdAsync(id);
        }

        public Task<IEnumerable<Papel>> BuscarTodosAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<Result<Papel>> SalvarAsync(Papel papel)
        {
            var entity = await _papelRepository.SalvarAsync(papel);
            return new Result<Papel>()
            {
                Status = true,
                Dados = entity
            };
        }

        public Task<Result<Papel>> AtualizarAsync(Papel papel)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RemoverAsync(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}

