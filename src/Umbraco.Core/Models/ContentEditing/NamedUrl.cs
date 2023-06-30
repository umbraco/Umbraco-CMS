using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

[DataContract(Name = "namedUrl", Namespace = "")]
public class NamedUrl
{
    [DataMember(Name = "name")]
    public required string Name { get; set; }

    [DataMember(Name = "url")]
    public required string Url { get; set; }
}
