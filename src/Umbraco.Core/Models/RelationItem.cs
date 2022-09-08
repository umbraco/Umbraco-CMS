using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models;

[DataContract(Name = "relationItem", Namespace = "")]
public class RelationItem
{
    [DataMember(Name = "id")]
    public int NodeId { get; set; }

    [DataMember(Name = "key")]
    public Guid NodeKey { get; set; }

    [DataMember(Name = "name")]
    public string? NodeName { get; set; }

    [DataMember(Name = "type")]
    public string? NodeType { get; set; }

    [DataMember(Name = "udi")]
    public Udi? NodeUdi => NodeType == Constants.UdiEntityType.Unknown ? null : Udi.Create(NodeType, NodeKey);

    [DataMember(Name = "icon")]
    public string? ContentTypeIcon { get; set; }

    [DataMember(Name = "alias")]
    public string? ContentTypeAlias { get; set; }

    [DataMember(Name = "contentTypeName")]
    public string? ContentTypeName { get; set; }

    [DataMember(Name = "relationTypeName")]
    public string? RelationTypeName { get; set; }

    [DataMember(Name = "relationTypeIsBidirectional")]
    public bool RelationTypeIsBidirectional { get; set; }

    [DataMember(Name = "relationTypeIsDependency")]
    public bool RelationTypeIsDependency { get; set; }
}
