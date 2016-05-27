using System.Collections.Generic;
using System.Linq;
using Umbraco.Web.Routing;

namespace Umbraco.Web.PublishedCache.NuCache
{
    class DomainCache : IDomainCache
    {
        private readonly SnapDictionary<int, Domain>.Snapshot _snapshot;

        public DomainCache(SnapDictionary<int, Domain>.Snapshot snapshot)
        {
            _snapshot = snapshot;
        }

        public IEnumerable<Domain> GetAll(bool includeWildcards)
        {
            var list = _snapshot.GetAll();
            if (includeWildcards == false) list = list.Where(x => x.IsWildcard == false);
            return list;
        }

        public IEnumerable<Domain> GetAssigned(int contentId, bool includeWildcards)
        {
            // probably this could be optimized with an index
            // but then we'd need a custom DomainStore of some sort

            var list = _snapshot.GetAll();
            list = list.Where(x => x.ContentId == contentId);
            if (includeWildcards == false) list = list.Where(x => x.IsWildcard == false);
            return list;
        }
    }
}
