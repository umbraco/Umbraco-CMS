using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.ContentApi;

public class DefaultOutputExpansionStrategy : IOutputExpansionStrategy
{
    public IDictionary<string, object?> MapElementProperties(IPublishedElement element)
        => MapProperties(element.Properties);

    public IDictionary<string, object?> MapProperties(IEnumerable<IPublishedProperty> properties)
        => properties.ToDictionary(p => p.Alias, p => p.GetContentApiValue(true));

    public IDictionary<string, object?> MapContentProperties(IPublishedContent content)
        => MapProperties(content.Properties);
}
