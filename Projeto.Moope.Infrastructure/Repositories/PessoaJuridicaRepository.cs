using Microsoft.EntityFrameworkCore;
using Projeto.Moope.Core.Enums;
using Projeto.Moope.Core.Interfaces.Repositories;
using Projeto.Moope.Core.Models;
using Projeto.Moope.Infrastructure.Data;

namespace Projeto.Moope.Infrastructure.Repositories
{
    public class PessoaJuridicaRepository : IPessoaJuridicaRepository
    {
        private readonly AppDbContext _context;
        public PessoaJuridicaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PessoaJuridica> BuscarPorIdAsync(Guid id)
        {
            return await _context.PessoasJuridicas.FirstOrDefaultAsync(pj => pj.Id == id);
        }
        
        public async Task<IEnumerable<PessoaJuridica>> BuscarTodosAsync()
        {
            return await _context.PessoasJuridicas.ToListAsync();
        }

        public async Task<PessoaJuridica> SalvarAsync(PessoaJuridica entity)
        {
            await _context.PessoasJuridicas.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<PessoaJuridica> AtualizarAsync(PessoaJuridica entity)
        {
            _context.PessoasJuridicas.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> RemoverAsync(Guid id)
        {
            var pessoaJuridica = await _context.PessoasJuridicas.FindAsync(id);
            if (pessoaJuridica != null)
            {
                _context.PessoasJuridicas.Remove(pessoaJuridica);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<PessoaJuridica> BuscarPorCnpjAsync(string cnpj)
        {
            return await _context.PessoasJuridicas.FirstOrDefaultAsync(pj => pj.Cnpj.Equals(cnpj));
        }

        public async Task<PessoaJuridica> BuscarPorIdAsNotrackingAsync(Guid id)
        {
            return await _context.PessoasJuridicas.AsNoTracking().FirstOrDefaultAsync(pj => pj.Id == id);
        }
    }
} 