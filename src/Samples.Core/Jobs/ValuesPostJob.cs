using Foundatio.Caching;
using Foundatio.Jobs;
using Foundatio.Mediator;
using Foundatio.Queues;
using Microsoft.Extensions.Logging;
using Samples.Core.Models;

namespace Samples.Core.Jobs;

/// <summary>
/// Background queue processor job. Continuously dequeues ValuesPost items,
/// caches the processed value, publishes an EntityChanged event, and completes
/// the queue entry. Runs in a separate worker process when scaled out.
/// </summary>
public class ValuesPostJob(
    IQueue<ValuesPost> queue,
    ICacheClient cache,
    IMediator mediator,
    ILoggerFactory loggerFactory) : QueueJobBase<ValuesPost>(queue, loggerFactory: loggerFactory)
{
    protected override async Task<JobResult> ProcessQueueEntryAsync(QueueEntryContext<ValuesPost> context)
    {
        var data = context.QueueEntry.Value;

        _logger.LogInformation("Processing queued value {Id}", data.Id);

        if (string.IsNullOrEmpty(data.Value))
        {
            _logger.LogWarning("Value for {Id} was empty", data.Id);
            return JobResult.FailedWithMessage($"Value for {data.Id} was empty");
        }

        await cache.SetAsync(data.Id, data.Value, TimeSpan.FromMinutes(30));
        await mediator.PublishAsync(new EntityChanged("Value", data.Id, ChangeType.Updated));

        _logger.LogInformation("Value {Id} processed and cached successfully", data.Id);
        return JobResult.Success;
    }
}
