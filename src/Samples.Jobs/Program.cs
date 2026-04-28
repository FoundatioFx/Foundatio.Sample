using Foundatio.Mediator;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = Host.CreateApplicationBuilder(args);

Samples.Core.Bootstrapper.RegisterServices(builder.Services);

var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
Samples.Insulation.Bootstrapper.RegisterServices(builder.Services, redisConnectionString);

builder.AddRedisClient("Redis");

builder.Services.AddMediator();

builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("foundatio-sample-jobs"))
    .WithTracing(t => t
        .AddHttpClientInstrumentation()
        .AddSource("Foundatio.*")
        .AddOtlpExporter())
    .WithMetrics(m => m
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddMeter("Foundatio.*")
        .AddOtlpExporter());

builder.Services.AddHealthChecks();

var host = builder.Build();
host.Run();
