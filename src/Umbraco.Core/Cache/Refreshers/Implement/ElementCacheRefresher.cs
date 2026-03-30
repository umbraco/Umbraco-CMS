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

/// <summary>
/// Provides cache refresh functionality for element items, ensuring that element-related caches are updated or
/// invalidated in response to element changes.
/// </summary>
/// <remarks>
/// The ElementCacheRefresher coordinates cache invalidation for elements, including memory caches and
/// isolated caches. It responds to element change notifications and ensures that all relevant caches reflect
/// the current state of published and unpublished elements. This refresher is used internally to maintain
/// cache consistency after element operations such as publish, unpublish, or delete.
/// </remarks>
public sealed class ElementCacheRefresher : PayloadCacheRefresherBase<ElementCacheRefresherNotification, ElementCacheRefresher.JsonPayload>
{
    private readonly IIdKeyMap _idKeyMap;
    private readonly IElementCacheService _elementCacheService;
    private readonly ICacheManager _cacheManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementCacheRefresher"/> class.
    /// </summary>
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

    /// <summary>
    /// Represents a JSON-serializable payload containing information about an element change event, including
    /// identifiers, change types, and culture-specific publishing details.
    /// </summary>
    public class JsonPayload
    {
        public JsonPayload(int id, Guid key, TreeChangeTypes changeTypes)
        {
            Id = id;
            Key = key;
            ChangeTypes = changeTypes;
        }

        /// <summary>
        /// Gets the unique integer identifier for the entity.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Gets the unique GUID key associated with the entity.
        /// </summary>
        public Guid Key { get; }

        /// <summary>
        /// Gets the types of changes that have occurred in the tree.
        /// </summary>
        public TreeChangeTypes ChangeTypes { get; }

        /// <summary>
        /// Gets the collection of culture codes in which the element is published.
        /// </summary>
        public string[]? PublishedCultures { get; init; }

        /// <summary>
        /// Gets the collection of culture codes for which the element has been unpublished.
        /// </summary>
        public string[]? UnpublishedCultures { get; init; }
    }

    #endregion

    #region Define

    /// <summary>
    /// Represents a unique identifier for the cache refresher.
    /// </summary>
    public static readonly Guid UniqueId = Guid.Parse("EE5BB23A-A656-4F7E-A234-16F21AAABFD1");

    /// <inheritdoc/>
    public override Guid RefresherUniqueId => UniqueId;

    /// <inheritdoc/>
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

            if (payload.ChangeTypes.HasType(TreeChangeTypes.Remove))
            {
                _idKeyMap.ClearCache(payload.Id);
            }
        }

        if (ShouldClearPartialViewCache(payloads))
        {
            AppCaches.ClearPartialViewCache();
        }

        base.Refresh(payloads);
    }

    private static bool ShouldClearPartialViewCache(JsonPayload[] payloads)
    {
        return payloads.Any(x =>
        {
            // Check for relevant change type
            var isRelevantChangeType = x.ChangeTypes.HasType(TreeChangeTypes.RefreshAll) ||
                x.ChangeTypes.HasType(TreeChangeTypes.Remove) ||
                x.ChangeTypes.HasType(TreeChangeTypes.RefreshNode) ||
                x.ChangeTypes.HasType(TreeChangeTypes.RefreshBranch);

            // Check for published/unpublished changes
            var hasChanges = x.PublishedCultures?.Length > 0 ||
                   x.UnpublishedCultures?.Length > 0;

            // There's no other way to detect trashed elements as the change type is only Remove when deleted permanently
            var isTrashed = x.ChangeTypes.HasType(TreeChangeTypes.RefreshBranch) && x.PublishedCultures is null && x.UnpublishedCultures is null;

            // Only clear the partial cache for removals or refreshes with changes
            return isTrashed || (isRelevantChangeType && hasChanges);
        });
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
}
