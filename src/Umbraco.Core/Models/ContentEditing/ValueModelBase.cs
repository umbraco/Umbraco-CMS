using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     Represents the base model for property values with culture and segment support.
/// </summary>
public abstract class ValueModelBase : IHasCultureAndSegment
{
    /// <summary>
    ///     Gets or sets the culture code for this value, or <c>null</c> for invariant properties.
    /// </summary>
    public string? Culture { get; set; }

    /// <summary>
    ///     Gets or sets the segment identifier for this value, or <c>null</c> for non-segmented properties.
    /// </summary>
    public string? Segment { get; set; }

    /// <summary>
    ///     Gets or sets the property type alias.
    /// </summary>
    [Required]
    public string Alias { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the property value.
    /// </summary>
    public object? Value { get; set; }
}
