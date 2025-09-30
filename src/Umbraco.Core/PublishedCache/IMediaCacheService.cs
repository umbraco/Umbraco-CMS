using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.PublishedCache;

public interface IMediaCacheService
{
    Task<IPublishedContent?> GetByKeyAsync(Guid key);

    Task<IPublishedContent?> GetByIdAsync(int id);

    Task<bool> HasContentByIdAsync(int id);

    Task RefreshMediaAsync(IMedia media);

    Task RebuildMemoryCacheByContentTypeAsync(IEnumerable<int> mediaTypeIds);

    Task ClearMemoryCacheAsync(CancellationToken cancellationToken);

    Task RefreshMemoryCacheAsync(Guid key);

    Task RemoveFromMemoryCacheAsync(Guid key);

    Task DeleteItemAsync(IContentBase media);

    Task SeedAsync(CancellationToken cancellationToken);

    void Rebuild(IReadOnlyCollection<int> contentTypeIds);
    IEnumerable<IPublishedContent> GetByContentType(IPublishedContentType contentType);
}
