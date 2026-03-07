# Changelog

All notable changes to this project will be documented in this file.

## 2026-03-07

### Added

- CLAUDE.md for Claude Code guidance
- Markdownlint configuration (120 char line length)
- GitHub branch workflow: `Development` as default, `main` as production
- Branch protection rulesets for both `Development` and `main`

### Changed

- Upgraded from .NET 9 to .NET 10 across all projects
- Upgraded Aspire from 9.2.1 to 13.1.2
- Updated Entity Framework Core packages to 10.0.3
- Updated ASP.NET Core packages to 10.0.3
- Updated OpenTelemetry packages to 1.15.0
- Updated Microsoft.Extensions packages to 10.x
- Updated remaining dependencies to latest stable versions
- Updated README.md with full project documentation

## 2025-05-05

### Added

- Quick Links feature with management UI
- Parallel health checks using `Task.WhenAll`
- Dark mode with ThemeService

### Changed

- Updated Aspire SDK and package references
- Removed Seq logging references
- Enhanced UI with hover effects and dark mode styles
- Refactored environment management and navigation UI

## 2025-05-04

### Added

- HealthCheckService background service for periodic health monitoring
- SignalR hub (`MonitorHub`) for real-time client updates
- `LastUpdated` timestamps on environments and applications
- `OrderNumber` property on environments for custom sorting
- `SwaggerUrl` property on applications
- ThemeService for dark mode management
- Configurable health check interval via appsettings

### Changed

- Refactored ApplicationCard styling
- Refactored monitoring components and Home page
- Enhanced project structure and functionality

## 2025-05-03

### Added

- Initial project setup with .NET 9 Aspire
- Blazor Server + WebAssembly (Interactive Auto) dashboard
- SQL Server integration via Aspire
- Entity Framework Core with `HealthConfigDbContext`
- Migration Service for automated database setup and seeding
- CRUD API endpoints for environments and applications
- `MonitoredEnvironment` and `MonitoredApplication` entities
