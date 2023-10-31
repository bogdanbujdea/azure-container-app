using Exercises.Storage.Entities;
using Microsoft.EntityFrameworkCore;

namespace Exercises.Storage
{
    public class UsersDbContext : DbContext
    {
        public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options)
        {
        }

        public DbSet<ApplicationUser> Users { get; set; }
    }
}