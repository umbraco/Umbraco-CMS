using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     Cache refresher for media caches.
/// </summary>
public sealed class MediaCacheRefresher : PayloadCacheRefresherBase<MediaCacheRefresherNotification, MediaCacheRefresher.JsonPayload>
{
    private readonly IIdKeyMap _idKeyMap;
    private readonly IMediaNavigationQueryService _mediaNavigationQueryService;
    private readonly IMediaNavigationManagementService _mediaNavigationManagementService;
    private readonly IMediaService _mediaService;
    private readonly IMediaCacheService _mediaCacheService;
    private readonly ICacheManager _cacheManager;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaCacheRefresher" /> class.
    /// </summary>
    /// <param name="appCaches">The application caches.</param>
    /// <param name="serializer">The JSON serializer.</param>
    /// <param name="idKeyMap">The ID-key mapping service.</param>
    /// <param name="eventAggregator">The event aggregator.</param>
    /// <param name="factory">The cache refresher notification factory.</param>
    /// <param name="mediaNavigationQueryService">The media navigation query service.</param>
    /// <param name="mediaNavigationManagementService">The media navigation management service.</param>
    /// <param name="mediaService">The media service.</param>
    /// <param name="mediaCacheService">The media cache service.</param>
    /// <param name="cacheManager">The cache manager.</param>
    public MediaCacheRefresher(
        AppCaches appCaches,
        IJsonSerializer serializer,
        IIdKeyMap idKeyMap,
        IEventAggregator eventAggregator,
        ICacheRefresherNotificationFactory factory,
        IMediaNavigationQueryService mediaNavigationQueryService,
        IMediaNavigationManagementService mediaNavigationManagementService,
        IMediaService mediaService,
        IMediaCacheService mediaCacheService,
        ICacheManager cacheManager)
        : base(appCaches, serializer, eventAggregator, factory)
    {
        _idKeyMap = idKeyMap;
        _mediaNavigationQueryService = mediaNavigationQueryService;
        _mediaNavigationManagementService = mediaNavigationManagementService;
        _mediaService = mediaService;
        _mediaCacheService = mediaCacheService;

        // TODO: Use IElementsCache instead of ICacheManager, see ContentCacheRefresher for more information.
        _cacheManager = cacheManager;
    }

    #region Indirect

    /// <summary>
    ///     Refreshes media type caches by clearing all cached media.
    /// </summary>
    /// <param name="appCaches">The application caches.</param>
    public static void RefreshMediaTypes(AppCaches appCaches) => appCaches.IsolatedCaches.ClearCache<IMedia>();

    #endregion

    #region Json

    /// <summary>
    ///     Represents the JSON payload for media cache refresh operations.
    /// </summary>
    public class JsonPayload
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="JsonPayload" /> class.
        /// </summary>
        /// <param name="id">The identifier of the media item.</param>
        /// <param name="key">The unique key of the media item.</param>
        /// <param name="changeTypes">The types of changes that occurred.</param>
        public JsonPayload(int id, Guid? key, TreeChangeTypes changeTypes)
        {
            Id = id;
            Key = key;
            ChangeTypes = changeTypes;
        }

        /// <summary>
        ///     Gets the identifier of the media item.
        /// </summary>
        public int Id { get; }

        /// <summary>
        ///     Gets the unique key of the media item.
        /// </summary>
        public Guid? Key { get; }

        /// <summary>
        ///     Gets the types of changes that occurred.
        /// </summary>
        public TreeChangeTypes ChangeTypes { get; }
    }

    #endregion

    #region Define

    /// <summary>
    ///     The unique identifier for this cache refresher.
    /// </summary>
    public static readonly Guid UniqueId = Guid.Parse("B29286DD-2D40-4DDB-B325-681226589FEC");

    /// <inheritdoc />
    public override Guid RefresherUniqueId => UniqueId;

    /// <inheritdoc />
    public override string Name => "Media Cache Refresher";

    #endregion

    #region Refresher

    /// <inheritdoc />
    public override void RefreshInternal(JsonPayload[] payloads)
    {
        // actions that always need to happen
        AppCaches.RuntimeCache.ClearByKey(CacheKeys.MediaRecycleBinCacheKey);
        Attempt<IAppPolicyCache?> mediaCache = AppCaches.IsolatedCaches.Get<IMedia>();

        // Ideally, we'd like to not have to clear the entire cache here. However, this was the existing behavior in NuCache.
        // The reason for this is that we have no way to know which elements are affected by the changes or what their keys are.
        // This is because currently published elements live exclusively in a JSON blob in the umbracoPropertyData table.
        // This means that the only way to resolve these keys is to actually parse this data with a specific value converter, and for all cultures, which is not possible.
        // If published elements become their own entities with relations, instead of just property data, we can revisit this.
        _cacheManager.ElementsCache.Clear();

        foreach (JsonPayload payload in payloads)
        {
            if (payload.ChangeTypes == TreeChangeTypes.Remove)
            {
                _idKeyMap.ClearCache(payload.Id);
            }

            if (mediaCache.Success is false || mediaCache.Result is null)
            {
                continue;
            }

            // repository cache
            // it *was* done for each pathId but really that does not make sense
            // only need to do it for the current media
            mediaCache.Result.Clear(RepositoryCacheKeys.GetKey<IMedia, int>(payload.Id));
            mediaCache.Result.Clear(RepositoryCacheKeys.GetKey<IMedia, Guid?>(payload.Key));

            // remove those that are in the branch
            if (payload.ChangeTypes.HasTypesAny(TreeChangeTypes.RefreshBranch | TreeChangeTypes.Remove))
            {
                var pathid = "," + payload.Id + ",";
                mediaCache.Result.ClearOfType<IMedia>((_, v) => v.Path?.Contains(pathid) ?? false);
            }
        }

        base.RefreshInternal(payloads);
    }

    /// <inheritdoc />
    public override void Refresh(JsonPayload[]? payloads)
    {
        if (payloads is null)
        {
            return;
        }

        foreach (JsonPayload payload in payloads)
        {
            HandleMemoryCache(payload);
            HandleNavigation(payload);
        }

        AppCaches.ClearPartialViewCache();


        base.Refresh(payloads);
    }

    private void HandleMemoryCache(JsonPayload payload)
    {
        Guid key = payload.Key ?? _idKeyMap.GetKeyForId(payload.Id, UmbracoObjectTypes.Document).Result;


        if (payload.ChangeTypes.HasType(TreeChangeTypes.RefreshNode))
        {
            _mediaCacheService.RefreshMemoryCacheAsync(key).GetAwaiter().GetResult();
        }

        if (payload.ChangeTypes.HasType(TreeChangeTypes.RefreshBranch))
        {
            if (_mediaNavigationQueryService.TryGetDescendantsKeys(key, out IEnumerable<Guid> descendantsKeys))
            {
                var branchKeys = descendantsKeys.ToList();
                branchKeys.Add(key);

                foreach (Guid branchKey in branchKeys)
                {
                    _mediaCacheService.RefreshMemoryCacheAsync(branchKey).GetAwaiter().GetResult();
                }
            }
        }

        if (payload.ChangeTypes.HasType(TreeChangeTypes.RefreshAll))
        {
            _mediaCacheService.ClearMemoryCacheAsync(CancellationToken.None).GetAwaiter().GetResult();
        }

        if (payload.ChangeTypes.HasType(TreeChangeTypes.Remove))
        {
            _mediaCacheService.RemoveFromMemoryCacheAsync(key).GetAwaiter().GetResult();
        }
    }

    private void HandleNavigation(JsonPayload payload)
    {
        if (payload.Key is null)
        {
            return;
        }

        if (payload.ChangeTypes.HasType(TreeChangeTypes.Remove))
        {
            _mediaNavigationManagementService.MoveToBin(payload.Key.Value);
            _mediaNavigationManagementService.RemoveFromBin(payload.Key.Value);
        }

        if (payload.ChangeTypes.HasType(TreeChangeTypes.RefreshAll))
        {
            _mediaNavigationManagementService.RebuildAsync().GetAwaiter().GetResult();
            _mediaNavigationManagementService.RebuildBinAsync().GetAwaiter().GetResult();
        }

        if (payload.ChangeTypes.HasType(TreeChangeTypes.RefreshNode))
        {
            IMedia? media = _mediaService.GetById(payload.Id);

            if (media is null)
            {
                return;
            }

            HandleNavigationForSingleMedia(media);
        }

        if (payload.ChangeTypes.HasType(TreeChangeTypes.RefreshBranch))
        {
            IMedia? media = _mediaService.GetById(payload.Id);

            if (media is null)
            {
                return;
            }

            IEnumerable<IMedia> descendants = _mediaService.GetPagedDescendants(media.Id, 0, int.MaxValue, out _);
            foreach (IMedia descendant in media.Yield().Concat(descendants))
            {
                HandleNavigationForSingleMedia(descendant);
            }
        }
    }

    private void HandleNavigationForSingleMedia(IMedia media)
    {
        // First creation
        if (ExistsInNavigation(media.Key) is false && ExistsInNavigationBin(media.Key) is false)
        {
            _mediaNavigationManagementService.Add(media.Key, media.ContentType.Key, GetParentKey(media), media.SortOrder);
            if (media.Trashed)
            {
                // If created as trashed, move to bin
                _mediaNavigationManagementService.MoveToBin(media.Key);
            }
        }
        else if (ExistsInNavigation(media.Key) && ExistsInNavigationBin(media.Key) is false)
        {
            if (media.Trashed)
            {
                // It must have been trashed
                _mediaNavigationManagementService.MoveToBin(media.Key);
            }
            else
            {
                if (_mediaNavigationQueryService.TryGetParentKey(media.Key, out var oldParentKey) is false)
                {
                    return;
                }

                // It must have been saved. Check if parent is different
                Guid? newParentKey = GetParentKey(media);
                if (oldParentKey != newParentKey)
                {
                    _mediaNavigationManagementService.Move(media.Key, newParentKey);
                }
                else
                {
                    _mediaNavigationManagementService.UpdateSortOrder(media.Key, media.SortOrder);
                }
            }
        }
        else if (ExistsInNavigation(media.Key) is false && ExistsInNavigationBin(media.Key))
        {
            if (media.Trashed is false)
            {
                // It must have been restored
                _mediaNavigationManagementService.RestoreFromBin(media.Key, GetParentKey(media));
            }
        }
    }

    private Guid? GetParentKey(IMedia media) => (media.ParentId == -1) ? null : _idKeyMap.GetKeyForId(media.ParentId, UmbracoObjectTypes.Media).Result;

    private bool ExistsInNavigation(Guid contentKey) => _mediaNavigationQueryService.TryGetParentKey(contentKey, out _);

    private bool ExistsInNavigationBin(Guid contentKey) => _mediaNavigationQueryService.TryGetParentKeyInBin(contentKey, out _);


    // these events should never trigger
    // everything should be JSON

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
