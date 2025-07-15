using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.PublishedCache;

public interface IDocumentCacheService
{
    Task<IPublishedContent?> GetByKeyAsync(Guid key, bool? preview = null);

    Task<IPublishedContent?> GetByIdAsync(int id, bool? preview = null);

    Task SeedAsync(CancellationToken cancellationToken);

    Task<bool> HasContentByIdAsync(int id, bool preview = false);

    Task RefreshContentAsync(IContent content);

    Task DeleteItemAsync(IContentBase content);

    void Rebuild(IReadOnlyCollection<int> contentTypeIds);

    IEnumerable<IPublishedContent> GetByContentType(IPublishedContentType contentType);

    Task ClearMemoryCacheAsync(CancellationToken cancellationToken);

    Task RefreshMemoryCacheAsync(Guid key);

    Task RemoveFromMemoryCacheAsync(Guid key);

    Task RebuildMemoryCacheByContentTypeAsync(IEnumerable<int> contentTypeIds);
}
