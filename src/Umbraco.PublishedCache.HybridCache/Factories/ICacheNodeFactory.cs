using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Infrastructure.HybridCache.Factories;

internal interface ICacheNodeFactory
{
    ContentCacheNode ToContentCacheNode(IContent content, bool preview);
    ContentCacheNode ToContentCacheNode(IMedia media);
}
