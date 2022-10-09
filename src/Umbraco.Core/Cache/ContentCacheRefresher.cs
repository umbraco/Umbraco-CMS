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

public sealed class ContentCacheRefresher : PayloadCacheRefresherBase<ContentCacheRefresherNotification,
    ContentCacheRefresher.JsonPayload>
{
    private readonly IDomainService _domainService;
    private readonly IIdKeyMap _idKeyMap;
    private readonly IPublishedSnapshotService _publishedSnapshotService;

    public ContentCacheRefresher(
        AppCaches appCaches,
        IJsonSerializer serializer,
        IPublishedSnapshotService publishedSnapshotService,
        IIdKeyMap idKeyMap,
        IDomainService domainService,
        IEventAggregator eventAggregator,
        ICacheRefresherNotificationFactory factory)
        : base(appCaches, serializer, eventAggregator, factory)
    {
        _publishedSnapshotService = publishedSnapshotService;
        _idKeyMap = idKeyMap;
        _domainService = domainService;
    }

    #region Indirect

    public static void RefreshContentTypes(AppCaches appCaches)
    {
        // we could try to have a mechanism to notify the PublishedCachesService
        // and figure out whether published items were modified or not... keep it
        // simple for now, just clear the whole thing
        appCaches.ClearPartialViewCache();

        appCaches.IsolatedCaches.ClearCache<PublicAccessEntry>();
        appCaches.IsolatedCaches.ClearCache<IContent>();
    }

    #endregion

    #region Define

    public static readonly Guid UniqueId = Guid.Parse("900A4FBE-DF3C-41E6-BB77-BE896CD158EA");

    public override Guid RefresherUniqueId => UniqueId;

    public override string Name => "ContentCacheRefresher";

    #endregion

    #region Refresher

    public override void Refresh(JsonPayload[] payloads)
    {
        AppCaches.RuntimeCache.ClearOfType<PublicAccessEntry>();
        AppCaches.RuntimeCache.ClearByKey(CacheKeys.ContentRecycleBinCacheKey);

        var idsRemoved = new HashSet<int>();
        IAppPolicyCache isolatedCache = AppCaches.IsolatedCaches.GetOrCreate<IContent>();

        foreach (JsonPayload payload in payloads.Where(x => x.Id != default))
        {
            // By INT Id
            isolatedCache.Clear(RepositoryCacheKeys.GetKey<IContent, int>(payload.Id));

            // By GUID Key
            isolatedCache.Clear(RepositoryCacheKeys.GetKey<IContent, Guid?>(payload.Key));

            _idKeyMap.ClearCache(payload.Id);

            // remove those that are in the branch
            if (payload.ChangeTypes.HasTypesAny(TreeChangeTypes.RefreshBranch | TreeChangeTypes.Remove))
            {
                var pathid = "," + payload.Id + ",";
                isolatedCache.ClearOfType<IContent>((k, v) => v.Path?.Contains(pathid) ?? false);
            }

            // if the item is being completely removed, we need to refresh the domains cache if any domain was assigned to the content
            if (payload.ChangeTypes.HasTypesAny(TreeChangeTypes.Remove))
            {
                idsRemoved.Add(payload.Id);
            }
        }

        if (idsRemoved.Count > 0)
        {
            var assignedDomains = _domainService.GetAll(true)
                ?.Where(x => x.RootContentId.HasValue && idsRemoved.Contains(x.RootContentId.Value)).ToList();

            if (assignedDomains?.Count > 0)
            {
                // TODO: this is duplicating the logic in DomainCacheRefresher BUT we cannot inject that into this because it it not registered explicitly in the container,
                // and we cannot inject the CacheRefresherCollection since that would be a circular reference, so what is the best way to call directly in to the
                // DomainCacheRefresher?
                ClearAllIsolatedCacheByEntityType<IDomain>();

                // note: must do what's above FIRST else the repositories still have the old cached
                // content and when the PublishedCachesService is notified of changes it does not see
                // the new content...
                // notify
                _publishedSnapshotService.Notify(assignedDomains
                    .Select(x => new DomainCacheRefresher.JsonPayload(x.Id, DomainChangeTypes.Remove)).ToArray());
            }
        }

        // note: must do what's above FIRST else the repositories still have the old cached
        // content and when the PublishedCachesService is notified of changes it does not see
        // the new content...

        // TODO: what about this?
        // should rename it, and then, this is only for Deploy, and then, ???
        // if (Suspendable.PageCacheRefresher.CanUpdateDocumentCache)
        //   ...
        NotifyPublishedSnapshotService(_publishedSnapshotService, AppCaches, payloads);

        base.Refresh(payloads);
    }

    // these events should never trigger
    // everything should be PAYLOAD/JSON
    public override void RefreshAll() => throw new NotSupportedException();

    public override void Refresh(int id) => throw new NotSupportedException();

    public override void Refresh(Guid id) => throw new NotSupportedException();

    public override void Remove(int id) => throw new NotSupportedException();

    #endregion

    #region Json

    /// <summary>
    ///     Refreshes the publish snapshot service and if there are published changes ensures that partial view caches are
    ///     refreshed too
    /// </summary>
    /// <param name="service"></param>
    /// <param name="appCaches"></param>
    /// <param name="payloads"></param>
    internal static void NotifyPublishedSnapshotService(IPublishedSnapshotService service, AppCaches appCaches, JsonPayload[] payloads)
    {
        service.Notify(payloads, out _, out var publishedChanged);

        if (payloads.Any(x => x.ChangeTypes.HasType(TreeChangeTypes.RefreshAll)) || publishedChanged)
        {
            // when a public version changes
            appCaches.ClearPartialViewCache();
        }
    }

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
}
