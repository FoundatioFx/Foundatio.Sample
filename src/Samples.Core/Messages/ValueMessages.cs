namespace Samples.Core.Messages;

// Commands
public record CreateValue(string Value);
public record DeleteValue(string Id);

// Queries
public record GetValue(string Id);

// Streaming
public record GetEventStream;
