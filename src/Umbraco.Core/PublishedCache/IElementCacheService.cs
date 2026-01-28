using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.PublishedCache;

// TODO ELEMENTS: refactor IDocumentCacheService into a common base interface and use it both here and for IDocumentCacheService
public interface IElementCacheService
{
    Task<IPublishedElement?> GetByKeyAsync(Guid key, bool? preview = null);

    Task ClearMemoryCacheAsync(CancellationToken cancellationToken);

    Task RefreshMemoryCacheAsync(Guid key);

    Task RemoveFromMemoryCacheAsync(Guid key);
}
