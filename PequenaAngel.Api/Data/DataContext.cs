using Microsoft.EntityFrameworkCore;

namespace PequenaAngel.Api.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
    }
}
