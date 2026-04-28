namespace Samples.Core.Models;

/// <summary>Queued item representing a value to be processed by the background job.</summary>
public record ValuesPost(string Id, string Value);
