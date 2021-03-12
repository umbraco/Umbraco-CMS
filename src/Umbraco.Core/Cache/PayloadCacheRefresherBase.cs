using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Cache
{
    /// <summary>
    /// A base class for "payload" class refreshers.
    /// </summary>
    /// <typeparam name="TInstanceType">The actual cache refresher type.</typeparam>
    /// <typeparam name="TPayload">The payload type.</typeparam>
    /// <remarks>The actual cache refresher type is used for strongly typed events.</remarks>
    public abstract class PayloadCacheRefresherBase<TNotification, TPayload> : JsonCacheRefresherBase<TNotification, TPayload>, IPayloadCacheRefresher<TPayload>
        where TNotification : CacheRefresherNotificationBase, new()

    {

        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadCacheRefresherBase{TInstanceType, TPayload}"/>.
        /// </summary>
        /// <param name="appCaches">A cache helper.</param>
        /// <param name="serializer"></param>
        protected PayloadCacheRefresherBase(AppCaches appCaches, IJsonSerializer serializer, IEventAggregator eventAggregator)
            : base(appCaches, serializer, eventAggregator)
        {
        }


        #region Refresher

        public override void Refresh(string json)
        {
            var payload = Deserialize(json);
            Refresh(payload);
        }

        /// <summary>
        /// Refreshes as specified by a payload.
        /// </summary>
        /// <param name="payloads">The payload.</param>
        public virtual void Refresh(TPayload[] payloads)
        {
            OnCacheUpdated(new TNotification().Init(payloads, MessageType.RefreshByPayload));
        }

        #endregion
    }
}
