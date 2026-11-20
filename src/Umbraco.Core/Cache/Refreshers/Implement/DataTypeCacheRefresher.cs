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
    private readonly IElementCacheService _elementCacheService;
    private readonly IContentTypeCommonRepository _contentTypeCommonRepository;
    private readonly IDataTypeConfigurationCache _dataTypeConfigurationCache;

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
    /// <param name="dataTypeConfigurationCache">The data type configuration cache.</param>
    /// <param name="elementCacheService">The element cache service.</param>
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
        IContentTypeCommonRepository contentTypeCommonRepository,
        IDataTypeConfigurationCache dataTypeConfigurationCache,
        IElementCacheService elementCacheService)
        : base(appCaches, serializer, eventAggregator, factory)
    {
        _idKeyMap = idKeyMap;
        _publishedModelFactory = publishedModelFactory;
        _publishedContentTypeFactory = publishedContentTypeFactory;
        _publishedContentTypeCache = publishedContentTypeCache;
        _documentCacheService = documentCacheService;
        _mediaCacheService = mediaCacheService;
        _contentTypeCommonRepository = contentTypeCommonRepository;
        _dataTypeConfigurationCache = dataTypeConfigurationCache;
        _elementCacheService = elementCacheService;
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
    /// <param name="contentTypeCommonRepository">The content type common repository.</param>
    /// <param name="dataTypeConfigurationCache">The data type configuration cache.</param>
    [Obsolete("Use the constructor with all parameters. Scheduled for removal in Umbraco 20.")]
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
        IContentTypeCommonRepository contentTypeCommonRepository,
        IDataTypeConfigurationCache dataTypeConfigurationCache)
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
            contentTypeCommonRepository,
            dataTypeConfigurationCache,
            StaticServiceProvider.Instance.GetRequiredService<IElementCacheService>())
    {
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
    /// <param name="contentTypeCommonRepository">The content type common repository.</param>
    [Obsolete("Use the constructor with all parameters. Scheduled for removal in Umbraco 19.")]
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
            contentTypeCommonRepository,
            StaticServiceProvider.Instance.GetRequiredService<IDataTypeConfigurationCache>(),
            StaticServiceProvider.Instance.GetRequiredService<IElementCacheService>())
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

        _dataTypeConfigurationCache.ClearCache(payloads.Select(x => x.Key));

        Attempt<IAppPolicyCache?> dataTypeCache = AppCaches.IsolatedCaches.Get<IDataType>();

        foreach (JsonPayload payload in payloads)
        {
            _idKeyMap.ClearCache(payload.Id);

            if (dataTypeCache.Success)
            {
                dataTypeCache.Result?.Clear(RepositoryCacheKeys.GetKey<IDataType, int>(payload.Id));
                dataTypeCache.Result?.Clear(RepositoryCacheKeys.GetGuidKey<IDataType>(payload.Key));
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
            var documentTypeIds = removedContentTypes
                .Where(x => x.ItemType == PublishedItemType.Content)
                .Select(x => x.Id)
                .ToArray();

            var mediaTypeIds = removedContentTypes
                .Where(x => x.ItemType == PublishedItemType.Media)
                .Select(x => x.Id)
                .ToArray();

            var elementTypeIds = removedContentTypes
                .Where(x => x.ItemType == PublishedItemType.Element)
                .Select(x => x.Id)
                .ToArray();

            if (documentTypeIds.Length > 0)
            {
                _documentCacheService.RebuildMemoryCacheByContentTypeAsync(documentTypeIds).GetAwaiter().GetResult();
            }

            if (mediaTypeIds.Length > 0)
            {
                _mediaCacheService.RebuildMemoryCacheByContentTypeAsync(mediaTypeIds).GetAwaiter().GetResult();
            }

            if (elementTypeIds.Length > 0)
            {
                _elementCacheService.RebuildMemoryCacheByContentTypeAsync(elementTypeIds).GetAwaiter().GetResult();
            }

            // In auto models builder mode (InMemoryAuto), the factory reset above invalidates ALL compiled
            // model types, whichever types (if any) use the changed data types. Converted items of every kind
            // can hold instances of them (e.g. Model.Parent<T>(), or a document's block items built from an
            // element type), so every converted content cache is cleared, not just those of the rebuilt types.
            if (_publishedModelFactory is IAutoPublishedModelFactory)
            {
                _documentCacheService.ClearConvertedContentCache();
                _mediaCacheService.ClearConvertedContentCache();
                _elementCacheService.ClearConvertedContentCache();
            }
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
