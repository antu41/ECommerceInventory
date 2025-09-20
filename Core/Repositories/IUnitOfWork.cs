using Domain.Entities;

namespace Domain.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<User> Users { get; }
        IRepository<Product> Products { get; }
        IRepository<Category> Categories { get; }
        Task<int> SaveChangesAsync(CancellationToken token);
    }
}
