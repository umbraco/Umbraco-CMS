using System.Runtime.Serialization;

namespace Umbraco.Cms.ManagementApi.ViewModels.Search;

[DataContract(Name = "searchResultViewModel", Namespace = "")]
public class SearchResultViewModel
{
    [DataMember(Name = "id")]

    public string Id { get; set; } = null!;
    [DataMember(Name = "score")]

    public float Score { get; set; }
    [DataMember(Name = "fieldCount")]

    public int FieldCount => Fields.Count();
    [DataMember(Name = "fields")]

    public IEnumerable<FieldViewModel> Fields { get; set; } = null!;
}
