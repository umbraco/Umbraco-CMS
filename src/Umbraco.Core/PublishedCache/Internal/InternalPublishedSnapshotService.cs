using System.ComponentModel;
using Umbraco.Cms.Core.Cache;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PublishedCache.Internal;

// TODO: Only used in unit tests, needs to be moved to test project
[EditorBrowsable(EditorBrowsableState.Never)]
public class InternalPublishedSnapshotService : IPublishedSnapshotService
{
    private InternalPublishedSnapshot? _previewSnapshot;
    private InternalPublishedSnapshot? _snapshot;

    public Task CollectAsync() => Task.CompletedTask;

    public IPublishedSnapshot CreatePublishedSnapshot(string? previewToken)
    {
        if (previewToken.IsNullOrWhiteSpace())
        {
            return _snapshot ??= new InternalPublishedSnapshot();
        }

        return _previewSnapshot ??= new InternalPublishedSnapshot();
    }

    public void Dispose()
    {
        _snapshot?.Dispose();
        _previewSnapshot?.Dispose();
    }

    public void Notify(ContentCacheRefresher.JsonPayload[] payloads, out bool draftChanged, out bool publishedChanged)
    {
        draftChanged = false;
        publishedChanged = false;
    }

    public void Notify(MediaCacheRefresher.JsonPayload[] payloads, out bool anythingChanged) => anythingChanged = false;

    public void Notify(ContentTypeCacheRefresher.JsonPayload[] payloads)
    {
    }

    public void Notify(DataTypeCacheRefresher.JsonPayload[] payloads)
    {
    }

    public void Notify(DomainCacheRefresher.JsonPayload[] payloads)
    {
    }

    public void Rebuild(IReadOnlyCollection<int>? contentTypeIds = null, IReadOnlyCollection<int>? mediaTypeIds = null, IReadOnlyCollection<int>? memberTypeIds = null)
    {
    }
}
