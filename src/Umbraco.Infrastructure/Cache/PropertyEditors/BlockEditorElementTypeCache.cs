using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Cache.PropertyEditors;

internal sealed class BlockEditorElementTypeCache : IBlockEditorElementTypeCache
{
    private readonly IContentTypeService _contentTypeService;
    private readonly AppCaches _appCaches;

    public BlockEditorElementTypeCache(IContentTypeService contentTypeService, AppCaches appCaches)
    {
        _contentTypeService = contentTypeService;
        _appCaches = appCaches;
    }

    public IEnumerable<IContentType> GetAll(IEnumerable<Guid> keys)
    {
        // TODO: make this less dumb; don't fetch all elements, only fetch the items that aren't yet in the cache and amend the cache as more elements are loaded

        const string cacheKey = $"{nameof(BlockEditorElementTypeCache)}_ElementTypes";
        IEnumerable<IContentType>? cachedElements = _appCaches.RequestCache.GetCacheItem<IEnumerable<IContentType>>(cacheKey);
        if (cachedElements is null)
        {
            cachedElements = _contentTypeService.GetAllElementTypes();
            _appCaches.RequestCache.Set(cacheKey, cachedElements);
        }

        return cachedElements.Where(elementType => keys.Contains(elementType.Key));
    }
}
