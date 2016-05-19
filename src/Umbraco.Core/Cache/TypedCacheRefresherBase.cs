using Umbraco.Core.Sync;

namespace Umbraco.Core.Cache
{
    /// <summary>
    /// A base class for cache refreshers to inherit from that ensures the correct events are raised
    /// when cache refreshing occurs.
    /// </summary>
    /// <typeparam name="TInstanceType">The real cache refresher type, this is used for raising strongly typed events</typeparam>
    /// <typeparam name="TEntityType">The entity type that this refresher can update cache for</typeparam>
    public abstract class TypedCacheRefresherBase<TInstanceType, TEntityType> : CacheRefresherBase<TInstanceType>, ICacheRefresher<TEntityType>
        where TInstanceType : ICacheRefresher
    {
        protected TypedCacheRefresherBase(CacheHelper cacheHelper) : base(cacheHelper)
        {
        }

        public virtual void Refresh(TEntityType instance)
        {
            OnCacheUpdated(Instance, new CacheRefresherEventArgs(instance, MessageType.RefreshByInstance));
        }

        public virtual void Remove(TEntityType instance)
        {
            OnCacheUpdated(Instance, new CacheRefresherEventArgs(instance, MessageType.RemoveByInstance));
        }
    }
}