using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

[DataContract(Name = "DomainSave")]
public class DomainSave
{
    [DataMember(Name = "valid")]
    public bool Valid { get; set; }

    [DataMember(Name = "nodeId")]
    public int NodeId { get; set; }

    [DataMember(Name = "language")]
    public int Language { get; set; }

    [DataMember(Name = "domains")]
    public DomainDisplay[]? Domains { get; set; }
}
