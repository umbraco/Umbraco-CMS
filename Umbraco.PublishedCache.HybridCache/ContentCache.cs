using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.HybridCache.Services;

namespace Umbraco.Cms.Infrastructure.HybridCache;

internal sealed class ContentCache : IPublishedContentHybridCache
{
    private readonly IContentCacheService _contentCacheService;
    private readonly IIdKeyMap _idKeyMap;

    public ContentCache(IContentCacheService contentCacheService, IIdKeyMap idKeyMap)
    {
        _contentCacheService = contentCacheService;
        _idKeyMap = idKeyMap;
    }

    public async Task<IPublishedContent?> GetById(int id, bool preview = false)
    {
        Attempt<Guid> keyAttempt = _idKeyMap.GetKeyForId(id, UmbracoObjectTypes.Document);
        if (keyAttempt.Success is false)
        {
            return null;
        }

        return await _contentCacheService.GetByIdAsync(id, preview);
    }

    public async Task<IPublishedContent?> GetById(Guid key, bool preview = false) => await _contentCacheService.GetByKeyAsync(key, preview);

    public async Task<bool> HasById(int id, bool preview = false) => await _contentCacheService.HasContentByIdAsync(id, preview);
}
