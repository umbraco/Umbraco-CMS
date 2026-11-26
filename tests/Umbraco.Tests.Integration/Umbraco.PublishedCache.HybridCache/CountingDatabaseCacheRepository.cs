using System.Collections.Concurrent;
using Umbraco.Cms.Infrastructure.HybridCache;
using Umbraco.Cms.Infrastructure.HybridCache.Persistence;
using Umbraco.Cms.Infrastructure.HybridCache.Serialization;

namespace Umbraco.Cms.Tests.Integration.Umbraco.PublishedCache.HybridCache;

/// <summary>
/// Decorates the real <see cref="IDatabaseCacheRepository"/>, forwarding every call while counting
/// single-row versus batched reads, and recording the keys each read asked for. The batched methods must be
/// overridden explicitly: the interface default loops the single-row method, so without these overrides the
/// decorator would never reach the real batched query.
/// </summary>
internal sealed class CountingDatabaseCacheRepository : IDatabaseCacheRepository
{
    private readonly IDatabaseCacheRepository _inner;
    private readonly ConcurrentQueue<Guid> _singleContentKeys = new();
    private readonly ConcurrentQueue<Guid[]> _batchContentKeys = new();
    private readonly ConcurrentQueue<Guid> _singleMediaKeys = new();
    private readonly ConcurrentQueue<Guid[]> _batchMediaKeys = new();

    public CountingDatabaseCacheRepository(IDatabaseCacheRepository inner) => _inner = inner;

    public int SingleContentReads { get; private set; }

    public int BatchContentReads { get; private set; }

    public int SingleMediaReads { get; private set; }

    public int BatchMediaReads { get; private set; }

    public int AsyncReads { get; private set; }

    /// <summary>Gets the key of every single-row document read since the last <see cref="Reset"/>.</summary>
    public IReadOnlyCollection<Guid> SingleContentKeys => _singleContentKeys;

    /// <summary>Gets the keys asked for by every batched document read since the last <see cref="Reset"/>, one entry per read.</summary>
    public IReadOnlyCollection<Guid[]> BatchContentKeys => _batchContentKeys;

    /// <summary>Gets the key of every single-row media read since the last <see cref="Reset"/>.</summary>
    public IReadOnlyCollection<Guid> SingleMediaKeys => _singleMediaKeys;

    /// <summary>Gets the keys asked for by every batched media read since the last <see cref="Reset"/>, one entry per read.</summary>
    public IReadOnlyCollection<Guid[]> BatchMediaKeys => _batchMediaKeys;

    public void Reset()
    {
        SingleContentReads = 0;
        BatchContentReads = 0;
        SingleMediaReads = 0;
        BatchMediaReads = 0;
        AsyncReads = 0;
        _singleContentKeys.Clear();
        _batchContentKeys.Clear();
        _singleMediaKeys.Clear();
        _batchMediaKeys.Clear();
    }

    public Task<ContentCacheNode?> GetDocumentSourceAsync(Guid key, bool preview = false)
    {
        SingleContentReads++;
        AsyncReads++;
        _singleContentKeys.Enqueue(key);
        return _inner.GetDocumentSourceAsync(key, preview);
    }

    public Task<IEnumerable<ContentCacheNode>> GetDocumentSourcesAsync(IEnumerable<Guid> keys, bool preview = false)
    {
        BatchContentReads++;
        AsyncReads++;
        Guid[] keysArray = keys as Guid[] ?? keys.ToArray();
        _batchContentKeys.Enqueue(keysArray);
        return _inner.GetDocumentSourcesAsync(keysArray, preview);
    }

    public Task<(ContentCacheNode? Draft, ContentCacheNode? Published)> GetDocumentSourceForPublishStatesAsync(Guid key)
        => _inner.GetDocumentSourceForPublishStatesAsync(key);

    public Task<ContentCacheNode?> GetMediaSourceAsync(Guid key)
    {
        SingleMediaReads++;
        AsyncReads++;
        _singleMediaKeys.Enqueue(key);
        return _inner.GetMediaSourceAsync(key);
    }

    public Task<IEnumerable<ContentCacheNode>> GetMediaSourcesAsync(IEnumerable<Guid> keys)
    {
        BatchMediaReads++;
        AsyncReads++;
        Guid[] keysArray = keys as Guid[] ?? keys.ToArray();
        _batchMediaKeys.Enqueue(keysArray);
        return _inner.GetMediaSourcesAsync(keysArray);
    }

    public Task<ContentCacheNode?> GetElementSourceAsync(Guid key, bool preview = false)
        => _inner.GetElementSourceAsync(key, preview);

    public ContentCacheNode? GetElementSource(Guid key, bool preview = false)
        => _inner.GetElementSource(key, preview);

    public Task<(ContentCacheNode? Draft, ContentCacheNode? Published)> GetElementSourceForPublishStatesAsync(Guid key)
        => _inner.GetElementSourceForPublishStatesAsync(key);

    public Task<IEnumerable<ContentCacheNode>> GetElementSourcesAsync(IEnumerable<Guid> keys, bool preview = false)
        => _inner.GetElementSourcesAsync(keys, preview);

    public IEnumerable<ContentCacheNode> GetElementSources(IEnumerable<Guid> keys, bool preview = false)
        => _inner.GetElementSources(keys, preview);

    public IEnumerable<Guid> GetElementKeysByContentTypeKeys(IEnumerable<Guid> keys, bool published = false)
        => _inner.GetElementKeysByContentTypeKeys(keys, published);

    public Task RefreshElementAsync(ContentCacheNode contentCacheNode) => _inner.RefreshElementAsync(contentCacheNode);

    public Task RemovePublishedElementAsync(int id) => _inner.RemovePublishedElementAsync(id);

    public ContentCacheNode? GetDocumentSource(Guid key, bool preview = false)
    {
        SingleContentReads++;
        _singleContentKeys.Enqueue(key);
        return _inner.GetDocumentSource(key, preview);
    }

    public IEnumerable<ContentCacheNode> GetDocumentSources(IEnumerable<Guid> keys, bool preview = false)
    {
        BatchContentReads++;
        Guid[] keysArray = keys as Guid[] ?? keys.ToArray();
        _batchContentKeys.Enqueue(keysArray);
        return _inner.GetDocumentSources(keysArray, preview);
    }

    public ContentCacheNode? GetMediaSource(Guid key)
    {
        SingleMediaReads++;
        _singleMediaKeys.Enqueue(key);
        return _inner.GetMediaSource(key);
    }

    public IEnumerable<ContentCacheNode> GetMediaSources(IEnumerable<Guid> keys)
    {
        BatchMediaReads++;
        Guid[] keysArray = keys as Guid[] ?? keys.ToArray();
        _batchMediaKeys.Enqueue(keysArray);
        return _inner.GetMediaSources(keysArray);
    }

    public Task DeleteContentItemAsync(int id) => _inner.DeleteContentItemAsync(id);

    public IEnumerable<ContentCacheNode> GetContentByContentTypeKey(IEnumerable<Guid> keys, ContentCacheDataSerializerEntityType entityType)
        => _inner.GetContentByContentTypeKey(keys, entityType);

    public ISet<Guid> GetKeysOfEntityTypes(IReadOnlyCollection<Guid> keys, ContentCacheDataSerializerEntityType entityTypes)
        => _inner.GetKeysOfEntityTypes(keys, entityTypes);

    public IEnumerable<Guid> GetDocumentKeysByContentTypeKeys(IEnumerable<Guid> keys, bool published = false)
        => _inner.GetDocumentKeysByContentTypeKeys(keys, published);

    public IEnumerable<(Guid Key, bool IsDraft)> GetDocumentKeysWithPublishedStatus(IEnumerable<Guid> contentTypeKeys)
        => _inner.GetDocumentKeysWithPublishedStatus(contentTypeKeys);

    public IEnumerable<Guid> GetMediaKeysByContentTypeKeys(IEnumerable<Guid> mediaTypeKeys)
        => _inner.GetMediaKeysByContentTypeKeys(mediaTypeKeys);

    public Task RefreshDocumentAsync(ContentCacheNode contentCacheNode) => _inner.RefreshDocumentAsync(contentCacheNode);

    public Task RefreshMediaAsync(ContentCacheNode contentCacheNode) => _inner.RefreshMediaAsync(contentCacheNode);

    public Task RemovePublishedDocumentAsync(int id) => _inner.RemovePublishedDocumentAsync(id);

    public void Rebuild(
        IReadOnlyCollection<int>? contentTypeIds,
        IReadOnlyCollection<int>? mediaTypeIds,
        IReadOnlyCollection<int>? elementTypeIds,
        Action<Action>? executeStep)
        => _inner.Rebuild(contentTypeIds, mediaTypeIds, elementTypeIds, executeStep);
}
