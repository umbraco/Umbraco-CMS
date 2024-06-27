using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Infrastructure.HybridCache;

internal class ContentCache : IPublishedHybridCache
{
    public ContentCache(Microsoft.Extensions.Caching.Hybrid.HybridCache cache)
    {
    }

    public IPublishedContent? GetById(bool preview, int contentId) => throw new NotImplementedException();

    public IPublishedContent? GetById(bool preview, Guid contentId) => throw new NotImplementedException();

    public IPublishedContent? GetById(int contentId) => throw new NotImplementedException();

    public IPublishedContent? GetById(Guid contentId) => throw new NotImplementedException();

    public bool HasById(bool preview, int contentId) => throw new NotImplementedException();

    public bool HasById(int contentId) => throw new NotImplementedException();

    public bool HasContent(bool preview) => throw new NotImplementedException();

    public bool HasContent() => throw new NotImplementedException();
}
