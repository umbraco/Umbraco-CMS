namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     Represents a variant model for content editing operations.
/// </summary>
public class VariantModel
{
    /// <summary>
    ///     Gets or sets the culture code for this variant, or <c>null</c> for invariant content.
    /// </summary>
    public string? Culture { get; set; }

    /// <summary>
    ///     Gets or sets the segment identifier for this variant, or <c>null</c> for non-segmented content.
    /// </summary>
    public string? Segment { get; set; }

    /// <summary>
    ///     Gets or sets the name of the content for this variant.
    /// </summary>
    public required string Name { get; set; }
}
