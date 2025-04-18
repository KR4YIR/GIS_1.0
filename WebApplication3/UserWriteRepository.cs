using WebApplication3.Entities;

namespace WebApplication3.Repositories
{
    public class UserWriteRepository : WriteRepository<User>, IUserWriteRepository<User>
    {
        public UserWriteRepository(CbsDbContext dbContext) : base(dbContext)
        {
        }
    }
}
