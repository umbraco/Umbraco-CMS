using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Composing;

namespace Umbraco.Core.Cache
{
    public class CacheRefresherCollection : BuilderCollectionBase<ICacheRefresher>
    {
        public CacheRefresherCollection(IEnumerable<ICacheRefresher> items)
            : base(items)
        { }

        public ICacheRefresher this[Guid id]
            =>  this.FirstOrDefault(x => x.RefresherUniqueId == id);
    }
}
