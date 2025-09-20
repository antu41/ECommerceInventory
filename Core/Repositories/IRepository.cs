using System.Linq.Expressions;

namespace Domain.Repositories
{
    public interface IRepository<TEntity> where TEntity : class
    {
        Task<TEntity> GetByIdAsync(Guid id, CancellationToken token);
        Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken token);
        IQueryable<TEntity> GetAll();
        Task AddAsync(TEntity entity, CancellationToken token);
        void Update(TEntity entity);
        void Delete(TEntity entity);
    }
}
