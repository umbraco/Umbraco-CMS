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
/// Provides cache refresh functionality for content items, ensuring that content-related caches are updated or
/// invalidated in response to content changes.
/// </summary>
/// <remarks>
/// The ContentCacheRefresher coordinates cache invalidation for content, including memory caches, URL
/// caches, navigation structures, and domain assignments. It responds to content change notifications and ensures that
/// all relevant caches reflect the current state of published and unpublished content. This refresher is used
/// internally to maintain cache consistency after content operations such as publish, unpublish, move,
/// or delete.
/// </remarks>
public sealed class ContentCacheRefresher : PayloadCacheRefresherBase<ContentCacheRefresherNotification,
    ContentCacheRefresher.JsonPayload>
{
    private readonly IDomainService _domainService;
    private readonly IDomainCacheService _domainCacheService;
    private readonly IDocumentUrlService _documentUrlService;
    private readonly IDocumentUrlAliasService _documentUrlAliasService;
    private readonly IDocumentNavigationQueryService _documentNavigationQueryService;
    private readonly IDocumentNavigationManagementService _documentNavigationManagementService;
    private readonly IContentService _contentService;
    private readonly IDocumentCacheService _documentCacheService;
    private readonly ICacheManager _cacheManager;
    private readonly IPublishStatusManagementService _publishStatusManagementService;
    private readonly IIdKeyMap _idKeyMap;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentCacheRefresher"/> class.
    /// </summary>
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 19.")]
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
        : this(
            appCaches,
            serializer,
            idKeyMap,
            domainService,
            eventAggregator,
            factory,
            documentUrlService,
            StaticServiceProvider.Instance.GetRequiredService<IDocumentUrlAliasService>(),
            domainCacheService,
            documentNavigationQueryService,
            documentNavigationManagementService,
            contentService,
            publishStatusManagementService,
            documentCacheService,
            cacheManager)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentCacheRefresher"/> class.
    /// </summary>
    public ContentCacheRefresher(
        AppCaches appCaches,
        IJsonSerializer serializer,
        IIdKeyMap idKeyMap,
        IDomainService domainService,
        IEventAggregator eventAggregator,
        ICacheRefresherNotificationFactory factory,
        IDocumentUrlService documentUrlService,
        IDocumentUrlAliasService documentUrlAliasService,
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
        _documentUrlAliasService = documentUrlAliasService;
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

    /// <summary>
    /// Clears cached content and public access data from the provided application caches.
    /// </summary>
    /// <param name="appCaches">The application caches instance from which to clear content and public access entries.</param>
    public static void RefreshContentTypes(AppCaches appCaches)
    {
        // We could try to have a mechanism to notify the PublishedCachesService
        // and figure out whether published items were modified or not... keep it
        // simple for now, just clear the whole thing.
        appCaches.ClearPartialViewCache();

        appCaches.IsolatedCaches.ClearCache<PublicAccessEntry>();
        appCaches.IsolatedCaches.ClearCache<IContent>();
    }

    #endregion

    #region Define

    /// <summary>
    /// Represents a unique identifier for the cache refresher.
    /// </summary>
    public static readonly Guid UniqueId = Guid.Parse("900A4FBE-DF3C-41E6-BB77-BE896CD158EA");

    /// <inheritdoc/>
    public override Guid RefresherUniqueId => UniqueId;

    /// <inheritdoc/>
    public override string Name => "ContentCacheRefresher";

    #endregion

    #region Refresher

    /// <inheritdoc/>
    public override void RefreshInternal(JsonPayload[] payloads)
    {
        AppCaches.RuntimeCache.ClearOfType<PublicAccessEntry>();
        AppCaches.RuntimeCache.ClearByKey(CacheKeys.ContentRecycleBinCacheKey);

        // Ideally, we'd like to not have to clear the entire cache here. However, this was the existing behavior in NuCache.
        // The reason for this is that we have no way to know which elements are affected by the changes or what their keys are.
        // This is because currently published elements live exclusively in a JSON blob in the umbracoPropertyData table.
        // This means that the only way to resolve these keys is to actually parse this data with a specific value converter, and for all cultures, which is not possible.
        // If published elements become their own entities with relations, instead of just property data, we can revisit this.
        _cacheManager.ElementsCache.Clear();

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
        }

        base.RefreshInternal(payloads);
    }

    /// <inheritdoc/>
    public override void Refresh(JsonPayload[] payloads)
    {
        var idsRemoved = new HashSet<int>();

        foreach (JsonPayload payload in payloads)
        {
            // If the item is not a blueprint and is being completely removed, we need to refresh the domains cache if any domain was assigned to the content.
            // So track the IDs that have been removed.
            if (payload.Blueprint is false && payload.ChangeTypes.HasTypesAny(TreeChangeTypes.Remove))
            {
                idsRemoved.Add(payload.Id);
            }

            HandleMemoryCache(payload);
            HandleRouting(payload);

            HandleNavigation(payload);
            HandlePublishedAsync(payload, CancellationToken.None).GetAwaiter().GetResult();

            HandleIdKeyMap(payload);
        }

        // Clear partial view cache when published content changes.
        if (ShouldClearPartialViewCache(payloads))
        {
            AppCaches.ClearPartialViewCache();
        }

        // Clear the domain cache if any domain is assigned to removed content.
        HandleDomainCache(idsRemoved);

        base.Refresh(payloads);
    }

    private static bool ShouldClearPartialViewCache(JsonPayload[] payloads)
    {
        return payloads.Any(x =>
        {
            // Check for relelvant change type
            var isRelevantChangeType = x.ChangeTypes.HasType(TreeChangeTypes.RefreshAll) ||
                x.ChangeTypes.HasType(TreeChangeTypes.Remove) ||
                x.ChangeTypes.HasType(TreeChangeTypes.RefreshNode) ||
                x.ChangeTypes.HasType(TreeChangeTypes.RefreshBranch);

            // Check for published/unpublished changes
            var hasChanges = x.PublishedCultures?.Length > 0 ||
                   x.UnpublishedCultures?.Length > 0;

            // There's no other way to detect trashed content as the change type is only Remove when deleted permanently
            var isTrashed = x.ChangeTypes.HasType(TreeChangeTypes.RefreshBranch) && x.PublishedCultures is null && x.UnpublishedCultures is null;

            // Skip blueprints and only clear the partial cache for removals or refreshes with changes
            return x.Blueprint == false && (isTrashed || (isRelevantChangeType && hasChanges));
        });
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

    private static bool IsBranchUnpublished(JsonPayload payload) =>

        // If unpublished cultures has one or more values, but published cultures does not, this means that the branch is unpublished entirely
        // And therefore should no longer be resolve-able from the cache, so we need to remove it instead.
        // Otherwise, some culture is still published, so it should be resolve-able from cache, and published cultures should instead be used.
        payload.UnpublishedCultures is not null && payload.UnpublishedCultures.Length != 0 &&
               (payload.PublishedCultures is null || payload.PublishedCultures.Length == 0);

    private void HandleRouting(JsonPayload payload)
    {
        if (payload.ChangeTypes.HasType(TreeChangeTypes.Remove))
        {
            Guid key = payload.Key ?? _idKeyMap.GetKeyForId(payload.Id, UmbracoObjectTypes.Document).Result;

            // Note that we need to clear the navigation service as the last thing.
            if (_documentNavigationQueryService.TryGetDescendantsKeysOrSelfKeys(key, out IEnumerable<Guid>? descendantsOrSelfKeys))
            {
                _documentUrlService.DeleteUrlsFromCacheAsync(descendantsOrSelfKeys).GetAwaiter().GetResult();
                _documentUrlAliasService.DeleteAliasesFromCacheAsync(descendantsOrSelfKeys).GetAwaiter().GetResult();
            }
            else if (_documentNavigationQueryService.TryGetDescendantsKeysOrSelfKeysInBin(key, out IEnumerable<Guid>? descendantsOrSelfKeysInBin))
            {
                _documentUrlService.DeleteUrlsFromCacheAsync(descendantsOrSelfKeysInBin).GetAwaiter().GetResult();
                _documentUrlAliasService.DeleteAliasesFromCacheAsync(descendantsOrSelfKeysInBin).GetAwaiter().GetResult();
            }
        }

        if (payload.ChangeTypes.HasType(TreeChangeTypes.RefreshAll))
        {
            _documentUrlService.InitAsync(false, CancellationToken.None).GetAwaiter().GetResult(); // TODO: make async
            _documentUrlAliasService.InitAsync(false, CancellationToken.None).GetAwaiter().GetResult();
        }

        if (payload.ChangeTypes.HasType(TreeChangeTypes.RefreshNode))
        {
            Guid key = payload.Key ?? _idKeyMap.GetKeyForId(payload.Id, UmbracoObjectTypes.Document).Result;
            _documentUrlService.CreateOrUpdateUrlSegmentsAsync(key).GetAwaiter().GetResult();
            _documentUrlAliasService.CreateOrUpdateAliasesAsync(key).GetAwaiter().GetResult();
        }

        if (payload.ChangeTypes.HasType(TreeChangeTypes.RefreshBranch))
        {
            Guid key = payload.Key ?? _idKeyMap.GetKeyForId(payload.Id, UmbracoObjectTypes.Document).Result;
            _documentUrlService.CreateOrUpdateUrlSegmentsWithDescendantsAsync(key).GetAwaiter().GetResult();
            _documentUrlAliasService.CreateOrUpdateAliasesWithDescendantsAsync(key).GetAwaiter().GetResult();
        }
    }

    private void HandleNavigation(JsonPayload payload)
    {
        if (payload.ChangeTypes.HasType(TreeChangeTypes.RefreshAll))
        {
            _documentNavigationManagementService.RebuildAsync().GetAwaiter().GetResult();
            _documentNavigationManagementService.RebuildBinAsync().GetAwaiter().GetResult();
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
        else if (payload.ChangeTypes.HasType(TreeChangeTypes.RefreshNode) && HasPublishStatusUpdates(payload))
        {
            await _publishStatusManagementService.AddOrUpdateStatusAsync(payload.Key.Value, cancellationToken);
        }
        else if (payload.ChangeTypes.HasType(TreeChangeTypes.RefreshBranch) && HasPublishStatusUpdates(payload))
        {
            await _publishStatusManagementService.AddOrUpdateStatusWithDescendantsAsync(payload.Key.Value, cancellationToken);
        }
    }

    private static bool HasPublishStatusUpdates(JsonPayload payload) =>
        (payload.PublishedCultures is not null && payload.PublishedCultures.Length > 0) ||
        (payload.UnpublishedCultures is not null && payload.UnpublishedCultures.Length > 0);

    private void HandleIdKeyMap(JsonPayload payload)
    {
        // We only need to flush the ID/Key map when content is deleted.
        if (payload.ChangeTypes.HasTypesAny(TreeChangeTypes.Remove) is false)
        {
            return;
        }

        if (payload.Id != default)
        {
            _idKeyMap.ClearCache(payload.Id);
        }

        if (payload.Key.HasValue)
        {
            _idKeyMap.ClearCache(payload.Key.Value);
        }
    }

    private void HandleDomainCache(HashSet<int> idsRemoved)
    {
        if (idsRemoved.Count == 0)
        {
            return;
        }

#pragma warning disable CS0618 // Type or member is obsolete
        var assignedDomains = _domainService.GetAll(true)
            .Where(x => x.RootContentId.HasValue && idsRemoved.Contains(x.RootContentId.Value))
            .ToList();
#pragma warning restore CS0618 // Type or member is obsolete
        if (assignedDomains.Count <= 0)
        {
            return;
        }

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

    // These events should never trigger. Everything should be PAYLOAD/JSON.

    /// <inheritdoc/>
    public override void RefreshAll() => throw new NotSupportedException();

    /// <inheritdoc/>
    public override void Refresh(int id) => throw new NotSupportedException();

    /// <inheritdoc/>
    public override void Refresh(Guid id) => throw new NotSupportedException();

    /// <inheritdoc/>
    public override void Remove(int id) => throw new NotSupportedException();

    #endregion

    #region Json

    // TODO (V14): Change into a record
    /// <summary>
    /// Represents a JSON-serializable payload containing information about a content or tree change event, including
    /// identifiers, change types, and culture-specific publishing details.
    /// </summary>
    public class JsonPayload
    {
        /// <summary>
        /// Gets the unique integer identifier for the entity.
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Gets the unique GUID key associated with the entity, or null if no key is assigned.
        /// </summary>
        public Guid? Key { get; init; }

        /// <summary>
        /// Gets the types of changes that have occurred in the tree.
        /// </summary>
        public TreeChangeTypes ChangeTypes { get; init; }

        /// <summary>
        /// Gets a value indicating whether the content represents a document blueprint.
        /// </summary>
        public bool Blueprint { get; init; }

        /// <summary>
        /// Gets the collection of culture codes in which the content is published.
        /// </summary>
        public string[]? PublishedCultures { get; init; }

        /// <summary>
        /// Gets the collection of culture codes for which the content has been unpublished.
        /// </summary>
        public string[]? UnpublishedCultures { get; init; }
    }

    #endregion
}
