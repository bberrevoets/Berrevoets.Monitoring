using Microsoft.EntityFrameworkCore;

namespace Berrevoets.Monitoring.Entities;

public class HealthConfigDbContext : DbContext
{
    public HealthConfigDbContext(DbContextOptions<HealthConfigDbContext> options) : base(options)
    {
    }

    public DbSet<MonitoredEnvironment> MonitoredEnvironments { get; set; }
    public DbSet<MonitoredApplication> MonitoredApplications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure relationships
        modelBuilder.Entity<MonitoredApplication>()
            .HasOne(app => app.Environment)
            .WithMany(env => env.Applications)
            .HasForeignKey(app => app.EnvironmentId);

        base.OnModelCreating(modelBuilder);
    }
}
