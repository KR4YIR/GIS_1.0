using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using WebApplication3.Entities.Common;

namespace WebApplication3.Repositories
{
    public interface IRepository<T> where T : BaseEntity
    {
        DbSet<T> Table {  get; }
    }
}
