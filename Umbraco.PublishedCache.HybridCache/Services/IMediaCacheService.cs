using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Infrastructure.HybridCache.Services;

public interface IMediaCacheService
{
    Task<IPublishedContent?> GetByKeyAsync(Guid key, bool preview = false);

    Task<IPublishedContent?> GetByIdAsync(int id, bool preview = false);

    Task<bool> HasContentByIdAsync(int id, bool preview = false);

    Task RefreshMediaAsync(IMedia media);

    Task DeleteItemAsync(int id);
}
