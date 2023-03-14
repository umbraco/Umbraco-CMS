using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

[DataContract(Name = "objectType", Namespace = "")]
public class ObjectType
{
    [DataMember(Name = "name")]
    public string? Name { get; set; }

    [DataMember(Name = "id")]
    public Guid Id { get; set; }
}
