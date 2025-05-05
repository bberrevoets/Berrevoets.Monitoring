using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var password = builder.AddParameter("password", true);

var sqlServer = builder.AddSqlServer("sqlserver", password, 1433)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume("Monitoring_SQL");

var db = sqlServer.AddDatabase("health-config");

builder.AddProject<Berrevoets_Monitoring_MigrationService>("berrevoets-monitoring-migrationservice")
    .WithReference(db)
    .WaitFor(db)
    .WithParentRelationship(sqlServer);

var monitoring = builder.AddProject<Berrevoets_Monitoring>("berrevoets-monitoring")
    .WithReference(db)
    .WaitFor(db)
    .WithExternalHttpEndpoints();

monitoring.WithUrl($"{monitoring.GetEndpoint("https")}/api/environments", "See all");

builder.Build().Run();
