using Berrevoets.Monitoring;
using Berrevoets.Monitoring.Client.Services;
using Berrevoets.Monitoring.Components;
using Berrevoets.Monitoring.Entities;
using Berrevoets.Monitoring.Extensions;
using Berrevoets.Monitoring.HUB;
using Berrevoets.Monitoring.Services;
using _Imports = Berrevoets.Monitoring.Client._Imports;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", false, true);
builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true, true);

builder.Services.Configure<HealthCheckSettings>(
    builder.Configuration.GetSection("HealthCheckService"));

builder.Services.AddScoped<ThemeService>();

builder.AddServiceDefaults();

builder.AddSqlServerDbContext<HealthConfigDbContext>("health-config");

builder.Services.AddSignalR(options => { options.EnableDetailedErrors = true; });
builder.Services.AddScoped<MonitoringHubService>();

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

app.MapEnvironmentEndpoints();
app.MapApplicationEndpoints();
app.MapQuickLinkEndpoints();

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
    .AddAdditionalAssemblies(typeof(_Imports).Assembly);

app.Run();
