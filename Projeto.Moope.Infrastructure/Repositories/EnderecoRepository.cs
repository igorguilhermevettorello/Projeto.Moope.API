using Microsoft.EntityFrameworkCore;
using Projeto.Moope.Core.Interfaces.Repositories;
using Projeto.Moope.Core.Models;
using Projeto.Moope.Infrastructure.Data;

namespace Projeto.Moope.Infrastructure.Repositories
{
    public class EnderecoRepository : IEnderecoRepository
    {
        private readonly AppDbContext _context;
        public EnderecoRepository(AppDbContext context)
        {
            _context = context;
        }
        
        public async Task<Endereco> BuscarPorIdAsNotrackingAsync(Guid id)
        {
            return await _context.Enderecos.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<Endereco> BuscarPorIdAsync(Guid id)
        {
            return await _context.Enderecos.FirstOrDefaultAsync(e => e.Id == id);
        }
        
        public async Task<IEnumerable<Endereco>> BuscarTodosAsync()
        {
            return await _context.Enderecos.ToListAsync();
        }

        public async Task<Endereco> SalvarAsync(Endereco entity)
        {
            await _context.Enderecos.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<Endereco> AtualizarAsync(Endereco entity)
        {
            _context.Enderecos.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> RemoverAsync(Guid id)
        {
            var endereco = await _context.Enderecos.FindAsync(id);
            if (endereco != null)
            {
                _context.Enderecos.Remove(endereco);
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }
    }
} 