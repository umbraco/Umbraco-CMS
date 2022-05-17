using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Cache;

public class CacheRefresherCollectionBuilder : LazyCollectionBuilderBase<CacheRefresherCollectionBuilder,
    CacheRefresherCollection, ICacheRefresher>
{
    protected override CacheRefresherCollectionBuilder This => this;
}
