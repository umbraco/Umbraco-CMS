using Umbraco.Core.Sync;
using umbraco.interfaces;

namespace Umbraco.Core.Cache
{
    /// <summary>
    /// Provides a base class for "payload" cache refreshers.
    /// </summary>
    /// <typeparam name="TInstance">The actual cache refresher type.</typeparam>
    /// <remarks>Ensures that the correct events are raised when cache refreshing occurs.</remarks>
    public abstract class PayloadCacheRefresherBase<TInstance> : JsonCacheRefresherBase<TInstance>, IPayloadCacheRefresher
        where TInstance : ICacheRefresher
    {
        protected abstract object Deserialize(string json);

        public override void Refresh(string json)
        {
            var payload = Deserialize(json);
            Refresh(payload);
        }

        public virtual void Refresh(object payload)
        {
            OnCacheUpdated(Instance, new CacheRefresherEventArgs(payload, MessageType.RefreshByPayload));
        }
    }
}
