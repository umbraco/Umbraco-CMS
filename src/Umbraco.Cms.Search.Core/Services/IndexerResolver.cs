using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Search.Core.Configuration;

namespace Umbraco.Cms.Search.Core.Services;

/// <summary>
/// Resolves the <see cref="IIndexer"/> registered for a given index alias, based on the configured index registrations.
/// </summary>
internal sealed class IndexerResolver : ResolverBase<IIndexer>, IIndexerResolver
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IndexerResolver"/> class.
    /// </summary>
    public IndexerResolver(IOptions<IndexOptions> indexOptions, IServiceProvider serviceProvider, ILogger<IndexerResolver> logger)
        : base(indexOptions, serviceProvider, logger)
    {
    }

    /// <inheritdoc />
    public IIndexer? GetIndexer(string indexAlias)
        => Resolve(indexAlias, indexRegistration => indexRegistration.Indexer);
}
