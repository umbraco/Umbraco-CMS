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
    public async Task<IPublishedElement?> GetByKeyAsync(Guid key, bool? preview = null)
        => await _elementCacheService.GetByKeyAsync(key, preview);
}
