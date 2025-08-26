using Microsoft.EntityFrameworkCore;
using Projeto.Moope.Core.Interfaces.Repositories;
using Projeto.Moope.Core.Models;
using Projeto.Moope.Infrastructure.Data;

namespace Projeto.Moope.Infrastructure.Repositories
{
    public class TransacaoRepository : ITransacaoRepository
    {
        private readonly AppDbContext _context;
        public TransacaoRepository(AppDbContext context)
        {
            _context = context;
        }
        
        
        public async Task<Transacao> BuscarPorPedidoIdAsync(Guid pedidoId)
        {
            return await _context.Transacoes.Include(t => t.Pedido).FirstOrDefaultAsync(t => t.Pedido.Id == pedidoId);
        }
            
        public async Task<Transacao> BuscarPorIdAsync(Guid id)
        {
            return await _context.Transacoes.Include(t => t.Pedido).FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<Transacao>> BuscarTodosAsync()
        {
            return await _context.Transacoes.Include(t => t.Pedido).ToListAsync();
        }

        public async Task<Transacao> SalvarAsync(Transacao entity)
        {
            await _context.Transacoes.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<Transacao> AtualizarAsync(Transacao entity)
        {
            _context.Transacoes.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> RemoverAsync(Guid id)
        {
            var transacao = await _context.Transacoes.FindAsync(id);
            if (transacao != null)
            {
                _context.Transacoes.Remove(transacao);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
    }
} 