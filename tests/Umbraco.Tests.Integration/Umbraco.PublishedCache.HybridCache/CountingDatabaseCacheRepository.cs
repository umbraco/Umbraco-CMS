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

    public Task<ContentCacheNode?> GetContentSourceAsync(Guid key, bool preview = false)
    {
        SingleContentReads++;
        AsyncReads++;
        _singleContentKeys.Enqueue(key);
        return _inner.GetContentSourceAsync(key, preview);
    }

    public Task<IEnumerable<ContentCacheNode>> GetContentSourcesAsync(IEnumerable<Guid> keys, bool preview = false)
    {
        BatchContentReads++;
        AsyncReads++;
        Guid[] keysArray = keys as Guid[] ?? keys.ToArray();
        _batchContentKeys.Enqueue(keysArray);
        return _inner.GetContentSourcesAsync(keysArray, preview);
    }

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

    public ContentCacheNode? GetContentSource(Guid key, bool preview = false)
    {
        SingleContentReads++;
        _singleContentKeys.Enqueue(key);
        return _inner.GetContentSource(key, preview);
    }

    public IEnumerable<ContentCacheNode> GetContentSources(IEnumerable<Guid> keys, bool preview = false)
    {
        BatchContentReads++;
        Guid[] keysArray = keys as Guid[] ?? keys.ToArray();
        _batchContentKeys.Enqueue(keysArray);
        return _inner.GetContentSources(keysArray, preview);
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

    public IEnumerable<Guid> GetDocumentKeysByContentTypeKeys(IEnumerable<Guid> keys, bool published = false)
        => _inner.GetDocumentKeysByContentTypeKeys(keys, published);

    public Task RefreshContentAsync(ContentCacheNode contentCacheNode) => _inner.RefreshContentAsync(contentCacheNode);

    public Task RefreshMediaAsync(ContentCacheNode contentCacheNode) => _inner.RefreshMediaAsync(contentCacheNode);

    public Task RemovePublishedContentAsync(int id) => _inner.RemovePublishedContentAsync(id);

    public void Rebuild(
        IReadOnlyCollection<int>? contentTypeIds,
        IReadOnlyCollection<int>? mediaTypeIds,
        Action<Action>? executeStep)
        => _inner.Rebuild(contentTypeIds, mediaTypeIds, executeStep);
}
