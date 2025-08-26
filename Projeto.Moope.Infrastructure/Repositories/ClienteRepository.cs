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
    }
} 