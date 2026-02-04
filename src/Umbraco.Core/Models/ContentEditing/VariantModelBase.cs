using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     Represents the base model for content variants with culture and segment support.
/// </summary>
public abstract class VariantModelBase : IHasCultureAndSegment
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
    [Required]
    public string Name { get; set; } = string.Empty;
}
