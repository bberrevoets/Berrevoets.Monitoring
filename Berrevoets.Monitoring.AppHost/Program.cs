var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Berrevoets_Monitoring>("berrevoets-monitoring");

builder.Build().Run();
