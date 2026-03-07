# Berrevoets.Monitoring

A real-time health monitoring dashboard built with .NET 10 Aspire and Blazor.
Periodically checks HTTP health endpoints of configured applications and
displays live status updates via SignalR.

## Features

- Real-time health monitoring with configurable check intervals
- Parallel health checks with 5-second timeout per endpoint
- Status normalization (Healthy, Unhealthy, Degraded, Error, Unknown)
- Admin UI for managing environments, applications, and quick links
- Dark mode support
- SignalR-based live updates (no polling from the browser)

## Tech Stack

- .NET 10 / Aspire 13.1.2
- Blazor (Interactive Auto: Server + WebAssembly)
- Entity Framework Core with SQL Server
- SignalR for real-time communication
- Bootstrap + Bootstrap Icons

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
  (for Aspire-managed SQL Server container)

### Running the Application

```bash
dotnet run --project Berrevoets.Monitoring.AppHost
```

This starts the Aspire orchestrator which provisions a SQL Server container,
runs database migrations, seeds sample data, and launches the web application.

## Project Structure

| Project                                  | Description                                       |
| ---------------------------------------- | ------------------------------------------------- |
| `Berrevoets.Monitoring.AppHost`          | Aspire orchestrator                               |
| `Berrevoets.Monitoring`                  | ASP.NET Core server (API, SignalR, Blazor host)   |
| `Berrevoets.Monitoring.Client`           | Blazor WebAssembly client                         |
| `Berrevoets.Monitoring.Entities`         | EF Core DbContext, entities, DTOs, migrations     |
| `Berrevoets.Monitoring.MigrationService` | Database migration and seeding worker             |
| `Berrevoets.Monitoring.ServiceDefaults`  | Shared Aspire defaults (telemetry, resilience)    |

## Author

Bert Berrevoets - Berrevoets Systems
