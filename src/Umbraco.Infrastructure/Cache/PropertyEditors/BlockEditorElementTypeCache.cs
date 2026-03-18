using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Cache.PropertyEditors;

internal sealed class BlockEditorElementTypeCache : IBlockEditorElementTypeCache
{
    private const string CacheKey = $"{nameof(BlockEditorElementTypeCache)}_ElementTypes";
    private readonly IContentTypeService _contentTypeService;
    private readonly AppCaches _appCaches;


    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Core.Cache.PropertyEditors.BlockEditorElementTypeCache"/> class.
    /// </summary>
    /// <param name="contentTypeService">The service used to manage and retrieve content types.</param>
    /// <param name="appCaches">The application-level cache manager used for caching element type data.</param>
    public BlockEditorElementTypeCache(IContentTypeService contentTypeService, AppCaches appCaches)
    {
        _contentTypeService = contentTypeService;
        _appCaches = appCaches;
    }

    /// <summary>
    /// Retrieves all content types whose unique identifiers are contained in the specified collection of keys.
    /// </summary>
    /// <param name="keys">A collection of <see cref="System.Guid"/> values representing the unique identifiers of the content types to retrieve.</param>
    /// <returns>An <see cref="IEnumerable{IContentType}"/> containing the content types that match the provided keys.</returns>
    public IEnumerable<IContentType> GetMany(IEnumerable<Guid> keys) => GetAll().Where(elementType => keys.Contains(elementType.Key));

    /// <summary>
    /// Retrieves all content types that are configured as element types for use in the block editor.
    /// </summary>
    /// <returns>An <see cref="IEnumerable{IContentType}"/> containing all element content types available to the block editor.</returns>
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

    /// <summary>
    /// Clears all cached block editor element types from the request cache.
    /// </summary>
    public void ClearAll() => _appCaches.RequestCache.Remove(CacheKey);
}
