using Berrevoets.Monitoring.Entities.DTO;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace Berrevoets.Monitoring.Client.Services;

public class MonitoringHubService : IAsyncDisposable
{
    private readonly NavigationManager _navigationManager;

    private HubConnection? _hubConnection;

    public MonitoringHubService(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
    }

    public List<MonitoredEnvironmentDto> Environments
    {
        [UsedImplicitly]
        get;
        set;
    } = new();

    public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection is not null)
            await _hubConnection.DisposeAsync();
    }

    public event Action<List<MonitoredEnvironmentDto>>? OnEnvironmentUpdated;

    public async Task StartConnectionAsync()
    {
        if (_hubConnection is not null) return;

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(_navigationManager.ToAbsoluteUri("/monitorHub"))
            .WithAutomaticReconnect()
            .Build();

        // Add connection state change logging
        _hubConnection.Closed += error =>
        {
            Console.WriteLine($"Connection closed with error: {error?.Message}");
            return Task.CompletedTask;
        };

        _hubConnection.Reconnecting += error =>
        {
            Console.WriteLine($"Connection reconnecting due to error: {error?.Message}");
            return Task.CompletedTask;
        };

        _hubConnection.Reconnected += connectionId =>
        {
            Console.WriteLine($"Connection reconnected with connection ID: {connectionId}");
            return Task.CompletedTask;
        };

        // Register handler for ReceiveMonitoringUpdate messages with explicit logging
        _hubConnection.On<List<MonitoredEnvironmentDto>>("ReceiveMonitoringUpdate", environments =>
        {
            Console.WriteLine($"Received update with {environments.Count} environments");
            Environments = environments;

            if (OnEnvironmentUpdated != null)
            {
                Console.WriteLine("Invoking OnEnvironmentUpdated event");
                OnEnvironmentUpdated.Invoke(environments);
            }
            else
            {
                Console.WriteLine("OnEnvironmentUpdated has no subscribers");
            }
        });

        try
        {
            await _hubConnection.StartAsync();
            Console.WriteLine("Connection started successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error starting connection: {ex.Message}");
        }
    }

    public void UpdateEnvironments(List<MonitoredEnvironmentDto> environments)
    {
        Environments = environments;
        OnEnvironmentUpdated?.Invoke(environments);
    }
}
