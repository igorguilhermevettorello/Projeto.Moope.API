using AutoMapper;
using Projeto.Moope.Core.Interfaces.Notifications;
using Projeto.Moope.Core.Interfaces.Repositories;
using Projeto.Moope.Core.Interfaces.Services;
using Projeto.Moope.Core.Models;
using Projeto.Moope.Core.Models.Validators;
using Projeto.Moope.Core.Models.Validators.Base;
using Projeto.Moope.Core.Models.Validators.Usuario;
using Projeto.Moope.Core.Services.Base;

namespace Projeto.Moope.Core.Services
{
    public class UsuarioService : BaseService, IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IPapelRepository _papelRepository;
        private readonly IMapper _mapper;
        public UsuarioService(
            IUsuarioRepository usuarioRepository,
            IPapelRepository papelRepository,
            IMapper mapper,
            INotificador notificador) : base(notificador)
        {
            _usuarioRepository = usuarioRepository;
            _papelRepository = papelRepository;
            _mapper = mapper;
        }

        public async Task<Usuario> BuscarPorIdAsync(Guid id)
        {
            return await _usuarioRepository.BuscarPorIdAsync(id);
        }

        public async Task<IEnumerable<Usuario>> BuscarTodosAsync()
        {
            return await _usuarioRepository.BuscarTodosAsync();
        }

        public async Task<Result<Usuario>> SalvarAsync(Usuario usuario)
        {
            if (!ExecutarValidacao(new UsuarioValidator(), usuario))
                return new Result<Usuario>() { Status = false, Mensagem = "Dados do usuário são inválidos"};

            usuario.Created = DateTime.UtcNow;
            var entity = await _usuarioRepository.SalvarAsync(usuario);
            
            await _papelRepository.SalvarAsync(new Papel()
            {
                Usuario = usuario,
                TipoUsuario = usuario.TipoUsuario,
                Created = DateTime.UtcNow
            });
            
            return new Result<Usuario>()
            {
                Status = true,
                Dados = entity
            };
        }

        public async Task<Result<Usuario>> AtualizarAsync(Usuario usuario)
        {
            if(!ExecutarValidacao(new UsuarioValidator(), usuario))
                return new Result<Usuario>() { Status = false };
            
            var usuarioAtual = await _usuarioRepository.BuscarPorIdAsync(usuario.Id);
            if (usuarioAtual == null)
                return new Result<Usuario> { Status = false, Mensagem = "Usuário não encontrado" };
            
            usuarioAtual.Nome = usuario.Nome;
            usuarioAtual.TipoUsuario = usuario.TipoUsuario;
            usuarioAtual.Updated = DateTime.UtcNow;
            
            var entity = await _usuarioRepository.AtualizarAsync(usuarioAtual);
            return new Result<Usuario>()
            {
                Status = true,
                Dados = entity
            };
        }

        public async Task<bool> RemoverAsync(Guid id)
        {
            await _usuarioRepository.RemoverAsync(id);
            return true;
        }

        public async Task<Usuario> BuscarPorIdAsNotrackingAsync(Guid id)
        {
            return await _usuarioRepository.BuscarPorIdAsNotrackingAsync(id);
        }
    }
} 