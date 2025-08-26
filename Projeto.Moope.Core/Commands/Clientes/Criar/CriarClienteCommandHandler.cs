using Microsoft.AspNetCore.Identity;
using Projeto.Moope.Core.Commands.Base;
using Projeto.Moope.Core.Enums;
using Projeto.Moope.Core.Interfaces.Notifications;
using Projeto.Moope.Core.Interfaces.Repositories;
using Projeto.Moope.Core.Interfaces.Services;
using Projeto.Moope.Core.Interfaces.UnitOfWork;
using Projeto.Moope.Core.Models;
using Projeto.Moope.Core.Models.Validators.Base;
using Projeto.Moope.Core.Notifications;

namespace Projeto.Moope.Core.Commands.Clientes.Criar
{
    public class CriarClienteCommandHandler : ICommandHandler<CriarClienteCommand, Result<Guid>>
    {
        private readonly IClienteService _clienteService;
        private readonly IUsuarioService _usuarioService;
        private readonly IEnderecoService _enderecoService;
        private readonly IIdentityUserService _identityUserService;
        private readonly IPapelService _papelService;
        private readonly IPessoaFisicaRepository _pessoaFisicaRepository;
        private readonly IPessoaJuridicaRepository _pessoaJuridicaRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificador _notificador;

        public CriarClienteCommandHandler(
            IClienteService clienteService,
            IUsuarioService usuarioService,
            IEnderecoService enderecoService,
            IIdentityUserService identityUserService,
            IPapelService papelService,
            IPessoaFisicaRepository pessoaFisicaRepository,
            IPessoaJuridicaRepository pessoaJuridicaRepository,
            IUnitOfWork unitOfWork,
            INotificador notificador)
        {
            _clienteService = clienteService;
            _usuarioService = usuarioService;
            _enderecoService = enderecoService;
            _identityUserService = identityUserService;
            _papelService = papelService;
            _pessoaFisicaRepository = pessoaFisicaRepository;
            _pessoaJuridicaRepository = pessoaJuridicaRepository;
            _unitOfWork = unitOfWork;
            _notificador = notificador;
        }

        public async Task<Result<Guid>> Handle(CriarClienteCommand request, CancellationToken cancellationToken)
        {
            var clienteId = Guid.NewGuid();
            var usuarioExistente = false;
            var identityUser = new IdentityUser<Guid>();

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Criar entidades baseadas no command
                var cliente = CriarCliente(request);
                var endereco = CriarEndereco(request);
                var usuario = CriarUsuario(request);
                var pessoaFisica = CriarPessoaFisica(request);
                var pessoaJuridica = CriarPessoaJuridica(request);

                // Criar usuário no Identity
                var rsIdentity = await _identityUserService.CriarUsuarioAsync(
                    request.Email,
                    request.Senha,
                    telefone: request.Telefone,
                    tipoUsuario: TipoUsuario.Cliente);

                usuarioExistente = rsIdentity.UsuarioExiste;

                if (!rsIdentity.Status)
                {
                    _notificador.Handle(new Notificacao()
                    {
                        Campo = "Mensagem",
                        Mensagem = rsIdentity.Mensagem ?? "Erro ao criar usuário no sistema"
                    });

                    return new Result<Guid>
                    {
                        Status = false,
                        Mensagem = rsIdentity.Mensagem ?? "Erro ao criar usuário no sistema"
                    };
                }

                identityUser = rsIdentity.Dados;

                if (rsIdentity.UsuarioExiste)
                {
                    // Usuário já existe, apenas adicionar papel de cliente
                    await ProcessarUsuarioExistente(cliente, identityUser, usuario, pessoaFisica, pessoaJuridica);
                    clienteId = identityUser.Id;
                }
                else
                {
                    // Novo usuário, processo completo
                    clienteId = await ProcessarNovoUsuario(request, cliente, endereco, usuario, pessoaFisica, pessoaJuridica, rsIdentity.Dados.Id);
                }

                await _unitOfWork.CommitAsync();

                return new Result<Guid>
                {
                    Status = true,
                    Dados = clienteId,
                    Mensagem = "Cliente criado com sucesso"
                };
            }
            catch (Exception ex)
            {
                if (!usuarioExistente && identityUser != null)
                {
                    await _identityUserService.RemoverAoFalharAsync(identityUser);
                }

                _notificador.Handle(new Notificacao()
                {
                    Campo = "Erro",
                    Mensagem = ex.Message
                });

                await _unitOfWork.RollbackAsync();

                return new Result<Guid>
                {
                    Status = false,
                    Mensagem = ex.Message
                };
            }
        }

        private async Task ProcessarUsuarioExistente(Cliente cliente, IdentityUser<Guid> identityUser, Usuario usuario, PessoaFisica pessoaFisica, PessoaJuridica pessoaJuridica)
        {
            var usuarioExiste = await _usuarioService.BuscarPorIdAsNotrackingAsync(identityUser.Id);
            if (usuarioExiste == null)
            {
                usuario.Id = identityUser.Id;
                var rsUsuario = await _usuarioService.SalvarAsync(usuario);
                if (!rsUsuario.Status)
                    throw new Exception(rsUsuario.Mensagem ?? "Erro ao salvar usuário");
            }

            cliente.Id = identityUser.Id;

            var rsPapel = await _papelService.SalvarAsync(new Papel()
            {
                UsuarioId = identityUser.Id,
                TipoUsuario = TipoUsuario.Cliente,
                Created = DateTime.UtcNow
            });

            if (!rsPapel.Status)
                throw new Exception(rsPapel.Mensagem ?? "Erro ao salvar papel");

            var pf = await _pessoaFisicaRepository.BuscarPorIdAsNotrackingAsync(identityUser.Id);
            var pj = await _pessoaJuridicaRepository.BuscarPorIdAsNotrackingAsync(identityUser.Id);
            if (pf == null && pj == null)
            {
                var rsCliente = await _clienteService.SalvarAsync(cliente, pessoaFisica, pessoaJuridica);
                if (!rsCliente.Status)
                    throw new Exception(rsCliente.Mensagem ?? "Erro ao salvar cliente");
            }
            else
            {
                var rsCliente = await _clienteService.SalvarAsync(cliente);
                if (!rsCliente.Status)
                    throw new Exception(rsCliente.Mensagem ?? "Erro ao salvar cliente");

            }
        }

        private async Task<Guid> ProcessarNovoUsuario(
            CriarClienteCommand request,
            Cliente cliente,
            Endereco endereco,
            Usuario usuario,
            PessoaFisica pessoaFisica,
            PessoaJuridica pessoaJuridica,
            Guid identityUserId)
        {
            // Configurar usuário
            usuario.Id = identityUserId;
            usuario.TipoUsuario = TipoUsuario.Cliente;

            // Salvar endereço apenas se informado
            if (TemEnderecoInformado(request))
            {
                var rsEndereco = await _enderecoService.SalvarAsync(endereco);
                if (!rsEndereco.Status)
                    throw new Exception(rsEndereco.Mensagem ?? "Erro ao salvar endereço");

                usuario.EnderecoId = rsEndereco.Dados.Id;
            }

            var rsUsuario = await _usuarioService.SalvarAsync(usuario);
            if (!rsUsuario.Status)
                throw new Exception(rsUsuario.Mensagem ?? "Erro ao salvar usuário");

            // Configurar cliente e pessoas
            var clienteId = rsUsuario.Dados.Id;
            cliente.Id = clienteId;
            pessoaFisica.Id = clienteId;
            pessoaJuridica.Id = clienteId;

            // Configurar vendedor se não for admin
            if (request.VendedorId.HasValue)
            {
                cliente.VendedorId = request.VendedorId;
            }

            // Salvar cliente com pessoa física ou jurídica
            var rsCliente = await _clienteService.SalvarAsync(cliente, pessoaFisica, pessoaJuridica);
            if (!rsCliente.Status)
                throw new Exception(rsCliente.Mensagem ?? "Erro ao salvar cliente");

            return clienteId;
        }

        private Cliente CriarCliente(CriarClienteCommand request)
        {
            return new Cliente
            {
                TipoPessoa = request.TipoPessoa,
                CpfCnpj = request.CpfCnpj,
                VendedorId = request.VendedorId,
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            };
        }

        private Endereco CriarEndereco(CriarClienteCommand request)
        {
            return new Endereco
            {
                Logradouro = request.Logradouro ?? string.Empty,
                Numero = request.Numero ?? string.Empty,
                Complemento = request.Complemento ?? string.Empty,
                Bairro = request.Bairro ?? string.Empty,
                Cidade = request.Cidade ?? string.Empty,
                Estado = request.Estado ?? string.Empty,
                Cep = request.Cep ?? string.Empty,
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            };
        }

        private bool TemEnderecoInformado(CriarClienteCommand request)
        {
            return !string.IsNullOrWhiteSpace(request.Logradouro) ||
                   !string.IsNullOrWhiteSpace(request.Numero) ||
                   !string.IsNullOrWhiteSpace(request.Bairro) ||
                   !string.IsNullOrWhiteSpace(request.Cidade) ||
                   !string.IsNullOrWhiteSpace(request.Estado) ||
                   !string.IsNullOrWhiteSpace(request.Cep);
        }

        private Usuario CriarUsuario(CriarClienteCommand request)
        {
            return new Usuario
            {
                Nome = request.Nome,
                //Email = request.Email,
                //Telefone = request.Telefone,
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            };
        }

        private PessoaFisica CriarPessoaFisica(CriarClienteCommand request)
        {
            return new PessoaFisica
            {
                Nome = request.Nome,
                Cpf = request.CpfCnpj,
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            };
        }

        private PessoaJuridica CriarPessoaJuridica(CriarClienteCommand request)
        {
            return new PessoaJuridica
            {
                Cnpj = request.CpfCnpj,
                RazaoSocial = request.Nome,
                NomeFantasia = request.NomeFantasia,
                InscricaoEstadual = request.InscricaoEstadual,
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            };
        }
    }
}
