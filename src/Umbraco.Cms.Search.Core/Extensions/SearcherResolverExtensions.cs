using Umbraco.Cms.Search.Core.Services;

namespace Umbraco.Cms.Search.Core.Extensions;

/// <summary>
/// Provides extension methods for <see cref="ISearcherResolver"/>.
/// </summary>
public static class SearcherResolverExtensions
{
    /// <summary>
    /// Gets the searcher registered for the specified index alias, throwing if none was found.
    /// </summary>
    /// <param name="searcherResolver">The searcher resolver.</param>
    /// <param name="indexAlias">The alias of the index.</param>
    /// <returns>The resolved searcher.</returns>
    public static ISearcher GetRequiredSearcher(this ISearcherResolver searcherResolver, string indexAlias)
        => searcherResolver.GetSearcher(indexAlias)
           ?? throw new InvalidOperationException($"No searcher was registered for the index: {indexAlias}. Check the logs for more information.");
}
