using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     Default implementation of <see cref="IApiPropertyRenderer"/> that renders property values for the Delivery API.
/// </summary>
public class ApiPropertyRenderer : IApiPropertyRenderer
{
    private readonly IPublishedValueFallback _publishedValueFallback;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ApiPropertyRenderer"/> class.
    /// </summary>
    /// <param name="publishedValueFallback">The published value fallback service.</param>
    public ApiPropertyRenderer(IPublishedValueFallback publishedValueFallback)
        => _publishedValueFallback = publishedValueFallback;

    /// <inheritdoc />
    public object? GetPropertyValue(IPublishedProperty property, bool expanding)
    {
        if (property.HasValue())
        {
            return property.GetDeliveryApiValue(expanding);
        }

        return _publishedValueFallback.TryGetValue(property, null, null, Fallback.To(Fallback.None), null, out var fallbackValue)
            ? fallbackValue
            : null;
    }
}
