namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     Represents an entity that has culture and segment properties for content variation.
/// </summary>
public interface IHasCultureAndSegment
{
    /// <summary>
    ///     Gets the culture code for this variant, or <c>null</c> for invariant content.
    /// </summary>
    public string? Culture { get; }

    /// <summary>
    ///     Gets the segment identifier for this variant, or <c>null</c> for non-segmented content.
    /// </summary>
    public string? Segment { get; }
}
