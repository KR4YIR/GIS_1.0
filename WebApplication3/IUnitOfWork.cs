using WebApplication3.Entities;
using WebApplication3.Repositories;

namespace WebApplication3
{
    public interface IUnitOfWork : IDisposable
    {
        IWriteRepository<Point> Points { get; }
        IUserWriteRepository<User> Users { get; }
        IReadRepository<Point> ReadPoints { get; }
        IUserReadRepository<User> ReadUsers { get; }
        Task<int> SaveAsync();
    }
}
