using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Search.Core.Models.Indexing;
using Umbraco.Cms.Search.Core.Services.ContentIndexing;
using Umbraco.Cms.Core.Sync;
using Umbraco.Extensions;

namespace Umbraco.Cms.Search.Core.NotificationHandlers;

/// <summary>
/// Re-indexes documents that reference a changed external (reusable) element - directly, or transitively through
/// other published elements - when external block element indexing is enabled.
/// </summary>
/// <remarks>
/// Unlike the other indexing notification handlers, this one reacts directly to Core's own, genuinely distributed
/// <see cref="ElementCacheRefresherNotification"/> rather than a Search-owned mirror broadcast (see
/// <see cref="IndexingNotificationHandlerBase"/>) - elements are not themselves indexed as their own document type
/// here, only used to trigger a refresh of already-indexed documents, so no origin server needs to be tracked
/// across the farm: every server that receives the notification independently finds and refreshes the documents
/// it has indexed. The one tradeoff is that a same-origin-only index registration would be (harmlessly) refreshed
/// from every server rather than just the originating one.
/// </remarks>
internal sealed class ElementIndexingNotificationHandler : IndexingNotificationHandlerBase, INotificationHandler<ElementCacheRefresherNotification>
{
    private readonly IContentIndexingService _contentIndexingService;
    private readonly IRelationService _relationService;
    private readonly IOptions<IndexingSettings> _indexingSettings;
    private readonly IOriginProvider _originProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementIndexingNotificationHandler"/> class.
    /// </summary>
    /// <param name="coreScopeProvider">The scope provider used to defer actions until the ambient scope completes.</param>
    /// <param name="contentIndexingService">The service used to re-index the affected documents.</param>
    /// <param name="relationService">The service used to traverse element-to-document and element-to-element references.</param>
    /// <param name="indexingSettings">The indexing settings, used to determine whether external element content is indexed at all.</param>
    /// <param name="originProvider">The provider used to determine the current server's origin.</param>
    public ElementIndexingNotificationHandler(
        ICoreScopeProvider coreScopeProvider,
        IContentIndexingService contentIndexingService,
        IRelationService relationService,
        IOptions<IndexingSettings> indexingSettings,
        IOriginProvider originProvider)
        : base(coreScopeProvider)
    {
        _contentIndexingService = contentIndexingService;
        _relationService = relationService;
        _indexingSettings = indexingSettings;
        _originProvider = originProvider;
    }

    /// <summary>
    /// Re-indexes the documents that reference the changed elements described by the notification.
    /// </summary>
    /// <param name="notification">The notification describing the element changes to react to.</param>
    public void Handle(ElementCacheRefresherNotification notification)
    {
        // external element content only ever participates in the index when the feature is enabled; with it off,
        // referencing documents have nothing to refresh.
        if (_indexingSettings.Value.IndexExternalBlockElements is false)
        {
            return;
        }

        if (notification.MessageType != MessageType.RefreshByPayload
            || notification.MessageObject is not ElementCacheRefresher.JsonPayload[] payloads)
        {
            return;
        }

        // a RefreshAll payload (Id=0, e.g. from a full element cache reload) carries no specific element id, so we
        // cannot know which elements actually changed - conservatively treat every element ever referenced via an
        // external block relation as changed, to avoid leaving stale flattened content behind.
        int[] changedElementIds = payloads.Any(payload => payload.ChangeTypes.HasType(TreeChangeTypes.RefreshAll))
            ? GetAllReferencedElementIds()
            : payloads
                .Where(payload => payload.ChangeTypes != TreeChangeTypes.None)
                .Select(payload => payload.Id)
                .Distinct()
                .ToArray();

        if (changedElementIds.Length == 0)
        {
            return;
        }

        Guid[] documentKeys = FindDocumentKeysReferencingElements(changedElementIds);
        if (documentKeys.Length == 0)
        {
            return;
        }

        ContentChange[] changes = documentKeys
            .Select(key => ContentChange.Document(key, ChangeImpact.Refresh, ContentState.Published))
            .ToArray();

        var origin = _originProvider.GetCurrent();
        ExecuteDeferred(() => _contentIndexingService.Handle(changes, origin));
    }

    // Breadth-first traversal of the "umbExternalBlockElement" relation graph: a changed element can be referenced
    // directly by documents, or by other elements (which are themselves referenced by documents, or further
    // elements). Climbing is only continued through a published element - an unpublished element's content (and
    // anything nested below it) is not part of any document's published index, so a change below it cannot affect
    // one further up.
    // Internal (rather than private) so integration tests can verify the traversal directly - the pruning at an
    // unpublished intermediate element has no observable effect on index *content* (the index-time flattening
    // already excludes it independently), so it can only be verified by calling this method directly.
    internal Guid[] FindDocumentKeysReferencingElements(int[] elementIds)
    {
        var documentKeys = new HashSet<Guid>();
        var visitedElementIds = new HashSet<int>(elementIds);
        var currentLevel = elementIds;

        while (currentLevel.Length > 0)
        {
            foreach (IUmbracoEntity document in GetParentEntities(currentLevel, UmbracoObjectTypes.Document))
            {
                documentKeys.Add(document.Key);
            }

            var nextLevel = new HashSet<int>();
            foreach (IUmbracoEntity entity in GetParentEntities(currentLevel, UmbracoObjectTypes.Element))
            {
                if (visitedElementIds.Add(entity.Id) && entity is IPublishableContentEntitySlim { Published: true })
                {
                    nextLevel.Add(entity.Id);
                }
            }

            currentLevel = nextLevel.ToArray();
        }

        return documentKeys.ToArray();
    }

    private IEnumerable<IUmbracoEntity> GetParentEntities(int[] childIds, UmbracoObjectTypes entityType)
        => childIds
            .InGroupsOf(Umbraco.Cms.Core.Constants.Sql.MaxParameterCount)
            .SelectMany(batch => _relationService.GetParentEntitiesByChildIds(
                batch,
                [Umbraco.Cms.Core.Constants.Conventions.RelationTypes.RelatedExternalBlockElementAlias],
                entityType));

    private int[] GetAllReferencedElementIds()
        => _relationService
            .GetByRelationTypeAlias(Umbraco.Cms.Core.Constants.Conventions.RelationTypes.RelatedExternalBlockElementAlias)
            .Select(relation => relation.ChildId)
            .Distinct()
            .ToArray();
}
