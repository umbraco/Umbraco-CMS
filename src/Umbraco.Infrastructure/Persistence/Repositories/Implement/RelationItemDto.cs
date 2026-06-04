using NPoco;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
/// DTO for the projected join used when reading relation items along with the related child node and content type metadata.
/// </summary>
internal sealed class RelationItemDto
{
    /// <summary>
    /// Gets or sets the identifier of the child node in the relation.
    /// </summary>
    [Column(Name = "nodeId")]
    public int ChildNodeId { get; set; }

    /// <summary>
    /// Gets or sets the unique key (GUID) of the child node in this relation.
    /// </summary>
    [Column(Name = "nodeKey")]
    public Guid ChildNodeKey { get; set; }

    /// <summary>
    /// Gets or sets the name of the child node associated with this relation item.
    /// This property is mapped to the 'nodeName' column in the database.
    /// </summary>
    [Column(Name = "nodeName")]
    public string? ChildNodeName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the child node is published.
    /// </summary>
    [Column(Name = "nodePublished")]
    public bool? ChildNodePublished { get; set; }

    /// <summary>
    /// Gets or sets the object type identifier of the child node in the relation.
    /// </summary>
    [Column(Name = "nodeObjectType")]
    public Guid ChildNodeObjectType { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier (key) of the child content type.
    /// </summary>
    [Column(Name = "contentTypeKey")]
    public Guid ChildContentTypeKey { get; set; }

    /// <summary>
    /// Gets or sets the icon associated with the child content type.
    /// </summary>
    [Column(Name = "contentTypeIcon")]
    public string? ChildContentTypeIcon { get; set; }

    /// <summary>
    /// Gets or sets the alias of the child content type.
    /// </summary>
    [Column(Name = "contentTypeAlias")]
    public string? ChildContentTypeAlias { get; set; }

    /// <summary>
    /// Gets or sets the name of the child content type.
    /// </summary>
    [Column(Name = "contentTypeName")]
    public string? ChildContentTypeName { get; set; }

    /// <summary>
    /// Gets or sets the display name of the relation type associated with this relation item.
    /// </summary>
    [Column(Name = "relationTypeName")]
    public string? RelationTypeName { get; set; }

    /// <summary>
    /// Gets or sets the alias of the relation type.
    /// </summary>
    [Column(Name = "relationTypeAlias")]
    public string? RelationTypeAlias { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the relation type represents a dependency between related items.
    /// </summary>
    [Column(Name = "relationTypeIsDependency")]
    public bool RelationTypeIsDependency { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the relation type is bidirectional.
    /// </summary>
    [Column(Name = "relationTypeIsBidirectional")]
    public bool RelationTypeIsBidirectional { get; set; }
}
