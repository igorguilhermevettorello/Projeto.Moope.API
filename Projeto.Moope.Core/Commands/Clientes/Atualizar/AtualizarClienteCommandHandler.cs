using Projeto.Moope.Core.Commands.Base;
using Projeto.Moope.Core.Enums;
using Projeto.Moope.Core.Interfaces.Notifications;
using Projeto.Moope.Core.Interfaces.Repositories;
using Projeto.Moope.Core.Interfaces.Services;
using Projeto.Moope.Core.Interfaces.UnitOfWork;
using Projeto.Moope.Core.Models;
using Projeto.Moope.Core.Models.Validators.Base;
using Projeto.Moope.Core.Notifications;

namespace Projeto.Moope.Core.Commands.Clientes.Atualizar
{
    public class AtualizarClienteCommandHandler : ICommandHandler<AtualizarClienteCommand, Result<bool>>
    {
        private readonly IClienteService _clienteService;
        private readonly IUsuarioService _usuarioService;
        private readonly IEnderecoService _enderecoService;
        private readonly IIdentityUserService _identityUserService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificador _notificador;

        public AtualizarClienteCommandHandler(
            IClienteService clienteService,
            IUsuarioService usuarioService,
            IEnderecoService enderecoService,
            IIdentityUserService identityUserService,
            IUnitOfWork unitOfWork,
            INotificador notificador)
        {
            _clienteService = clienteService;
            _usuarioService = usuarioService;
            _enderecoService = enderecoService;
            _identityUserService = identityUserService;
            _unitOfWork = unitOfWork;
            _notificador = notificador;
        }

        public async Task<Result<bool>> Handle(AtualizarClienteCommand request, CancellationToken cancellationToken)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Verificar se o cliente existe
                var clienteExistente = await _clienteService.BuscarPorIdAsNotrackingAsync(request.Id);
                if (clienteExistente == null)
                {
                    _notificador.Handle(new Notificacao
                    {
                        Campo = "Cliente",
                        Mensagem = "Cliente não encontrado"
                    });
                    return new Result<bool>
                    {
                        Status = false,
                        Mensagem = "Cliente não encontrado"
                    };
                }

                // Buscar usuário associado
                var usuarioExistente = await _usuarioService.BuscarPorIdAsNotrackingAsync(request.Id);
                if (usuarioExistente == null)
                {
                    _notificador.Handle(new Notificacao
                    {
                        Campo = "Usuario",
                        Mensagem = "Usuário associado ao cliente não encontrado"
                    });
                    return new Result<bool>
                    {
                        Status = false,
                        Mensagem = "Usuário associado ao cliente não encontrado"
                    };
                }

                // Atualizar usuário no Identity
                var rsIdentity = await _identityUserService.AlterarUsuarioAsync(
                    request.Id,
                    request.Email,
                    telefone: request.Telefone);

                if (!rsIdentity.Status)
                {
                    _notificador.Handle(new Notificacao
                    {
                        Campo = "Identity",
                        Mensagem = rsIdentity.Mensagem ?? "Erro ao atualizar usuário no Identity"
                    });
                    return new Result<bool>
                    {
                        Status = false,
                        Mensagem = rsIdentity.Mensagem ?? "Erro ao atualizar usuário no Identity"
                    };
                }

                // Atualizar endereço se existir
                if (usuarioExistente.EnderecoId.HasValue)
                {
                    var endereco = CriarEndereco(request, usuarioExistente.EnderecoId.Value);
                    var rsEndereco = await _enderecoService.AtualizarAsync(endereco);
                    if (!rsEndereco.Status)
                    {
                        throw new Exception(rsEndereco.Mensagem ?? "Erro ao atualizar endereço");
                    }
                }
                else if (TemEnderecoInformado(request))
                {
                    // Criar novo endereço se não existia antes
                    var novoEndereco = CriarEndereco(request);
                    var rsNovoEndereco = await _enderecoService.SalvarAsync(novoEndereco);
                    if (!rsNovoEndereco.Status)
                    {
                        throw new Exception(rsNovoEndereco.Mensagem ?? "Erro ao criar endereço");
                    }

                    // Atualizar usuário com o novo endereço
                    var usuarioParaAtualizar = CriarUsuario(request);
                    usuarioParaAtualizar.EnderecoId = rsNovoEndereco.Dados.Id;
                    var rsUsuarioComEndereco = await _usuarioService.AtualizarAsync(usuarioParaAtualizar);
                    if (!rsUsuarioComEndereco.Status)
                    {
                        throw new Exception(rsUsuarioComEndereco.Mensagem ?? "Erro ao atualizar usuário com endereço");
                    }
                }
                else
                {
                    // Atualizar usuário sem alterar endereço
                    var usuario = CriarUsuario(request);
                    usuario.EnderecoId = usuarioExistente.EnderecoId;
                    var rsUsuario = await _usuarioService.AtualizarAsync(usuario);
                    if (!rsUsuario.Status)
                    {
                        throw new Exception(rsUsuario.Mensagem ?? "Erro ao atualizar usuário");
                    }
                }

                // Atualizar cliente e pessoas
                var cliente = CriarCliente(request);
                var pessoaFisica = CriarPessoaFisica(request);
                var pessoaJuridica = CriarPessoaJuridica(request);

                var rsCliente = await _clienteService.AtualizarAsync(cliente, pessoaFisica, pessoaJuridica);
                if (!rsCliente.Status)
                {
                    throw new Exception(rsCliente.Mensagem ?? "Erro ao atualizar cliente");
                }

                await _unitOfWork.CommitAsync();

                return new Result<bool>
                {
                    Status = true,
                    Dados = true,
                    Mensagem = "Cliente atualizado com sucesso"
                };
            }
            catch (Exception ex)
            {
                _notificador.Handle(new Notificacao
                {
                    Campo = "Erro",
                    Mensagem = ex.Message
                });

                await _unitOfWork.RollbackAsync();

                return new Result<bool>
                {
                    Status = false,
                    Mensagem = ex.Message
                };
            }
        }

        private Cliente CriarCliente(AtualizarClienteCommand request)
        {
            return new Cliente
            {
                Id = request.Id,
                TipoPessoa = request.TipoPessoa,
                CpfCnpj = request.CpfCnpj,
                VendedorId = request.VendedorId,
                Updated = DateTime.UtcNow
            };
        }

        private Endereco CriarEndereco(AtualizarClienteCommand request, Guid? enderecoId = null)
        {
            var endereco = new Endereco
            {
                Logradouro = request.Logradouro ?? string.Empty,
                Numero = request.Numero ?? string.Empty,
                Complemento = request.Complemento ?? string.Empty,
                Bairro = request.Bairro ?? string.Empty,
                Cidade = request.Cidade ?? string.Empty,
                Estado = request.Estado ?? string.Empty,
                Cep = request.Cep ?? string.Empty,
                Updated = DateTime.UtcNow
            };

            if (enderecoId.HasValue)
            {
                endereco.Id = enderecoId.Value;
            }
            else
            {
                endereco.Created = DateTime.UtcNow;
            }

            return endereco;
        }

        private bool TemEnderecoInformado(AtualizarClienteCommand request)
        {
            return !string.IsNullOrWhiteSpace(request.Logradouro) ||
                   !string.IsNullOrWhiteSpace(request.Numero) ||
                   !string.IsNullOrWhiteSpace(request.Bairro) ||
                   !string.IsNullOrWhiteSpace(request.Cidade) ||
                   !string.IsNullOrWhiteSpace(request.Estado) ||
                   !string.IsNullOrWhiteSpace(request.Cep);
        }

        private Usuario CriarUsuario(AtualizarClienteCommand request)
        {
            return new Usuario
            {
                Id = request.Id,
                Nome = request.Nome,
                TipoUsuario = TipoUsuario.Cliente,
                Updated = DateTime.UtcNow
            };
        }

        private PessoaFisica CriarPessoaFisica(AtualizarClienteCommand request)
        {
            return new PessoaFisica
            {
                Id = request.Id,
                Cpf = request.CpfCnpj,
                Updated = DateTime.UtcNow
            };
        }

        private PessoaJuridica CriarPessoaJuridica(AtualizarClienteCommand request)
        {
            return new PessoaJuridica
            {
                Id = request.Id,
                Cnpj = request.CpfCnpj,
                RazaoSocial = request.Nome,
                NomeFantasia = request.NomeFantasia,
                InscricaoEstadual = request.InscricaoEstadual,
                Updated = DateTime.UtcNow
            };
        }
    }
}
