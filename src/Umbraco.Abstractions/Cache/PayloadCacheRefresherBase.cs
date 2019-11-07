using System;
using Umbraco.Core.Serialization;
using Umbraco.Core.Sync;

namespace Umbraco.Core.Cache
{
    /// <summary>
    /// A base class for "payload" class refreshers.
    /// </summary>
    /// <typeparam name="TInstanceType">The actual cache refresher type.</typeparam>
    /// <typeparam name="TPayload">The payload type.</typeparam>
    /// <remarks>The actual cache refresher type is used for strongly typed events.</remarks>
    public abstract class PayloadCacheRefresherBase<TInstanceType, TPayload> : JsonCacheRefresherBase<TInstanceType>, IPayloadCacheRefresher<TPayload>
        where TInstanceType : class, ICacheRefresher
    {
        private readonly IJsonSerializer _serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadCacheRefresherBase{TInstanceType, TPayload}"/>.
        /// </summary>
        /// <param name="appCaches">A cache helper.</param>
        /// <param name="serializer"></param>
        protected PayloadCacheRefresherBase(AppCaches appCaches, IJsonSerializer serializer) : base(appCaches)
        {
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        }

        #region Json

        /// <summary>
        /// Deserializes a json payload into an object payload.
        /// </summary>
        /// <param name="json">The json payload.</param>
        /// <returns>The deserialized object payload.</returns>
        protected virtual TPayload[] Deserialize(string json)
        {
            return _serializer.Deserialize<TPayload[]>(json);
        }

        #endregion

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
            OnCacheUpdated(This, new CacheRefresherEventArgs(payloads, MessageType.RefreshByPayload));
        }

        #endregion
    }
}
