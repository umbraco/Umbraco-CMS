using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Infrastructure.HybridCache.Factories;
using Umbraco.Cms.Infrastructure.HybridCache.Persistence;
using Umbraco.Cms.Infrastructure.HybridCache.Serialization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.HybridCache.Services;

internal sealed class DocumentCacheService : IDocumentCacheService
{
    private readonly IDatabaseCacheRepository _databaseCacheRepository;
    private readonly IIdKeyMap _idKeyMap;
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly Microsoft.Extensions.Caching.Hybrid.HybridCache _hybridCache;
    private readonly IPublishedContentFactory _publishedContentFactory;
    private readonly ICacheNodeFactory _cacheNodeFactory;
    private readonly IEnumerable<IDocumentSeedKeyProvider> _seedKeyProviders;
    private readonly IPublishedModelFactory _publishedModelFactory;
    private readonly IPreviewService _previewService;
    private readonly IPublishStatusQueryService _publishStatusQueryService;
    private readonly IDocumentNavigationQueryService _documentNavigationQueryService;
    private readonly CacheSettings _cacheSettings;
    private HashSet<Guid>? _seedKeys;

    private HashSet<Guid> SeedKeys
    {
        get
        {
            if (_seedKeys is not null)
            {
                return _seedKeys;
            }

            _seedKeys = [];

            foreach (IDocumentSeedKeyProvider provider in _seedKeyProviders)
            {
                _seedKeys.UnionWith(provider.GetSeedKeys());
            }

            return _seedKeys;
        }
    }

    public DocumentCacheService(
        IDatabaseCacheRepository databaseCacheRepository,
        IIdKeyMap idKeyMap,
        ICoreScopeProvider scopeProvider,
        Microsoft.Extensions.Caching.Hybrid.HybridCache hybridCache,
        IPublishedContentFactory publishedContentFactory,
        ICacheNodeFactory cacheNodeFactory,
        IEnumerable<IDocumentSeedKeyProvider> seedKeyProviders,
        IOptions<CacheSettings> cacheSettings,
        IPublishedModelFactory publishedModelFactory,
        IPreviewService previewService,
        IPublishStatusQueryService publishStatusQueryService,
        IDocumentNavigationQueryService documentNavigationQueryService)
    {
        _databaseCacheRepository = databaseCacheRepository;
        _idKeyMap = idKeyMap;
        _scopeProvider = scopeProvider;
        _hybridCache = hybridCache;
        _publishedContentFactory = publishedContentFactory;
        _cacheNodeFactory = cacheNodeFactory;
        _seedKeyProviders = seedKeyProviders;
        _publishedModelFactory = publishedModelFactory;
        _previewService = previewService;
        _publishStatusQueryService = publishStatusQueryService;
        _documentNavigationQueryService = documentNavigationQueryService;
        _cacheSettings = cacheSettings.Value;
    }

    public async Task<IPublishedContent?> GetByKeyAsync(Guid key, bool? preview = null)
    {
        bool calculatedPreview = preview ?? GetPreview();

        return await GetNodeAsync(key, calculatedPreview);
    }

    public async Task<IPublishedContent?> GetByIdAsync(int id, bool? preview = null)
    {
        Attempt<Guid> keyAttempt = _idKeyMap.GetKeyForId(id, UmbracoObjectTypes.Document);
        if (keyAttempt.Success is false)
        {
            return null;
        }

        bool calculatedPreview = preview ?? GetPreview();
        Guid key = keyAttempt.Result;

        return await GetNodeAsync(key, calculatedPreview);
    }

    private async Task<IPublishedContent?> GetNodeAsync(Guid key, bool preview)
    {
        var cacheKey = GetCacheKey(key, preview);

        ContentCacheNode? contentCacheNode = await _hybridCache.GetOrCreateAsync(
            cacheKey,
            async cancel =>
            {
                using ICoreScope scope = _scopeProvider.CreateCoreScope();
                ContentCacheNode? contentCacheNode = await _databaseCacheRepository.GetContentSourceAsync(key, preview);

                // If we can resolve the content cache node, we still need to check if the ancestor path is published.
                // This does cost some performance, but it's necessary to ensure that the content is actually published.
                // When unpublishing a node, a payload with RefreshBranch is published, so we don't have to worry about this.
                // Similarly, when a branch is published, next time the content is requested, the parent will be published,
                // this works because we don't cache null values.
                if (preview is false && contentCacheNode is not null && HasPublishedAncestorPath(contentCacheNode.Key) is false)
                {
                    // Careful not to early return here. We need to complete the scope even if returning null.
                    contentCacheNode = null;
                }

                scope.Complete();
                return contentCacheNode;
            },
            GetEntryOptions(key, preview),
            GenerateTags(key));

        // We don't want to cache removed items, this may cause issues if the L2 serializer changes.
        if (contentCacheNode is null)
        {
            await _hybridCache.RemoveAsync(cacheKey);
            return null;
        }

        return _publishedContentFactory.ToIPublishedContent(contentCacheNode, preview).CreateModel(_publishedModelFactory);
    }

    private bool HasPublishedAncestorPath(Guid contentKey)
    {
        var success = _documentNavigationQueryService.TryGetAncestorsKeys(contentKey, out IEnumerable<Guid> keys);
        if (success is false)
        {
            // This might happen is certain cases, since 0notifications are not ordered, for instance, if you save and publish a content node in the same scope.
            // In this case we'll try and update the node in the cache even though it hasn't been updated in the document navigation cache yet.
            // It's okay to just return false here, since the node will be loaded later when it's actually requested.
            return false;
        }

        foreach (Guid key in keys)
        {
            if (_publishStatusQueryService.IsDocumentPublishedInAnyCulture(key) is false)
            {
                return false;
            }
        }

        return true;
    }

    private bool GetPreview() => _previewService.IsInPreview();

    public IEnumerable<IPublishedContent> GetByContentType(IPublishedContentType contentType)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        IEnumerable<ContentCacheNode> nodes = _databaseCacheRepository.GetContentByContentTypeKey([contentType.Key], ContentCacheDataSerializerEntityType.Document);
        scope.Complete();

        return nodes
            .Select(x => _publishedContentFactory.ToIPublishedContent(x, x.IsDraft).CreateModel(_publishedModelFactory))
            .WhereNotNull();
    }

    public async Task ClearMemoryCacheAsync(CancellationToken cancellationToken)
    {
        await _hybridCache.RemoveByTagAsync(Constants.Cache.Tags.Content, cancellationToken);

        // We have to run seeding again after the cache is cleared
        await SeedAsync(cancellationToken);
    }

    public async Task RefreshMemoryCacheAsync(Guid key)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();

        ContentCacheNode? draftNode = await _databaseCacheRepository.GetContentSourceAsync(key, true);
        if (draftNode is not null)
        {
            await _hybridCache.SetAsync(GetCacheKey(draftNode.Key, true), draftNode, GetEntryOptions(draftNode.Key, true), GenerateTags(key));
        }

        ContentCacheNode? publishedNode = await _databaseCacheRepository.GetContentSourceAsync(key, false);
        if (publishedNode is not null && HasPublishedAncestorPath(publishedNode.Key))
        {
            await _hybridCache.SetAsync(GetCacheKey(publishedNode.Key, false), publishedNode, GetEntryOptions(publishedNode.Key, false), GenerateTags(key));
        }

        scope.Complete();
    }

    public async Task RemoveFromMemoryCacheAsync(Guid key)
    {
        await _hybridCache.RemoveAsync(GetCacheKey(key, true));
        await _hybridCache.RemoveAsync(GetCacheKey(key, false));
    }

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        foreach (Guid key in SeedKeys)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            var cacheKey = GetCacheKey(key, false);

            // We'll use GetOrCreateAsync because it may be in the second level cache, in which case we don't have to re-seed.
            ContentCacheNode? cachedValue = await _hybridCache.GetOrCreateAsync(
                cacheKey,
                async cancel =>
                {
                    using ICoreScope scope = _scopeProvider.CreateCoreScope();

                    ContentCacheNode? cacheNode = await _databaseCacheRepository.GetContentSourceAsync(key);

                    scope.Complete();

                    // We don't want to seed drafts
                    if (cacheNode is null || cacheNode.IsDraft)
                    {
                        return null;
                    }

                    return cacheNode;
                },
                GetSeedEntryOptions(),
                GenerateTags(key),
                cancellationToken: cancellationToken);

            // If the value is null, it's likely because
            if (cachedValue is null)
            {
                await _hybridCache.RemoveAsync(cacheKey, cancellationToken);
            }
        }
    }

    // Internal for test purposes.
    internal void ResetSeedKeys() => _seedKeys = null;

    private HybridCacheEntryOptions GetSeedEntryOptions() => new()
    {
        Expiration = _cacheSettings.Entry.Document.SeedCacheDuration,
        LocalCacheExpiration = _cacheSettings.Entry.Document.SeedCacheDuration
    };

    private HybridCacheEntryOptions GetEntryOptions(Guid key, bool preview)
    {
        if (SeedKeys.Contains(key) && preview is false)
        {
            return GetSeedEntryOptions();
        }

        return new HybridCacheEntryOptions
        {
            Expiration = _cacheSettings.Entry.Document.RemoteCacheDuration,
            LocalCacheExpiration = _cacheSettings.Entry.Document.LocalCacheDuration,
        };
    }

    public async Task<bool> HasContentByIdAsync(int id, bool preview = false)
    {
        Attempt<Guid> keyAttempt = _idKeyMap.GetKeyForId(id, UmbracoObjectTypes.Document);
        if (keyAttempt.Success is false)
        {
            return false;
        }

        ContentCacheNode? contentCacheNode = await _hybridCache.GetOrCreateAsync<ContentCacheNode?>(
            GetCacheKey(keyAttempt.Result, preview), // Unique key to the cache entry
            cancel => ValueTask.FromResult<ContentCacheNode?>(null));

        if (contentCacheNode is null)
        {
            await _hybridCache.RemoveAsync(GetCacheKey(keyAttempt.Result, preview));
        }

        return contentCacheNode is not null;
    }

    public async Task RefreshContentAsync(IContent content)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();

        // Always set draft node
        // We have nodes seperate in the cache, cause 99% of the time, you are only using one
        // and thus we won't get too much data when retrieving from the cache.
        var draftCacheNode = _cacheNodeFactory.ToContentCacheNode(content, true);

        await _databaseCacheRepository.RefreshContentAsync(draftCacheNode, content.PublishedState);

        if (content.PublishedState == PublishedState.Publishing || content.PublishedState == PublishedState.Unpublishing)
        {
            var publishedCacheNode = _cacheNodeFactory.ToContentCacheNode(content, false);

            await _databaseCacheRepository.RefreshContentAsync(publishedCacheNode, content.PublishedState);

            if (content.PublishedState == PublishedState.Unpublishing)
            {
                await _hybridCache.RemoveAsync(GetCacheKey(publishedCacheNode.Key, false));
            }
        }

        scope.Complete();
    }

    private static string GetCacheKey(Guid key, bool preview) => preview ? $"{key}+draft" : $"{key}";

    // Generates the cache tags for a given CacheNode
    // We use the tags to be able to clear all cache entries that are related to a given content item.
    // Tags for now are content/media, draft/published and all its ancestors, so we can clear when ChangeType.TreeChange
    private static HashSet<string> GenerateTags(Guid? key) => key is null ? [] : [Constants.Cache.Tags.Content];

    public async Task DeleteItemAsync(IContentBase content)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        await _databaseCacheRepository.DeleteContentItemAsync(content.Id);
        scope.Complete();
    }

    public void Rebuild(IReadOnlyCollection<int> contentTypeIds)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        _databaseCacheRepository.Rebuild(contentTypeIds.ToList());
        RebuildMemoryCacheByContentTypeAsync(contentTypeIds).GetAwaiter().GetResult();
        scope.Complete();
    }

    public async Task RebuildMemoryCacheByContentTypeAsync(IEnumerable<int> contentTypeIds)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();

        IEnumerable<ContentCacheNode> contentByContentTypeKey = _databaseCacheRepository.GetContentByContentTypeKey(contentTypeIds.Select(x => _idKeyMap.GetKeyForId(x, UmbracoObjectTypes.DocumentType).Result), ContentCacheDataSerializerEntityType.Document);
        scope.Complete();

        foreach (ContentCacheNode content in contentByContentTypeKey)
        {
            _hybridCache.RemoveAsync(GetCacheKey(content.Key, true)).GetAwaiter().GetResult();

            if (content.IsDraft is false)
            {
                _hybridCache.RemoveAsync(GetCacheKey(content.Key, false)).GetAwaiter().GetResult();
            }
        }
    }
}
