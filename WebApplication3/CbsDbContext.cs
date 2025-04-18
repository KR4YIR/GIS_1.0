using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApplication3.Entities;
using WebApplication3.Identities;

namespace WebApplication3
{
    public class CbsDbContext : IdentityDbContext<AppUser,AppRole,Guid>
    {
        public CbsDbContext(DbContextOptions<CbsDbContext> options) : base(options)
        {

        }
        public DbSet<Point> Points { get; set; }
        public DbSet<User> Users { get; set; }



    }

}
