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

public sealed class ContentCacheRefresher : PayloadCacheRefresherBase<ContentCacheRefresherNotification,
    ContentCacheRefresher.JsonPayload>
{
    private readonly IDomainService _domainService;
    private readonly IDomainCacheService _domainCacheService;
    private readonly IDocumentUrlService _documentUrlService;
    private readonly IDocumentNavigationQueryService _documentNavigationQueryService;
    private readonly IDocumentNavigationManagementService _documentNavigationManagementService;
    private readonly IContentService _contentService;
    private readonly IDocumentCacheService _documentCacheService;
    private readonly ICacheManager _cacheManager;
    private readonly IPublishStatusManagementService _publishStatusManagementService;
    private readonly IIdKeyMap _idKeyMap;

    [Obsolete("Use the constructor with ICacheManager instead, scheduled for removal in V17.")]
    public ContentCacheRefresher(
        AppCaches appCaches,
        IJsonSerializer serializer,
        IIdKeyMap idKeyMap,
        IDomainService domainService,
        IEventAggregator eventAggregator,
        ICacheRefresherNotificationFactory factory,
        IDocumentUrlService documentUrlService,
        IDomainCacheService domainCacheService,
        IDocumentNavigationQueryService documentNavigationQueryService,
        IDocumentNavigationManagementService documentNavigationManagementService,
        IContentService contentService,
        IPublishStatusManagementService publishStatusManagementService,
        IDocumentCacheService documentCacheService)
        : this(
            appCaches,
            serializer,
            idKeyMap,
            domainService,
            eventAggregator,
            factory,
            documentUrlService,
            domainCacheService,
            documentNavigationQueryService,
            documentNavigationManagementService,
            contentService,
            publishStatusManagementService,
            documentCacheService,
            StaticServiceProvider.Instance.GetRequiredService<ICacheManager>())
    {
    }

    public ContentCacheRefresher(
        AppCaches appCaches,
        IJsonSerializer serializer,
        IIdKeyMap idKeyMap,
        IDomainService domainService,
        IEventAggregator eventAggregator,
        ICacheRefresherNotificationFactory factory,
        IDocumentUrlService documentUrlService,
        IDomainCacheService domainCacheService,
        IDocumentNavigationQueryService documentNavigationQueryService,
        IDocumentNavigationManagementService documentNavigationManagementService,
        IContentService contentService,
        IPublishStatusManagementService publishStatusManagementService,
        IDocumentCacheService documentCacheService,
        ICacheManager cacheManager)
        : base(appCaches, serializer, eventAggregator, factory)
    {
        _idKeyMap = idKeyMap;
        _domainService = domainService;
        _domainCacheService = domainCacheService;
        _documentUrlService = documentUrlService;
        _documentNavigationQueryService = documentNavigationQueryService;
        _documentNavigationManagementService = documentNavigationManagementService;
        _contentService = contentService;
        _documentCacheService = documentCacheService;
        _publishStatusManagementService = publishStatusManagementService;

        // TODO: Ideally we should inject IElementsCache
        // this interface is in infrastructure, and changing this is very breaking
        // so as long as we have the cache manager, which casts the IElementsCache to a simple AppCache we might as well use that.
        _cacheManager = cacheManager;
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

        // Ideally, we'd like to not have to clear the entire cache here. However, this was the existing behavior in NuCache.
        // The reason for this is that we have no way to know which elements are affected by the changes or what their keys are.
        // This is because currently published elements live exclusively in a JSON blob in the umbracoPropertyData table.
        // This means that the only way to resolve these keys is to actually parse this data with a specific value converter, and for all cultures, which is not possible.
        // If published elements become their own entities with relations, instead of just property data, we can revisit this.
        _cacheManager.ElementsCache.Clear();

        var idsRemoved = new HashSet<int>();
        IAppPolicyCache isolatedCache = AppCaches.IsolatedCaches.GetOrCreate<IContent>();

        foreach (JsonPayload payload in payloads)
        {
            if (payload.Id != default)
            {
                // By INT Id
                isolatedCache.Clear(RepositoryCacheKeys.GetKey<IContent, int>(payload.Id));

                // By GUID Key
                isolatedCache.Clear(RepositoryCacheKeys.GetKey<IContent, Guid?>(payload.Key));
            }

            // remove those that are in the branch
            if (payload.ChangeTypes.HasTypesAny(TreeChangeTypes.RefreshBranch | TreeChangeTypes.Remove))
            {
                var pathid = "," + payload.Id + ",";
                isolatedCache.ClearOfType<IContent>((k, v) => v.Path?.Contains(pathid) ?? false);
            }

            // if the item is not a blueprint and is being completely removed, we need to refresh the domains cache if any domain was assigned to the content
            if (payload.Blueprint is false && payload.ChangeTypes.HasTypesAny(TreeChangeTypes.Remove))
            {
                idsRemoved.Add(payload.Id);
            }


            HandleMemoryCache(payload);
            HandleRouting(payload);

            HandleNavigation(payload);
            HandlePublishedAsync(payload, CancellationToken.None).GetAwaiter().GetResult();
            if (payload.Id != default)
            {
                _idKeyMap.ClearCache(payload.Id);
            }
            if (payload.Key.HasValue)
            {
                _idKeyMap.ClearCache(payload.Key.Value);
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
                _domainCacheService.Refresh(assignedDomains
                    .Select(x => new DomainCacheRefresher.JsonPayload(x.Id, DomainChangeTypes.Remove)).ToArray());
            }
        }

        base.Refresh(payloads);
    }

    private void HandleMemoryCache(JsonPayload payload)
    {
        Guid key = payload.Key ?? _idKeyMap.GetKeyForId(payload.Id, UmbracoObjectTypes.Document).Result;

        if (payload.Blueprint)
        {
            return;
        }

        if (payload.ChangeTypes.HasType(TreeChangeTypes.RefreshNode))
        {
            _documentCacheService.RefreshMemoryCacheAsync(key).GetAwaiter().GetResult();
        }

        if (payload.ChangeTypes.HasType(TreeChangeTypes.RefreshBranch))
        {
            if (_documentNavigationQueryService.TryGetDescendantsKeys(key, out IEnumerable<Guid> descendantsKeys))
            {
                var branchKeys = descendantsKeys.ToList();
                branchKeys.Add(key);

                // If the branch is unpublished, we need to remove it from cache instead of refreshing it
                if (IsBranchUnpublished(payload))
                {
                    foreach (Guid branchKey in branchKeys)
                    {
                        _documentCacheService.RemoveFromMemoryCacheAsync(branchKey).GetAwaiter().GetResult();
                    }
                }
                else
                {
                    foreach (Guid branchKey in branchKeys)
                    {
                        _documentCacheService.RefreshMemoryCacheAsync(branchKey).GetAwaiter().GetResult();
                    }
                }
            }
        }

        if (payload.ChangeTypes.HasType(TreeChangeTypes.RefreshAll))
        {
            _documentCacheService.ClearMemoryCacheAsync(CancellationToken.None).GetAwaiter().GetResult();
        }

        if (payload.ChangeTypes.HasType(TreeChangeTypes.Remove))
        {
            _documentCacheService.RemoveFromMemoryCacheAsync(key).GetAwaiter().GetResult();
        }
    }

    private bool IsBranchUnpublished(JsonPayload payload)
    {
        // If unpublished cultures has one or more values, but published cultures does not, this means that the branch is unpublished entirely
        // And therefore should no longer be resolve-able from the cache, so we need to remove it instead.
        // Otherwise, some culture is still published, so it should be resolve-able from cache, and published cultures should instead be used.
        return payload.UnpublishedCultures is not null && payload.UnpublishedCultures.Length != 0 &&
               (payload.PublishedCultures is null || payload.PublishedCultures.Length == 0);
    }

    private void HandleNavigation(JsonPayload payload)
    {

        if (payload.ChangeTypes.HasType(TreeChangeTypes.RefreshAll))
        {
            _documentNavigationManagementService.RebuildAsync();
            _documentNavigationManagementService.RebuildBinAsync();
        }

        if (payload.Key is null)
        {
            return;
        }

        if (payload.ChangeTypes.HasType(TreeChangeTypes.Remove))
        {
            _documentNavigationManagementService.MoveToBin(payload.Key.Value);
            _documentNavigationManagementService.RemoveFromBin(payload.Key.Value);
        }

        if (payload.ChangeTypes.HasType(TreeChangeTypes.RefreshNode))
        {
            IContent? content = _contentService.GetById(payload.Key.Value);

            if (content is null)
            {
                return;
            }

            HandleNavigationForSingleContent(content);
        }

        if (payload.ChangeTypes.HasType(TreeChangeTypes.RefreshBranch))
        {
            IContent? content = _contentService.GetById(payload.Key.Value);

            if (content is null)
            {
                return;
            }

            IEnumerable<IContent> descendants = _contentService.GetPagedDescendants(content.Id, 0, int.MaxValue, out _);
            foreach (IContent descendant in content.Yield().Concat(descendants))
            {
                HandleNavigationForSingleContent(descendant);
            }
        }
    }

    private void HandleNavigationForSingleContent(IContent content)
    {
        // First creation
        if (ExistsInNavigation(content.Key) is false && ExistsInNavigationBin(content.Key) is false)
        {
            _documentNavigationManagementService.Add(content.Key, content.ContentType.Key, GetParentKey(content), content.SortOrder);
            if (content.Trashed)
            {
                // If created as trashed, move to bin
                _documentNavigationManagementService.MoveToBin(content.Key);
            }
        }
        else if (ExistsInNavigation(content.Key) && ExistsInNavigationBin(content.Key) is false)
        {
            if (content.Trashed)
            {
                // It must have been trashed
                _documentNavigationManagementService.MoveToBin(content.Key);
            }
            else
            {
                if (_documentNavigationQueryService.TryGetParentKey(content.Key, out Guid? oldParentKey) is false)
                {
                    return;
                }

                // It must have been saved. Check if parent is different
                Guid? newParentKey = GetParentKey(content);
                if (oldParentKey != newParentKey)
                {
                    _documentNavigationManagementService.Move(content.Key, newParentKey);
                }
                else
                {
                    _documentNavigationManagementService.UpdateSortOrder(content.Key, content.SortOrder);
                }
            }
        }
        else if (ExistsInNavigation(content.Key) is false && ExistsInNavigationBin(content.Key))
        {
            if (content.Trashed is false)
            {
                // It must have been restored
                _documentNavigationManagementService.RestoreFromBin(content.Key, GetParentKey(content));
            }
        }
    }

    private Guid? GetParentKey(IContent content) => (content.ParentId == -1) ? null : _idKeyMap.GetKeyForId(content.ParentId, UmbracoObjectTypes.Document).Result;

    private bool ExistsInNavigation(Guid contentKey) => _documentNavigationQueryService.TryGetParentKey(contentKey, out _);

    private bool ExistsInNavigationBin(Guid contentKey) => _documentNavigationQueryService.TryGetParentKeyInBin(contentKey, out _);

    private async Task HandlePublishedAsync(JsonPayload payload, CancellationToken cancellationToken)
    {

        if (payload.ChangeTypes.HasType(TreeChangeTypes.RefreshAll))
        {
            await _publishStatusManagementService.InitializeAsync(cancellationToken);
        }

        if (payload.Key.HasValue is false)
        {
            return;
        }

        if (payload.ChangeTypes.HasType(TreeChangeTypes.Remove))
        {
            await _publishStatusManagementService.RemoveAsync(payload.Key.Value, cancellationToken);
        }
        else if (payload.ChangeTypes.HasType(TreeChangeTypes.RefreshNode))
        {
            await _publishStatusManagementService.AddOrUpdateStatusAsync(payload.Key.Value, cancellationToken);
        }
        else if (payload.ChangeTypes.HasType(TreeChangeTypes.RefreshBranch))
        {
            await _publishStatusManagementService.AddOrUpdateStatusWithDescendantsAsync(payload.Key.Value, cancellationToken);
        }
    }
    private void HandleRouting(JsonPayload payload)
    {
        if(payload.ChangeTypes.HasType(TreeChangeTypes.Remove))
        {
            var key = payload.Key ?? _idKeyMap.GetKeyForId(payload.Id, UmbracoObjectTypes.Document).Result;

            //Note the we need to clear the navigation service as the last thing
            if (_documentNavigationQueryService.TryGetDescendantsKeysOrSelfKeys(key, out var descendantsOrSelfKeys))
            {
                _documentUrlService.DeleteUrlsFromCacheAsync(descendantsOrSelfKeys).GetAwaiter().GetResult();
            }
            else if(_documentNavigationQueryService.TryGetDescendantsKeysOrSelfKeysInBin(key, out var descendantsOrSelfKeysInBin))
            {
                _documentUrlService.DeleteUrlsFromCacheAsync(descendantsOrSelfKeysInBin).GetAwaiter().GetResult();
            }

        }
        if(payload.ChangeTypes.HasType(TreeChangeTypes.RefreshAll))
        {
            _documentUrlService.InitAsync(false, CancellationToken.None).GetAwaiter().GetResult(); //TODO make async
        }

        if(payload.ChangeTypes.HasType(TreeChangeTypes.RefreshNode))
        {
            var key = payload.Key ?? _idKeyMap.GetKeyForId(payload.Id, UmbracoObjectTypes.Document).Result;
            _documentUrlService.CreateOrUpdateUrlSegmentsAsync(key).GetAwaiter().GetResult();
        }

        if(payload.ChangeTypes.HasType(TreeChangeTypes.RefreshBranch))
        {
            var key = payload.Key ?? _idKeyMap.GetKeyForId(payload.Id, UmbracoObjectTypes.Document).Result;
            _documentUrlService.CreateOrUpdateUrlSegmentsWithDescendantsAsync(key).GetAwaiter().GetResult();
        }

    }

    // these events should never trigger
    // everything should be PAYLOAD/JSON
    public override void RefreshAll() => throw new NotSupportedException();

    public override void Refresh(int id) => throw new NotSupportedException();

    public override void Refresh(Guid id) => throw new NotSupportedException();

    public override void Remove(int id) => throw new NotSupportedException();

    #endregion

    #region Json

    // TODO (V14): Change into a record
    public class JsonPayload
    {

        public int Id { get; init; }

        public Guid? Key { get; init; }

        public TreeChangeTypes ChangeTypes { get; init; }

        public bool Blueprint { get; init; }

        public string[]? PublishedCultures { get; init; }

        public string[]? UnpublishedCultures { get; init; }
    }

    #endregion
}
