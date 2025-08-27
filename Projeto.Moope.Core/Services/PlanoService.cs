using Projeto.Moope.Core.Interfaces.Notifications;
using Projeto.Moope.Core.Interfaces.Repositories;
using Projeto.Moope.Core.Interfaces.Services;
using Projeto.Moope.Core.Models;
using Projeto.Moope.Core.Models.Validators;
using Projeto.Moope.Core.Models.Validators.Base;
using Projeto.Moope.Core.Services.Base;

namespace Projeto.Moope.Core.Services
{
    public class PlanoService : BaseService, IPlanoService
    {
        private readonly IPlanoRepository _planoRepository;
        public PlanoService(
            IPlanoRepository planoRepository,
            INotificador notificador) : base(notificador) 
        {
            _planoRepository = planoRepository;
        }

        public async Task<Plano> BuscarPorIdAsync(Guid id)
        {
            return await _planoRepository.BuscarPorIdAsync(id);
        }

        public async Task<IEnumerable<Plano>> BuscarTodosAsync()
        {
            return await _planoRepository.BuscarTodosAsync();
        }

        public async Task<Result<Plano>> SalvarAsync(Plano plano)
        {
            if (!ExecutarValidacao(new PlanoValidator(), plano))
            {
                return new Result<Plano>()
                {
                    Status = false
                };
            }

            var entity = await _planoRepository.SalvarAsync(plano);
            return new Result<Plano>()
            {
                Status = true,
                Dados = entity
            };
        }

        public async Task<Result<Plano>> AtualizarAsync(Plano plano)
        {
            if (!ExecutarValidacao(new PlanoValidator(), plano))
            {
                return new Result<Plano>()
                {
                    Status = false
                };
            }

            var entity = await _planoRepository.AtualizarAsync(plano);
            return new Result<Plano>()
            {
                Status = true,
                Dados = entity
            };
        }

        public async Task<bool> RemoverAsync(Guid id)
        {
            await _planoRepository.RemoverAsync(id);
            return true;
        }

        public async Task<Result<Plano>> AtivarInativarAsync(Plano plano, bool status)
        {
            plano.Status = status;
            var entity = await _planoRepository.AtualizarAsync(plano);
            return new Result<Plano>()
            {
                Status = true,
                Dados = entity
            };
        }
    }
} 