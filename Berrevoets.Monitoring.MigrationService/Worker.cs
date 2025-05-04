using System.Diagnostics;
using Berrevoets.Monitoring.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace Berrevoets.Monitoring.MigrationService;

public class Worker : BackgroundService
{
    private const string ActivitySourceName = "Migrations";
    private static readonly ActivitySource ActivitySource = new(ActivitySourceName);
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly ILogger<Worker> _logger;
    private readonly IServiceProvider _serviceProvider;

    public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider,
        IHostApplicationLifetime hostApplicationLifetime)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _hostApplicationLifetime = hostApplicationLifetime;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var activity = ActivitySource.StartActivity("Migrating database", ActivityKind.Client);

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<HealthConfigDbContext>();

            await EnsureDatabase(dbContext, stoppingToken);
            await RunMigrationAsync(dbContext, stoppingToken);
            await SeedDataAsync(dbContext, stoppingToken);
        }
        catch (Exception ex)
        {
            activity?.AddException(ex);
            _logger.LogError(ex, "Something went wrong while migrating the database");
            throw;
        }

        _hostApplicationLifetime.StopApplication();
    }

    private static async Task EnsureDatabase(HealthConfigDbContext dbContext, CancellationToken stoppingToken)
    {
        var dbCreator = dbContext.GetService<IRelationalDatabaseCreator>();

        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            if (!await dbCreator.ExistsAsync(stoppingToken))
                await dbCreator.CreateAsync(stoppingToken);
        });
    }

    private static async Task RunMigrationAsync(HealthConfigDbContext dbContext, CancellationToken stoppingToken)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () => { await dbContext.Database.MigrateAsync(stoppingToken); });
    }

    private static async Task SeedDataAsync(HealthConfigDbContext dbContext, CancellationToken stoppingToken)
    {
        var development = new MonitoredEnvironment
        {
            Id = Guid.NewGuid(),
            Name = "Development",
            IsActive = true,
            OrderNumber = 1,
            Applications =
            [
                new MonitoredApplication
                {
                    Id = Guid.NewGuid(),
                    Name = "Sample Application",
                    Description = "A sample application for monitoring.",
                    HealthEndpoint = "https://run.mocky.io/v3/b3361eec-d30f-4078-87f2-d4c89cee4858",
                    SwaggerUrl = "https://petstore.swagger.io",
                    IsActive = true
                },
                new MonitoredApplication
                {
                    Id = Guid.NewGuid(),
                    Name = "Sample Application",
                    Description = "A sample application for monitoring.",
                    HealthEndpoint = "https://run.mocky.io/v3/4a82791c-c38f-49b8-a4a4-259652f0da4e",
                    IsActive = true
                },
                new MonitoredApplication
                {
                    Id = Guid.NewGuid(),
                    Name = "Sample Application",
                    Description = "A sample application for monitoring.",
                    HealthEndpoint = "https://run.mocky.io/v3/263cac1e-50a9-4a64-b8c5-3f52dc46deb4",
                    SwaggerUrl = "https://petstore.swagger.io",
                    IsActive = true
                }
            ]
        };

        var acceptation = new MonitoredEnvironment
        {
            Id = Guid.NewGuid(),
            Name = "Acceptation",
            OrderNumber = 2,
            IsActive = false,
            Applications =
            [
                new MonitoredApplication
                {
                    Id = Guid.NewGuid(),
                    Name = "Sample Application",
                    Description = "A sample application for monitoring.",
                    HealthEndpoint = "https://run.mocky.io/v3/b3361eec-d30f-4078-87f2-d4c89cee4858",
                    IsActive = true
                },
                new MonitoredApplication
                {
                    Id = Guid.NewGuid(),
                    Name = "Sample Application",
                    Description = "A sample application for monitoring.",
                    HealthEndpoint = "https://run.mocky.io/v3/4a82791c-c38f-49b8-a4a4-259652f0da4e",
                    SwaggerUrl = "https://petstore.swagger.io",
                    IsActive = false
                },
                new MonitoredApplication
                {
                    Id = Guid.NewGuid(),
                    Name = "Sample Application",
                    Description = "A sample application for monitoring.",
                    HealthEndpoint = "https://run.mocky.io/v3/263cac1e-50a9-4a64-b8c5-3f52dc46deb4",
                    IsActive = true
                }
            ]
        };

        var routevision = new MonitoredEnvironment
        {
            Id = Guid.NewGuid(),
            Name = "Routevision",
            IsActive = true,
            OrderNumber = 3,
            Applications =
            [
                new MonitoredApplication
                {
                    Id = Guid.NewGuid(),
                    Name = "Sample Application",
                    Description = "A sample application for monitoring.",
                    HealthEndpoint = "https://run.mocky.io/v3/b3361eec-d30f-4078-87f2-d4c89cee4858",
                    IsActive = true
                },
                new MonitoredApplication
                {
                    Id = Guid.NewGuid(),
                    Name = "Sample Application",
                    Description = "A sample application for monitoring.",
                    HealthEndpoint = "https://run.mocky.io/v3/4a82791c-c38f-49b8-a4a4-259652f0da4e",
                    SwaggerUrl = "https://petstore.swagger.io",
                    IsActive = false
                },
                new MonitoredApplication
                {
                    Id = Guid.NewGuid(),
                    Name = "Sample Application",
                    Description = "A sample application for monitoring.",
                    HealthEndpoint = "https://run.mocky.io/v3/263cac1e-50a9-4a64-b8c5-3f52dc46deb4",
                    IsActive = true
                }
            ]
        };

        var fleetfinder = new MonitoredEnvironment
        {
            Id = Guid.NewGuid(),
            Name = "Fleetfinder",
            IsActive = true,
            OrderNumber = 4,
            Applications =
            [
                new MonitoredApplication
                {
                    Id = Guid.NewGuid(),
                    Name = "Sample Application",
                    Description = "A sample application for monitoring.",
                    HealthEndpoint = "https://run.mocky.io/v3/b3361eec-d30f-4078-87f2-d4c89cee4858",
                    SwaggerUrl = "https://petstore.swagger.io",
                    IsActive = true
                },
                new MonitoredApplication
                {
                    Id = Guid.NewGuid(),
                    Name = "Sample Application",
                    Description = "A sample application for monitoring.",
                    HealthEndpoint = "https://run.mocky.io/v3/4a82791c-c38f-49b8-a4a4-259652f0da4e",
                    IsActive = true
                },
                new MonitoredApplication
                {
                    Id = Guid.NewGuid(),
                    Name = "Sample Application",
                    Description = "A sample application for monitoring.",
                    HealthEndpoint = "https://run.mocky.io/v3/263cac1e-50a9-4a64-b8c5-3f52dc46deb4",
                    IsActive = false
                }
            ]
        };

        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            if (!await dbContext.MonitoredEnvironments.AnyAsync(stoppingToken))
            {
                await using var transaction = await dbContext.Database.BeginTransactionAsync(stoppingToken);
                await dbContext.AddRangeAsync(development, acceptation, routevision, fleetfinder);
                await dbContext.SaveChangesAsync(stoppingToken);
                await transaction.CommitAsync(stoppingToken);
            }
        });
    }
}
