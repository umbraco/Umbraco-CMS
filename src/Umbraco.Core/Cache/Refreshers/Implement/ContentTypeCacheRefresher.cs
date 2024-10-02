using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Cache;

public sealed class ContentTypeCacheRefresher : PayloadCacheRefresherBase<ContentTypeCacheRefresherNotification, ContentTypeCacheRefresher.JsonPayload>
{
    private readonly IContentTypeCommonRepository _contentTypeCommonRepository;
    private readonly IPublishedModelFactory _publishedModelFactory;
    private readonly IIdKeyMap _idKeyMap;

    public ContentTypeCacheRefresher(
        AppCaches appCaches,
        IJsonSerializer serializer,
        IIdKeyMap idKeyMap,
        IContentTypeCommonRepository contentTypeCommonRepository,
        IEventAggregator eventAggregator,
        ICacheRefresherNotificationFactory factory,
        IPublishedModelFactory publishedModelFactory)
        : base(appCaches, serializer, eventAggregator, factory)
    {
        _idKeyMap = idKeyMap;
        _contentTypeCommonRepository = contentTypeCommonRepository;
        _publishedModelFactory = publishedModelFactory;
    }

    #region Json

    public class JsonPayload
    {
        public JsonPayload(string itemType, int id, ContentTypeChangeTypes changeTypes)
        {
            ItemType = itemType;
            Id = id;
            ChangeTypes = changeTypes;
        }

        public string ItemType { get; }

        public int Id { get; }

        public ContentTypeChangeTypes ChangeTypes { get; }
    }

    #endregion

    #region Define

    public static readonly Guid UniqueId = Guid.Parse("6902E22C-9C10-483C-91F3-66B7CAE9E2F5");

    public override Guid RefresherUniqueId => UniqueId;

    public override string Name => "Content Type Cache Refresher";

    #endregion

    #region Refresher

    public override void Refresh(JsonPayload[] payloads)
    {
        // TODO: refactor
        // we should NOT directly clear caches here, but instead ask whatever class
        // is managing the cache to please clear that cache properly
        _contentTypeCommonRepository.ClearCache(); // always

        if (payloads.Any(x => x.ItemType == typeof(IContentType).Name))
        {
            ClearAllIsolatedCacheByEntityType<IContent>();
            ClearAllIsolatedCacheByEntityType<IContentType>();
        }

        if (payloads.Any(x => x.ItemType == typeof(IMediaType).Name))
        {
            ClearAllIsolatedCacheByEntityType<IMedia>();
            ClearAllIsolatedCacheByEntityType<IMediaType>();
        }

        if (payloads.Any(x => x.ItemType == typeof(IMemberType).Name))
        {
            ClearAllIsolatedCacheByEntityType<IMember>();
            ClearAllIsolatedCacheByEntityType<IMemberType>();
        }

        foreach (var id in payloads.Select(x => x.Id))
        {
            _idKeyMap.ClearCache(id);
        }

        if (payloads.Any(x => x.ItemType == typeof(IContentType).Name))
        {
            // don't try to be clever - refresh all
            ContentCacheRefresher.RefreshContentTypes(AppCaches);
        }

        if (payloads.Any(x => x.ItemType == typeof(IMediaType).Name))
        {
            // don't try to be clever - refresh all
            MediaCacheRefresher.RefreshMediaTypes(AppCaches);
        }

        if (payloads.Any(x => x.ItemType == typeof(IMemberType).Name))
        {
            // don't try to be clever - refresh all
            MemberCacheRefresher.RefreshMemberTypes(AppCaches);
        }

        // TODO: We need to clear the HybridCache of any content using the ContentType, but NOT the database cache here, and this should be done within the "WithSafeLiveFactoryReset" to ensure that the factory is locked in the meantime.
        _publishedModelFactory.WithSafeLiveFactoryReset(() => { });

        // now we can trigger the event
        base.Refresh(payloads);
    }

    public override void RefreshAll() => throw new NotSupportedException();

    public override void Refresh(int id) => throw new NotSupportedException();

    public override void Refresh(Guid id) => throw new NotSupportedException();

    public override void Remove(int id) => throw new NotSupportedException();

    #endregion
}
