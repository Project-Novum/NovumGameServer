using Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Database
{
    public class DBContext : DbContext
    {
        public DbSet<GameWorld> GameWorlds { get; set; }

        public DBContext(DbContextOptions<DBContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.HasDefaultSchema("Game");

            GameWorld.Setup(builder.Entity<GameWorld>());
        }
    }
}
