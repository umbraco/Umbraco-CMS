using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     A base class for "payload" class refreshers.
/// </summary>
/// <typeparam name="TPayload">The payload type.</typeparam>
/// <typeparam name="TNotification">The notification type</typeparam>
/// <remarks>The actual cache refresher type is used for strongly typed events.</remarks>
public abstract class
    PayloadCacheRefresherBase<TNotification, TPayload> : JsonCacheRefresherBase<TNotification, TPayload>,
        IPayloadCacheRefresher<TPayload>
    where TNotification : CacheRefresherNotification
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PayloadCacheRefresherBase{TInstanceType, TPayload}" />.
    /// </summary>
    /// <param name="appCaches">A cache helper.</param>
    /// <param name="serializer"></param>
    /// <param name="eventAggregator"></param>
    /// <param name="factory"></param>
    protected PayloadCacheRefresherBase(AppCaches appCaches, IJsonSerializer serializer, IEventAggregator eventAggregator, ICacheRefresherNotificationFactory factory)
        : base(appCaches, serializer, eventAggregator, factory)
    {
    }

    #region Refresher

    public override void Refresh(string json)
    {
        TPayload[]? payload = Deserialize(json);
        if (payload is not null)
        {
            Refresh(payload);
        }
    }

    /// <summary>
    ///     Refreshes as specified by a payload.
    /// </summary>
    /// <param name="payloads">The payload.</param>
    public virtual void Refresh(TPayload[] payloads) =>
        OnCacheUpdated(NotificationFactory.Create<TNotification>(payloads, MessageType.RefreshByPayload));

    #endregion
}
