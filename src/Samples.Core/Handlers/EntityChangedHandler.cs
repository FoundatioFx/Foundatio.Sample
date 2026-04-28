using Microsoft.Extensions.Logging;
using Samples.Core.Models;

namespace Samples.Core.Handlers;

/// <summary>Reacts to all EntityChanged notifications — logs the event.</summary>
public class EntityChangedHandler(ILogger<EntityChangedHandler> logger)
{
    public void Handle(EntityChanged notification)
    {
        logger.LogInformation("Entity changed: {Type} {Id} was {ChangeType}",
            notification.Type, notification.Id, notification.ChangeType);
    }
}
