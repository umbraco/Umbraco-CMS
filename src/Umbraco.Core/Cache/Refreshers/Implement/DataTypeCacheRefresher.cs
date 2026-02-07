using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     Cache refresher for data type caches.
/// </summary>
public sealed class DataTypeCacheRefresher : PayloadCacheRefresherBase<DataTypeCacheRefresherNotification, DataTypeCacheRefresher.JsonPayload>
{
    private readonly IIdKeyMap _idKeyMap;
    private readonly IPublishedModelFactory _publishedModelFactory;
    private readonly IPublishedContentTypeFactory _publishedContentTypeFactory;
    private readonly IPublishedContentTypeCache _publishedContentTypeCache;
    private readonly IDocumentCacheService _documentCacheService;
    private readonly IMediaCacheService _mediaCacheService;
    private readonly IContentTypeCommonRepository _contentTypeCommonRepository;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DataTypeCacheRefresher" /> class.
    /// </summary>
    /// <param name="appCaches">The application caches.</param>
    /// <param name="serializer">The JSON serializer.</param>
    /// <param name="idKeyMap">The ID-key mapping service.</param>
    /// <param name="eventAggregator">The event aggregator.</param>
    /// <param name="factory">The cache refresher notification factory.</param>
    /// <param name="publishedModelFactory">The published model factory.</param>
    /// <param name="publishedContentTypeFactory">The published content type factory.</param>
    /// <param name="publishedContentTypeCache">The published content type cache.</param>
    /// <param name="documentCacheService">The document cache service.</param>
    /// <param name="mediaCacheService">The media cache service.</param>
    /// <param name="contentTypeCommonRepository">The content type common repository.</param>
    public DataTypeCacheRefresher(
        AppCaches appCaches,
        IJsonSerializer serializer,
        IIdKeyMap idKeyMap,
        IEventAggregator eventAggregator,
        ICacheRefresherNotificationFactory factory,
        IPublishedModelFactory publishedModelFactory,
        IPublishedContentTypeFactory publishedContentTypeFactory,
        IPublishedContentTypeCache publishedContentTypeCache,
        IDocumentCacheService documentCacheService,
        IMediaCacheService mediaCacheService,
        IContentTypeCommonRepository contentTypeCommonRepository)
        : base(appCaches, serializer, eventAggregator, factory)
    {
        _idKeyMap = idKeyMap;
        _publishedModelFactory = publishedModelFactory;
        _publishedContentTypeFactory = publishedContentTypeFactory;
        _publishedContentTypeCache = publishedContentTypeCache;
        _documentCacheService = documentCacheService;
        _mediaCacheService = mediaCacheService;
        _contentTypeCommonRepository = contentTypeCommonRepository;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DataTypeCacheRefresher" /> class.
    /// </summary>
    /// <param name="appCaches">The application caches.</param>
    /// <param name="serializer">The JSON serializer.</param>
    /// <param name="idKeyMap">The ID-key mapping service.</param>
    /// <param name="eventAggregator">The event aggregator.</param>
    /// <param name="factory">The cache refresher notification factory.</param>
    /// <param name="publishedModelFactory">The published model factory.</param>
    /// <param name="publishedContentTypeFactory">The published content type factory.</param>
    /// <param name="publishedContentTypeCache">The published content type cache.</param>
    /// <param name="documentCacheService">The document cache service.</param>
    /// <param name="mediaCacheService">The media cache service.</param>
    [Obsolete("Use the non-obsolete constructor instead. Scheduled for removal in V18.")]
    public DataTypeCacheRefresher(
        AppCaches appCaches,
        IJsonSerializer serializer,
        IIdKeyMap idKeyMap,
        IEventAggregator eventAggregator,
        ICacheRefresherNotificationFactory factory,
        IPublishedModelFactory publishedModelFactory,
        IPublishedContentTypeFactory publishedContentTypeFactory,
        IPublishedContentTypeCache publishedContentTypeCache,
        IDocumentCacheService documentCacheService,
        IMediaCacheService mediaCacheService)
        : this(
            appCaches,
            serializer,
            idKeyMap,
            eventAggregator,
            factory,
            publishedModelFactory,
            publishedContentTypeFactory,
            publishedContentTypeCache,
            documentCacheService,
            mediaCacheService,
            StaticServiceProvider.Instance.GetRequiredService<IContentTypeCommonRepository>())
    {
    }

    #region Json

    /// <summary>
    ///     Represents the JSON payload for data type cache refresh operations.
    /// </summary>
    public class JsonPayload
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="JsonPayload" /> class.
        /// </summary>
        /// <param name="id">The identifier of the data type.</param>
        /// <param name="key">The unique key of the data type.</param>
        /// <param name="removed">Whether the data type was removed.</param>
        public JsonPayload(int id, Guid key, bool removed)
        {
            Id = id;
            Key = key;
            Removed = removed;
        }

        /// <summary>
        ///     Gets the identifier of the data type.
        /// </summary>
        public int Id { get; }

        /// <summary>
        ///     Gets the unique key of the data type.
        /// </summary>
        public Guid Key { get; }

        /// <summary>
        ///     Gets a value indicating whether the data type was removed.
        /// </summary>
        public bool Removed { get; }
    }

    #endregion

    #region Define

    /// <summary>
    ///     The unique identifier for this cache refresher.
    /// </summary>
    public static readonly Guid UniqueId = Guid.Parse("35B16C25-A17E-45D7-BC8F-EDAB1DCC28D2");

    /// <inheritdoc />
    public override Guid RefresherUniqueId => UniqueId;

    /// <inheritdoc />
    public override string Name => "Data Type Cache Refresher";

    #endregion

    #region Refresher

    /// <inheritdoc />
    public override void RefreshInternal(JsonPayload[] payloads)
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

        // Also clear the 5 minute runtime cache held in ContentTypeCommonRepository.
        _contentTypeCommonRepository.ClearCache();

        Attempt<IAppPolicyCache?> dataTypeCache = AppCaches.IsolatedCaches.Get<IDataType>();

        foreach (JsonPayload payload in payloads)
        {
            _idKeyMap.ClearCache(payload.Id);

            if (dataTypeCache.Success)
            {
                dataTypeCache.Result?.Clear(RepositoryCacheKeys.GetKey<IDataType, int>(payload.Id));
            }
        }

        base.RefreshInternal(payloads);
    }

    /// <inheritdoc />
    public override void Refresh(JsonPayload[] payloads)
    {
        List<IPublishedContentType> removedContentTypes = new();
        foreach (JsonPayload payload in payloads)
        {
            removedContentTypes.AddRange(_publishedContentTypeCache.ClearByDataTypeId(payload.Id));
        }

        var changedIds = Array.ConvertAll(payloads, x => x.Id);
        _publishedContentTypeFactory.NotifyDataTypeChanges(changedIds);

        _publishedModelFactory.WithSafeLiveFactoryReset(() =>
        {
            IEnumerable<int> documentTypeIds = removedContentTypes
                .Where(x => x.ItemType == PublishedItemType.Content)
                .Select(x => x.Id);
            _documentCacheService.RebuildMemoryCacheByContentTypeAsync(documentTypeIds).GetAwaiter().GetResult();

            IEnumerable<int> mediaTypeIds = removedContentTypes
                .Where(x => x.ItemType == PublishedItemType.Media)
                .Select(x => x.Id);
            _mediaCacheService.RebuildMemoryCacheByContentTypeAsync(mediaTypeIds).GetAwaiter().GetResult();
        });
        base.Refresh(payloads);
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

    #endregion
}
