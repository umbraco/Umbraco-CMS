using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Cache.PropertyEditors;

internal sealed class BlockEditorElementTypeCache : IBlockEditorElementTypeCache
{
    private const string CacheKey = $"{nameof(BlockEditorElementTypeCache)}_ElementTypes";
    private readonly IContentTypeService _contentTypeService;
    private readonly AppCaches _appCaches;


    public BlockEditorElementTypeCache(IContentTypeService contentTypeService, AppCaches appCaches)
    {
        _contentTypeService = contentTypeService;
        _appCaches = appCaches;
    }

    public IEnumerable<IContentType> GetMany(IEnumerable<Guid> keys) => GetAll().Where(elementType => keys.Contains(elementType.Key));

    public IEnumerable<IContentType> GetAll()
    {
        // TODO: make this less dumb; don't fetch all elements, only fetch the items that aren't yet in the cache and amend the cache as more elements are loaded
        IEnumerable<IContentType>? cachedElements = _appCaches.RequestCache.GetCacheItem<IEnumerable<IContentType>>(CacheKey);
        if (cachedElements is null)
        {
            cachedElements = _contentTypeService.GetAllElementTypes();
            _appCaches.RequestCache.Set(CacheKey, cachedElements);
        }

        return cachedElements;
    }

    public void ClearAll() => _appCaches.RequestCache.Remove(CacheKey);
}
