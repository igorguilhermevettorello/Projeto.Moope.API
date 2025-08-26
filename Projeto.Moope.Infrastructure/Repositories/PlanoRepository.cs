using Microsoft.EntityFrameworkCore;
using Projeto.Moope.Core.Interfaces.Repositories;
using Projeto.Moope.Core.Models;
using Projeto.Moope.Infrastructure.Data;
using Projeto.Moope.Infrastructure.Repositories.Base;

namespace Projeto.Moope.Infrastructure.Repositories
{
    public class PlanoRepository : Repository<Plano>, IPlanoRepository
    {
        public PlanoRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Plano> BuscarPorIdAsNotrackingAsync(Guid id)
        {
            return await _dbSet.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        }
    }
} 