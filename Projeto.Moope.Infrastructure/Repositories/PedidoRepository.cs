using Microsoft.EntityFrameworkCore;
using Projeto.Moope.Core.Interfaces.Repositories;
using Projeto.Moope.Core.Interfaces.Repositories.Base;
using Projeto.Moope.Core.Models;
using Projeto.Moope.Infrastructure.Data;

namespace Projeto.Moope.Infrastructure.Repositories
{
    public class PedidoRepository : IPedidoRepository
    {
        private readonly AppDbContext _context;
        public PedidoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Pedido>> BuscarPorVendedorIdAsync(Guid vendedorId)
        {
            return await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Vendedor)
                .Where(p => p.VendedorId == vendedorId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Pedido>> BuscarPorClienteIdAsync(Guid clienteId)
        {
            return await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Vendedor)
                .Where(p => p.Cliente.Id == clienteId)
                .ToListAsync();
        }

        public async Task<Pedido?> BuscarPorGalaxPayIdAsync(int galaxPayId)
        {
            return await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Vendedor)
                .FirstOrDefaultAsync(p => p.GalaxPayId == galaxPayId);
        }

        public async Task<Pedido> BuscarPorIdAsync(Guid id)
        {
            return await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Vendedor)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Pedido>> BuscarTodosAsync()
        {
            return await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Vendedor)
                .ToListAsync();
        }

        public async Task<Pedido> SalvarAsync(Pedido entity)
        {
            await _context.Pedidos.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<Pedido> AtualizarAsync(Pedido entity)
        {
            _context.Pedidos.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> RemoverAsync(Guid id)
        {
            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido != null)
            {
                _context.Pedidos.Remove(pedido);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
    }
} 