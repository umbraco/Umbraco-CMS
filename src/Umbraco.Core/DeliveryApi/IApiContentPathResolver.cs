using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.DeliveryApi;

public interface IApiContentPathResolver
{
    [Obsolete("No longer used in V15. Scheduled for removal in V15.")]
    bool IsResolvablePath(string path) => true;

    IPublishedContent? ResolveContentPath(string path);
}
