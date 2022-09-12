using System.Runtime.Serialization;

namespace Umbraco.Cms.ManagementApi.ViewModels.Dictionary;

[DataContract]
public class DictionaryItemViewModel
{
    [DataMember(Name = "parentId")]
    public int ParentId { get; set; }

    [DataMember(Name = "key")]
    public string Key { get; set; } = null!;
}
