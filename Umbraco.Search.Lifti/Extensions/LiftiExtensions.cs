using System.Globalization;
using Lifti;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Models.Search;
using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Search.Lifti.Extensions;

public static class LiftiExtensions
{
    public static IEnumerable<IUmbracoSearchResult> ToUmbracoResults(this IEnumerable<SearchResult<string>> results)
    {
        var resultsTarget = results.Select(x =>
            new UmbracoSearchResult(x.Key, (float)x.Score, new Dictionary<string, string>().AsReadOnly()));
        return resultsTarget;
    }

    public static IEnumerable<PublishedSearchResult> ToPublishedSearchResults(
        this ISearchResults<string> results,
        IPublishedCache? cache)
    {
        if (cache == null)
        {
            throw new ArgumentNullException(nameof(cache));
        }

        var publishedSearchResults = new List<PublishedSearchResult>();

        foreach (SearchResult<string> result in results)
        {
            if (int.TryParse(result.Key, NumberStyles.Integer, CultureInfo.InvariantCulture, out var contentId))
            {
                IPublishedContent? content = cache.GetById(contentId);
                if (content is not null)
                {
                    publishedSearchResults.Add(new PublishedSearchResult(content, (float)result.Score));
                }
            }
        }

        return publishedSearchResults;
    }
}
