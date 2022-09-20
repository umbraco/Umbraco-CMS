using System.Runtime.Serialization;

namespace Umbraco.Cms.ManagementApi.ViewModels.Dictionary;

[DataContract]
public class DictionaryItemViewModel
{
    [DataMember(Name = "parentId")]
    public Guid? ParentId { get; set; }

    [DataMember(Name = "key")]
    public Guid Key { get; set; }
}
