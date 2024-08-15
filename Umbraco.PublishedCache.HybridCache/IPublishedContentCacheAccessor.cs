using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Infrastructure.HybridCache;

public interface IPublishedContentCacheAccessor
{
    PublishedContentTypeCache Get();
}
