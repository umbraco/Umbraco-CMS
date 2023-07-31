using System.Collections;
using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.Search;

[DataContract(Name = "umbracoSearchResults", Namespace = "")]
public class UmbracoSearchResults : IUmbracoSearchResults
{
    public UmbracoSearchResults(long totalRecords, int pageSize, IEnumerable<UmbracoSearchResult> results)
    {
        TotalRecords = totalRecords;
        PageSize = pageSize;
        Results = results;
    }

    [DataMember(Name = "pageSize")] public int PageSize { get; set; }
    [DataMember(Name = "totalRecords")] public long TotalRecords { get; set; }
    [DataMember(Name = "results")] public IEnumerable<UmbracoSearchResult>? Results { get; set; }


    public long TotalItemCount
    {
        get
        {
            return TotalRecords;
        }
    }

    public static UmbracoSearchResults Empty()
    {
        return new UmbracoSearchResults(0,0,new List<UmbracoSearchResult>());
    }
}
