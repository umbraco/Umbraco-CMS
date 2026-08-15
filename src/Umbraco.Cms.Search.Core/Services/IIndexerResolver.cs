namespace Umbraco.Cms.Search.Core.Services;

/// <summary>
/// Resolves the <see cref="IIndexer"/> implementation registered for a given index alias.
/// </summary>
public interface IIndexerResolver
{
    /// <summary>
    /// Gets the indexer registered for the given index alias.
    /// </summary>
    /// <param name="indexAlias">The index alias to resolve.</param>
    /// <returns>The registered indexer, or null if none could be resolved.</returns>
    public IIndexer? GetIndexer(string indexAlias);
}
