namespace Umbraco.Cms.Infrastructure.HybridCache.Snapshot;

public interface IPublishedSnapshotElementsFactory
{
    PublishedSnapshotElements CreateElements(bool preview);
}
