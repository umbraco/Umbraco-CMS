namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
/// Model for patching a specific variant.
/// </summary>
public class VariantPatchModel
{
    public string? Culture { get; set; }

    public string? Segment { get; set; }

    public required string Name { get; set; }

    /// <summary>
    /// Gets the composite key for this variant.
    /// </summary>
    /// <remarks>
    /// TODO: Consider refactoring to a base interface or abstract class with IKeyed pattern for consistency.
    /// </remarks>
    public (string? culture, string? segment) Key => (Culture, Segment);
}
