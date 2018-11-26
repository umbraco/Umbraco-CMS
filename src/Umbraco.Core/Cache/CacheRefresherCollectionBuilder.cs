using Umbraco.Core.Composing;

namespace Umbraco.Core.Cache
{
    public class CacheRefresherCollectionBuilder : LazyCollectionBuilderBase<CacheRefresherCollectionBuilder, CacheRefresherCollection, ICacheRefresher>
    {
        protected override CacheRefresherCollectionBuilder This => this;
    }
}
