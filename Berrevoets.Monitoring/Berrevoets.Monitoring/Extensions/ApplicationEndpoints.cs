using Berrevoets.Monitoring.Entities;
using Berrevoets.Monitoring.Entities.DTO;
using Microsoft.EntityFrameworkCore;

namespace Berrevoets.Monitoring.Extensions;

public static class ApplicationEndpoints
{
    public static void MapApplicationEndpoints(this WebApplication app)
    {
        app.MapGet("/api/applications", GetAllApplications);
        app.MapGet("/api/applications/{id}", GetApplicationById);
        app.MapPost("/api/applications", CreateApplication);
        app.MapPut("/api/applications/{id}", UpdateApplication);
        app.MapDelete("/api/applications/{id}", DeleteApplication);
    }

    private static async Task<IResult> GetAllApplications(HealthConfigDbContext db)
    {
        var applications = await db.MonitoredApplications
            .Select(app => new MonitoredApplicationDto
            {
                Id = app.Id,
                Name = app.Name,
                Description = app.Description,
                HealthEndpoint = app.HealthEndpoint,
                IsActive = app.IsActive,
                Status = app.Status,
                StatusMessage = app.StatusMessage,
                LastUpdated = app.LastUpdated,
                SwaggerUrl = app.SwaggerUrl,
                EnvironmentId = app.EnvironmentId
            })
            .ToListAsync();

        return Results.Ok(applications);
    }

    private static async Task<IResult> GetApplicationById(Guid id, HealthConfigDbContext db)
    {
        var application = await db.MonitoredApplications
            .Where(a => a.Id == id)
            .Select(app => new MonitoredApplicationDto
            {
                Id = app.Id,
                Name = app.Name,
                Description = app.Description,
                HealthEndpoint = app.HealthEndpoint,
                IsActive = app.IsActive,
                Status = app.Status,
                StatusMessage = app.StatusMessage,
                LastUpdated = app.LastUpdated,
                SwaggerUrl = app.SwaggerUrl,
                EnvironmentId = app.EnvironmentId
            })
            .FirstOrDefaultAsync();

        if (application == null)
            return Results.NotFound();

        return Results.Ok(application);
    }

    private static async Task<IResult> CreateApplication(MonitoredApplicationDto applicationDto,
        HealthConfigDbContext db)
    {
        // Check if the environment exists
        var environment = await db.MonitoredEnvironments.FindAsync(applicationDto.EnvironmentId);
        if (environment == null)
            return Results.BadRequest("Environment not found");

        var application = new MonitoredApplication
        {
            Id = applicationDto.Id,
            Name = applicationDto.Name,
            Description = applicationDto.Description,
            HealthEndpoint = applicationDto.HealthEndpoint,
            IsActive = applicationDto.IsActive,
            EnvironmentId = applicationDto.EnvironmentId,
            Status = "Unknown",
            LastUpdated = DateTime.UtcNow,
            SwaggerUrl = applicationDto.SwaggerUrl
        };

        db.MonitoredApplications.Add(application);
        await db.SaveChangesAsync();

        return Results.Created($"/api/applications/{application.Id}", application);
    }

    private static async Task<IResult> UpdateApplication(Guid id, MonitoredApplicationDto applicationDto,
        HealthConfigDbContext db)
    {
        var application = await db.MonitoredApplications.FindAsync(id);
        if (application == null)
            return Results.NotFound();

        // Check if the environment exists
        if (application.EnvironmentId != applicationDto.EnvironmentId)
        {
            var environment = await db.MonitoredEnvironments.FindAsync(applicationDto.EnvironmentId);
            if (environment == null)
                return Results.BadRequest("Environment not found");
        }

        // Update properties
        application.Name = applicationDto.Name;
        application.Description = applicationDto.Description;
        application.HealthEndpoint = applicationDto.HealthEndpoint;
        application.IsActive = applicationDto.IsActive;
        application.EnvironmentId = applicationDto.EnvironmentId;
        application.SwaggerUrl = applicationDto.SwaggerUrl;
        application.LastUpdated = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteApplication(Guid id, HealthConfigDbContext db)
    {
        var application = await db.MonitoredApplications.FindAsync(id);
        if (application == null)
            return Results.NotFound();

        db.MonitoredApplications.Remove(application);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }
}
