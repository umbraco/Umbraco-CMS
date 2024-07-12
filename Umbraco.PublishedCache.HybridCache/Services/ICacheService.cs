using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Infrastructure.HybridCache.Services;

internal interface ICacheService
{
    Task<IPublishedContent?> GetByKey(Guid key, bool preview = false);

    Task<IPublishedContent?> GetById(int id, bool preview = false);

    Task<bool> HasContentById(int id, bool preview = false);

    Task RefreshContent(IContent content);

    Task DeleteItem(int id);
}
