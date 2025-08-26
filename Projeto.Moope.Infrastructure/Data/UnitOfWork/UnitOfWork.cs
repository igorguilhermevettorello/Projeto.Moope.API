using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Projeto.Moope.Core.Interfaces.UnitOfWork;

namespace Projeto.Moope.Infrastructure.Data.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IDbContextTransaction _transaction;
        
        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }
        
        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }
        
        public async Task CommitAsync()
        {
            await _context.SaveChangesAsync();
            await _transaction?.CommitAsync();
        }
        
        public async Task RollbackAsync()
        {
            await _transaction?.RollbackAsync();
        }
    }
}
