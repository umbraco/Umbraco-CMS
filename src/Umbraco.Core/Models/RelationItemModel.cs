namespace Umbraco.Cms.Core.Models;

/// <summary>
/// Represents a model for a relation item, containing node and content type information.
/// </summary>
public class RelationItemModel
{
    /// <summary>
    /// Gets or sets the unique key (GUID) of the node.
    /// </summary>
    public Guid NodeKey { get; set; }

    /// <summary>
    /// Gets or sets the alias of the node.
    /// </summary>
    public string? NodeAlias { get; set; }

    /// <summary>
    /// Gets or sets the name of the node.
    /// </summary>
    public string? NodeName { get; set; }

    /// <summary>
    /// Gets or sets the type of the node (e.g., document, media).
    /// </summary>
    public string? NodeType { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the node is published.
    /// </summary>
    public bool? NodePublished { get; set; }

    /// <summary>
    /// Gets or sets the unique key of the content type.
    /// </summary>
    public Guid ContentTypeKey { get; set; }

    /// <summary>
    /// Gets or sets the icon associated with the content type.
    /// </summary>
    public string? ContentTypeIcon { get; set; }

    /// <summary>
    /// Gets or sets the alias of the content type.
    /// </summary>
    public string? ContentTypeAlias { get; set; }

    /// <summary>
    /// Gets or sets the name of the content type.
    /// </summary>
    public string? ContentTypeName { get; set; }

    /// <summary>
    /// Gets or sets the name of the relation type.
    /// </summary>
    public string? RelationTypeName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the relation type is bidirectional.
    /// </summary>
    public bool RelationTypeIsBidirectional { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the relation type represents a dependency.
    /// </summary>
    public bool RelationTypeIsDependency { get; set; }
}
