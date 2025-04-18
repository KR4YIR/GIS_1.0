using WebApplication3.Entities;

namespace WebApplication3.Repositories
{
    public class PointReadRepository : ReadRepository<Point>, IPointReadRepository
    {
        public PointReadRepository(CbsDbContext dbContext) : base(dbContext)
        {
        }
    }
}
