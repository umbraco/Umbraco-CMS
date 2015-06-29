using Umbraco.Core.Sync;
using umbraco.interfaces;

namespace Umbraco.Core.Cache
{
    /// <summary>
    /// A base class for json cache refreshers that ensures the correct events are raised when 
    /// cache refreshing occurs.
    /// </summary>
    /// <typeparam name="TInstanceType">The real cache refresher type, this is used for raising strongly typed events</typeparam>
    public abstract class JsonCacheRefresherBase<TInstanceType> : CacheRefresherBase<TInstanceType>, IJsonCacheRefresher 
        where TInstanceType : ICacheRefresher
    {        

        public virtual void Refresh(string jsonPayload)
        {            
            OnCacheUpdated(Instance, new CacheRefresherEventArgs(jsonPayload, MessageType.RefreshByJson));
        }
    }
}