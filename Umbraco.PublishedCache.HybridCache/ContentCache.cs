using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Infrastructure.HybridCache;

internal sealed class ContentCache : IPublishedHybridCache
{
    private readonly Microsoft.Extensions.Caching.Hybrid.HybridCache _cache;
    private readonly IDocumentRepository _documentRepository;

    public ContentCache(Microsoft.Extensions.Caching.Hybrid.HybridCache cache, IDocumentRepository documentRepository)
    {
        _cache = cache;
        _documentRepository = documentRepository;
    }

    public IPublishedContent? GetById(bool preview, int contentId) => throw new NotImplementedException();

    public IPublishedContent? GetById(bool preview, Guid contentId) => throw new NotImplementedException();

    public IPublishedContent? GetById(int contentId) => throw new NotImplementedException();

    public IPublishedContent? GetById(Guid contentId)
    {
        return null;
        // var getContent = _cache.GetOrCreateAsync(
        //     $"{contentId}", // Unique key to the cache entry
        //     async cancel =>
        //     {
        //         IContent? content = _documentRepository.Get(contentId);
        //         return null;
        //     }).GetAwaiter().GetResult();
    }

    public bool HasById(bool preview, int contentId) => throw new NotImplementedException();

    public bool HasById(int contentId) => throw new NotImplementedException();

    public bool HasContent(bool preview) => throw new NotImplementedException();

    public bool HasContent() => throw new NotImplementedException();
}
