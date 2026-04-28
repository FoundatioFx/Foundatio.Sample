using Foundatio.Caching;
using Foundatio.Jobs;
using Foundatio.Mediator;
using Microsoft.Extensions.Logging;
using Samples.Core.Models;

namespace Samples.Core.Jobs;

/// <summary>
/// Work item handler for deleting cached values. Runs in the shared WorkItemJob pool
/// alongside other work item types — great for infrequent, long-running tasks.
/// Demonstrates progress reporting via the message bus.
/// </summary>
public class DeleteValueWorkItemHandler(
    ICacheClient cache,
    IMediator mediator,
    ILoggerFactory loggerFactory) : WorkItemHandlerBase
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<DeleteValueWorkItemHandler>();

    public override async Task HandleItemAsync(WorkItemContext ctx)
    {
        var workItem = ctx.GetData<DeleteValueWorkItem>();
        if (workItem is null)
        {
            _logger.LogWarning("Received null work item data");
            return;
        }

        await ctx.ReportProgressAsync(0, $"Starting delete of value {workItem.Id}");
        _logger.LogInformation("Processing delete work item for value {Id}", workItem.Id);

        await cache.RemoveAsync(workItem.Id);
        await ctx.ReportProgressAsync(50, "Removed from cache");

        await mediator.PublishAsync(new EntityChanged("Value", workItem.Id, ChangeType.Removed));
        await ctx.ReportProgressAsync(100, "Delete complete");

        _logger.LogInformation("Value {Id} deleted successfully", workItem.Id);
    }
}
