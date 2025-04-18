using WebApplication3.Entities;
using WebApplication3.Repositories;

namespace WebApplication3.Repositories
{
    public interface IUserReadRepository<T> : IReadRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByUsernameAsync(string email);

    }
}
