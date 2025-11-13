namespace Umbraco.Cms.Core.Models;

/// <summary>
/// Represents the subset of content type information that is needed for typed swagger schema generation.
/// </summary>
public class ContentTypeInfo
{
    /// <summary>
    /// Gets or sets the content type alias.
    /// </summary>
    public required string Alias { get; set; }

    /// <summary>
    /// Gets or sets the content type schema id.
    /// </summary>
    public required string SchemaId { get; set; }

    /// <summary>
    /// Gets or sets the list of schema ids of the content type's compositions.
    /// </summary>
    public required List<string> CompositionSchemaIds { get; set; }

    /// <summary>
    /// Gets or sets the list of content type's properties.
    /// </summary>
    public required List<ContentTypePropertyInfo> Properties { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the content type is an element type.
    /// </summary>
    public bool IsElement { get; set; }
}
