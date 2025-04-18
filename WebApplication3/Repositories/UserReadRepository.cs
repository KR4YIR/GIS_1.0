using Microsoft.EntityFrameworkCore;
using WebApplication3.Entities;

namespace WebApplication3.Repositories
{

    public class UserReadRepository : ReadRepository<User>, IUserReadRepository<User>
    {
        private readonly CbsDbContext _dbContext;
        public UserReadRepository(CbsDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<User?> GetByEmailAsync(string email)
        {
            return await Table.FirstOrDefaultAsync(user => user.Email == email);
        }
        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await Table.FirstOrDefaultAsync(user => user.Username == username);
        }
    }
        
}
