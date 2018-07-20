using Umbraco.Core.Composing;

namespace Umbraco.Core.Cache
{
    public class CacheRefresherCollectionBuilder : LazyCollectionBuilderBase<CacheRefresherCollectionBuilder, CacheRefresherCollection, ICacheRefresher>
    {
        public CacheRefresherCollectionBuilder(IContainer container)
            : base(container)
        { }

        protected override CacheRefresherCollectionBuilder This => this;
    }
}
