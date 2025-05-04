using System.ComponentModel.DataAnnotations;

namespace Berrevoets.Monitoring.Entities;

public class MonitoredEnvironment
{
    public Guid Id { get; set; }

    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; }
    public DateTime LastUpdated { get; set; }
    public List<MonitoredApplication> Applications { get; set; } = new();
}
