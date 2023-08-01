using Lifti;
using Umbraco.Cms.Core.Models.Search;
using Umbraco.Search.ValueSet;

namespace Umbraco.Search.InMemory;

public static class Extensions
{
    public static IEnumerable<IUmbracoSearchResult> ToUmbracoResults(this IEnumerable<SearchResult<string>> results)
    {

        var resultsTarget = results.Select(x => new UmbracoSearchResult(x.Key, (float)x.Score,  new Dictionary<string, string>().AsReadOnly() ));
        return resultsTarget;
    }
}
