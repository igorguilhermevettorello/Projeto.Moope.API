using Microsoft.EntityFrameworkCore;
using Projeto.Moope.Core.Interfaces.Repositories;
using Projeto.Moope.Core.Models;
using Projeto.Moope.Infrastructure.Data;
using Projeto.Moope.Infrastructure.Repositories.Base;

namespace Projeto.Moope.Infrastructure.Repositories
{
    public class ClienteRepository : Repository<Cliente>, IClienteRepository
    {
        public ClienteRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Cliente> BuscarPorIdAsNotrackingAsync(Guid id)
        {
            return await _dbSet.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Cliente> BuscarPorEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.CpfCnpj == email);
        }

        public async Task<IEnumerable<T>> BuscarClientesComDadosAsync<T>()
        {
            var query = @"
                SELECT c.Id as Id,
                       u.Nome as Nome, 
                       au.Email as Email, 
                       pf.Cpf, 
                       pj.Cnpj,
                       CASE 
                           WHEN pf.Cpf IS NOT NULL THEN '1'
                           WHEN pj.Cnpj IS NOT NULL THEN '2'
                           ELSE NULL
                       END as TipoPessoa,
                       COALESCE(pf.Cpf, pj.Cnpj) as CpfCnpj,
                       au.PhoneNumber as Telefone, 
                       e.Cidade as Cidade, 
                       e.Estado as Estado, 
                       au.LockoutEnabled as Ativo
                FROM Cliente c
                LEFT JOIN AspNetUsers au ON au.Id = c.Id
                LEFT JOIN Usuario u ON u.Id = c.Id 
                LEFT JOIN Endereco e ON e.Id = u.EnderecoId 
                LEFT JOIN PessoaFisica pf ON pf.Id = c.Id
                LEFT JOIN PessoaJuridica pj ON pj.Id = c.Id";

            return await _context.Database.SqlQueryRaw<T>(query).ToListAsync();
        }

        public async Task<T?> BuscarClientePorIdComDadosAsync<T>(Guid id)
        {
            var query = @"
                SELECT c.Id as Id, 
                       u.Nome as Nome, 
                       au.Email as Email, 
                       CASE 
                         WHEN pf.Cpf IS NOT NULL THEN '1'
                         WHEN pj.Cnpj IS NOT NULL THEN '2'
                         ELSE NULL
                       END as TipoPessoa,
                       COALESCE(pf.Cpf, pj.Cnpj) as CpfCnpj,
                       au.PhoneNumber as Telefone, 
                       au.LockoutEnabled as Ativo,
                       e.Cep as Cep, 
                       e.Logradouro as Logradouro,
                       e.Numero as Numero,
                       e.Complemento as Complemento,
                       e.Bairro as Bairro,
                       e.Cidade as Cidade,
                       e.Estado as Estado
                FROM Cliente c
                LEFT JOIN AspNetUsers au ON au.Id = c.Id
                LEFT JOIN Usuario u ON u.Id = c.Id
                LEFT JOIN Endereco e ON e.Id = u.EnderecoId
                LEFT JOIN PessoaFisica pf ON pf.Id = c.Id
                LEFT JOIN PessoaJuridica pj ON pj.Id = c.Id
                WHERE c.Id = {0}";

            return await _context.Database.SqlQueryRaw<T>(query, id).FirstOrDefaultAsync();
        }
    }
} 