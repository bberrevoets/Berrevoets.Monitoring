# AGENTS.md

Instructions for AI coding agents (Claude Code, Codex and others) working in this repository. `CLAUDE.md` only imports
this file, so edit this one. [README.md](README.md) describes the project for people; this file lists what the code
alone does not tell you.

<!-- repo-init:start -->
## Repository Info

- **Company:** Berrevoets Systems
- **Author:** Bert Berrevoets
- **Contact:** <bert@berrevoets.net>
- **Remote:** <https://github.com/bberrevoets/Berrevoets.Monitoring> (public)

## Workflow Rules

- **GitHub Flow:** `main` is the default and only long-lived branch. It is protected by the ruleset "Protect main":
  no direct pushes, no deletion, changes only through a pull request, and all PR conversations must be resolved
  before merging. Merged head branches are deleted automatically.
- After cloning, run `git config pull.rebase true` once, so `git pull` rebases instead of creating merge commits.
- Work on a branch per topic, push it, and open a PR to `main` only when Bert asks for it, assigned to him
  (`gh pr create --assignee bberrevoets`). PRs are squash-merged. After a merge: switch to `main` and pull, then delete the
  local branch with `git branch -D <branch>` (`-d` refuses after a squash merge) once
  `gh pr view <branch> --json state,headRefOid` shows `MERGED` and the same commit as `git rev-parse <branch>`.
- **"Update the documentation"** means `AGENTS.md`, the project's own `README.md` files (not vendored third-party ones)
  and `CHANGELOG.md`; then commit and push.
- [CHANGELOG.md](CHANGELOG.md) follows Keep a Changelog: the Attributors list at the top, and one `Author: *Name*`
  line under each section heading (`### Added`, `### Changed`, ...).
- Markdown must pass `markdownlint-cli2 "**/*.md"` with [.markdownlint.json](.markdownlint.json) (180-character
  lines) before every commit.
- Binaries go through Git LFS: every `filter=lfs` pattern in `.gitattributes` — the standard set (PDF, ZIP, PNG,
  JPEG, STEP/STP, STL, 3MF, F3D/F3Z) plus any pattern this repository adds. Add a pattern there before committing
  another binary type (GIF, BMP, ...). The patterns are lowercase and match `X.PNG` only where `core.ignorecase` is
  true (Windows, macOS); on a case-sensitive system check a new binary with `git check-attr filter -- <file>` before
  committing it.
- Commit messages use the conventional style (`feat:`, `fix:`, `docs:`, `chore:`).
<!-- repo-init:end -->

## Project Overview

Bert's own health dashboard (since 2025-05): it polls the HTTP health endpoints of configured applications and shows
their status live in a Blazor UI over SignalR. It runs only on a development PC through the Aspire AppHost (SQL Server
in a persistent container); nothing is deployed and there is no CI. `main` is the only long-lived branch since
2026-11-03 (`Development` was retired), and `wwwroot/favicon.png` is in Git LFS since the history rewrite of that day.

## Build and Run Commands

```bash
# Build the entire solution
dotnet build Berrevoets.Monitoring.sln

# Run via Aspire AppHost (starts SQL Server container, migration service, and web app)
dotnet run --project Berrevoets.Monitoring.AppHost

# Run only the web app (requires SQL Server already running)
dotnet run --project Berrevoets.Monitoring/Berrevoets.Monitoring

# Add a new EF Core migration (run from repo root)
dotnet ef migrations add <MigrationName> \
  --project Berrevoets.Monitoring.Entities \
  --startup-project Berrevoets.Monitoring/Berrevoets.Monitoring
```

There are no tests in this solution.

## Configuration

All configuration uses `appsettings.json` only — there are no environment-specific
settings files (`appsettings.Development.json` is gitignored). Sensitive values use
the placeholder `"InSecrets"` in `appsettings.json`, with actual values stored in
.NET User Secrets for local development.

```bash
# Set the SQL Server password for local development
dotnet user-secrets set "Parameters:password" "<actual-password>" \
  --project Berrevoets.Monitoring.AppHost
```

The deploy pipeline transforms `appsettings.json` to inject real secrets.

## Architecture

This is a **.NET 10 Aspire** health monitoring dashboard that periodically checks
HTTP health endpoints of configured applications and displays real-time status
via a Blazor UI.

### Project Structure

- **Berrevoets.Monitoring.AppHost** - Aspire orchestrator. Provisions a SQL Server
  container with a persistent volume, wires up the migration service and web app.
  Entry point for local development.
- **Berrevoets.Monitoring** (server) - ASP.NET Core host running Blazor Server +
  WebAssembly (Interactive Auto rendering mode). Contains:
  - `Services/HealthCheckService.cs` - `BackgroundService` that polls health
    endpoints in parallel on a configurable interval, detects changes, persists
    status to the DB, and pushes updates via SignalR.
  - `HUB/MonitorHub.cs` - SignalR hub at `/monitorHub` broadcasting
    `ReceiveMonitoringUpdate` messages.
  - `Extensions/*Endpoints.cs` - Minimal API CRUD endpoints for environments
    (`/api/environments`), applications (`/api/applications`), and quick links
    (`/api/quicklinks`).
- **Berrevoets.Monitoring.Client** - Blazor WebAssembly project. Pages render in
  `InteractiveWebAssembly` mode. `MonitoringHubService` manages the SignalR
  connection client-side. UI uses Bootstrap + BootstrapIconsForDotNet.
- **Berrevoets.Monitoring.Entities** - Shared library with EF Core
  `HealthConfigDbContext`, entity models (`MonitoredEnvironment`,
  `MonitoredApplication`, `QuickLink`), DTOs, and migrations. Used by both
  server and migration service.
- **Berrevoets.Monitoring.MigrationService** - Worker service that applies EF
  migrations and seeds sample data on first run, then shuts down.
- **Berrevoets.Monitoring.ServiceDefaults** - Aspire shared defaults:
  OpenTelemetry, service discovery, resilience, health check endpoints
  (`/health`, `/alive`).

### Key Data Flow

1. AppHost starts SQL Server, MigrationService (creates/migrates DB), then the
   web app.
2. `HealthCheckService` runs on a timer (default 10s, configurable via
   `HealthCheckService:CheckIntervalSeconds` in appsettings). It loads
   environments+applications from DB, checks health endpoints in parallel with
   5s timeout, normalizes status to `Healthy`/`Unhealthy`/`Degraded`/`Error`/
   `Unknown`, and only pushes SignalR updates when changes are detected.
3. The Blazor client connects to `/monitorHub`, receives
   `ReceiveMonitoringUpdate` with `List<MonitoredEnvironmentDto>`, and renders
   application cards sorted by status priority (unhealthy first).

### Database

SQL Server via Aspire integration. Connection string name: `health-config`.
Three tables: `MonitoredEnvironments`, `MonitoredApplications`, `QuickLinks`.
Environments have a one-to-many relationship with Applications.

### Conventions

- API endpoints follow REST conventions and are organized as static extension
  methods in `Extensions/` directory.
- DTOs in `Entities/DTO/` are used for API responses and SignalR messages;
  entity models are never exposed directly.
- Status values are normalized to: `Healthy`, `Unhealthy`, `Degraded`, `Error`,
  `Unknown`.
- Environments and applications sorted by `OrderNumber` and status priority
  respectively.
