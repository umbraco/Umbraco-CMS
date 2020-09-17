using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Web.Routing;

namespace Umbraco.Web.PublishedCache.NuCache
{
    internal interface IDomainSnapshotAccessor
    {
        SnapDictionary<int, Domain>.Snapshot GetDomainSnapshot();
        void SetDomainSnapshot(SnapDictionary<int, Domain>.Snapshot snapshot);
    }
}
