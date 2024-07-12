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

    public ContentCache(ICacheService cacheService, IIdKeyMap idKeyMap)
    {
        _cacheService = cacheService;
        _idKeyMap = idKeyMap;
    }

    public async Task<IPublishedContent?> GetById(int contentId, bool preview = false)
    {
        Attempt<Guid> keyAttempt = _idKeyMap.GetKeyForId(contentId, UmbracoObjectTypes.Document);
        if (keyAttempt.Success is false)
        {
            return null;
        }

        return await _cacheService.GetById(contentId, preview);
    }

    public async Task<IPublishedContent?> GetById(Guid contentId, bool preview = false) => await _cacheService.GetByKey(contentId, preview);

    public Task<bool> HasById(int contentId, bool preview = false) => throw new NotImplementedException();

    public Task<bool> HasContent(bool preview = false) => throw new NotImplementedException();
}
