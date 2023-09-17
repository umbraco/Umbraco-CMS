using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.Search;

[DataContract(Name = "searcher", Namespace = "")]
public class SearcherModel
{
    [DataMember(Name = "name")]
    public string? Name { get; set; }
    [DataMember(Name = "searchProviderDetails")]
    public  IReadOnlyDictionary<string, object?>? SearchProviderDetails { get; set; }
}
