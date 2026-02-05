using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     Cache refresher for content type, media type, and member type caches.
/// </summary>
public sealed class ContentTypeCacheRefresher : PayloadCacheRefresherBase<ContentTypeCacheRefresherNotification, ContentTypeCacheRefresher.JsonPayload>
{
    private readonly IContentTypeCommonRepository _contentTypeCommonRepository;
    private readonly IPublishedModelFactory _publishedModelFactory;
    private readonly IPublishedContentTypeFactory _publishedContentTypeFactory;
    private readonly IDocumentCacheService _documentCacheService;
    private readonly IPublishedContentTypeCache _publishedContentTypeCache;
    private readonly IMediaCacheService _mediaCacheService;
    private readonly IIdKeyMap _idKeyMap;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentTypeCacheRefresher" /> class.
    /// </summary>
    /// <param name="appCaches">The application caches.</param>
    /// <param name="serializer">The JSON serializer.</param>
    /// <param name="idKeyMap">The ID-key mapping service.</param>
    /// <param name="contentTypeCommonRepository">The content type common repository.</param>
    /// <param name="eventAggregator">The event aggregator.</param>
    /// <param name="factory">The cache refresher notification factory.</param>
    /// <param name="publishedModelFactory">The published model factory.</param>
    /// <param name="publishedContentTypeFactory">The published content type factory.</param>
    /// <param name="documentCacheService">The document cache service.</param>
    /// <param name="publishedContentTypeCache">The published content type cache.</param>
    /// <param name="mediaCacheService">The media cache service.</param>
    public ContentTypeCacheRefresher(
        AppCaches appCaches,
        IJsonSerializer serializer,
        IIdKeyMap idKeyMap,
        IContentTypeCommonRepository contentTypeCommonRepository,
        IEventAggregator eventAggregator,
        ICacheRefresherNotificationFactory factory,
        IPublishedModelFactory publishedModelFactory,
        IPublishedContentTypeFactory publishedContentTypeFactory,
        IDocumentCacheService documentCacheService,
        IPublishedContentTypeCache publishedContentTypeCache,
        IMediaCacheService mediaCacheService)
        : base(appCaches, serializer, eventAggregator, factory)
    {
        _idKeyMap = idKeyMap;
        _contentTypeCommonRepository = contentTypeCommonRepository;
        _publishedModelFactory = publishedModelFactory;
        _publishedContentTypeFactory = publishedContentTypeFactory;
        _documentCacheService = documentCacheService;
        _publishedContentTypeCache = publishedContentTypeCache;
        _mediaCacheService = mediaCacheService;
    }

    #region Json

    /// <summary>
    ///     Represents the JSON payload for content type cache refresh operations.
    /// </summary>
    public class JsonPayload
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="JsonPayload" /> class.
        /// </summary>
        /// <param name="itemType">The type name of the content type.</param>
        /// <param name="id">The identifier of the content type.</param>
        /// <param name="changeTypes">The types of changes that occurred.</param>
        public JsonPayload(string itemType, int id, ContentTypeChangeTypes changeTypes)
        {
            ItemType = itemType;
            Id = id;
            ChangeTypes = changeTypes;
        }

        /// <summary>
        ///     Gets the type name of the content type (e.g., IContentType, IMediaType, IMemberType).
        /// </summary>
        public string ItemType { get; }

        /// <summary>
        ///     Gets the identifier of the content type.
        /// </summary>
        public int Id { get; }

        /// <summary>
        ///     Gets the types of changes that occurred.
        /// </summary>
        public ContentTypeChangeTypes ChangeTypes { get; }
    }

    #endregion

    #region Define

    /// <summary>
    ///     The unique identifier for this cache refresher.
    /// </summary>
    public static readonly Guid UniqueId = Guid.Parse("6902E22C-9C10-483C-91F3-66B7CAE9E2F5");

    /// <inheritdoc />
    public override Guid RefresherUniqueId => UniqueId;

    /// <inheritdoc />
    public override string Name => "Content Type Cache Refresher";

    #endregion

    #region Refresher

    /// <inheritdoc />
    public override void RefreshInternal(JsonPayload[] payloads)
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

        base.RefreshInternal(payloads);
    }

    /// <inheritdoc />
    public override void Refresh(JsonPayload[] payloads)
    {
        _publishedContentTypeCache.ClearContentTypes(payloads.Select(x => x.Id));
        _publishedContentTypeFactory.NotifyDataTypeChanges();
        _publishedModelFactory.WithSafeLiveFactoryReset(() =>
        {
            IEnumerable<int> documentTypeIds = payloads.Where(x => x.ItemType == nameof(IContentType)).Select(x => x.Id);
            IEnumerable<int> mediaTypeIds = payloads.Where(x => x.ItemType == nameof(IMediaType)).Select(x => x.Id);

            _documentCacheService.RebuildMemoryCacheByContentTypeAsync(documentTypeIds).GetAwaiter().GetResult();
            _mediaCacheService.RebuildMemoryCacheByContentTypeAsync(mediaTypeIds).GetAwaiter().GetResult();
        });

        // now we can trigger the event
        base.Refresh(payloads);
    }

    /// <inheritdoc />
    public override void RefreshAll() => throw new NotSupportedException();

    /// <inheritdoc />
    public override void Refresh(int id) => throw new NotSupportedException();

    /// <inheritdoc />
    public override void Refresh(Guid id) => throw new NotSupportedException();

    /// <inheritdoc />
    public override void Remove(int id) => throw new NotSupportedException();

    #endregion
}
