using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using WebApplication3.Entities.Common;

namespace WebApplication3.Repositories
{
    public class ReadRepository<T> : IReadRepository<T> where T : BaseEntity
    {
        private readonly CbsDbContext _dbContext;

        public ReadRepository(CbsDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public DbSet<T> Table => _dbContext.Set<T>();
        public IQueryable<T> GetAll()
            => Table;

        public IQueryable<T> GetWhere(Expression<Func<T, bool>> method)
            => Table.Where(method);
        public async Task<T> GetSingleAsync(Expression<Func<T, bool>> method)
            => await Table.FirstOrDefaultAsync(method);


        public Task<T> GetByIdAsync(long id)
            => Table.FirstOrDefaultAsync(data => data.Id == id);
    }
}
