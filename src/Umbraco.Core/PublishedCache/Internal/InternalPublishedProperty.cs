using System.ComponentModel;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.PublishedCache.Internal;

// TODO: Only used in unit tests, needs to be moved to test project

/// <summary>
/// Provides an internal implementation of <see cref="IPublishedProperty"/> for testing purposes.
/// </summary>
/// <remarks>
/// This class is intended for unit testing only and should not be used in production code.
/// It allows setting property values directly via properties rather than through conversion.
/// It should be moved to a test project in a future refactoring.
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
public class InternalPublishedProperty : IPublishedProperty
{
    /// <summary>
    /// Gets or sets the solid source value to be returned by <see cref="GetSourceValue"/>.
    /// </summary>
    public object? SolidSourceValue { get; set; }

    /// <summary>
    /// Gets or sets the solid converted value to be returned by <see cref="GetValue"/>.
    /// </summary>
    public object? SolidValue { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the property has a value.
    /// </summary>
    public bool SolidHasValue { get; set; }

    /// <summary>
    /// Gets or sets the solid Delivery API value to be returned by <see cref="GetDeliveryApiValue"/>.
    /// </summary>
    public object? SolidDeliveryApiValue { get; set; }

    /// <inheritdoc />
    public IPublishedPropertyType PropertyType { get; set; } = null!;

    /// <inheritdoc />
    public string Alias { get; set; } = string.Empty;

    /// <inheritdoc />
    public virtual object? GetSourceValue(string? culture = null, string? segment = null) => SolidSourceValue;

    /// <inheritdoc />
    public virtual object? GetValue(string? culture = null, string? segment = null) => SolidValue;

    /// <inheritdoc />
    public virtual object? GetDeliveryApiValue(bool expanding, string? culture = null, string? segment = null) => SolidDeliveryApiValue;

    /// <inheritdoc />
    public virtual bool HasValue(string? culture = null, string? segment = null) => SolidHasValue;
}
