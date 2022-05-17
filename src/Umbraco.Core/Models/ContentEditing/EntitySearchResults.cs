using System.Collections;
using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

[DataContract(Name = "searchResults", Namespace = "")]
public class EntitySearchResults : IEnumerable<SearchResultEntity?>
{
    private readonly IEnumerable<SearchResultEntity?> _results;

    public EntitySearchResults(IEnumerable<SearchResultEntity?> results, long totalFound)
    {
        _results = results;
        TotalResults = totalFound;
    }

    [DataMember(Name = "totalResults")]
    public long TotalResults { get; set; }

    public IEnumerator<SearchResultEntity?> GetEnumerator() => _results.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_results).GetEnumerator();
}
