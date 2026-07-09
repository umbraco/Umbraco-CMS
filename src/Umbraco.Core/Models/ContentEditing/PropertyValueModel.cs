namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     Represents a property value model for content editing operations.
/// </summary>
public class PropertyValueModel
{
    /// <summary>
    ///     Gets or sets the property type alias.
    /// </summary>
    public required string Alias { get; set; }

    /// <summary>
    ///     Gets or sets the property value.
    /// </summary>
    public required object? Value { get; set; }

    /// <summary>
    ///     Gets or sets the culture code for this property value, or <c>null</c> for invariant properties.
    /// </summary>
    public string? Culture { get; set; }

    /// <summary>
    ///     Gets or sets the segment identifier for this property value, or <c>null</c> for non-segmented properties.
    /// </summary>
    public string? Segment { get; set; }
}
