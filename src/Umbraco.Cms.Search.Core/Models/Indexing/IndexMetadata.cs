namespace Umbraco.Cms.Search.Core.Models.Indexing;

/// <summary>
/// Describes the current state of a search index.
/// </summary>
/// <param name="DocumentCount">The number of documents in the index.</param>
/// <param name="HealthStatus">The health of the index.</param>
/// <param name="ProviderName">The name of the search provider backing the index.</param>
public record IndexMetadata(long DocumentCount, HealthStatus HealthStatus, string ProviderName);
