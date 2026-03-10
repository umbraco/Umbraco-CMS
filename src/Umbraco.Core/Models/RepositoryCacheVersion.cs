namespace Umbraco.Cms.Core.Models;

/// <summary>
/// Represents a version of a repository cache.
/// </summary>
public class RepositoryCacheVersion
{
    /// <summary>
    /// The unique identifier for the cache.
    /// </summary>
    public required string Identifier { get; init; }

    /// <summary>
    /// The identifier of the version of the cache.
    /// </summary>
    public required string? Version { get; init; }
}
