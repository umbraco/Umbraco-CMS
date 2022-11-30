using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.Headless;

public interface IHeadlessPropertyMapper
{
    IDictionary<string, object?> Map(IPublishedElement element);
}
