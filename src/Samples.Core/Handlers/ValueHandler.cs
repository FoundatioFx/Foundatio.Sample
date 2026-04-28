using Foundatio.Caching;
using Foundatio.Jobs;
using Foundatio.Mediator;
using Foundatio.Queues;
using Foundatio.Storage;
using Microsoft.Extensions.Logging;
using Samples.Core.Messages;
using Samples.Core.Models;

namespace Samples.Core.Handlers;

/// <summary>
/// Handles value CRUD operations. The mediator source generator auto-discovers these methods
/// and MapMediatorEndpoints() creates minimal API routes from them:
///   POST   /api/values       → HandleAsync(CreateValue)
///   GET    /api/values/{id}  → HandleAsync(GetValue)
///   DELETE /api/values/{id}  → HandleAsync(DeleteValue)
/// </summary>
public class ValueHandler(
    IFileStorage storage,
    IQueue<ValuesPost> queue,
    IQueue<WorkItemData> workItemQueue,
    ICacheClient cache,
    ILogger<ValueHandler> logger)
{
    /// <summary>
    /// Saves the value to file storage for local persistence and enqueues it for background processing.
    /// The ValuesPostJob will pick it up, cache the processed value, and publish an EntityChanged event.
    /// Returns a cascading EntityChanged event that is automatically published.
    /// </summary>
    public async Task<(Result<ValueCreated>, EntityChanged?)> HandleAsync(CreateValue command, CancellationToken ct)
    {
        var id = Guid.NewGuid().ToString("N");

        await storage.SaveFileAsync($"{id}.txt", command.Value);
        await queue.EnqueueAsync(new ValuesPost(id, command.Value));

        logger.LogInformation("Value {Id} created and queued for processing", id);

        return (Result<ValueCreated>.Created(new ValueCreated(id, command.Value)),
                new EntityChanged("Value", id, ChangeType.Added));
    }

    /// <summary>Retrieves a processed value from the cache.</summary>
    public async Task<Result<CachedValue>> HandleAsync(GetValue query, CancellationToken ct)
    {
        var value = await cache.GetAsync<string>(query.Id);

        if (!value.HasValue)
            return Result.NotFound($"Value '{query.Id}' not found. It may still be processing.");

        return new CachedValue(query.Id, value.Value);
    }

    /// <summary>
    /// Enqueues a work item for background deletion with progress reporting.
    /// The DeleteValueWorkItemHandler processes it in the shared WorkItemJob pool.
    /// </summary>
    public async Task<(Result, EntityChanged?)> HandleAsync(DeleteValue command, CancellationToken ct)
    {
        await workItemQueue.EnqueueAsync(new DeleteValueWorkItem(command.Id));
        logger.LogInformation("Delete work item queued for value {Id}", command.Id);

        return (Result.Success(), new EntityChanged("Value", command.Id, ChangeType.Removed));
    }
}

public record ValueCreated(string Id, string Value);
public record CachedValue(string Id, string Value);
