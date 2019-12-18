using Umbraco.Core.Sync;

namespace Umbraco.Core.Cache
{
    /// <summary>
    /// A base class for "typed" cache refreshers.
    /// </summary>
    /// <typeparam name="TInstanceType">The actual cache refresher type.</typeparam>
    /// <typeparam name="TEntityType">The entity type.</typeparam>
    /// <remarks>The actual cache refresher type is used for strongly typed events.</remarks>
    public abstract class TypedCacheRefresherBase<TInstanceType, TEntityType> : CacheRefresherBase<TInstanceType>, ICacheRefresher<TEntityType>
        where TInstanceType : class, ICacheRefresher
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypedCacheRefresherBase{TInstanceType, TEntityType}"/>.
        /// </summary>
        /// <param name="appCaches">A cache helper.</param>
        protected TypedCacheRefresherBase(AppCaches appCaches)
            : base(appCaches)
        { }

        #region Refresher

        public virtual void Refresh(TEntityType instance)
        {
            OnCacheUpdated(This, new CacheRefresherEventArgs(instance, MessageType.RefreshByInstance));
        }

        public virtual void Remove(TEntityType instance)
        {
            OnCacheUpdated(This, new CacheRefresherEventArgs(instance, MessageType.RemoveByInstance));
        }

        #endregion
    }
}
