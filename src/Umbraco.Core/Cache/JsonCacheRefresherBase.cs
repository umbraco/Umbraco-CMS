using Umbraco.Core.Sync;
using umbraco.interfaces;

namespace Umbraco.Core.Cache
{
    /// <summary>
    /// Provides a base class for "json" cache refreshers.
    /// </summary>
    /// <typeparam name="TInstance">The actual cache refresher type.</typeparam>
    /// <remarks>Ensures that the correct events are raised when cache refreshing occurs.</remarks>
    public abstract class JsonCacheRefresherBase<TInstance> : CacheRefresherBase<TInstance>, IJsonCacheRefresher
        where TInstance : ICacheRefresher
    {        
        public virtual void Refresh(string json)
        {            
            OnCacheUpdated(Instance, new CacheRefresherEventArgs(json, MessageType.RefreshByJson));
        }
    }
}