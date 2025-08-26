using Microsoft.EntityFrameworkCore;
using Projeto.Moope.Core.Interfaces.Repositories;
using Projeto.Moope.Core.Interfaces.Repositories.Base;
using Projeto.Moope.Core.Models;
using Projeto.Moope.Infrastructure.Data;

namespace Projeto.Moope.Infrastructure.Repositories
{
    public class PapelRepository : IPapelRepository
    {
        private readonly AppDbContext _context;
        public PapelRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Papel> BuscarPorIdAsync(Guid id)
        {
            return await _context.Papeis.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Papel>> BuscarTodosAsync()
        {
            return await _context.Papeis.ToListAsync();
        }

        public async Task<IEnumerable<Papel>> BuscarPorUsuarioIdAsync(Guid usuarioId)
        {
            return await _context.Papeis
                .Where(p => p.UsuarioId == usuarioId)
                .ToListAsync();
        }

        public async Task<Papel> SalvarAsync(Papel entity)
        {
            await _context.Papeis.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<Papel> AtualizarAsync(Papel entity)
        {
            _context.Papeis.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> RemoverAsync(Guid id)
        {
            var pedido = await _context.Papeis.FindAsync(id);
            if (pedido != null)
            {
                _context.Papeis.Remove(pedido);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
    }
} 