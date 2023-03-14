using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     A base class for "json" cache refreshers.
/// </summary>
/// <remarks>The actual cache refresher type is used for strongly typed events.</remarks>
public abstract class JsonCacheRefresherBase<TNotification, TJsonPayload> : CacheRefresherBase<TNotification>,
    IJsonCacheRefresher
    where TNotification : CacheRefresherNotification
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="JsonCacheRefresherBase{TInstanceType}" />.
    /// </summary>
    protected JsonCacheRefresherBase(
        AppCaches appCaches,
        IJsonSerializer jsonSerializer,
        IEventAggregator eventAggregator,
        ICacheRefresherNotificationFactory factory)
        : base(appCaches, eventAggregator, factory) =>
        JsonSerializer = jsonSerializer;

    protected IJsonSerializer JsonSerializer { get; }

    /// <summary>
    ///     Refreshes as specified by a json payload.
    /// </summary>
    /// <param name="json">The json payload.</param>
    public virtual void Refresh(string json) =>
        OnCacheUpdated(NotificationFactory.Create<TNotification>(json, MessageType.RefreshByJson));

    #region Json

    /// <summary>
    ///     Deserializes a json payload into an object payload.
    /// </summary>
    /// <param name="json">The json payload.</param>
    /// <returns>The deserialized object payload.</returns>
    public TJsonPayload[]? Deserialize(string json) => JsonSerializer.Deserialize<TJsonPayload[]>(json);

    public string Serialize(params TJsonPayload[] jsonPayloads) => JsonSerializer.Serialize(jsonPayloads);

    #endregion
}
