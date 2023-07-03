using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Cache;

public sealed class ValueEditorCacheRefresher : PayloadCacheRefresherBase<DataTypeCacheRefresherNotification,
    DataTypeCacheRefresher.JsonPayload>
{
    public static readonly Guid UniqueId = Guid.Parse("D28A1DBB-2308-4918-9A92-2F8689B6CBFE");
    private readonly IValueEditorCache _valueEditorCache;

    public ValueEditorCacheRefresher(
        AppCaches appCaches,
        IJsonSerializer serializer,
        IEventAggregator eventAggregator,
        ICacheRefresherNotificationFactory factory,
        IValueEditorCache valueEditorCache)
        : base(appCaches, serializer, eventAggregator, factory) =>
        _valueEditorCache = valueEditorCache;

    public override Guid RefresherUniqueId => UniqueId;

    public override string Name => "ValueEditorCacheRefresher";

    public override void Refresh(DataTypeCacheRefresher.JsonPayload[] payloads)
    {
        IEnumerable<int> ids = payloads.Select(x => x.Id);
        _valueEditorCache.ClearCache(ids);
    }

    // these events should never trigger
    // everything should be PAYLOAD/JSON
    public override void RefreshAll() => throw new NotSupportedException();

    public override void Refresh(int id) => throw new NotSupportedException();

    public override void Refresh(Guid id) => throw new NotSupportedException();

    public override void Remove(int id) => throw new NotSupportedException();
}
