using Projeto.Moope.Core.Enums;
using Projeto.Moope.Core.Interfaces.Notifications;
using Projeto.Moope.Core.Interfaces.Repositories;
using Projeto.Moope.Core.Interfaces.Services;
using Projeto.Moope.Core.Models;
using Projeto.Moope.Core.Models.Validators;
using Projeto.Moope.Core.Models.Validators.Base;
using Projeto.Moope.Core.Services.Base;
using Projeto.Moope.Core.Validation;

namespace Projeto.Moope.Core.Services
{
    public class VendedorService : BaseService, IVendedorService
    {
        private readonly IVendedorRepository _vendedorRepository;
        private readonly IPessoaFisicaRepository _pessoaFisicaRepository;
        private readonly IPessoaJuridicaRepository _pessoaJuridicaRepository;
        public VendedorService(
            IVendedorRepository vendedorRepository,
            IPessoaFisicaRepository pessoaFisicaRepository,
            IPessoaJuridicaRepository pessoaJuridicaRepository,
            INotificador notificador) : base(notificador)
        {
            _vendedorRepository = vendedorRepository;
            _pessoaFisicaRepository = pessoaFisicaRepository;
            _pessoaJuridicaRepository = pessoaJuridicaRepository;
        }

        public async Task<Vendedor> BuscarPorIdAsNotrackingAsync(Guid id)
        {
            return await _vendedorRepository.BuscarPorIdAsNotrackingAsync(id);
        }
        
        public async Task<Vendedor> BuscarPorIdAsync(Guid id)
        {
            return await _vendedorRepository.BuscarPorIdAsync(id);
        }
        
        public async Task<IEnumerable<Vendedor>> BuscarTodosAsync()
        {
            return await _vendedorRepository.BuscarTodosAsync();
        }

        public async Task<IEnumerable<Vendedor>> BuscarTodosComDadosRelacionadosAsync()
        {
            return await _vendedorRepository.BuscarTodosComDadosRelacionadosAsync();
        }

        public async Task<IEnumerable<T>> BuscarVendedoresComDadosAsync<T>()
        {
            return await _vendedorRepository.BuscarVendedoresComDadosAsync<T>();
        }

        public async Task<T?> BuscarVendedorPorIdComDadosAsync<T>(Guid id)
        {
            return await _vendedorRepository.BuscarVendedorPorIdComDadosAsync<T>(id);
        }

        public async Task<Result<Vendedor>> SalvarAsync(Vendedor vendedor)
        {
            if (!ExecutarValidacao(new VendedorValidator(), vendedor))
                return new Result<Vendedor>() { Status = false, Mensagem = "Dados do revendedor são inválidos" };

            var entidade = await _vendedorRepository.SalvarAsync(vendedor);
            return new Result<Vendedor>()
            {
                Status = true,
                Dados = entidade,
                Mensagem = "Revendedor salvo com sucesso"
            };
        }
        public async Task<Result<Vendedor>> SalvarAsync(Vendedor vendedor, PessoaFisica pessoaFisica, PessoaJuridica pessoaJuridica)
        {
            if (!ExecutarValidacao(new VendedorValidator(), vendedor))
                return new Result<Vendedor>() { Status = false, Mensagem = "Dados do revendedor são inválidos" };

            if (vendedor.TipoPessoa == TipoPessoa.JURIDICA)
            {
                var pj = await _pessoaJuridicaRepository.BuscarPorCnpjAsync(Documentos.OnlyDigits(vendedor.CpfCnpj));
                if (pj != null)
                {
                    Notificar("CpfCnpj", "Cnpj já cadastrado");
                    return new Result<Vendedor>() { Status = false, Mensagem = "Dados do revendedor são inválidos" };
                }
                pessoaJuridica.Created = DateTime.UtcNow;
                pessoaJuridica.Cnpj = Documentos.OnlyDigits(pessoaJuridica.Cnpj);
                var _ = await _pessoaJuridicaRepository.SalvarAsync(pessoaJuridica);
            }
            
            if (vendedor.TipoPessoa == TipoPessoa.FISICA)
            {
                var pf = await _pessoaFisicaRepository.BuscarPorCpfAsync(Documentos.OnlyDigits(vendedor.CpfCnpj));
                if (pf != null)
                {
                    Notificar("CpfCnpj", "Cpf já cadastrado");
                    return new Result<Vendedor>() { Status = false, Mensagem = "Dados do cliente são inválidos" };
                }
                
                pessoaFisica.Cpf = Documentos.OnlyDigits(pessoaFisica.Cpf);
                pessoaFisica.Created = DateTime.UtcNow;
                var _ = await _pessoaFisicaRepository.SalvarAsync(pessoaFisica);
            }
                
            var entidade = await _vendedorRepository.SalvarAsync(vendedor);
            
            return new Result<Vendedor>()
            {
                Status = true,
                Dados = entidade,
                Mensagem = "Revendedor salvo com sucesso"
            };
        }

        public async Task<Result<Vendedor>> AtualizarAsync(Vendedor vendedor, PessoaFisica pessoaFisica, PessoaJuridica pessoaJuridica)
        {
            if (!ExecutarValidacao(new VendedorValidator(), vendedor))
                return new Result<Vendedor>() { Status = false, Mensagem = "Dados do revendedor são inválidos" };

            var vendedorAtual = await _vendedorRepository.BuscarPorIdAsync(vendedor.Id);
            if (vendedorAtual == null)
                return new Result<Vendedor> { Status = false, Mensagem = "Revendedor não encontrado." };

            if (vendedor.TipoPessoa == TipoPessoa.JURIDICA)
            {
                var pj = await _pessoaJuridicaRepository.BuscarPorCnpjAsync(Documentos.OnlyDigits(vendedor.CpfCnpj));
                if (pj != null && vendedor.Id != pj.Id)
                {
                    Notificar("CpfCnpj", "Cnpj já cadastrado");
                    return new Result<Vendedor>() { Status = false, Mensagem = "Dados do revendedor são inválidos" };        
                }
                
                var _pf = await _pessoaFisicaRepository.BuscarPorIdAsync(vendedor.Id);
                if (_pf != null)
                    await _pessoaFisicaRepository.RemoverAsync(_pf.Id);
                
                var _pj = await _pessoaJuridicaRepository.BuscarPorIdAsync(vendedor.Id);
                if (_pj != null)
                    await _pessoaJuridicaRepository.RemoverAsync(_pj.Id);
                
                pessoaJuridica.Created = DateTime.UtcNow;
                pessoaJuridica.Cnpj = Documentos.OnlyDigits(pessoaJuridica.Cnpj);
                var _ = await _pessoaJuridicaRepository.SalvarAsync(pessoaJuridica);
                
            }

            if (vendedor.TipoPessoa == TipoPessoa.FISICA)
            {
                var pf = await _pessoaFisicaRepository.BuscarPorCpfAsync(Documentos.OnlyDigits(vendedor.CpfCnpj));
                if (pf != null && pf.Id != vendedor.Id)   
                {
                    Notificar("CpfCnpj", "Cpf já cadastrado");
                    return new Result<Vendedor>() { Status = false, Mensagem = "Dados do revendedor são inválidos" };
                }
            
                var _pf = await _pessoaFisicaRepository.BuscarPorIdAsync(vendedor.Id);
                if (_pf != null)
                    await _pessoaFisicaRepository.RemoverAsync(_pf.Id);
                
                var _pj = await _pessoaJuridicaRepository.BuscarPorIdAsync(vendedor.Id);
                if (_pj != null)
                    await _pessoaJuridicaRepository.RemoverAsync(_pj.Id);
            
                pessoaFisica.Cpf = Documentos.OnlyDigits(pessoaFisica.Cpf);
                pessoaFisica.Created = DateTime.UtcNow;
                await _pessoaFisicaRepository.AtualizarAsync(pessoaFisica);
            }
              
            vendedorAtual.TipoPessoa = vendedor.TipoPessoa;
            vendedorAtual.Updated = DateTime.UtcNow;
            var entidade = await _vendedorRepository.AtualizarAsync(vendedorAtual);
            
            return new Result<Vendedor>()
            {
                Status = true,
                Dados = entidade,
                Mensagem = "Revendedor atualizado com sucesso"
            };
        }

        public async Task<bool> RemoverAsync(Guid id)
        {
            // Aqui você pode adicionar lógica para remover o Papel, se necessário
            await _vendedorRepository.RemoverAsync(id);
            return true;
        }
    }
} 