using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Cache;

public sealed class DataTypeCacheRefresher : PayloadCacheRefresherBase<DataTypeCacheRefresherNotification, DataTypeCacheRefresher.JsonPayload>
{
    private readonly IIdKeyMap _idKeyMap;
    private readonly IPublishedModelFactory _publishedModelFactory;
    private readonly IPublishedSnapshotService _publishedSnapshotService;

    public DataTypeCacheRefresher(
        AppCaches appCaches,
        IJsonSerializer serializer,
        IPublishedSnapshotService publishedSnapshotService,
        IPublishedModelFactory publishedModelFactory,
        IIdKeyMap idKeyMap,
        IEventAggregator eventAggregator,
        ICacheRefresherNotificationFactory factory)
        : base(appCaches, serializer, eventAggregator, factory)
    {
        _publishedSnapshotService = publishedSnapshotService;
        _publishedModelFactory = publishedModelFactory;
        _idKeyMap = idKeyMap;
    }

    #region Json

    public class JsonPayload
    {
        public JsonPayload(int id, Guid key, bool removed)
        {
            Id = id;
            Key = key;
            Removed = removed;
        }

        public int Id { get; }

        public Guid Key { get; }

        public bool Removed { get; }
    }

    #endregion

    #region Define

    public static readonly Guid UniqueId = Guid.Parse("35B16C25-A17E-45D7-BC8F-EDAB1DCC28D2");

    public override Guid RefresherUniqueId => UniqueId;

    public override string Name => "Data Type Cache Refresher";

    #endregion

    #region Refresher

    public override void Refresh(JsonPayload[] payloads)
    {
        // we need to clear the ContentType runtime cache since that is what caches the
        // db data type to store the value against and anytime a datatype changes, this also might change
        // we basically need to clear all sorts of runtime caches here because so many things depend upon a data type
        ClearAllIsolatedCacheByEntityType<IContent>();
        ClearAllIsolatedCacheByEntityType<IContentType>();
        ClearAllIsolatedCacheByEntityType<IMedia>();
        ClearAllIsolatedCacheByEntityType<IMediaType>();
        ClearAllIsolatedCacheByEntityType<IMember>();
        ClearAllIsolatedCacheByEntityType<IMemberType>();

        Attempt<IAppPolicyCache?> dataTypeCache = AppCaches.IsolatedCaches.Get<IDataType>();

        foreach (JsonPayload payload in payloads)
        {
            _idKeyMap.ClearCache(payload.Id);

            if (dataTypeCache.Success)
            {
                dataTypeCache.Result?.Clear(RepositoryCacheKeys.GetKey<IDataType, int>(payload.Id));
            }
        }

        // TODO: not sure I like these?
        TagsValueConverter.ClearCaches();
        SliderValueConverter.ClearCaches();

        // refresh the models and cache
        _publishedModelFactory.WithSafeLiveFactoryReset(() =>
            _publishedSnapshotService.Notify(payloads));

        base.Refresh(payloads);
    }

    // these events should never trigger
    // everything should be PAYLOAD/JSON
    public override void RefreshAll() => throw new NotSupportedException();

    public override void Refresh(int id) => throw new NotSupportedException();

    public override void Refresh(Guid id) => throw new NotSupportedException();

    public override void Remove(int id) => throw new NotSupportedException();

    #endregion
}
