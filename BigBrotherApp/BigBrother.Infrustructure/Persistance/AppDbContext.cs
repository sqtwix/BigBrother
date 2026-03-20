using Microsoft.EntityFrameworkCore;
using BigBrother.Domain.Entities;

namespace BigBrother.Infrustructure.Persistance;

public class AppDbContext : DbContext
{
    public DbSet<ActivitySessions> Sessions { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ActivitySessions>().HasIndex(s => s.StartTime).HasKey(s => s.Id);
    }
}

