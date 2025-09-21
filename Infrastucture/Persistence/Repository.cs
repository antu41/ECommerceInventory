using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Persistence
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private readonly AppDbContext _context;
        private readonly DbSet<TEntity> _dbSet;

        public Repository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<TEntity>();
        }

        public async Task<TEntity> GetByIdAsync(Guid id, CancellationToken token)
        {
            var entity = await _dbSet.FindAsync(id, token);
            if (entity == null)
            {
                throw new InvalidOperationException($"Entity of type {typeof(TEntity).Name} with ID {id} was not found.");
            }
            return entity;
        }
        public async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken token)
              => await _dbSet.FirstOrDefaultAsync(predicate, token);
        public IQueryable<TEntity> GetAll() => _dbSet;
        public async Task AddAsync(TEntity entity, CancellationToken token) => await _dbSet.AddAsync(entity, token);
        public void Update(TEntity entity) => _dbSet.Update(entity);
        public void Delete(TEntity entity) => _dbSet.Remove(entity);
    }
}
