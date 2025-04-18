using WebApplication3.Entities;

namespace WebApplication3.Repositories
{
    public class PointWriteRepository : WriteRepository<Point>, IPointWriteRepository
    {
        public PointWriteRepository(CbsDbContext dbContext) : base(dbContext)
        {
        }
    }
}
