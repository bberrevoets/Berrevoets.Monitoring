using Berrevoets.Monitoring.Entities;
using Berrevoets.Monitoring.MigrationService;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddHostedService<Worker>();

builder.AddSqlServerDbContext<HealthConfigDbContext>("health-config");

var host = builder.Build();
host.Run();
