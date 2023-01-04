using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.ContentApi;

public class PropertyMapper : IPropertyMapper
{
    public IDictionary<string, object?> Map(IPublishedElement element) => Map(element.Properties);

    public IDictionary<string, object?> Map(IEnumerable<IPublishedProperty> properties) => properties.ToDictionary(
        p => p.Alias,
        p => p.GetContentApiValue());
}
