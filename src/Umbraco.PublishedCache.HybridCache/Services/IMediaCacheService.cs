using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Infrastructure.HybridCache.Services;

public interface IMediaCacheService
{
    Task<IPublishedContent?> GetByKeyAsync(Guid key);

    Task<IPublishedContent?> GetByIdAsync(int id);

    Task<bool> HasContentByIdAsync(int id);

    Task RefreshMediaAsync(IMedia media);

    Task DeleteItemAsync(int id);
}
