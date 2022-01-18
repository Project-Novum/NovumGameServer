using Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Database
{
    public class DBContext : DbContext
    {
        public DbSet<GameWorld> GameWorlds { get; set; }
        public DbSet<Character> Characters { get; set; }

        public DbSet<Appearance> Appearances { get; set; }

        public DBContext(DbContextOptions<DBContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.HasDefaultSchema("Game");

            GameWorld.Setup(builder.Entity<GameWorld>());
            Character.Setup(builder.Entity<Character>());
            Appearance.Setup(builder.Entity<Appearance>());
            
        }
    }
}
