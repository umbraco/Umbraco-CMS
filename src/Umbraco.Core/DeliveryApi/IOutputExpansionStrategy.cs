using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.DeliveryApi;

public interface IOutputExpansionStrategy
{
    IDictionary<string, object?> MapElementProperties(IPublishedElement element);

    IDictionary<string, object?> MapContentProperties(IPublishedContent content);

    IDictionary<string, object?> MapMediaProperties(IPublishedContent media, bool skipUmbracoProperties = true);
}
