using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     Cache refresher for value editor caches.
/// </summary>
public sealed class ValueEditorCacheRefresher : PayloadCacheRefresherBase<DataTypeCacheRefresherNotification,
    DataTypeCacheRefresher.JsonPayload>
{
    /// <summary>
    ///     The unique identifier for this cache refresher.
    /// </summary>
    public static readonly Guid UniqueId = Guid.Parse("D28A1DBB-2308-4918-9A92-2F8689B6CBFE");
    private readonly IValueEditorCache _valueEditorCache;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ValueEditorCacheRefresher" /> class.
    /// </summary>
    /// <param name="appCaches">The application caches.</param>
    /// <param name="serializer">The JSON serializer.</param>
    /// <param name="eventAggregator">The event aggregator.</param>
    /// <param name="factory">The notification factory.</param>
    /// <param name="valueEditorCache">The value editor cache.</param>
    public ValueEditorCacheRefresher(
        AppCaches appCaches,
        IJsonSerializer serializer,
        IEventAggregator eventAggregator,
        ICacheRefresherNotificationFactory factory,
        IValueEditorCache valueEditorCache)
        : base(appCaches, serializer, eventAggregator, factory) =>
        _valueEditorCache = valueEditorCache;

    /// <inheritdoc />
    public override Guid RefresherUniqueId => UniqueId;

    /// <inheritdoc />
    public override string Name => "ValueEditorCacheRefresher";

    /// <inheritdoc />
    public override void RefreshInternal(DataTypeCacheRefresher.JsonPayload[] payloads)
    {
        IEnumerable<int> ids = payloads.Select(x => x.Id);
        _valueEditorCache.ClearCache(ids);
        base.RefreshInternal(payloads);
    }

    // these events should never trigger
    // everything should be PAYLOAD/JSON

    /// <inheritdoc />
    public override void RefreshAll() => throw new NotSupportedException();

    /// <inheritdoc />
    public override void Refresh(int id) => throw new NotSupportedException();

    /// <inheritdoc />
    public override void Refresh(Guid id) => throw new NotSupportedException();

    /// <inheritdoc />
    public override void Remove(int id) => throw new NotSupportedException();
}
