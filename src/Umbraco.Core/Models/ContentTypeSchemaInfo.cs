namespace Umbraco.Cms.Core.Models;

/// <summary>
/// Represents the subset of content type information that is needed for schema generation.
/// </summary>
public class ContentTypeSchemaInfo
{
    /// <summary>
    /// Gets the content type alias.
    /// </summary>
    public required string Alias { get; init; }

    /// <summary>
    /// Gets the content type schema ID.
    /// </summary>
    public required string SchemaId { get; init; }

    /// <summary>
    /// Gets the schema IDs of the content type's compositions.
    /// </summary>
    public required IReadOnlyList<string> CompositionSchemaIds { get; init; }

    /// <summary>
    /// Gets the properties for this content type.
    /// </summary>
    public required IReadOnlyList<ContentTypePropertySchemaInfo> Properties { get; init; }

    /// <summary>
    /// Gets a value indicating whether the content type is an element type.
    /// </summary>
    public bool IsElement { get; init; }
}
