using WebApplication3.Entities.Common;

namespace WebApplication3.Repositories
{
    public interface IWriteRepository<T> : IRepository<T> where T : BaseEntity
    {
        Task<bool> AddAsync(T model);
        Task<bool> AddRangeAsync(List<T> datas);
        Task<bool> RemoveAsync(long id);
        bool Remove(T model);

        bool RemoveRange(List<T> datas);

        bool Update(T model);
        Task<int> SaveAsync();
    }
}
