using Foundatio.Mediator;

namespace Samples.Core.Models;

/// <summary>Domain event published when an entity is created, updated, or removed.</summary>
public record EntityChanged(string Type, string Id, ChangeType ChangeType) : INotification;

public enum ChangeType
{
    Added,
    Updated,
    Removed
}
