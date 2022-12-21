using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.ContentApi;

public interface IPropertyMapper
{
    IDictionary<string, object?> Map(IPublishedElement element);
}
