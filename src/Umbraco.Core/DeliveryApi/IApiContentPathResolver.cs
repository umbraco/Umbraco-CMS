using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.DeliveryApi;

public interface IApiContentPathResolver
{
    bool IsResolveablePath(string path) => true;

    IPublishedContent? ResolveContentPath(string path);
}
