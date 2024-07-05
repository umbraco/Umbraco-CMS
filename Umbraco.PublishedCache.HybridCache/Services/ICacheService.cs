using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Infrastructure.HybridCache.Services;

internal interface ICacheService
{
    Task<ContentCacheNode?> GetByKey(Guid key, bool preview = false);
    Task<ContentCacheNode?> GetById(int id, bool preview = false);

    Task RefreshContent(IContent content);
}
