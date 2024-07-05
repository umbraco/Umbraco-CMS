using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.HybridCache.Persistence;

namespace Umbraco.Cms.Infrastructure.HybridCache.Services;

internal sealed class CacheService : ICacheService
{
    private readonly INuCacheContentRepository _nuCacheContentRepository;
    private readonly IIdKeyMap _idKeyMap;
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;
    private readonly IVariationContextAccessor _variationContextAccessor;
    private readonly IPublishedModelFactory _publishedModelFactory;
    private readonly PublishedContentTypeCache _contentTypeCache;

    public CacheService(
        INuCacheContentRepository nuCacheContentRepository,
        IIdKeyMap idKeyMap,
        ICoreScopeProvider scopeProvider,
        IPublishedSnapshotAccessor publishedSnapshotAccessor,
        IVariationContextAccessor variationContextAccessor,
        IPublishedModelFactory publishedModelFactory,
        IContentTypeService contentTypeService,
        IPublishedContentTypeFactory publishedContentTypeFactory,
        ILoggerFactory loggerFactory)
    {
        _nuCacheContentRepository = nuCacheContentRepository;
        _idKeyMap = idKeyMap;
        _scopeProvider = scopeProvider;
        _publishedSnapshotAccessor = publishedSnapshotAccessor;
        _variationContextAccessor = variationContextAccessor;
        _publishedModelFactory = publishedModelFactory;
        _contentTypeCache = new PublishedContentTypeCache(
            contentTypeService,
            null,
            null,
            publishedContentTypeFactory,
            loggerFactory.CreateLogger<PublishedContentTypeCache>());
    }

    public async Task<IPublishedContent?> GetByKey(Guid key, bool preview = false)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        Attempt<int> idAttempt = _idKeyMap.GetIdForKey(key, UmbracoObjectTypes.Document);
        if (idAttempt.Success is false)
        {
            return null;
        }

        ContentNodeKit contentNodeKit = _nuCacheContentRepository.GetContentSource(idAttempt.Result);
        IPublishedContentType contentType = _contentTypeCache.Get(PublishedItemType.Content, key);

        contentNodeKit.Build(contentType, _publishedSnapshotAccessor, _variationContextAccessor, _publishedModelFactory);
        scope.Complete();
        return preview ? contentNodeKit.Node.DraftModel : contentNodeKit.Node.PublishedModel;
    }

    public async Task<IPublishedContent?> GetById(int id, bool preview = false)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        ContentNodeKit contentNodeKit = _nuCacheContentRepository.GetContentSource(id);
        IPublishedContentType contentType = _contentTypeCache.Get(PublishedItemType.Content, contentNodeKit.ContentTypeId);
        contentNodeKit.Build(contentType, _publishedSnapshotAccessor, _variationContextAccessor, _publishedModelFactory);
        scope.Complete();
        return await Task.FromResult(preview ? contentNodeKit.Node.DraftModel! : contentNodeKit.Node.PublishedModel!);
    }

    public Task RefreshContent(IContent content)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        _nuCacheContentRepository.RefreshContent(content);
        scope.Complete();
        return Task.CompletedTask;
    }
}
