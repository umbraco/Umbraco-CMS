using Umbraco.Cms.Core.Cache;

namespace Umbraco.Cms.Infrastructure.HybridCache.Snapshot;

public class PublishedSnapshotElements
{
    public ContentCache? ContentCache;
    public MediaCache? MediaCache;
    public MemberCache? MemberCache;
    public DomainCache? DomainCache;
    public IAppCache? ElementsCache;
}
