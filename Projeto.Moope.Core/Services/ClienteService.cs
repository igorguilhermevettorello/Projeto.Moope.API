using Projeto.Moope.Core.Enums;
using Projeto.Moope.Core.Interfaces.Notifications;
using Projeto.Moope.Core.Interfaces.Repositories;
using Projeto.Moope.Core.Interfaces.Services;
using Projeto.Moope.Core.Models;
using Projeto.Moope.Core.Models.Validators.Base;
using Projeto.Moope.Core.Models.Validators.Cliente;
using Projeto.Moope.Core.Services.Base;
using Projeto.Moope.Core.Validation;

namespace Projeto.Moope.Core.Services
{
    public class ClienteService : BaseService, IClienteService
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IPessoaFisicaRepository _pessoaFisicaRepository;
        private readonly IPessoaJuridicaRepository _pessoaJuridicaRepository;
        private readonly IIdentityUserService _identityUserService;

        public ClienteService(
            IClienteRepository clienteRepository,
            IPessoaFisicaRepository pessoaFisicaRepository,
            IPessoaJuridicaRepository pessoaJuridicaRepository,
            IIdentityUserService identityUserService,
            INotificador notificador) : base(notificador)
        {
            _clienteRepository = clienteRepository;
            _pessoaFisicaRepository = pessoaFisicaRepository;
            _pessoaJuridicaRepository = pessoaJuridicaRepository;
            _identityUserService = identityUserService;
        }

        public async Task<Cliente> BuscarPorIdAsNotrackingAsync(Guid id)
        {
            return await _clienteRepository.BuscarPorIdAsNotrackingAsync(id);
        }
            
        public async Task<Cliente> BuscarPorIdAsync(Guid id)
        {
            return await _clienteRepository.BuscarPorIdAsync(id);
        }

        public async Task<Cliente> BuscarPorEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                Notificar("Email", "Email é obrigatório para busca");
                return null;
            }

            try
            {
                // Buscar usuário no Identity pelo email
                var identityUser = await _identityUserService.BuscarPorEmailAsync(email);
                if (identityUser == null)
                {
                    return null; // Usuário não encontrado no Identity
                }

                // Usar o ID do usuário Identity para buscar o cliente
                var cliente = await _clienteRepository.BuscarPorIdAsync(identityUser.Id);
                return cliente;
            }
            catch (Exception ex)
            {
                Notificar("Email", $"Erro ao buscar cliente por email: {ex.Message}");
                return null;
            }
        }

        public async Task<IEnumerable<Cliente>> BuscarTodosAsync()
        {
            return await _clienteRepository.BuscarTodosAsync();
        }

        public async Task<Result<Cliente>> SalvarAsync(Cliente cliente)
        {
            if (!ExecutarValidacao(new ClienteValidator(), cliente))
                return new Result<Cliente>() { Status = false, Mensagem = "Dados do cliente são inválidos" };
    
            var entidade = await _clienteRepository.SalvarAsync(cliente);
            return new Result<Cliente>()
            {
                Status = true,
                Dados = cliente,
                Mensagem = "Cliente salvo com sucesso"
            };
        }
        
        public async Task<Result<Cliente>> SalvarAsync(Cliente cliente, PessoaFisica pessoaFisica, PessoaJuridica pessoaJuridica)
        {
            if (!ExecutarValidacao(new ClienteValidator(), cliente))
                return new Result<Cliente>() { Status = false, Mensagem = "Dados do cliente são inválidos" };

            if (cliente.TipoPessoa == TipoPessoa.JURIDICA)
            {
                var pj = await _pessoaJuridicaRepository.BuscarPorCnpjAsync(Documentos.OnlyDigits(cliente.CpfCnpj));
                if (pj != null)
                {
                    Notificar("CpfCnpj", "Cnpj já cadastrado");
                    return new Result<Cliente>() { Status = false, Mensagem = "Dados do cliente são inválidos" };
                }
                pessoaJuridica.Created = DateTime.UtcNow;
                pessoaJuridica.Cnpj = Documentos.OnlyDigits(pessoaJuridica.Cnpj);
                var _ = await _pessoaJuridicaRepository.SalvarAsync(pessoaJuridica);
            }
            
            if (cliente.TipoPessoa == TipoPessoa.FISICA)
            {
                var pf = await _pessoaFisicaRepository.BuscarPorCpfAsync(Documentos.OnlyDigits(cliente.CpfCnpj));
                if (pf != null)
                {
                    Notificar("CpfCnpj", "Cpf já cadastrado");
                    return new Result<Cliente>() { Status = false, Mensagem = "Dados do cliente são inválidos" };
                }

                try
                {
                    pessoaFisica.Cpf = Documentos.OnlyDigits(pessoaFisica.Cpf);
                    pessoaFisica.Created = DateTime.UtcNow;
                    var _ = await _pessoaFisicaRepository.SalvarAsync(pessoaFisica);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    throw;
                }
                
            }
                
            var entidade = await _clienteRepository.SalvarAsync(cliente);
            
            return new Result<Cliente>()
            {
                Status = true,
                Dados = cliente,
                Mensagem = "Cliente salvo com sucesso"
            };
        }

        public async Task<Result<Cliente>> AtualizarAsync(Cliente cliente, PessoaFisica pessoaFisica, PessoaJuridica pessoaJuridica)
        {
            if (!ExecutarValidacao(new ClienteValidator(), cliente))
                return new Result<Cliente>() { Status = false, Mensagem = "Dados do cliente são inválidos" };

            var clienteAtual = await _clienteRepository.BuscarPorIdAsync(cliente.Id);
            if (clienteAtual == null)
                return new Result<Cliente> { Status = false, Mensagem = "Cliente não encontrado." };

            if (cliente.TipoPessoa == TipoPessoa.JURIDICA)
            {
                var pj = await _pessoaJuridicaRepository.BuscarPorCnpjAsync(Documentos.OnlyDigits(cliente.CpfCnpj));
                if (pj != null && cliente.Id != pj.Id)
                {
                    Notificar("CpfCnpj", "Cnpj já cadastrado");
                    return new Result<Cliente>() { Status = false, Mensagem = "Dados do cliente são inválidos" };        
                }
                
                var _pf = await _pessoaFisicaRepository.BuscarPorIdAsync(cliente.Id);
                if (_pf != null)
                    await _pessoaFisicaRepository.RemoverAsync(_pf.Id);
                
                var _pj = await _pessoaJuridicaRepository.BuscarPorIdAsync(cliente.Id);
                if (_pj != null)
                    await _pessoaJuridicaRepository.RemoverAsync(_pj.Id);
                
                pessoaJuridica.Created = DateTime.UtcNow;
                pessoaJuridica.Cnpj = Documentos.OnlyDigits(pessoaJuridica.Cnpj);
                var _ = await _pessoaJuridicaRepository.SalvarAsync(pessoaJuridica);
                
            }

            if (cliente.TipoPessoa == TipoPessoa.FISICA)
            {
                var pf = await _pessoaFisicaRepository.BuscarPorCpfAsync(Documentos.OnlyDigits(cliente.CpfCnpj));
                if (pf != null && pf.Id != cliente.Id)   
                {
                    Notificar("CpfCnpj", "Cpf já cadastrado");
                    return new Result<Cliente>() { Status = false, Mensagem = "Dados do cliente são inválidos" };
                }
            
                var _pf = await _pessoaFisicaRepository.BuscarPorIdAsync(cliente.Id);
                if (_pf != null)
                    await _pessoaFisicaRepository.RemoverAsync(_pf.Id);
                
                var _pj = await _pessoaJuridicaRepository.BuscarPorIdAsync(cliente.Id);
                if (_pj != null)
                    await _pessoaJuridicaRepository.RemoverAsync(_pj.Id);
                
                pessoaFisica.Cpf = Documentos.OnlyDigits(pessoaFisica.Cpf);
                pessoaFisica.Created = DateTime.UtcNow;
                await _pessoaFisicaRepository.SalvarAsync(pessoaFisica);
            }
              
            clienteAtual.TipoPessoa = cliente.TipoPessoa;
            clienteAtual.Updated = DateTime.UtcNow;
            var entidade = await _clienteRepository.AtualizarAsync(clienteAtual);
            
            return new Result<Cliente>()
            {
                Status = true,
                Dados = cliente,
                Mensagem = "Cliente salvo com sucesso"
            };
        }
        
        public async Task<Result<Cliente>> AtualizarAsync(Cliente cliente)
        {
            if (!ExecutarValidacao(new ClienteValidator(), cliente))
            {
                return new Result<Cliente>()
                {
                    Status = false,
                    Mensagem = "Dados do cliente são inválidos"
                };
            }

            try
            {
                var clienteExistente = await _clienteRepository.BuscarPorIdAsync(cliente.Id);
                if (clienteExistente == null)
                {
                    Notificar("Cliente", "Cliente não encontrado");
                    return new Result<Cliente>()
                    {
                        Status = false,
                        Mensagem = "Cliente não encontrado"
                    };
                }

                var entity = await _clienteRepository.AtualizarAsync(cliente);
                return new Result<Cliente>()
                {
                    Status = true,
                    Dados = entity,
                    Mensagem = "Cliente atualizado com sucesso"
                };
            }
            catch (Exception ex)
            {
                Notificar("Cliente", $"Erro ao atualizar cliente: {ex.Message}");
                return new Result<Cliente>()
                {
                    Status = false,
                    Mensagem = "Erro interno ao atualizar cliente"
                };
            }
        }

        public async Task<bool> RemoverAsync(Guid id)
        {
            try
            {
                var clienteExistente = await _clienteRepository.BuscarPorIdAsync(id);
                if (clienteExistente == null)
                {
                    Notificar("Cliente", "Cliente não encontrado");
                    return false;
                }

                await _clienteRepository.RemoverAsync(id);
                return true;
            }
            catch (Exception ex)
            {
                Notificar("Cliente", $"Erro ao remover cliente: {ex.Message}");
                return false;
            }
        }
    }
}