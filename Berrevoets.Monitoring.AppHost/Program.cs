var builder = DistributedApplication.CreateBuilder(args);

var password = builder.AddParameter("password", true);

var sqlServer = builder.AddSqlServer("sqlserver", password, 1433)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume("MonitoringSql");

var db = sqlServer.AddDatabase("health-config");

builder.AddProject<Projects.Berrevoets_Monitoring_MigrationService>("berrevoets-monitoring-migrationservice")
    .WithReference(db)
    .WaitFor(db)
    .WithParentRelationship(sqlServer);

builder.AddProject<Projects.Berrevoets_Monitoring>("berrevoets-monitoring")
    .WithReference(db)
    .WaitFor(db)
    .WithExternalHttpEndpoints();

builder.Build().Run();
