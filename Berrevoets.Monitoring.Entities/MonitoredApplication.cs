using System.ComponentModel.DataAnnotations;

namespace Berrevoets.Monitoring.Entities;

public class MonitoredApplication
{
    public Guid Id { get; set; }

    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(2048)]
    public string HealthEndpoint { get; set; } = string.Empty;

    public Guid EnvironmentId { get; set; }
    public MonitoredEnvironment Environment { get; set; } = null!;
    public bool IsActive { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = "Unknown";

    [MaxLength(2048)]
    public string? StatusMessage { get; set; }

    public DateTime LastUpdated { get; set; }
}
