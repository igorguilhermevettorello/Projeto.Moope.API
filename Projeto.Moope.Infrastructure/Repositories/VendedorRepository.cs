using Microsoft.EntityFrameworkCore;
using Projeto.Moope.Core.Interfaces.Repositories;
using Projeto.Moope.Core.Models;
using Projeto.Moope.Infrastructure.Data;

namespace Projeto.Moope.Infrastructure.Repositories
{
    public class VendedorRepository : IVendedorRepository
    {
        private readonly AppDbContext _context;
        public VendedorRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Vendedor> BuscarPorIdAsNotrackingAsync(Guid id)
        {
            return await _context.Vendedores.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
        }
        
        public async Task<Vendedor> BuscarPorIdAsync(Guid id)
        {
            return await _context.Vendedores.FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<Vendedor>> BuscarTodosAsync()
        {
            return await _context.Vendedores.ToListAsync();
        }

        public async Task<Vendedor> SalvarAsync(Vendedor entity)
        {
            await _context.Vendedores.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<Vendedor> AtualizarAsync(Vendedor entity)
        {
            _context.Vendedores.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> RemoverAsync(Guid id)
        {
            var revendedor = await _context.Vendedores.FindAsync(id);
            if (revendedor != null)
            {
                _context.Vendedores.Remove(revendedor);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
    }
} 