namespace Umbraco.Cms.Core.Models;

/// <summary>
/// Represents content type information needed for inputting content values.
/// </summary>
public class ContentTypeInputSchema
{
    /// <summary>
    /// Gets the unique key of the content type.
    /// </summary>
    public required Guid Key { get; init; }

    /// <summary>
    /// Gets the content type alias.
    /// </summary>
    public required string Alias { get; init; }

    /// <summary>
    /// Gets all properties for this content type (including inherited from compositions).
    /// </summary>
    public required IReadOnlyList<PropertyInputSchema> Properties { get; init; }

    /// <summary>
    /// Gets a value indicating whether the content type is an element type.
    /// </summary>
    public bool IsElement { get; init; }

    /// <summary>
    /// Gets the content variation setting for this content type.
    /// </summary>
    public ContentVariation Variations { get; init; }
}
