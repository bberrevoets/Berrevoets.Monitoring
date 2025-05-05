using Microsoft.EntityFrameworkCore;

namespace Berrevoets.Monitoring.Entities;

public class HealthConfigDbContext : DbContext
{
    public HealthConfigDbContext(DbContextOptions<HealthConfigDbContext> options) : base(options)
    {
    }

    public DbSet<MonitoredEnvironment> MonitoredEnvironments { get; set; }
    public DbSet<MonitoredApplication> MonitoredApplications { get; set; }
    public DbSet<QuickLink> QuickLinks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure relationships
        modelBuilder.Entity<MonitoredApplication>()
            .HasOne(app => app.Environment)
            .WithMany(env => env.Applications)
            .HasForeignKey(app => app.EnvironmentId);

        modelBuilder.Entity<MonitoredEnvironment>()
            .Property(e => e.LastUpdated)
            .HasDefaultValueSql("GETUTCDATE()");

        modelBuilder.Entity<MonitoredApplication>()
            .Property(e => e.LastUpdated)
            .HasDefaultValueSql("GETUTCDATE()");

        base.OnModelCreating(modelBuilder);
    }
}
