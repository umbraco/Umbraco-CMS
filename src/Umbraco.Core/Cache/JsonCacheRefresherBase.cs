using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Cache
{
    /// <summary>
    /// A base class for "json" cache refreshers.
    /// </summary>
    /// <typeparam name="TInstanceType">The actual cache refresher type.</typeparam>
    /// <remarks>The actual cache refresher type is used for strongly typed events.</remarks>
    public abstract class JsonCacheRefresherBase<TNotification, TJsonPayload> : CacheRefresherBase<TNotification>, IJsonCacheRefresher
        where TNotification : CacheRefresherNotificationBase, new()
    {
        protected IJsonSerializer JsonSerializer { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonCacheRefresherBase{TInstanceType}"/>.
        /// </summary>
        /// <param name="appCaches">A cache helper.</param>
        protected JsonCacheRefresherBase(
            AppCaches appCaches,
            IJsonSerializer jsonSerializer,
            IEventAggregator eventAggregator)
            : base(appCaches, eventAggregator)
        {
            JsonSerializer = jsonSerializer;
        }

        /// <summary>
        /// Refreshes as specified by a json payload.
        /// </summary>
        /// <param name="json">The json payload.</param>
        public virtual void Refresh(string json)
        {
            OnCacheUpdated(new TNotification().Init(json, MessageType.RefreshByJson));
        }

        #region Json
        /// <summary>
        /// Deserializes a json payload into an object payload.
        /// </summary>
        /// <param name="json">The json payload.</param>
        /// <returns>The deserialized object payload.</returns>
        public TJsonPayload[] Deserialize(string json)
        {
            return JsonSerializer.Deserialize<TJsonPayload[]>(json);
        }


        public string Serialize(params TJsonPayload[] jsonPayloads)
        {
            return JsonSerializer.Serialize(jsonPayloads);
        }
        #endregion

    }
}
