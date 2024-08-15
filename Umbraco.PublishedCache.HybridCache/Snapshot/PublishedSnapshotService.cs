using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Infrastructure.HybridCache.Snapshot;

public class PublishedSnapshotService : IPublishedSnapshotService
{
    private readonly IPublishedSnapshotElementsFactory _publishedSnapshotElementsFactory;

    public PublishedSnapshotService(IPublishedSnapshotElementsFactory publishedSnapshotElementsFactory)
    {
        _publishedSnapshotElementsFactory = publishedSnapshotElementsFactory;
    }

    public IPublishedSnapshot CreatePublishedSnapshot(string? previewToken)
    {
        var preview = string.IsNullOrWhiteSpace(previewToken) == false;
        return new PublishedSnapshot(_publishedSnapshotElementsFactory, preview);
    }

    public void Rebuild(IReadOnlyCollection<int>? contentTypeIds = null, IReadOnlyCollection<int>? mediaTypeIds = null, IReadOnlyCollection<int>? memberTypeIds = null) =>
        throw new NotImplementedException();

    public void Notify(ContentCacheRefresher.JsonPayload[] payloads, out bool draftChanged, out bool publishedChanged) => throw new NotImplementedException();

    public void Notify(MediaCacheRefresher.JsonPayload[] payloads, out bool anythingChanged) => throw new NotImplementedException();

    public void Notify(ContentTypeCacheRefresher.JsonPayload[] payloads) => throw new NotImplementedException();

    public void Notify(DataTypeCacheRefresher.JsonPayload[] payloads) => throw new NotImplementedException();

    public void Notify(DomainCacheRefresher.JsonPayload[] payloads) => throw new NotImplementedException();

    public Task CollectAsync() => throw new NotImplementedException();

    public void Dispose() => throw new NotImplementedException();
}
