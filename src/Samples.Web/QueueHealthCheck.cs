using Foundatio.Queues;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Samples.Web;

/// <summary>
/// Health check that reports queue depth — surfaces in the Aspire dashboard
/// and in /ready so load balancers can detect backpressure.
/// </summary>
public class QueueHealthCheck<T>(IQueue<T> queue) : IHealthCheck where T : class
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct = default)
    {
        var stats = await queue.GetQueueStatsAsync();
        var data = new Dictionary<string, object>
        {
            ["queued"] = stats.Queued,
            ["working"] = stats.Working,
            ["deadletter"] = stats.Deadletter,
            ["enqueued"] = stats.Enqueued,
            ["dequeued"] = stats.Dequeued,
            ["completed"] = stats.Completed,
            ["errors"] = stats.Errors
        };

        if (stats.Queued > 1000)
            return HealthCheckResult.Degraded($"Queue depth is {stats.Queued}", data: data);

        return HealthCheckResult.Healthy($"Queue depth: {stats.Queued}", data: data);
    }
}
