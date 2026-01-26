namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
/// Model for patching a specific property value.
/// </summary>
public class PropertyPatchModel
{
    public required string Alias { get; set; }

    public string? Culture { get; set; }

    public string? Segment { get; set; }

    public object? Value { get; set; }

    /// <summary>
    /// Gets the composite key for this property value.
    /// </summary>
    /// <remarks>
    /// TODO: Consider refactoring to a base interface or abstract class with IKeyed pattern for consistency.
    /// </remarks>
    public (string alias, string? culture, string? segment) Key => (Alias, Culture, Segment);
}
