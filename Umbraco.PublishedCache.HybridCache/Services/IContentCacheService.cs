using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Infrastructure.HybridCache.Services;

internal interface IContentCacheService
{
    Task<IPublishedContent?> GetByKeyAsync(Guid key, bool preview = false);

    Task<IPublishedContent?> GetByIdAsync(int id, bool preview = false);

    Task SeedAsync(IReadOnlyCollection<int>? contentTypeIds = null);

    Task<bool> HasContentByIdAsync(int id, bool preview = false);

    Task RefreshContentAsync(IContent content);

    Task DeleteItemAsync(int id);
}
