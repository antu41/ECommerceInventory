using Domain.Entities;
using Domain.Repositories;

namespace Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        public IRepository<User> Users { get; }
        public IRepository<Product> Products { get; }
        public IRepository<Category> Categories { get; }

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            Users = new Repository<User>(context);
            Products = new Repository<Product>(context);
            Categories = new Repository<Category>(context);
        }

        public async Task<int> SaveChangesAsync(CancellationToken token) => await _context.SaveChangesAsync(token);
        public void Dispose() => _context.Dispose();
    }
}
