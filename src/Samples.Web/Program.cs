using Foundatio.Mediator;
using Foundatio.Queues;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Samples.Core.Models;
using Samples.Web;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<SampleOptions>(builder.Configuration.GetSection("Sample"));

var runJobsInProcess = builder.Configuration.GetValue("RunJobsInProcess", true);
Samples.Core.Bootstrapper.RegisterServices(builder.Services, runJobsInProcess);

var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
Samples.Insulation.Bootstrapper.RegisterServices(builder.Services, redisConnectionString);

builder.AddRedisClient("Redis");

builder.Services.AddMediator();

builder.Services.AddOpenApi();

var serviceName = builder.Configuration.GetValue<string>("Sample:AppName") ?? "foundatio-sample";

builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService(serviceName))
    .WithTracing(t => t
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddSource("Foundatio.*")
        .AddOtlpExporter())
    .WithMetrics(m => m
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddMeter("Foundatio.*")
        .AddOtlpExporter());

builder.Services.AddHealthChecks()
    .Add(new HealthCheckRegistration(
        "ValuesQueue",
        sp => new QueueHealthCheck<ValuesPost>(sp.GetRequiredService<IQueue<ValuesPost>>()),
        failureStatus: HealthStatus.Degraded,
        tags: ["ready"]));

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();
// Liveness — no checks, just confirm the process responds
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false
});
// Readiness — queue depth + any other "ready"-tagged checks
app.MapHealthChecks("/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.MapGet("/", () => Results.Redirect("/scalar")).ExcludeFromDescription();

app.MapMediatorEndpoints();

app.Run();
