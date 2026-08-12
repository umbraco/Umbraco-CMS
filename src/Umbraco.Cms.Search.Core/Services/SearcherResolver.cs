using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Search.Core.Configuration;

namespace Umbraco.Cms.Search.Core.Services;

/// <summary>
/// Resolves the <see cref="ISearcher"/> registered for a given index alias, based on the configured index registrations.
/// </summary>
internal sealed class SearcherResolver : ResolverBase<ISearcher>, ISearcherResolver
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SearcherResolver"/> class.
    /// </summary>
    public SearcherResolver(IOptions<IndexOptions> indexOptions, IServiceProvider serviceProvider, ILogger<SearcherResolver> logger)
        : base(indexOptions, serviceProvider, logger)
    {
    }

    /// <inheritdoc />
    public ISearcher? GetSearcher(string indexAlias)
        => Resolve(indexAlias, indexRegistration => indexRegistration.Searcher);
}
