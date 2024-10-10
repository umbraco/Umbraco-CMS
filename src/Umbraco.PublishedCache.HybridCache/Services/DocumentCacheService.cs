using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
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
        IPreviewService previewService)
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
        _cacheSettings = cacheSettings.Value;
    }

    public async Task<IPublishedContent?> GetByKeyAsync(Guid key, bool? preview = null)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();

        bool calculatedPreview = preview ?? GetPreview();

        ContentCacheNode? contentCacheNode = await _hybridCache.GetOrCreateAsync(
            GetCacheKey(key, calculatedPreview), // Unique key to the cache entry
            async cancel => await _databaseCacheRepository.GetContentSourceAsync(key, calculatedPreview));

        scope.Complete();
        return contentCacheNode is null ? null : _publishedContentFactory.ToIPublishedContent(contentCacheNode, calculatedPreview).CreateModel(_publishedModelFactory);
    }

    private bool GetPreview()
    {
        return _previewService.IsInPreview();
    }

    public async Task<IPublishedContent?> GetByIdAsync(int id, bool? preview = null)
    {
        Attempt<Guid> keyAttempt = _idKeyMap.GetKeyForId(id, UmbracoObjectTypes.Document);
        if (keyAttempt.Success is false)
        {
            return null;
        }

        bool calculatedPreview = preview ?? GetPreview();

        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        ContentCacheNode? contentCacheNode = await _hybridCache.GetOrCreateAsync(
            GetCacheKey(keyAttempt.Result, calculatedPreview), // Unique key to the cache entry
            async cancel => await _databaseCacheRepository.GetContentSourceAsync(id, calculatedPreview));
        scope.Complete();
        return contentCacheNode is null ? null : _publishedContentFactory.ToIPublishedContent(contentCacheNode, calculatedPreview).CreateModel(_publishedModelFactory);;
    }

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
        // TODO: This should be done with tags, however this is not implemented yet, so for now we have to naively get all content keys and clear them all.
        using ICoreScope scope = _scopeProvider.CreateCoreScope();

        // We have to get ALL document keys in order to be able to remove them from the cache,
        IEnumerable<Guid> documentKeys = await _databaseCacheRepository.GetContentKeysAsync(Constants.ObjectTypes.Document);

        foreach (Guid documentKey in documentKeys)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            // We'll remove both the draft and published cache
            await _hybridCache.RemoveAsync(GetCacheKey(documentKey, false), cancellationToken);
            await _hybridCache.RemoveAsync(GetCacheKey(documentKey, true), cancellationToken);
        }

        // We have to run seeding again after the cache is cleared
        await SeedAsync(cancellationToken);

        scope.Complete();
    }

    public async Task RefreshMemoryCacheAsync(Guid key)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();

        bool isSeeded = SeedKeys.Contains(key);

        ContentCacheNode? draftNode = await _databaseCacheRepository.GetContentSourceAsync(key, true);
        if (draftNode is not null)
        {
            await RefreshHybridCacheAsync(draftNode, GetCacheKey(draftNode.Key, true), isSeeded);
        }

        ContentCacheNode? publishedNode = await _databaseCacheRepository.GetContentSourceAsync(key, false);
        if (publishedNode is not null)
        {
            await RefreshHybridCacheAsync(publishedNode, GetCacheKey(publishedNode.Key, false), isSeeded);
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
        using ICoreScope scope = _scopeProvider.CreateCoreScope();

        foreach (Guid key in SeedKeys)
        {
            if(cancellationToken.IsCancellationRequested)
            {
                break;
            }

            var cacheKey = GetCacheKey(key, false);

            // We'll use GetOrCreateAsync because it may be in the second level cache, in which case we don't have to re-seed.
            ContentCacheNode? cachedValue = await _hybridCache.GetOrCreateAsync<ContentCacheNode?>(
                cacheKey,
                async cancel =>
                {
                    ContentCacheNode? cacheNode = await _databaseCacheRepository.GetContentSourceAsync(key, false);

                    // We don't want to seed drafts
                    if (cacheNode is null || cacheNode.IsDraft)
                    {
                        return null;
                    }

                    return cacheNode;
                },
                GetSeedEntryOptions());

            // If the value is null, it's likely because
            if (cachedValue is null)
            {
                await _hybridCache.RemoveAsync(cacheKey);
            }
        }

        scope.Complete();
    }

    private HybridCacheEntryOptions GetSeedEntryOptions() => new()
    {
        Expiration = _cacheSettings.SeedCacheDuration,
        LocalCacheExpiration = _cacheSettings.SeedCacheDuration
    };

    public async Task<bool> HasContentByIdAsync(int id, bool preview = false)
    {
        Attempt<Guid>  keyAttempt = _idKeyMap.GetKeyForId(id, UmbracoObjectTypes.Document);
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
        ContentCacheNode draftCacheNode = _cacheNodeFactory.ToContentCacheNode(content, true);

        await _databaseCacheRepository.RefreshContentAsync(draftCacheNode, content.PublishedState);

        if (content.PublishedState == PublishedState.Publishing)
        {
            var publishedCacheNode = _cacheNodeFactory.ToContentCacheNode(content, false);

            await _databaseCacheRepository.RefreshContentAsync(publishedCacheNode, content.PublishedState);
        }

        scope.Complete();
    }

    private async Task RefreshHybridCacheAsync(ContentCacheNode cacheNode, string cacheKey, bool isSeeded)
    {
        // If it's seeded we want it to stick around the cache for longer.
        if (isSeeded)
        {
            await _hybridCache.SetAsync(
                cacheKey,
                cacheNode,
                GetSeedEntryOptions());
        }
        else
        {
            await _hybridCache.SetAsync(cacheKey, cacheNode);
        }
    }

    private string GetCacheKey(Guid key, bool preview) => preview ? $"{key}+draft" : $"{key}";

    public async Task DeleteItemAsync(IContentBase content)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        await _databaseCacheRepository.DeleteContentItemAsync(content.Id);
        await _hybridCache.RemoveAsync(GetCacheKey(content.Key, true));
        await _hybridCache.RemoveAsync(GetCacheKey(content.Key, false));
        scope.Complete();
    }

    public void Rebuild(IReadOnlyCollection<int> contentTypeIds)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        _databaseCacheRepository.Rebuild(contentTypeIds.ToList());
        RebuildMemoryCacheByContentTypeAsync(contentTypeIds, UmbracoObjectTypes.DocumentType).GetAwaiter().GetResult();
        scope.Complete();
    }

    public async Task RebuildMemoryCacheByContentTypeAsync(IEnumerable<int> contentTypeIds, UmbracoObjectTypes objectType)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();

        ContentCacheDataSerializerEntityType serializerType = objectType switch
        {
            UmbracoObjectTypes.DocumentType => ContentCacheDataSerializerEntityType.Document,
            UmbracoObjectTypes.MediaType => ContentCacheDataSerializerEntityType.Media,
            _ => throw new NotSupportedException($"Object type {objectType} is not supported")
        };

        IEnumerable<ContentCacheNode> contentByContentTypeKey = _databaseCacheRepository.GetContentByContentTypeKey(contentTypeIds.Select(x => _idKeyMap.GetKeyForId(x, objectType).Result), serializerType);

        foreach (ContentCacheNode content in contentByContentTypeKey)
        {
            _hybridCache.RemoveAsync(GetCacheKey(content.Key, true)).GetAwaiter().GetResult();

            if (content.IsDraft is false)
            {
                _hybridCache.RemoveAsync(GetCacheKey(content.Key, false)).GetAwaiter().GetResult();
            }
        }
        scope.Complete();
    }
}
