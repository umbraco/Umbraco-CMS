using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Infrastructure.HybridCache.Services;

internal interface ICacheService
{
    Task<IPublishedContent> GetByKey(Guid key);
    Task<IPublishedContent> GetById(int id);
}
