using System;
using System.Collections.Generic;
using Umbraco.Core.ObjectResolution;
using CoreCurrent = Umbraco.Core.Composing.Current;
using LightInject;

// ReSharper disable once CheckNamespace
namespace Umbraco.Core.Cache
{
    public class CacheRefreshersResolver : LazyManyObjectsResolverBase<CacheRefresherCollectionBuilder, CacheRefresherCollection, ICacheRefresher>
    {
        private CacheRefreshersResolver(CacheRefresherCollectionBuilder builder)
            : base(builder)
        { }

        public static CacheRefreshersResolver Current { get; }
            = new CacheRefreshersResolver(CoreCurrent.Container.GetInstance<CacheRefresherCollectionBuilder>());

        public IEnumerable<ICacheRefresher> CacheRefreshers => CoreCurrent.CacheRefreshers;

        public ICacheRefresher GetById(Guid id) => CoreCurrent.CacheRefreshers[id];
    }
}
