using System.Text.Json;
using Berrevoets.Monitoring.Entities;
using Berrevoets.Monitoring.Entities.DTO;
using Berrevoets.Monitoring.HUB;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Berrevoets.Monitoring.Services;

public class HealthCheckService : BackgroundService
{
    private readonly TimeSpan _checkInterval;
    private readonly HttpClient _httpClient;
    private readonly IHubContext<MonitorHub> _hubContext;
    private readonly ILogger<HealthCheckService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private List<MonitoredEnvironment>? _previousEnvironments;

    public HealthCheckService(IHubContext<MonitorHub> hubContext,
        ILogger<HealthCheckService> logger, IServiceScopeFactory serviceScopeFactory, HttpClient httpClient,
        IOptions<HealthCheckSettings> settings)
    {
        _hubContext = hubContext;
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _httpClient = httpClient;

        _checkInterval = TimeSpan.FromSeconds(settings.Value.CheckIntervalSeconds);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("HealthCheckService is starting");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckHealthAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("HealthCheckService is stopping");
    }

    private async Task CheckHealthAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting health check at {time}", DateTimeOffset.Now);

        // Create a new scope for this operation
        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<HealthConfigDbContext>();

        // Load all environments with their applications from the database
        var environments = await dbContext.MonitoredEnvironments
            .Include(env => env.Applications)
            .ToListAsync(stoppingToken);

        // If this is the first run, always consider it a change
        var dataChanged = _previousEnvironments == null;
        
        // TODO: Remove this 
        dataChanged = true;

        // Create lookup dictionaries for previous state if available
        Dictionary<Guid, MonitoredEnvironment>? previousEnvLookup = null;
        Dictionary<Guid, MonitoredApplication>? previousAppLookup = null;

        if (_previousEnvironments != null)
        {
            previousEnvLookup = _previousEnvironments.ToDictionary(e => e.Id);
            previousAppLookup = _previousEnvironments
                .SelectMany(e => e.Applications)
                .ToDictionary(a => a.Id);
        }

        // First check for environment property changes
        if (previousEnvLookup != null)
            foreach (var environment in environments)
            {
                var environmentChanged = false;

                if (previousEnvLookup.TryGetValue(environment.Id, out var previousEnv))
                {
                    // Check for environment property changes
                    if (environment.IsActive != previousEnv.IsActive)
                    {
                        _logger.LogInformation("Environment {envName} IsActive changed: {oldState} -> {newState}",
                            environment.Name, previousEnv.IsActive, environment.IsActive);
                        dataChanged = true;
                        environmentChanged = true;
                    }

                    if (environment.Name != previousEnv.Name)
                    {
                        _logger.LogInformation("Environment name changed: {oldName} -> {newName}",
                            previousEnv.Name, environment.Name);
                        dataChanged = true;
                        environmentChanged = true;
                    }

                    // Update environment timestamp if changed
                    if (environmentChanged)
                        environment.LastUpdated = DateTime.UtcNow;
                }
                else
                {
                    // New environment
                    environment.LastUpdated = DateTime.UtcNow;
                }
            }
        else
            // First run, set timestamps for all environments
            foreach (var environment in environments)
                environment.LastUpdated = DateTime.UtcNow;

        // Then check applications
        foreach (var environment in environments.Where(e => e.IsActive))
        foreach (var application in environment.Applications.Where(a => a.IsActive))
        {
            var applicationChanged = false;

            try
            {
                // Get previous state if available
                var previousStatus = "Unknown";
                string? previousStatusMessage = null;
                var previousIsActive = false;

                if (previousAppLookup != null && previousAppLookup.TryGetValue(application.Id, out var previousApp))
                {
                    previousStatus = previousApp.Status;
                    previousStatusMessage = previousApp.StatusMessage;
                    previousIsActive = previousApp.IsActive;

                    // Check if IsActive state changed
                    if (previousIsActive != application.IsActive)
                    {
                        _logger.LogInformation(
                            "IsActive changed for application {appName} in environment {envName}: {oldState} -> {newState}",
                            application.Name, environment.Name, previousIsActive, application.IsActive);
                        applicationChanged = true;
                        dataChanged = true;
                    }
                }
                else
                {
                    // New application
                    applicationChanged = true;
                }

                // Perform health check
                var response = await _httpClient.GetAsync(application.HealthEndpoint, stoppingToken);

                // Process the response
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync(stoppingToken);

                    // Try to parse as JSON first
                    try
                    {
                        using var jsonDoc = JsonDocument.Parse(responseContent);

                        // Try to extract status from common health check formats
                        if (jsonDoc.RootElement.TryGetProperty("status", out var statusElement))
                            application.Status = statusElement.GetString() ?? "Healthy";
                        else
                            application.Status = "Healthy"; // Default if no status property

                        // Try to extract message if present
                        if (jsonDoc.RootElement.TryGetProperty("message", out var messageElement) ||
                            jsonDoc.RootElement.TryGetProperty("description", out messageElement) ||
                            jsonDoc.RootElement.TryGetProperty("statusMessage", out messageElement))
                            application.StatusMessage = messageElement.GetString();
                        else
                            application.StatusMessage = null;
                    }
                    catch (JsonException)
                    {
                        // Not valid JSON, treat as plain text status
                        application.Status = responseContent.Trim().Length > 0 ? responseContent.Trim() : "Healthy";
                        application.StatusMessage = null;
                    }
                }
                else
                {
                    application.Status = "Unhealthy";
                    application.StatusMessage = $"HTTP Status: {(int)response.StatusCode} {response.ReasonPhrase}";
                }

                // Normalize status to one of the standard values
                application.Status = NormalizeStatus(application.Status);

                // Check if status or message changed
                if (application.Status != previousStatus || application.StatusMessage != previousStatusMessage)
                {
                    _logger.LogInformation(
                        "Status changed for application {appName} in environment {envName}: {oldStatus} -> {newStatus}",
                        application.Name, environment.Name, previousStatus, application.Status);
                    dataChanged = true;
                    applicationChanged = true;
                }

                // Update application timestamp if changed
                if (applicationChanged)
                    application.LastUpdated = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                // Get previous state if available
                var previousStatus = "Unknown";
                string? previousStatusMessage = null;

                if (previousAppLookup != null && previousAppLookup.TryGetValue(application.Id, out var previousApp))
                {
                    previousStatus = previousApp.Status;
                    previousStatusMessage = previousApp.StatusMessage;
                }

                // Update application status to indicate error
                application.Status = "Error";
                application.StatusMessage = $"Exception: {ex.Message}";

                if (application.Status != previousStatus || application.StatusMessage != previousStatusMessage)
                {
                    application.LastUpdated = DateTime.UtcNow;
                    dataChanged = true;
                }

                _logger.LogWarning(ex, "Error checking health for application {appName} in environment {envName}",
                    application.Name, environment.Name);
            }
        }

        // Check for new or removed applications/environments if we have previous data
        if (_previousEnvironments != null)
        {
            var currentEnvIds = environments.Select(e => e.Id).ToHashSet();
            var previousEnvIds = _previousEnvironments.Select(e => e.Id).ToHashSet();

            if (!currentEnvIds.SetEquals(previousEnvIds))
            {
                _logger.LogInformation("Environments added or removed");
                dataChanged = true;
            }
            else
            {
                // Check if any applications were added or removed
                var currentAppIds = environments.SelectMany(e => e.Applications).Select(a => a.Id).ToHashSet();
                var previousAppIds =
                    _previousEnvironments.SelectMany(e => e.Applications).Select(a => a.Id).ToHashSet();

                if (!currentAppIds.SetEquals(previousAppIds))
                {
                    _logger.LogInformation("Applications added or removed");
                    dataChanged = true;
                }
            }
        }

        if (dataChanged)
            // Update all timestamps
            foreach (var environment in environments)
            {
                environment.LastUpdated = DateTime.UtcNow;

                foreach (var application in environment.Applications)
                    application.LastUpdated = DateTime.UtcNow;
            }

        if (dataChanged)
        {
            // Notify clients about the changes
            await NotifyClientsAsync(environments);

            // Save changes to the database
            await dbContext.SaveChangesAsync(stoppingToken);

            // Store a deep copy of the current environments for next comparison
            _previousEnvironments = DeepCopyEnvironments(environments);

            _logger.LogInformation("Health check completed with changes detected, clients notified");
        }
        else
        {
            _logger.LogInformation("Health check completed, no changes detected");
        }
    }


    // Helper method to normalize status values to standard ones
    private string NormalizeStatus(string status)
    {
        return status?.ToLowerInvariant() switch
        {
            "up" or "online" or "ok" or "pass" or "passing" or "healthy" or "available" => "Healthy",
            "down" or "offline" or "fail" or "failing" or "unhealthy" or "unavailable" => "Unhealthy",
            "warn" or "warning" or "degraded" or "partial" => "Degraded",
            "error" => "Error",
            null => "Unknown",
            _ => status // Keep original if no match
        };
    }

    // Helper method to create a deep copy of environments to avoid reference issues
    private List<MonitoredEnvironment> DeepCopyEnvironments(List<MonitoredEnvironment> environments)
    {
        return environments.Select(env => new MonitoredEnvironment
        {
            Id = env.Id,
            Name = env.Name,
            IsActive = env.IsActive,
            LastUpdated = env.LastUpdated,
            Applications = env.Applications.Select(app => new MonitoredApplication
            {
                Id = app.Id,
                Name = app.Name,
                Description = app.Description,
                HealthEndpoint = app.HealthEndpoint,
                EnvironmentId = app.EnvironmentId,
                IsActive = app.IsActive,
                Status = app.Status,
                StatusMessage = app.StatusMessage,
                LastUpdated = app.LastUpdated
            }).ToList()
        }).ToList();
    }

    private async Task NotifyClientsAsync(List<MonitoredEnvironment> environments)
    {
        var environmentDtos = environments.Select(env => new MonitoredEnvironmentDto
        {
            Id = env.Id,
            Name = env.Name,
            IsActive = env.IsActive,
            LastUpdated = env.LastUpdated,
            Applications = env.Applications.Select(app => new MonitoredApplicationDto
            {
                Id = app.Id,
                Name = app.Name,
                Description = app.Description,
                HealthEndpoint = app.HealthEndpoint,
                IsActive = app.IsActive,
                Status = app.Status,
                StatusMessage = app.StatusMessage,
                LastUpdated = app.LastUpdated
            }).ToList()
        }).ToList();

        var orderSequence = new[] { "Development", "Acceptation", "Routevision", "Fleetfinder" };

        var ordered = environmentDtos
            .OrderBy(env => Array.IndexOf(orderSequence, env.Name))
            .ToList();

        await _hubContext.Clients.All.SendAsync("ReceiveMonitoringUpdate", ordered);
    }
}
