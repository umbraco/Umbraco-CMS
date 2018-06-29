using Umbraco.Core.Sync;

namespace Umbraco.Core.Cache
{
    /// <summary>
    /// A base class for "json" cache refreshers.
    /// </summary>
    /// <typeparam name="TInstanceType">The actual cache refresher type.</typeparam>
    /// <remarks>The actual cache refresher type is used for strongly typed events.</remarks>
    public abstract class JsonCacheRefresherBase<TInstanceType> : CacheRefresherBase<TInstanceType>, IJsonCacheRefresher
        where TInstanceType : class, ICacheRefresher
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonCacheRefresherBase{TInstanceType}"/>.
        /// </summary>
        /// <param name="cacheHelper">A cache helper.</param>
        protected JsonCacheRefresherBase(CacheHelper cacheHelper) : base(cacheHelper)
        { }

        /// <summary>
        /// Refreshes as specified by a json payload.
        /// </summary>
        /// <param name="json">The json payload.</param>
        public virtual void Refresh(string json)
        {
            OnCacheUpdated(This, new CacheRefresherEventArgs(json, MessageType.RefreshByJson));
        }
    }
}
