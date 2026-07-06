using Umbraco.Cms.Search.Core.Services;

namespace Umbraco.Cms.Search.Core.Extensions;

public static class IndexerResolverExtensions
{
    public static IIndexer GetRequiredIndexer(this IIndexerResolver indexerResolver, string indexAlias)
        => indexerResolver.GetIndexer(indexAlias)
           ?? throw new InvalidOperationException($"No indexer was registered for the index: {indexAlias}. Check the logs for more information.");
}
