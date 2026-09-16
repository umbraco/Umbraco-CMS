using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Search.Core.Models.Searching;
using Umbraco.Cms.Search.Core.Models.Searching.Filtering;
using Umbraco.Cms.Search.Core.Services;
using SearchConstants = Umbraco.Cms.Search.Core.Constants;

namespace Umbraco.Extensions;

/// <summary>
///     Umbraco Search based search extensions for <see cref="IPublishedContent" />, replacing the legacy
///     Examine based SearchChildren/SearchDescendants extensions.
/// </summary>
public static class PublishedContentSearchExtensions
{
    // mirrors the legacy Examine default max results
    private const int DefaultMaxResults = 500;

    /// <summary>
    ///     Searches the descendants of the content item for a term.
    /// </summary>
    /// <param name="content">The content item whose descendants to search.</param>
    /// <param name="term">The term to search.</param>
    /// <param name="culture">The culture to search (defaults to the ambient variation context culture).</param>
    public static IEnumerable<PublishedSearchResult> SearchDescendants(
        this IPublishedContent content,
        string term,
        string? culture = null)
        // NOTE: PathIds contains ancestors-or-self, so the item itself is filtered from the results afterwards
        //       to retain the semantics of the legacy implementation.
        => Search(term, culture, new KeywordFilter(SearchConstants.FieldNames.PathIds, [content.Key.ToString("D")], Negate: false))
            .Where(result => result.Content.Key != content.Key);

    /// <summary>
    ///     Searches the children of the content item for a term.
    /// </summary>
    /// <param name="content">The content item whose children to search.</param>
    /// <param name="term">The term to search.</param>
    /// <param name="culture">The culture to search (defaults to the ambient variation context culture).</param>
    public static IEnumerable<PublishedSearchResult> SearchChildren(
        this IPublishedContent content,
        string term,
        string? culture = null)
        => Search(term, culture, new KeywordFilter(SearchConstants.FieldNames.ParentId, [content.Key.ToString("D")], Negate: false));

    private static IEnumerable<PublishedSearchResult> Search(string term, string? culture, Filter filter)
    {
        IServiceProvider services = StaticServiceProvider.Instance;
        ISearcher searcher = services.GetRequiredService<ISearcherResolver>().GetSearcher(Umbraco.Cms.Core.Constants.IndexAliases.PublishedContent)
                             ?? throw new InvalidOperationException($"No searcher could be resolved for the index alias {Umbraco.Cms.Core.Constants.IndexAliases.PublishedContent}.");

        var searchCulture = culture
                            ?? services.GetRequiredService<IVariationContextAccessor>().VariationContext?.Culture;
        searchCulture = string.IsNullOrWhiteSpace(searchCulture) ? null : searchCulture;

        SearchResult result = searcher
            .SearchAsync(
                Umbraco.Cms.Core.Constants.IndexAliases.PublishedContent,
                query: term,
                filters: [filter],
                culture: searchCulture,
                skip: 0,
                take: DefaultMaxResults)
            .GetAwaiter()
            .GetResult();

        IPublishedContentCache publishedContentCache = services.GetRequiredService<IPublishedContentCache>();

        // documents carry no score; synthesize strictly decreasing rank scores so ordering by score preserves relevance
        return result.Documents
            .Select((document, index) => new
            {
                Content = publishedContentCache.GetById(document.Id),
                Score = (float)(result.Total - index),
            })
            .Where(x => x.Content is not null)
            .Select(x => new PublishedSearchResult(x.Content!, x.Score))
            .ToArray();
    }
}
