using Berrevoets.Monitoring.Entities;
using Berrevoets.Monitoring.Entities.DTO;
using Microsoft.EntityFrameworkCore;

namespace Berrevoets.Monitoring.Extensions;

public static class EnvironmentEndpoints
{
    public static void MapEnvironmentEndpoints(this WebApplication app)
    {
        app.MapGet("/api/environments", GetAllEnvironments);
        app.MapGet("/api/environments/{id}", GetEnvironmentById);
        app.MapPost("/api/environments", CreateEnvironment);
        app.MapPut("/api/environments/{id}", UpdateEnvironment);
        app.MapDelete("/api/environments/{id}", DeleteEnvironment);
    }

    private static async Task<IResult> GetAllEnvironments(HealthConfigDbContext db)
    {
        var environments = await db.MonitoredEnvironments
            .Include(env => env.Applications)
            .Select(env => new MonitoredEnvironmentDto
            {
                Id = env.Id,
                Name = env.Name,
                IsActive = env.IsActive,
                LastUpdated = env.LastUpdated,
                OrderNumber = env.OrderNumber,
                Applications = env.Applications.Select(application => new MonitoredApplicationDto
                    {
                        Id = application.Id,
                        Name = application.Name,
                        Description = application.Description,
                        HealthEndpoint = application.HealthEndpoint,
                        IsActive = application.IsActive,
                        Status = application.Status,
                        StatusMessage = application.StatusMessage,
                        LastUpdated = application.LastUpdated,
                        SwaggerUrl = application.SwaggerUrl,
                        EnvironmentId = env.Id // Include the environment ID
                    })
                    .ToList()
            })
            .OrderBy(env => env.OrderNumber)
            .ToListAsync();

        return Results.Ok(environments);
    }

    private static async Task<IResult> GetEnvironmentById(Guid id, HealthConfigDbContext db)
    {
        var environment = await db.MonitoredEnvironments
            .Include(e => e.Applications)
            .Select(env => new MonitoredEnvironmentDto
            {
                Id = env.Id,
                Name = env.Name,
                IsActive = env.IsActive,
                LastUpdated = env.LastUpdated,
                OrderNumber = env.OrderNumber,
                Applications = env.Applications.Select(application => new MonitoredApplicationDto
                    {
                        Id = application.Id,
                        Name = application.Name,
                        Description = application.Description,
                        HealthEndpoint = application.HealthEndpoint,
                        IsActive = application.IsActive,
                        Status = application.Status,
                        StatusMessage = application.StatusMessage,
                        LastUpdated = application.LastUpdated,
                        SwaggerUrl = application.SwaggerUrl,
                        EnvironmentId = env.Id
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(e => e.Id == id);

        if (environment == null)
            return Results.NotFound();

        return Results.Ok(environment);
    }

    private static async Task<IResult> CreateEnvironment(MonitoredEnvironmentDto environmentDto,
        HealthConfigDbContext db)
    {
        var environment = new MonitoredEnvironment
        {
            Id = environmentDto.Id,
            Name = environmentDto.Name,
            IsActive = environmentDto.IsActive,
            OrderNumber = environmentDto.OrderNumber,
            LastUpdated = DateTime.UtcNow,
            Applications = new List<MonitoredApplication>()
        };

        db.MonitoredEnvironments.Add(environment);
        await db.SaveChangesAsync();

        return Results.Created($"/api/environments/{environment.Id}", environment);
    }

    private static async Task<IResult> UpdateEnvironment(Guid id, MonitoredEnvironmentDto environmentDto,
        HealthConfigDbContext db)
    {
        var environment = await db.MonitoredEnvironments.FindAsync(id);
        if (environment == null)
            return Results.NotFound();

        // Update properties
        environment.Name = environmentDto.Name;
        environment.IsActive = environmentDto.IsActive;
        environment.OrderNumber = environmentDto.OrderNumber;
        environment.LastUpdated = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteEnvironment(Guid id, HealthConfigDbContext db)
    {
        var environment = await db.MonitoredEnvironments
            .Include(e => e.Applications)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (environment == null)
            return Results.NotFound();

        db.MonitoredApplications.RemoveRange(environment.Applications);
        db.MonitoredEnvironments.Remove(environment);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }
}
