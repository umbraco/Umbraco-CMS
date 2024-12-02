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

public sealed class MediaCacheRefresher : PayloadCacheRefresherBase<MediaCacheRefresherNotification, MediaCacheRefresher.JsonPayload>
{
    private readonly IIdKeyMap _idKeyMap;
    private readonly IMediaNavigationQueryService _mediaNavigationQueryService;
    private readonly IMediaNavigationManagementService _mediaNavigationManagementService;
    private readonly IMediaService _mediaService;
    private readonly IMediaCacheService _mediaCacheService;

    public MediaCacheRefresher(
        AppCaches appCaches,
        IJsonSerializer serializer,
        IIdKeyMap idKeyMap,
        IEventAggregator eventAggregator,
        ICacheRefresherNotificationFactory factory,
        IMediaNavigationQueryService mediaNavigationQueryService,
        IMediaNavigationManagementService mediaNavigationManagementService,
        IMediaService mediaService,
        IMediaCacheService mediaCacheService)
        : base(appCaches, serializer, eventAggregator, factory)
    {
        _idKeyMap = idKeyMap;
        _mediaNavigationQueryService = mediaNavigationQueryService;
        _mediaNavigationManagementService = mediaNavigationManagementService;
        _mediaService = mediaService;
        _mediaCacheService = mediaCacheService;
    }

    #region Indirect

    public static void RefreshMediaTypes(AppCaches appCaches) => appCaches.IsolatedCaches.ClearCache<IMedia>();

    #endregion

    #region Json

    public class JsonPayload
    {
        public JsonPayload(int id, Guid? key, TreeChangeTypes changeTypes)
        {
            Id = id;
            Key = key;
            ChangeTypes = changeTypes;
        }

        public int Id { get; }

        public Guid? Key { get; }

        public TreeChangeTypes ChangeTypes { get; }
    }

    #endregion

    #region Define

    public static readonly Guid UniqueId = Guid.Parse("B29286DD-2D40-4DDB-B325-681226589FEC");

    public override Guid RefresherUniqueId => UniqueId;

    public override string Name => "Media Cache Refresher";

    #endregion

    #region Refresher

    public override void Refresh(JsonPayload[]? payloads)
    {
        if (payloads == null)
        {
            return;
        }

        // actions that always need to happen
        AppCaches.RuntimeCache.ClearByKey(CacheKeys.MediaRecycleBinCacheKey);
        Attempt<IAppPolicyCache?> mediaCache = AppCaches.IsolatedCaches.Get<IMedia>();

        foreach (JsonPayload payload in payloads)
        {
            if (payload.ChangeTypes == TreeChangeTypes.Remove)
            {
                _idKeyMap.ClearCache(payload.Id);
            }

            if (mediaCache.Success)
            {
                // repository cache
                // it *was* done for each pathId but really that does not make sense
                // only need to do it for the current media
                mediaCache.Result?.Clear(RepositoryCacheKeys.GetKey<IMedia, int>(payload.Id));
                mediaCache.Result?.Clear(RepositoryCacheKeys.GetKey<IMedia, Guid?>(payload.Key));

                // remove those that are in the branch
                if (payload.ChangeTypes.HasTypesAny(TreeChangeTypes.RefreshBranch | TreeChangeTypes.Remove))
                {
                    var pathid = "," + payload.Id + ",";
                    mediaCache.Result?.ClearOfType<IMedia>((_, v) => v.Path?.Contains(pathid) ?? false);
                }
            }

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
            _mediaNavigationManagementService.RebuildAsync();
            _mediaNavigationManagementService.RebuildBinAsync();
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
    public override void RefreshAll() => throw new NotSupportedException();

    public override void Refresh(int id) => throw new NotSupportedException();

    public override void Refresh(Guid id) => throw new NotSupportedException();

    public override void Remove(int id) => throw new NotSupportedException();

    #endregion
}
