# Changelog

All notable changes to this project will be documented in this file.

## Attributors

- **Bert Berrevoets** -- project owner
- **Claude Opus 4.6** -- co-author of the 2026-03 .NET 10 / Aspire 13.1 update and documentation
- **Claude Opus 5.5** -- co-author of the 2026-11 AI agent setup
- **@dependabot** -- grouped security updates

## [Unreleased]

### Changed

Author: *Bert Berrevoets, Claude Opus 5.5*

- AI agent setup: the instructions moved from `CLAUDE.md` to `AGENTS.md` (single source for Claude Code, Codex and
  other agents) with a `CLAUDE.md` stub that imports it; Markdown line limit 120 → 180; repo-init blocks in
  `.gitignore` and `.gitattributes`; Conventions section in `README.md`; this changelog got Attributors and
  `Author` lines
- `main` is the only long-lived branch: `Development` (the default since 2026-03) was merged into `main` (#26) and
  removed together with its "Protect Development" ruleset
- `wwwroot/favicon.png` moved to Git LFS. The history was rewritten (`git lfs migrate import --include="*.png"
  --everything`, force-pushed on 2026-11-03; every commit SHA changed, backup in
  `E:\Backups\GitHub\Berrevoets.Monitoring-2026-11-03`). Re-clone, or run
  `git fetch origin && git reset --hard origin/main && git lfs pull` in an existing clone

Author: *@dependabot*

- Grouped security updates of the NuGet packages (#23, #24, #25; 2026-04 to 2026-10)

## 2026-03-07

### Added

Author: *Bert Berrevoets, Claude Opus 4.6*

- CLAUDE.md for Claude Code guidance
- Markdownlint configuration (120 char line length)
- GitHub branch workflow: `Development` as default, `main` as production
- Branch protection rulesets for both `Development` and `main`

### Changed

Author: *Bert Berrevoets, Claude Opus 4.6*

- Consolidated settings to `appsettings.json` + User Secrets; removed all
  `appsettings.Development.json` files
- Added `Parameters.password` placeholder (`"InSecrets"`) to AppHost
  `appsettings.json`
- Removed redundant configuration loading in web app `Program.cs`
- Added `appsettings.Development.json` to `.gitignore`
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

Author: *Bert Berrevoets*

- Quick Links feature with management UI
- Parallel health checks using `Task.WhenAll`
- Dark mode with ThemeService

### Changed

Author: *Bert Berrevoets*

- Updated Aspire SDK and package references
- Removed Seq logging references
- Enhanced UI with hover effects and dark mode styles
- Refactored environment management and navigation UI

## 2025-05-04

### Added

Author: *Bert Berrevoets*

- HealthCheckService background service for periodic health monitoring
- SignalR hub (`MonitorHub`) for real-time client updates
- `LastUpdated` timestamps on environments and applications
- `OrderNumber` property on environments for custom sorting
- `SwaggerUrl` property on applications
- ThemeService for dark mode management
- Configurable health check interval via appsettings

### Changed

Author: *Bert Berrevoets*

- Refactored ApplicationCard styling
- Refactored monitoring components and Home page
- Enhanced project structure and functionality

## 2025-05-03

### Added

Author: *Bert Berrevoets*

- Initial project setup with .NET 9 Aspire
- Blazor Server + WebAssembly (Interactive Auto) dashboard
- SQL Server integration via Aspire
- Entity Framework Core with `HealthConfigDbContext`
- Migration Service for automated database setup and seeding
- CRUD API endpoints for environments and applications
- `MonitoredEnvironment` and `MonitoredApplication` entities
