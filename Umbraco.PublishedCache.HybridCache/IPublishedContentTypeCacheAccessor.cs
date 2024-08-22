using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Infrastructure.HybridCache;

public interface IPublishedContentTypeCacheAccessor
{
    PublishedContentTypeCache Get();
}
