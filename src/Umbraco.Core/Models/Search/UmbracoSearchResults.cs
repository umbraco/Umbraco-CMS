namespace Umbraco.Cms.Core.Models.Search;

public class UmbracoSearchResults {
    public UmbracoSearchResults(long totalRecords, IEnumerable<UmbracoSearchResult> results)
    {
        TotalRecords = totalRecords;
        Results = results;
    }

    public long TotalRecords { get; set; }
    public IEnumerable<UmbracoSearchResult> Results { get; set; }

    public static UmbracoSearchResults Empty()
    {
        return new UmbracoSearchResults(0, new List<UmbracoSearchResult>());
    }
}
