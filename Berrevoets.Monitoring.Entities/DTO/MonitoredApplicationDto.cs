namespace Berrevoets.Monitoring.Entities.DTO;

public class MonitoredApplicationDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string HealthEndpoint { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? StatusMessage { get; set; }
    public DateTime LastUpdated { get; set; }
    public string? SwaggerUrl { get; set; }
}
