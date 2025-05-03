using Berrevoets.Monitoring.Entities.DTO;
using Microsoft.AspNetCore.SignalR;

namespace Berrevoets.Monitoring.HUB;

public class MonitorHub : Hub
{
    private readonly ILogger<MonitorHub> _logger;

    public MonitorHub(ILogger<MonitorHub> logger)
    {
        _logger = logger;
    }

    public async Task SendUpdateToClients(List<MonitoredEnvironmentDto> environments)
    {
        _logger.LogInformation("Sending update to clients");
        await Clients.All.SendAsync("ReceiveMonitoringUpdate", environments);
    }
}
