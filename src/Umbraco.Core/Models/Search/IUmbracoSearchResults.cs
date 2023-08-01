using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Core.Models.Search;

public interface IUmbracoSearchResults
{

    [DataMember(Name = "pageSize")]
    public int PageSize { get; set; }

    [DataMember(Name = "totalRecords")]
    public long TotalRecords { get; set; }

    [DataMember(Name = "results")]
    public IEnumerable<IUmbracoSearchResult>? Results { get; set; }

}
