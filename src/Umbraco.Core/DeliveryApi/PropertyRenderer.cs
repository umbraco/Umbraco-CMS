using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.DeliveryApi;

public class ApiPropertyRenderer : IApiPropertyRenderer
{
    private readonly IPublishedValueFallback _publishedValueFallback;

    public ApiPropertyRenderer(IPublishedValueFallback publishedValueFallback)
        => _publishedValueFallback = publishedValueFallback;

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
