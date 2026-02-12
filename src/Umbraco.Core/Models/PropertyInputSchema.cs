namespace Umbraco.Cms.Core.Models;

/// <summary>
/// Represents property information needed for inputting content values.
/// </summary>
public class PropertyInputSchema
{
    /// <summary>
    /// Gets the property alias.
    /// </summary>
    public required string Alias { get; init; }

    /// <summary>
    /// Gets the unique key of the data type used by this property.
    /// </summary>
    public required Guid DataTypeKey { get; init; }

    /// <summary>
    /// Gets the property editor alias.
    /// </summary>
    public required string EditorAlias { get; init; }

    /// <summary>
    /// Gets a value indicating whether a value is required for this property.
    /// </summary>
    public bool Mandatory { get; init; }

    /// <summary>
    /// Gets the content variation setting for this property.
    /// </summary>
    public ContentVariation Variations { get; init; }
}
