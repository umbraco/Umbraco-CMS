using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Infrastructure.HybridCache.Services;

public interface IDocumentCacheService
{
    Task<IPublishedContent?> GetByKeyAsync(Guid key, bool preview = false);

    Task<IPublishedContent?> GetByIdAsync(int id, bool preview = false);

    Task SeedAsync(CancellationToken cancellationToken);

    Task<bool> HasContentByIdAsync(int id, bool preview = false);

    Task RefreshContentAsync(IContent content);

    Task DeleteItemAsync(IContentBase content);

    void Rebuild(IReadOnlyCollection<int> contentTypeKeys);
}
