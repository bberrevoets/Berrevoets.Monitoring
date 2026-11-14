# Changelog

All notable changes to this project will be documented in this file.

## Attributors

- **Bert Berrevoets** -- project owner
- **Claude Opus 4.6** -- co-author of the 2026-03 .NET 10 / Aspire 13.1 update and documentation
- **Claude Opus 5.5** -- co-author of the 2026-11 AI agent setup
- **Claude Fable 5.2** -- co-author of the 2026-11 Aspire 13.6.5 security update
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

### Security

Author: *Bert Berrevoets, Claude Opus 5.5, Claude Fable 5.2, @dependabot*

- Three high-severity transitive packages fixed (Dependabot did not report them):
  - `MessagePack` 2.5.192 → 2.5.307 in the AppHost, by upgrading Aspire 13.1.2 → 13.6.4 (`Aspire.AppHost.Sdk`,
    `Aspire.Hosting.AppHost`, `Aspire.Hosting.SqlServer`, `Aspire.Microsoft.EntityFrameworkCore.SqlServer`). The
    13.2–13.6 release notes were checked first: no breaking change touches this code (nothing reads the renamed
    service-discovery or `DatabaseName` variables)
  - `Microsoft.AspNetCore.SignalR.Common` 10.0.3 → 10.0.14 (CVE-2026-70159), with all Microsoft 10.0.x packages
    (ASP.NET Core, EF Core, Extensions.Hosting) on 10.0.14
  - `System.Security.Cryptography.Pkcs` 9.0.11 → 10.0.14 (CVE-2026-84008): `Microsoft.Data.SqlClient` 6.1.4 → 7.0.2
    (required by the Aspire 13.6 EF integration) still brings 9.0.13 / 10.0.12, so Entities and the AppHost reference
    Pkcs 10.0.14 directly
- Verified with an AppHost run on 2026-11-03: SQL Server container, migration service (exit 0, seed data) and web app
  all healthy; `/api/environments` returned the seeded data
- Aspire 13.6.4 → 13.6.5 (#29) for CVE-2026-120006, a high-severity elevation of privilege in the Aspire dashboard
  and CLI (Dependabot alert #6). Dependabot bumped only `Aspire.Hosting.AppHost`; `Aspire.AppHost.Sdk`,
  `Aspire.Hosting.SqlServer` and `Aspire.Microsoft.EntityFrameworkCore.SqlServer` were raised to 13.6.5 in the same
  PR, so all Aspire references stay on one version. Checked with a Release build and
  `dotnet list package --vulnerable --include-transitive` (no vulnerable packages); no AppHost run. The
  `System.Security.Cryptography.Pkcs` pin stays: `Microsoft.Data.SqlClient` is still 7.0.2

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
