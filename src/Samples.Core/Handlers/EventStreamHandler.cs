using System.Runtime.CompilerServices;
using Foundatio.Mediator;
using Samples.Core.Messages;
using Samples.Core.Models;

namespace Samples.Core.Handlers;

/// <summary>
/// SSE streaming handler — replaces the old SignalR MessageBusHub with zero dependencies.
/// Clients connect to GET /api/event-stream and receive real-time EntityChanged events
/// as they are published anywhere in the system.
/// </summary>
public class EventStreamHandler(IMediator mediator)
{
    [HandlerEndpoint(Streaming = EndpointStreaming.ServerSentEvents)]
    public async IAsyncEnumerable<EntityChanged> Handle(
        GetEventStream message,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var evt in mediator.SubscribeAsync<EntityChanged>(cancellationToken: cancellationToken))
        {
            yield return evt;
        }
    }
}
