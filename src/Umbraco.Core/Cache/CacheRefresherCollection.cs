using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Cache;

public class CacheRefresherCollection : BuilderCollectionBase<ICacheRefresher>
{
    public CacheRefresherCollection(Func<IEnumerable<ICacheRefresher>> items)
        : base(items)
    {
    }

    public ICacheRefresher? this[Guid id]
        => this.FirstOrDefault(x => x.RefresherUniqueId == id);
}
