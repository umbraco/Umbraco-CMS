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

public sealed class ElementCacheRefresher : PayloadCacheRefresherBase<ElementCacheRefresherNotification, ElementCacheRefresher.JsonPayload>
{
    private readonly IIdKeyMap _idKeyMap;
    private readonly IElementCacheService _elementCacheService;
    private readonly ICacheManager _cacheManager;

    public ElementCacheRefresher(
        AppCaches appCaches,
        IJsonSerializer serializer,
        IIdKeyMap idKeyMap,
        IEventAggregator eventAggregator,
        ICacheRefresherNotificationFactory factory,
        IElementCacheService elementCacheService,
        ICacheManager cacheManager)
        : base(appCaches, serializer, eventAggregator, factory)
    {
        _idKeyMap = idKeyMap;
        _elementCacheService = elementCacheService;

        // TODO ELEMENTS: Use IElementsCache instead of ICacheManager, see ContentCacheRefresher for more information.
        _cacheManager = cacheManager;
    }

    #region Json

    public class JsonPayload
    {
        public JsonPayload(int id, Guid key, TreeChangeTypes changeTypes)
        {
            Id = id;
            Key = key;
            ChangeTypes = changeTypes;
        }

        public int Id { get; }

        public Guid Key { get; }

        public TreeChangeTypes ChangeTypes { get; }

        // TODO ELEMENTS: should we support (un)published cultures in this payload? see ContentCacheRefresher.JsonPayload
    }

    #endregion

    #region Define

    public static readonly Guid UniqueId = Guid.Parse("EE5BB23A-A656-4F7E-A234-16F21AAABFD1");

    public override Guid RefresherUniqueId => UniqueId;

    public override string Name => "Element Cache Refresher";

    #endregion

    #region Refresher

    public override void Refresh(JsonPayload[] payloads)
    {
        AppCaches.RuntimeCache.ClearByKey(CacheKeys.ElementRecycleBinCacheKey);

        // Ideally, we'd like to not have to clear the entire cache here. However, this was the existing behavior in NuCache.
        // The reason for this is that we have no way to know which elements are affected by the changes or what their keys are.
        // This is because currently published elements live exclusively in a JSON blob in the umbracoPropertyData table.
        // This means that the only way to resolve these keys is to actually parse this data with a specific value converter, and for all cultures, which is not possible.
        // If published elements become their own entities with relations, instead of just property data, we can revisit this.
        _cacheManager.ElementsCache.Clear();

        IAppPolicyCache isolatedCache = AppCaches.IsolatedCaches.GetOrCreate<IElement>();

        foreach (JsonPayload payload in payloads)
        {
            // By INT Id
            isolatedCache.Clear(RepositoryCacheKeys.GetKey<IElement, int>(payload.Id));

            // By GUID Key
            isolatedCache.Clear(RepositoryCacheKeys.GetKey<IElement, Guid?>(payload.Key));

            HandleMemoryCache(payload);

            // TODO ELEMENTS: if we need published status caching for elements (e.g. for seeding purposes), make sure
            //                it is kept in sync here (see ContentCacheRefresher)

            if (payload.ChangeTypes == TreeChangeTypes.Remove)
            {
                _idKeyMap.ClearCache(payload.Id);
            }
        }

        AppCaches.ClearPartialViewCache();

        base.Refresh(payloads);
    }

    private void HandleMemoryCache(JsonPayload payload)
    {
        if (payload.ChangeTypes.HasType(TreeChangeTypes.RefreshAll))
        {
            _elementCacheService.ClearMemoryCacheAsync(CancellationToken.None).GetAwaiter().GetResult();
        }
        else if (payload.ChangeTypes.HasType(TreeChangeTypes.RefreshNode) || payload.ChangeTypes.HasType(TreeChangeTypes.RefreshBranch))
        {
            // NOTE: RefreshBranch might be triggered even though elements do not support branch publishing
            _elementCacheService.RefreshMemoryCacheAsync(payload.Key).GetAwaiter().GetResult();
        }

        if (payload.ChangeTypes.HasType(TreeChangeTypes.Remove))
        {
            _elementCacheService.RemoveFromMemoryCacheAsync(payload.Key).GetAwaiter().GetResult();
        }
    }

    // these events should never trigger
    // everything should be JSON
    public override void RefreshAll() => throw new NotSupportedException();

    public override void Refresh(int id) => throw new NotSupportedException();

    public override void Refresh(Guid id) => throw new NotSupportedException();

    public override void Remove(int id) => throw new NotSupportedException();

    #endregion
}
