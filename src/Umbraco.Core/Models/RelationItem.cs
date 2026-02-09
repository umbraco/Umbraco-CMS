using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models;

/// <summary>
/// Represents an item in a relation, containing node and content type information.
/// </summary>
[DataContract(Name = "relationItem", Namespace = "")]
public class RelationItem
{
    /// <summary>
    /// Gets or sets the unique identifier of the node.
    /// </summary>
    [DataMember(Name = "id")]
    public int NodeId { get; set; }

    /// <summary>
    /// Gets or sets the unique key (GUID) of the node.
    /// </summary>
    [DataMember(Name = "key")]
    public Guid NodeKey { get; set; }

    /// <summary>
    /// Gets or sets the name of the node.
    /// </summary>
    [DataMember(Name = "name")]
    public string? NodeName { get; set; }

    /// <summary>
    /// Gets or sets the type of the node (e.g., document, media).
    /// </summary>
    [DataMember(Name = "type")]
    public string? NodeType { get; set; }

    /// <summary>
    /// Gets the UDI (Umbraco Document Identifier) for the node.
    /// </summary>
    /// <remarks>
    /// Returns <c>null</c> if the node type is unknown.
    /// </remarks>
    [DataMember(Name = "udi")]
    public Udi? NodeUdi => NodeType == Constants.UdiEntityType.Unknown ? null : Udi.Create(NodeType, NodeKey);

    /// <summary>
    /// Gets or sets a value indicating whether the node is published.
    /// </summary>
    [DataMember(Name = "published")]
    public bool? NodePublished { get; set; }

    /// <summary>
    /// Gets or sets the unique key of the content type.
    /// </summary>
    [DataMember(Name = "contentTypeKey")]
    public Guid ContentTypeKey { get; set; }

    /// <summary>
    /// Gets or sets the icon associated with the content type.
    /// </summary>
    [DataMember(Name = "icon")]
    public string? ContentTypeIcon { get; set; }

    /// <summary>
    /// Gets or sets the alias of the content type.
    /// </summary>
    [DataMember(Name = "alias")]
    public string? ContentTypeAlias { get; set; }

    /// <summary>
    /// Gets or sets the name of the content type.
    /// </summary>
    [DataMember(Name = "contentTypeName")]
    public string? ContentTypeName { get; set; }

    /// <summary>
    /// Gets or sets the name of the relation type.
    /// </summary>
    [DataMember(Name = "relationTypeName")]
    public string? RelationTypeName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the relation type is bidirectional.
    /// </summary>
    [DataMember(Name = "relationTypeIsBidirectional")]
    public bool RelationTypeIsBidirectional { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the relation type represents a dependency.
    /// </summary>
    [DataMember(Name = "relationTypeIsDependency")]
    public bool RelationTypeIsDependency { get; set; }
}
