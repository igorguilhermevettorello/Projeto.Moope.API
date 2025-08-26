using Projeto.Moope.Core.Interfaces.Notifications;
using Projeto.Moope.Core.Interfaces.Repositories;
using Projeto.Moope.Core.Interfaces.Services;
using Projeto.Moope.Core.Models;
using Projeto.Moope.Core.Models.Validators.Base;
using Projeto.Moope.Core.Models.Validators.Endereco;
using Projeto.Moope.Core.Services.Base;

namespace Projeto.Moope.Core.Services
{
    public class EnderecoService : BaseService, IEnderecoService
    {
        private readonly IEnderecoRepository _enderecoRepository;
        
        public EnderecoService(
            IEnderecoRepository enderecoRepository,
            INotificador notificador) : base(notificador) 
        {
            _enderecoRepository = enderecoRepository;
        }

        public async Task<Endereco> BuscarPorIdAsNotrackingAsync(Guid id)
        {
            return await _enderecoRepository.BuscarPorIdAsync(id);
        }
            
        public async Task<Endereco> BuscarPorIdAsync(Guid id)
        {
            return await _enderecoRepository.BuscarPorIdAsync(id);
        }

        public async Task<IEnumerable<Endereco>> BuscarTodosAsync()
        {
            return await _enderecoRepository.BuscarTodosAsync();
        }

        public async Task<Result<Endereco>> SalvarAsync(Endereco endereco)
        {
            if (!ExecutarValidacao(new EnderecoValidator(), endereco))
            {
                return new Result<Endereco>()
                {
                    Status = false
                };
            }

            endereco.Created = DateTime.UtcNow;
            var entity = await _enderecoRepository.SalvarAsync(endereco);
            return new Result<Endereco>()
            {
                Status = true,
                Dados = entity
            };
        }

        public async Task<Result<Endereco>> AtualizarAsync(Endereco endereco)
        {
            if (!ExecutarValidacao(new EnderecoValidator(), endereco))
            {
                return new Result<Endereco>()
                {
                    Status = false
                };
            }

            endereco.Updated = DateTime.UtcNow;
            var entity = await _enderecoRepository.AtualizarAsync(endereco);
            return new Result<Endereco>()
            {
                Status = true,
                Dados = entity
            };
        }

        public async Task<bool> RemoverAsync(Guid id)
        {
            await _enderecoRepository.RemoverAsync(id);
            return true;
        }
    }
}

