using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.ContentApi;

public class PropertyMapper : IPropertyMapper
{
    public IDictionary<string, object?> Map(IPublishedElement element) => element.Properties.ToDictionary(
        p => p.Alias,
        p => p.GetContentApiValue()
    );
}
