using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.DeliveryApi;

public interface IApiContentPathProvider
{
    string? GetContentPath(IPublishedContent content, string? culture);
}
