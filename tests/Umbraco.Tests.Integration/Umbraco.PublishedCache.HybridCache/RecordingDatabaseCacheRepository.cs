using Umbraco.Cms.Infrastructure.HybridCache;
using Umbraco.Cms.Infrastructure.HybridCache.Persistence;
using Umbraco.Cms.Infrastructure.HybridCache.Serialization;

namespace Umbraco.Cms.Tests.Integration.Umbraco.PublishedCache.HybridCache;

/// <summary>
/// Forwards every call to the real repository and counts whether the save path used the synchronous or the
/// asynchronous members. The synchronous members must be forwarded explicitly: the interface defaults bridge to
/// the asynchronous ones, which would hide the distinction.
/// </summary>
internal sealed class RecordingDatabaseCacheRepository : IDatabaseCacheRepository
{
    private readonly IDatabaseCacheRepository _inner;

    public RecordingDatabaseCacheRepository(IDatabaseCacheRepository inner) => _inner = inner;

    public int SynchronousWrites { get; private set; }

    public int AsynchronousWrites { get; private set; }

    public int AsynchronousReads { get; private set; }

    public void Reset()
    {
        SynchronousWrites = 0;
        AsynchronousWrites = 0;
        AsynchronousReads = 0;
    }

    public void DeleteContentItem(int id)
    {
        SynchronousWrites++;
        _inner.DeleteContentItem(id);
    }

    public void RefreshDocument(ContentCacheNode contentCacheNode)
    {
        SynchronousWrites++;
        _inner.RefreshDocument(contentCacheNode);
    }

    public void RefreshMedia(ContentCacheNode contentCacheNode)
    {
        SynchronousWrites++;
        _inner.RefreshMedia(contentCacheNode);
    }

    public void RemovePublishedDocument(int id)
    {
        SynchronousWrites++;
        _inner.RemovePublishedDocument(id);
    }

    public (ContentCacheNode? Draft, ContentCacheNode? Published) GetDocumentSourceForPublishStates(Guid key)
        => _inner.GetDocumentSourceForPublishStates(key);

    public ContentCacheNode? GetDocumentSource(Guid key, bool preview = false)
        => _inner.GetDocumentSource(key, preview);

    public IEnumerable<ContentCacheNode> GetDocumentSources(IEnumerable<Guid> keys, bool preview = false)
        => _inner.GetDocumentSources(keys, preview);

    public ContentCacheNode? GetMediaSource(Guid key)
        => _inner.GetMediaSource(key);

    public IEnumerable<ContentCacheNode> GetMediaSources(IEnumerable<Guid> keys)
        => _inner.GetMediaSources(keys);

    public Task DeleteContentItemAsync(int id)
    {
        AsynchronousWrites++;
        return _inner.DeleteContentItemAsync(id);
    }

    public Task RefreshDocumentAsync(ContentCacheNode contentCacheNode)
    {
        AsynchronousWrites++;
        return _inner.RefreshDocumentAsync(contentCacheNode);
    }

    public Task RefreshMediaAsync(ContentCacheNode contentCacheNode)
    {
        AsynchronousWrites++;
        return _inner.RefreshMediaAsync(contentCacheNode);
    }

    public Task RemovePublishedDocumentAsync(int id)
    {
        AsynchronousWrites++;
        return _inner.RemovePublishedDocumentAsync(id);
    }

    public Task<(ContentCacheNode? Draft, ContentCacheNode? Published)> GetDocumentSourceForPublishStatesAsync(Guid key)
    {
        AsynchronousReads++;
        return _inner.GetDocumentSourceForPublishStatesAsync(key);
    }

    public Task<ContentCacheNode?> GetDocumentSourceAsync(Guid key, bool preview = false)
    {
        AsynchronousReads++;
        return _inner.GetDocumentSourceAsync(key, preview);
    }

    public Task<IEnumerable<ContentCacheNode>> GetDocumentSourcesAsync(IEnumerable<Guid> keys, bool preview = false)
    {
        AsynchronousReads++;
        return _inner.GetDocumentSourcesAsync(keys, preview);
    }

    public IEnumerable<Guid> GetDocumentKeysByContentTypeKeys(IEnumerable<Guid> keys, bool published = false)
        => _inner.GetDocumentKeysByContentTypeKeys(keys, published);

    public IEnumerable<(Guid Key, bool IsDraft)> GetDocumentKeysWithPublishedStatus(IEnumerable<Guid> contentTypeKeys)
        => _inner.GetDocumentKeysWithPublishedStatus(contentTypeKeys);

    public Task<ContentCacheNode?> GetMediaSourceAsync(Guid key)
        => _inner.GetMediaSourceAsync(key);

    public Task<IEnumerable<ContentCacheNode>> GetMediaSourcesAsync(IEnumerable<Guid> keys)
        => _inner.GetMediaSourcesAsync(keys);

    public IEnumerable<Guid> GetMediaKeysByContentTypeKeys(IEnumerable<Guid> mediaTypeKeys)
        => _inner.GetMediaKeysByContentTypeKeys(mediaTypeKeys);

    public Task<ContentCacheNode?> GetElementSourceAsync(Guid key, bool preview = false)
        => _inner.GetElementSourceAsync(key, preview);

    public ContentCacheNode? GetElementSource(Guid key, bool preview = false)
        => _inner.GetElementSource(key, preview);

    public (ContentCacheNode? Draft, ContentCacheNode? Published) GetElementSourceForPublishStates(Guid key)
        => _inner.GetElementSourceForPublishStates(key);

    public Task<(ContentCacheNode? Draft, ContentCacheNode? Published)> GetElementSourceForPublishStatesAsync(Guid key)
    {
        AsynchronousReads++;
        return _inner.GetElementSourceForPublishStatesAsync(key);
    }

    public Task<IEnumerable<ContentCacheNode>> GetElementSourcesAsync(IEnumerable<Guid> keys, bool preview = false)
        => _inner.GetElementSourcesAsync(keys, preview);

    public IEnumerable<ContentCacheNode> GetElementSources(IEnumerable<Guid> keys, bool preview = false)
        => _inner.GetElementSources(keys, preview);

    public IEnumerable<Guid> GetElementKeysByContentTypeKeys(IEnumerable<Guid> keys, bool published = false)
        => _inner.GetElementKeysByContentTypeKeys(keys, published);

    public void RefreshElement(ContentCacheNode contentCacheNode)
    {
        SynchronousWrites++;
        _inner.RefreshElement(contentCacheNode);
    }

    public Task RefreshElementAsync(ContentCacheNode contentCacheNode)
    {
        AsynchronousWrites++;
        return _inner.RefreshElementAsync(contentCacheNode);
    }

    public void RemovePublishedElement(int id)
    {
        SynchronousWrites++;
        _inner.RemovePublishedElement(id);
    }

    public Task RemovePublishedElementAsync(int id)
    {
        AsynchronousWrites++;
        return _inner.RemovePublishedElementAsync(id);
    }

    public IEnumerable<ContentCacheNode> GetContentByContentTypeKey(IEnumerable<Guid> keys, ContentCacheDataSerializerEntityType entityType)
        => _inner.GetContentByContentTypeKey(keys, entityType);

    public void Rebuild(IReadOnlyCollection<int>? contentTypeIds, IReadOnlyCollection<int>? mediaTypeIds, IReadOnlyCollection<int>? elementTypeIds, Action<Action>? executeStep)
        => _inner.Rebuild(contentTypeIds, mediaTypeIds, elementTypeIds, executeStep);
}
