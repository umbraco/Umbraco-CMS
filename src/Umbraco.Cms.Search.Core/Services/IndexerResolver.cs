using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Search.Core.Configuration;

namespace Umbraco.Cms.Search.Core.Services;

internal sealed class IndexerResolver : ResolverBase<IIndexer>, IIndexerResolver
{
    public IndexerResolver(IOptions<IndexOptions> indexOptions, IServiceProvider serviceProvider, ILogger<IndexerResolver> logger)
        : base(indexOptions, serviceProvider, logger)
    {
    }

    public IIndexer? GetIndexer(string indexAlias)
        => Resolve(indexAlias, indexRegistration => indexRegistration.Indexer);
}
