using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.HybridCache.Services;

namespace Umbraco.Cms.Infrastructure.HybridCache;

internal sealed class ContentCache : IPublishedHybridCache
{
    private readonly ICacheService _cacheService;
    private readonly IIdKeyMap _idKeyMap;
    private readonly IContentService _contentService;

    public ContentCache(ICacheService cacheService, IIdKeyMap idKeyMap, IContentService contentService)
    {
        _cacheService = cacheService;
        _idKeyMap = idKeyMap;
        _contentService = contentService;
    }

    public async Task<IPublishedContent?> GetById(int id, bool preview = false)
    {
        Attempt<Guid> keyAttempt = _idKeyMap.GetKeyForId(id, UmbracoObjectTypes.Document);
        if (keyAttempt.Success is false)
        {
            return null;
        }

        return await _cacheService.GetById(id, preview);
    }

    public async Task<IPublishedContent?> GetById(Guid key, bool preview = false) => await _cacheService.GetByKey(key, preview);

    public async Task<bool> HasById(int id, bool preview = false) => await _cacheService.HasContentById(id, preview);
}
