using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.HybridCache.Factories;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.HybridCache.Services;

// TODO ELEMENTS: implement IPublishedElement cache with an actual cache behind (see DocumentCacheService)
internal class ElementCacheService : IElementCacheService
{
    private readonly IElementService _elementService;
    private readonly ICacheNodeFactory _cacheNodeFactory;
    private readonly IPublishedContentFactory _publishedContentFactory;
    private readonly IPublishedModelFactory _publishedModelFactory;

    public ElementCacheService(
        IElementService elementService,
        ICacheNodeFactory cacheNodeFactory,
        IPublishedContentFactory publishedContentFactory,
        IPublishedModelFactory publishedModelFactory)
    {
        _elementService = elementService;
        _cacheNodeFactory = cacheNodeFactory;
        _publishedContentFactory = publishedContentFactory;
        _publishedModelFactory = publishedModelFactory;
    }

    public Task<IPublishedElement?> GetByKeyAsync(Guid key, bool? preview = null)
    {
        IPublishedElement? result = null;
        IElement? element = _elementService.GetById(key);
        if (element is null || element.Trashed is true)
        {
            return Task.FromResult(result);
        }

        if (preview is not true && element.Published is false)
        {
            return Task.FromResult(result);
        }

        preview ??= false;
        var cacheNode = _cacheNodeFactory.ToContentCacheNode(element, preview.Value);
        result = _publishedContentFactory.ToIPublishedElement(cacheNode, preview.Value);
        return Task.FromResult(result.CreateModel(_publishedModelFactory));
    }

    // TODO ELEMENTS: implement memory cache
    public Task ClearMemoryCacheAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    // TODO ELEMENTS: implement memory cache
    public Task RefreshMemoryCacheAsync(Guid key) => Task.CompletedTask;

    // TODO ELEMENTS: implement memory cache
    public Task RemoveFromMemoryCacheAsync(Guid key) => Task.CompletedTask;
}
