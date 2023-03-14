using System.ComponentModel;
using Umbraco.Cms.Core.Cache;

namespace Umbraco.Cms.Core.PublishedCache.Internal;

// TODO: Only used in unit tests, needs to be moved to test project
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class InternalPublishedSnapshot : IPublishedSnapshot
{
    public InternalPublishedContentCache InnerContentCache { get; } = new();

    public InternalPublishedContentCache InnerMediaCache { get; } = new();

    public IPublishedContentCache Content => InnerContentCache;

    public IPublishedMediaCache Media => InnerMediaCache;

    public IPublishedMemberCache? Members => null;

    public IDomainCache? Domains => null;

    public IAppCache? SnapshotCache => null;

    public IDisposable ForcedPreview(bool forcedPreview, Action<bool>? callback = null) =>
        throw new NotImplementedException();

    public IAppCache? ElementsCache => null;

    public void Dispose()
    {
    }

    public void Resync()
    {
    }
}
