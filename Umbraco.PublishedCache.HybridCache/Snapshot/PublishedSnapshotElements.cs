using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Infrastructure.HybridCache.Snapshot;

public class PublishedSnapshotElements
{
    public IPublishedContentHybridCache? ContentCache;
    public IPublishedMediaHybridCache? MediaCache;
    public IPublishedMemberHybridCache? MemberCache;
    public DomainCache? DomainCache;
    public IAppCache? ElementsCache;
}
