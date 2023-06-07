using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

[DataContract(Name = "results", Namespace = "")]
public class SearchResults
{
    [DataMember(Name = "pageSize")]
    public int PageSize { get; set; }

    [DataMember(Name = "totalRecords")]
    public long TotalRecords { get; set; }

    [DataMember(Name = "results")]
    public IEnumerable<SearchResult>? Results { get; set; }

    public static SearchResults Empty() => new() { Results = Enumerable.Empty<SearchResult>(), TotalRecords = 0 };
}
