using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Infrastructure.HybridCache;

/// <summary>
/// Provides access to cached published elements, delegating to <see cref="IElementCacheService"/>.
/// </summary>
public sealed class ElementCache : IPublishedElementCache
{
    private readonly IElementCacheService _elementCacheService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementCache"/> class.
    /// </summary>
    /// <param name="elementCacheService">The element cache service to delegate to.</param>
    public ElementCache(IElementCacheService elementCacheService)
    {
        _elementCacheService = elementCacheService;
    }

    /// <inheritdoc />
    public async Task<IPublishedElement?> GetByIdAsync(Guid key, bool? preview = null)
        => await _elementCacheService.GetByKeyAsync(key, preview);

    /// <inheritdoc />
    public IPublishedElement? GetById(bool preview, Guid key)
    {
        // Sync fast path: when the converted-element L0 cache already holds the item we can
        // return it without spinning up an async state machine. This is the dominant case for
        // property value converters (e.g. ElementPicker) that run sync-over-async on the
        // render hot path. On a miss we fall through to the async path which handles
        // HybridCache (L1/L2) and database lookups.
        if (_elementCacheService.TryGetCached(key, preview, out IPublishedElement? cached))
        {
            return cached;
        }

        return GetByIdAsync(key, preview).GetAwaiter().GetResult();
    }
}
