using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.DeliveryApi;

public interface IOutputExpansionStrategy
{
    IDictionary<string, object?> MapElementProperties(IPublishedElement element);

    IDictionary<string, object?> MapProperties(IEnumerable<IPublishedProperty> properties);

    IDictionary<string, object?> MapContentProperties(IPublishedContent content);
}
