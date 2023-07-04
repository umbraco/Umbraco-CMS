using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.DeliveryApi;

public interface IApiPropertyRenderer
{
    object? GetPropertyValue(IPublishedProperty property, bool expanding);
}
