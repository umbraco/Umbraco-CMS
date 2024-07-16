using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Infrastructure.HybridCache.Services;

internal interface IMemberCacheService
{
    Task<IPublishedMember?> GetByKey(Guid key);
}
