using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Core.Models.PublishedContent;

/// <inheritdoc />
/// <summary>
///     Represents a published property that has a unique invariant-neutral value
///     and caches conversion results locally.
/// </summary>
/// <remarks>
///     <para>
///         Conversions results are stored within the property and will not
///         be refreshed, so this class is not suitable for cached properties.
///     </para>
///     <para>
///         Does not support variations: the ctor throws if the property type
///         supports variations.
///     </para>
/// </remarks>
public class RawValueProperty : PublishedPropertyBase
{
    private readonly Lazy<object?> _objectValue;
    private readonly object _sourceValue; // the value in the db
    private readonly Lazy<object?> _deliveryApiValue;

    public RawValueProperty(IPublishedPropertyType propertyType, IPublishedElement content, object sourceValue, bool isPreviewing = false)
        : base(propertyType, PropertyCacheLevel.Unknown) // cache level is ignored
    {
        if (propertyType.Variations != ContentVariation.Nothing)
        {
            throw new ArgumentException("Property types with variations are not supported here.", nameof(propertyType));
        }

        _sourceValue = sourceValue;

        var interValue =
            new Lazy<object?>(() => PropertyType.ConvertSourceToInter(content, _sourceValue, isPreviewing));
        _objectValue = new Lazy<object?>(() =>
            PropertyType.ConvertInterToObject(content, PropertyCacheLevel.Unknown, interValue?.Value, isPreviewing));
        _deliveryApiValue = new Lazy<object?>(() =>
            PropertyType.ConvertInterToDeliveryApiObject(content, PropertyCacheLevel.Unknown, interValue?.Value, isPreviewing, false));
    }

    // RawValueProperty does not (yet?) support variants,
    // only manages the current "default" value
    public override object? GetSourceValue(string? culture = null, string? segment = null)
        => string.IsNullOrEmpty(culture) & string.IsNullOrEmpty(segment) ? _sourceValue : null;

    public override bool HasValue(string? culture = null, string? segment = null)
    {
        var sourceValue = GetSourceValue(culture, segment);
        return sourceValue is string s ? !string.IsNullOrWhiteSpace(s) : sourceValue != null;
    }

    public override object? GetValue(string? culture = null, string? segment = null)
        => string.IsNullOrEmpty(culture) & string.IsNullOrEmpty(segment) ? _objectValue.Value : null;

    public override object? GetDeliveryApiValue(bool expanding, string? culture = null, string? segment = null)
        => string.IsNullOrEmpty(culture) & string.IsNullOrEmpty(segment) ? _deliveryApiValue.Value : null;
}
