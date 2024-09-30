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
    private readonly IPublishedSnapshotService _publishedSnapshotService;

    public MediaCacheRefresher(
        AppCaches appCaches,
        IJsonSerializer serializer,
        IPublishedSnapshotService publishedSnapshotService,
        IIdKeyMap idKeyMap,
        IEventAggregator eventAggregator,
        ICacheRefresherNotificationFactory factory,
        IMediaNavigationQueryService mediaNavigationQueryService,
        IMediaNavigationManagementService mediaNavigationManagementService,
        IMediaService mediaService)
        : base(appCaches, serializer, eventAggregator, factory)
    {
        _publishedSnapshotService = publishedSnapshotService;
        _idKeyMap = idKeyMap;
        _mediaNavigationQueryService = mediaNavigationQueryService;
        _mediaNavigationManagementService = mediaNavigationManagementService;
        _mediaService = mediaService;
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
            HandleNavigation(payload);

        }

        _publishedSnapshotService.Notify(payloads, out var hasPublishedDataChanged);
        // we only need to clear this if the published cache has changed
        if (hasPublishedDataChanged)
        {
            AppCaches.ClearPartialViewCache();
        }


        base.Refresh(payloads);
    }

    private void HandleNavigation(JsonPayload payload)
    {
        if (payload.Key is null)
        {
            return;
        }

        if(payload.ChangeTypes.HasType(TreeChangeTypes.Remove))
        {
            _mediaNavigationManagementService.MoveToBin(payload.Key.Value);
            _mediaNavigationManagementService.RemoveFromBin(payload.Key.Value);
        }
        if(payload.ChangeTypes.HasType(TreeChangeTypes.RefreshAll))
        {
            _mediaNavigationManagementService.RebuildAsync();
            _mediaNavigationManagementService.RebuildBinAsync();
        }
        if(payload.ChangeTypes.HasType(TreeChangeTypes.RefreshNode))
        {
            IMedia? content = _mediaService.GetById(payload.Id);

            if (content is null)
            {
                return;
            }

            HandleNavigationForSingleContent(content);
        }

        if (payload.ChangeTypes.HasType(TreeChangeTypes.RefreshBranch))
        {
            IMedia? content = _mediaService.GetById(payload.Id);

            if (content is null)
            {
                return;
            }

            IEnumerable<IMedia> descendants = _mediaService.GetPagedDescendants(content.Id, 0, int.MaxValue, out _);
            foreach (IMedia descendant in content.Yield().Concat(descendants))
            {
                HandleNavigationForSingleContent(descendant);
            }
        }
    }

    private void HandleNavigationForSingleContent(IMedia media)
    {

        //First creation
        if(ExistsInNavigation(media.Key) is false && ExistsInNavigationBin(media.Key) is false)
        {
            _mediaNavigationManagementService.Add(media.Key, GetParentKey(media));
            if (media.Trashed)
            {
                // If created as trashed, move to bin
                _mediaNavigationManagementService.MoveToBin(media.Key);
            }
        }else if(ExistsInNavigation(media.Key) is true && ExistsInNavigationBin(media.Key) is false)
        {
            if (media.Trashed)
            {
                // It must have been trashed
                _mediaNavigationManagementService.MoveToBin(media.Key);
            }
            else
            {
                // it most have been saved. Check if parent is different
                if (_mediaNavigationQueryService.TryGetParentKey(media.Key, out var oldParentKey))
                {
                    Guid? newParentKey = GetParentKey(media);
                    if (oldParentKey != newParentKey)
                    {
                        _mediaNavigationManagementService.Move(media.Key, newParentKey);
                    }
                }
            }
        }
        else if (ExistsInNavigation(media.Key) is false && ExistsInNavigationBin(media.Key) is true)
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
