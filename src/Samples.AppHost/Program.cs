var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("Redis")
    .WithLifetime(ContainerLifetime.Persistent);

builder.AddProject<Projects.Samples_Web>("web")
    .WithReference(redis)
    .WaitFor(redis)
    .WithEnvironment("RunJobsInProcess", "false")
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints();

builder.AddProject<Projects.Samples_Jobs>("jobs")
    .WithReference(redis)
    .WaitFor(redis)
    .WithReplicas(3);

builder.Build().Run();
