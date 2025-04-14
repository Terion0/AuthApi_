using AutenticationApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthApi.DbsContext
{
    public class AuDbContext : DbContext
    {
        public AuDbContext(DbContextOptions<AuDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
    }
}
