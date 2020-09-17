using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Web.Routing;

namespace Umbraco.Web.PublishedCache.NuCache
{
    internal class PublishedSnapshotAccessor : IContentSnapshotAccessor, IDomainSnapshotAccessor
    {
        private ContentStore.Snapshot _snapshot;

        private SnapDictionary<int, Domain>.Snapshot _domainSnapshot;
        public SnapDictionary<int, Domain>.Snapshot GetDomainSnapshot() => _domainSnapshot;

        public void SetDomainSnapshot(SnapDictionary<int, Domain>.Snapshot snapshot) => _domainSnapshot = snapshot;

        ContentStore.Snapshot IContentSnapshotAccessor.GetContentSnapshot()
        {
            return _snapshot;
        }

        void IContentSnapshotAccessor.SetContentSnapshot(ContentStore.Snapshot snapshot)
        {
            _snapshot = snapshot;
        }
    }
}
