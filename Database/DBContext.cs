using Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Database;

public class DBContext : DbContext
{
    public virtual DbSet<Sessions?> Sessions { get; set; }

    public DBContext(DbContextOptions<DBContext> options) : base(options)
    {
    }

    /// <summary>
    ///     Override anything needed here for migrations
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}