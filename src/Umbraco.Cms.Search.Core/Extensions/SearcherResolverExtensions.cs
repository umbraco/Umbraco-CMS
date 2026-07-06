using Umbraco.Cms.Search.Core.Services;

namespace Umbraco.Cms.Search.Core.Extensions;

public static class SearcherResolverExtensions
{
    public static ISearcher GetRequiredSearcher(this ISearcherResolver searcherResolver, string indexAlias)
        => searcherResolver.GetSearcher(indexAlias)
           ?? throw new InvalidOperationException($"No searcher was registered for the index: {indexAlias}. Check the logs for more information.");
}
