using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Cache;

public sealed class MediaCacheRefresher : PayloadCacheRefresherBase<MediaCacheRefresherNotification, MediaCacheRefresher.JsonPayload>
{
    private readonly IIdKeyMap _idKeyMap;
    private readonly IPublishedSnapshotService _publishedSnapshotService;

    public MediaCacheRefresher(
        AppCaches appCaches,
        IJsonSerializer serializer,
        IPublishedSnapshotService publishedSnapshotService,
        IIdKeyMap idKeyMap,
        IEventAggregator eventAggregator,
        ICacheRefresherNotificationFactory factory)
        : base(appCaches, serializer, eventAggregator, factory)
    {
        _publishedSnapshotService = publishedSnapshotService;
        _idKeyMap = idKeyMap;
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

        _publishedSnapshotService.Notify(payloads, out var anythingChanged);

        if (anythingChanged)
        {
            AppCaches.ClearPartialViewCache();
            AppCaches.RuntimeCache.ClearByKey(CacheKeys.MediaRecycleBinCacheKey);

            Attempt<IAppPolicyCache?> mediaCache = AppCaches.IsolatedCaches.Get<IMedia>();

            foreach (JsonPayload payload in payloads)
            {
                if (payload.ChangeTypes == TreeChangeTypes.Remove)
                {
                    _idKeyMap.ClearCache(payload.Id);
                }

                if (!mediaCache.Success)
                {
                    continue;
                }

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
        }

        base.Refresh(payloads);
    }

    // these events should never trigger
    // everything should be JSON
    public override void RefreshAll() => throw new NotSupportedException();

    public override void Refresh(int id) => throw new NotSupportedException();

    public override void Refresh(Guid id) => throw new NotSupportedException();

    public override void Remove(int id) => throw new NotSupportedException();

    #endregion
}
