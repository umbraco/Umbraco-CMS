namespace Umbraco.Cms.Core.Models;

public interface IPropertyValue
{
    /// <summary>
    ///     Gets or sets the culture of the property.
    /// </summary>
    /// <remarks>
    ///     The culture is either null (invariant) or a non-empty string. If the property is
    ///     set with an empty or whitespace value, its value is converted to null.
    /// </remarks>
    string? Culture { get; set; }

    /// <summary>
    ///     Gets or sets the segment of the property.
    /// </summary>
    /// <remarks>
    ///     The segment is either null (neutral) or a non-empty string. If the property is
    ///     set with an empty or whitespace value, its value is converted to null.
    /// </remarks>
    string? Segment { get; set; }

    /// <summary>
    ///     Gets or sets the edited value of the property.
    /// </summary>
    object? EditedValue { get; set; }

    /// <summary>
    ///     Gets or sets the published value of the property.
    /// </summary>
    object? PublishedValue { get; set; }

    /// <summary>
    ///     Clones the property value.
    /// </summary>
    IPropertyValue Clone();
}
