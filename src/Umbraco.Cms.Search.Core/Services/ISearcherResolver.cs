namespace Umbraco.Cms.Search.Core.Services;

/// <summary>
/// Resolves the <see cref="ISearcher"/> implementation registered for a given index alias.
/// </summary>
public interface ISearcherResolver
{
    /// <summary>
    /// Gets the searcher registered for the given index alias.
    /// </summary>
    /// <param name="indexAlias">The index alias to resolve.</param>
    /// <returns>The registered searcher, or null if none could be resolved.</returns>
    public ISearcher? GetSearcher(string indexAlias);
}
