using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Infrastructure.HybridCache.Snapshot;

public class PublishedSnapshotElements
{
    public IPublishedContentCache? ContentCache;
    public IPublishedMediaCache? MediaCache;
    public IPublishedMemberCache? MemberCache;
    public DomainCache? DomainCache;
    public IAppCache? ElementsCache;
}
