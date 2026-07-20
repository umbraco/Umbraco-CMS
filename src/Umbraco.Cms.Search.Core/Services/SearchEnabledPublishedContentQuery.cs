using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Infrastructure;
using Umbraco.Cms.Search.Core.Models.Searching;

namespace Umbraco.Cms.Search.Core.Services;

/// <summary>
///     The Umbraco Search based implementation of the <see cref="Cms.Core.IPublishedContentQuery" /> search members.
/// </summary>
/// <remarks>
///     Replaces the base implementation (whose search members are not supported without a search engine) via
///     <see cref="DependencyInjection.UmbracoBuilderExtensions.AddSearchCore" />.
/// </remarks>
internal sealed class SearchEnabledPublishedContentQuery : PublishedContentQuery
{
    // mirrors the legacy Examine default max results, used when searching without explicit pagination
    private const int DefaultMaxResults = 500;

    private readonly ISearcherResolver _searcherResolver;
    private readonly IVariationContextAccessor _variationContextAccessor;
    private readonly IPublishedContentCache _publishedContentCache;

    public SearchEnabledPublishedContentQuery(
        ISearcherResolver searcherResolver,
        IVariationContextAccessor variationContextAccessor,
        IPublishedContentCache publishedContent,
        IPublishedMediaCache publishedMediaCache,
        IDocumentNavigationQueryService documentNavigationQueryService,
        IMediaNavigationQueryService mediaNavigationQueryService)
        : base(variationContextAccessor, publishedContent, publishedMediaCache, documentNavigationQueryService, mediaNavigationQueryService)
    {
        _searcherResolver = searcherResolver;
        _variationContextAccessor = variationContextAccessor;
        _publishedContentCache = publishedContent;
    }

    public override IEnumerable<PublishedSearchResult> Search(
        string term,
        int skip,
        int take,
        out long totalRecords,
        string culture = "*",
        string indexName = Umbraco.Cms.Core.Constants.IndexAliases.PublishedContent,
        ISet<string>? loadedFields = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(skip);
        ArgumentOutOfRangeException.ThrowIfNegative(take);

        ISearcher searcher = _searcherResolver.GetSearcher(indexName)
                             ?? throw new InvalidOperationException($"No searcher could be resolved for the index alias {indexName}.");

        // NOTE: the legacy "*" culture searched across all cultures; the search abstraction searches one culture
        //       (plus invariant), so "*" is mapped to the ambient variation context culture.
        var searchCulture = culture == "*"
            ? _variationContextAccessor.VariationContext?.Culture
            : culture;
        searchCulture = string.IsNullOrWhiteSpace(searchCulture) ? null : searchCulture;

        SearchResult result = searcher
            .SearchAsync(
                indexName,
                query: term,
                culture: searchCulture,
                skip: skip,
                take: take > 0 ? take : DefaultMaxResults)
            .GetAwaiter()
            .GetResult();

        totalRecords = result.Total;

        IEnumerable<PublishedSearchResult> results = ToPublishedSearchResults(result, _publishedContentCache);
        return searchCulture is null
            ? results
            : new CultureContextualSearchResults(results, _variationContextAccessor, searchCulture);
    }

    // documents carry no score; synthesize strictly decreasing rank scores so ordering by score preserves relevance
    private static IEnumerable<PublishedSearchResult> ToPublishedSearchResults(SearchResult result, IPublishedContentCache publishedContentCache)
        => result.Documents
            .Select((document, index) => new
            {
                Content = publishedContentCache.GetById(document.Id),
                Score = (float)(result.Total - index),
            })
            .Where(x => x.Content is not null)
            .Select(x => new PublishedSearchResult(x.Content!, x.Score))
            .ToArray();
}
