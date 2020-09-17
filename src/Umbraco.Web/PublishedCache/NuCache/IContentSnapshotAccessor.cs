

namespace Umbraco.Web.PublishedCache.NuCache
{
    interface IContentSnapshotAccessor
    {
        ContentStore.Snapshot GetContentSnapshot();
        void SetContentSnapshot(ContentStore.Snapshot snapshot);
    }

   
}
