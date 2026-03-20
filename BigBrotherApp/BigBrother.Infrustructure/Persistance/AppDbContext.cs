using Microsoft.EntityFrameworkCore;
using BigBrother.Domain.Entities;

namespace BigBrother.Infrustructure.Persistance;

public class AppDbContext : DbContext
{
    public DbSet<ActivitySession> Sessions { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Set PK for ActivitySession table
        modelBuilder.Entity<ActivitySessions>()
            .HasKey(s => s.Id);

        // Make index by StartTime for ActivitySession table
        modelBuilder.Entity<ActivitySessions>().
            HasIndex(s => s.StartTime);
    }
}

