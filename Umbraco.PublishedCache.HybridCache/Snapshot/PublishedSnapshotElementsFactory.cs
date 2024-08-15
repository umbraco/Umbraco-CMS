namespace Umbraco.Cms.Infrastructure.HybridCache.Snapshot;

internal class PublishedSnapshotElementsFactory : IPublishedSnapshotElementsFactory
{
    public PublishedSnapshotElements CreateElements(bool preview)
    {
        return new PublishedSnapshotElements();
    }
}
