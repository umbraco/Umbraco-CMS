using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.Headless;

public class HeadlessPropertyMapper : IHeadlessPropertyMapper
{
    public IDictionary<string, object?> Map(IPublishedElement element) => element.Properties.ToDictionary(
        p => p.Alias,
        p => p.GetHeadlessValue()
    );
}
