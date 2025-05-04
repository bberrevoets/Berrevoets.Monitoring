using Berrevoets.Monitoring;
using Berrevoets.Monitoring.Components;
using Berrevoets.Monitoring.Entities;
using Berrevoets.Monitoring.Entities.DTO;
using Berrevoets.Monitoring.HUB;
using Berrevoets.Monitoring.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<HealthCheckSettings>(
    builder.Configuration.GetSection("HealthCheckService"));

builder.AddServiceDefaults();

builder.AddSqlServerDbContext<HealthConfigDbContext>("health-config");

builder.Services.AddSignalR(options => { options.EnableDetailedErrors = true; });
builder.Services.AddScoped<Berrevoets.Monitoring.Client.Services.MonitoringHubService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy
            .SetIsOriginAllowed(_ => true)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

builder.Services.AddHostedService<HealthCheckService>();
builder.Services.AddHttpClient();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

var app = builder.Build();

app.UseCors("CorsPolicy");

app.MapHub<MonitorHub>("/monitorHub");

app.MapGet("/see_all", async (HealthConfigDbContext db) =>
{
    var all = await db.MonitoredEnvironments
        .Include(env => env.Applications)
        .Select(env => new MonitoredEnvironmentDto
        {
            Id = env.Id,
            Name = env.Name,
            IsActive = env.IsActive,
            LastUpdated = env.LastUpdated,
            Applications = env.Applications.Select(application => new MonitoredApplicationDto
                {
                    Id = application.Id,
                    Name = application.Name,
                    Description = application.Description,
                    HealthEndpoint = application.HealthEndpoint,
                    IsActive = application.IsActive,
                    Status = application.Status,
                    StatusMessage = application.StatusMessage,
                    LastUpdated = application.LastUpdated
                })
                .ToList()
        })
        .ToListAsync();

    var orderSequence = new[] { "Development", "Acceptation", "Routevision", "Fleetfinder" };

    var orderedAll = all
        .OrderBy(env => Array.IndexOf(orderSequence, env.Name))
        .ToList();

    return orderedAll;
});

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Berrevoets.Monitoring.Client._Imports).Assembly);

app.Run();
