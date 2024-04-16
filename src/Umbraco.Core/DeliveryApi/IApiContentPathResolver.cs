using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.DeliveryApi;

public interface IApiContentPathResolver
{
    IPublishedContent? ResolveContentPath(string path);
}
