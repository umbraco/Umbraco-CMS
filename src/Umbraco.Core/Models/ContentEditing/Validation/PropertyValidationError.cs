namespace Umbraco.Cms.Core.Models.ContentEditing.Validation;

/// <summary>
///     Represents a validation error for a content property.
/// </summary>
public class PropertyValidationError
{
    /// <summary>
    ///     Gets the JSON path to the property value that failed validation.
    /// </summary>
    public required string JsonPath { get; init; }

    /// <summary>
    ///     Gets the collection of error messages describing the validation failures.
    /// </summary>
    public required string[] ErrorMessages { get; init; }

    /// <summary>
    ///     Gets or sets the property type alias of the property that failed validation.
    /// </summary>
    public required string Alias { get; set; }

    /// <summary>
    ///     Gets or sets the culture code for the property value that failed validation, or <c>null</c> for invariant properties.
    /// </summary>
    public required string? Culture { get; set; }

    /// <summary>
    ///     Gets or sets the segment identifier for the property value that failed validation, or <c>null</c> for non-segmented properties.
    /// </summary>
    public required string? Segment { get; set; }
}
