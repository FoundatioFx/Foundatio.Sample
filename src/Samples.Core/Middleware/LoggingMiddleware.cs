using Microsoft.Extensions.Logging;

namespace Samples.Core.Middleware;

/// <summary>
/// Mediator pipeline middleware — logs every message before and after handler execution.
/// Discovered automatically by the source generator (class name ends in "Middleware").
/// </summary>
public class LoggingMiddleware
{
    public void Before(object message, ILogger<LoggingMiddleware> logger)
        => logger.LogDebug("→ Handling {MessageType}", message.GetType().Name);

    public void After(object message, ILogger<LoggingMiddleware> logger)
        => logger.LogDebug("← Handled {MessageType}", message.GetType().Name);
}
