using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Core.DeliveryApi;

public class ApiPropertyRenderer : IApiPropertyRenderer
{
    private readonly IPublishedValueFallback _publishedValueFallback;

    public ApiPropertyRenderer(IPublishedValueFallback publishedValueFallback)
        => _publishedValueFallback = publishedValueFallback;

    public object? GetPropertyValue(IPublishedProperty property, bool expanding, string? culture = null)
    {
        var propertyValue = property.GetDeliveryApiValue(expanding, culture);
        if (property.PropertyType.IsDeliveryApiValue(propertyValue, PropertyValueLevel.Object) is not false)
        {
            return propertyValue;
        }

        return _publishedValueFallback.TryGetValue(property, culture, null, Fallback.To(Fallback.None), null, out var fallbackValue)
            ? fallbackValue
            : null;
    }
}
