using Umbraco.Cms.Search.Core.Services;

namespace Umbraco.Cms.Search.Core.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IIndexerResolver"/>.
/// </summary>
public static class IndexerResolverExtensions
{
    /// <summary>
    /// Gets the indexer registered for the specified index alias, throwing if none was found.
    /// </summary>
    /// <param name="indexerResolver">The indexer resolver.</param>
    /// <param name="indexAlias">The alias of the index.</param>
    /// <returns>The resolved indexer.</returns>
    public static IIndexer GetRequiredIndexer(this IIndexerResolver indexerResolver, string indexAlias)
        => indexerResolver.GetIndexer(indexAlias)
           ?? throw new InvalidOperationException($"No indexer was registered for the index: {indexAlias}. Check the logs for more information.");
}
