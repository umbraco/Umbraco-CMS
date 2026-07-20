using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Search.Core.Configuration;

namespace Umbraco.Cms.Search.Core.Services;

internal sealed class SearcherResolver : ResolverBase<ISearcher>, ISearcherResolver
{
    public SearcherResolver(IOptions<IndexOptions> indexOptions, IServiceProvider serviceProvider, ILogger<SearcherResolver> logger)
        : base(indexOptions, serviceProvider, logger)
    {
    }

    public ISearcher? GetSearcher(string indexAlias)
        => Resolve(indexAlias, indexRegistration => indexRegistration.Searcher);
}
