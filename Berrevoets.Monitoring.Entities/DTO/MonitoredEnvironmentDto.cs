namespace Berrevoets.Monitoring.Entities.DTO;

public class MonitoredEnvironmentDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime LastUpdated { get; set; }
    public int OrderNumber { get; set; }
    public List<MonitoredApplicationDto> Applications { get; set; } = new();
}
