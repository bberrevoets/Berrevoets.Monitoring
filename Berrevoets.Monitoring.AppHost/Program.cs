using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var password = builder.AddParameter("password", true);

var sqlServer = builder.AddSqlServer("sqlserver", password, 1433)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume("Monitoring_SQL");

var db = sqlServer.AddDatabase("health-config");

var seq = builder.AddSeq("seq")
    .ExcludeFromManifest()
    .WithLifetime(ContainerLifetime.Persistent)
    .WithEnvironment("ACCEPT_EULA", "Y")
    .WithExternalHttpEndpoints()
    .WithDataVolume("Monitoring_Seq");

builder.AddProject<Berrevoets_Monitoring_MigrationService>("berrevoets-monitoring-migrationservice")
    .WithReference(db)
    .WaitFor(db)
    .WithParentRelationship(sqlServer);

var monitoring = builder.AddProject<Berrevoets_Monitoring>("berrevoets-monitoring")
    .WithReference(db)
    .WaitFor(db)
    .WithReference(seq)
    .WaitFor(seq)
    .WithExternalHttpEndpoints();

monitoring.WithUrl($"{monitoring.GetEndpoint("https")}/api/environments", "See all");

builder.Build().Run();
