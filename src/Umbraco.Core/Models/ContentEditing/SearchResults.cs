using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "results", Namespace = "")]
    public class SearchResults
    {
        public static SearchResults Empty() => new SearchResults
        {
            Results = Enumerable.Empty<SearchResult>(),
            TotalRecords = 0
        };

        [DataMember(Name = "totalRecords")]
        public long TotalRecords { get; set; }

        [DataMember(Name = "results")]
        public IEnumerable<SearchResult> Results { get; set; }
    }
}
