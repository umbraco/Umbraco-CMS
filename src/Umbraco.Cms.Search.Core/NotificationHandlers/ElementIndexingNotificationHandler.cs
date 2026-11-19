using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Search.Indexing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Search.Core.Cache.Element;
using Umbraco.Extensions;

namespace Umbraco.Cms.Search.Core.NotificationHandlers;

/// <summary>
/// Re-indexes documents that reference a changed external (reusable) element - directly, or transitively through
/// other published elements - when external block element indexing is enabled.
/// </summary>
/// <remarks>
/// Reacts to <see cref="PublishedElementCacheRefresherNotification"/> - a Search-owned broadcast raised only when
/// an element is actually published, unpublished or trashed (see <see cref="PublishedElementNotificationHandler"/>)
/// - so a plain draft save of a reusable element never triggers a reindex of the documents referencing it, since
/// only a published change is ever reflected in the published index.
/// <para>
/// Also reacts to <see cref="ElementDeletingNotification"/>: a permanent delete removes the element's relations along
/// with the element itself, so the referencing documents can only be found before the delete happens.
/// </para>
/// </remarks>
internal sealed class ElementIndexingNotificationHandler : IndexingNotificationHandlerBase,
    INotificationHandler<PublishedElementCacheRefresherNotification>,
    INotificationHandler<ElementDeletingNotification>
{
    private readonly IContentIndexingService _contentIndexingService;
    private readonly IRelationService _relationService;
    private readonly IOptions<IndexingSettings> _indexingSettings;
    private readonly IIndexDocumentService _indexDocumentService;
    private readonly IContentService _contentService;
    private readonly IDistributedContentIndexRefresher _distributedContentIndexRefresher;

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementIndexingNotificationHandler"/> class.
    /// </summary>
    /// <param name="coreScopeProvider">The scope provider used to defer actions until the ambient scope completes.</param>
    /// <param name="contentIndexingService">The service used to re-index the affected documents.</param>
    /// <param name="relationService">The service used to traverse element-to-document and element-to-element references.</param>
    /// <param name="indexingSettings">The indexing settings, used to determine whether external element content is indexed at all.</param>
    /// <param name="indexDocumentService">The service used to flush the change-detection cache for affected documents.</param>
    /// <param name="contentService">The service used to load the documents referencing a deleted element.</param>
    /// <param name="distributedContentIndexRefresher">The refresher used to re-index the documents referencing a deleted element on all servers.</param>
    public ElementIndexingNotificationHandler(
        ICoreScopeProvider coreScopeProvider,
        IContentIndexingService contentIndexingService,
        IRelationService relationService,
        IOptions<IndexingSettings> indexingSettings,
        IIndexDocumentService indexDocumentService,
        IContentService contentService,
        IDistributedContentIndexRefresher distributedContentIndexRefresher)
        : base(coreScopeProvider)
    {
        _contentIndexingService = contentIndexingService;
        _relationService = relationService;
        _indexingSettings = indexingSettings;
        _indexDocumentService = indexDocumentService;
        _contentService = contentService;
        _distributedContentIndexRefresher = distributedContentIndexRefresher;
    }

    /// <summary>
    /// Re-indexes the documents that reference the elements published, unpublished or trashed as described by the notification.
    /// </summary>
    /// <param name="notification">The notification describing the element publish status changes to react to.</param>
    public void Handle(PublishedElementCacheRefresherNotification notification)
    {
        // external element content only ever participates in the index when the feature is enabled; with it off,
        // referencing documents have nothing to refresh.
        if (_indexingSettings.Value.IndexExternalBlockElements is false)
        {
            return;
        }

        PublishedElementCacheRefresher.JsonPayload[] payloads = GetNotificationPayloads<PublishedElementCacheRefresher.JsonPayload>(notification, out var origin);

        ReindexDocumentsReferencing(payloads.Select(payload => payload.Id).Distinct().ToArray(), origin);
    }

    /// <summary>
    /// Re-indexes the documents that reference the published elements about to be permanently deleted, once the delete has completed.
    /// </summary>
    /// <param name="notification">The notification describing the elements about to be deleted.</param>
    /// <remarks>
    /// This notification is only raised on the server performing the delete, so unlike the other re-indexing in this
    /// handler, the re-index is broadcast to all servers.
    /// </remarks>
    public void Handle(ElementDeletingNotification notification)
    {
        if (_indexingSettings.Value.IndexExternalBlockElements is false)
        {
            return;
        }

        // a trashed or unpublished element is already absent from every published index entry
        var deletedElementIds = notification.DeletedEntities
            .Where(element => element is { Published: true, Trashed: false })
            .Select(element => element.Id)
            .ToArray();
        if (deletedElementIds.Length == 0)
        {
            return;
        }

        Guid[] documentKeys = FindDocumentKeysReferencingElements(deletedElementIds);
        if (documentKeys.Length == 0)
        {
            return;
        }

        ExecuteDeferred(() =>
        {
            IContent[] documents = documentKeys
                .InGroupsOf(Umbraco.Cms.Core.Constants.Sql.MaxParameterCount)
                .SelectMany(batch => _contentService.GetByIds(batch))
                .ToArray();
            _distributedContentIndexRefresher.RefreshContent(documents, ContentState.Published);
        });
    }

    /// <summary>
    /// Finds the documents that reference the given elements, directly or transitively through other published elements.
    /// </summary>
    /// <remarks>
    /// Performs a breadth-first traversal of the external block element relation graph: a changed element can be
    /// referenced directly by documents, or by other elements (which are themselves referenced by documents, or
    /// further elements). Climbing only continues through a published element - an unpublished element's content
    /// (and anything nested below it) is not part of any document's published index, so a change below it cannot
    /// affect one further up.
    /// <para>
    /// Internal (rather than private) so integration tests can verify the traversal directly - the pruning at an
    /// unpublished intermediate element has no observable effect on index content (the index-time flattening already
    /// excludes it independently), so it can only be verified by calling this method directly.
    /// </para>
    /// </remarks>
    /// <param name="elementIds">The IDs of the changed elements.</param>
    /// <returns>The keys of the documents referencing the elements.</returns>
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

    private void ReindexDocumentsReferencing(int[] changedElementIds, string origin)
    {
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

        ExecuteDeferred(() =>
        {
            // the referencing documents' own content is unchanged, so their persisted index document snapshots are
            // still in the change-detection cache - without flushing them first, the reindex below would just find
            // and re-use the stale snapshot instead of re-collecting property values (see IIndexDocumentService).
            // This has to wait until the ambient scope completes, same as the reindex call itself - running it
            // eagerly, while the triggering save/publish request's own scope is still open, deadlocks.
            _indexDocumentService.DeleteAsync(documentKeys, true).GetAwaiter().GetResult();
            _contentIndexingService.Handle(changes, origin);
        });
    }

    private IEnumerable<IUmbracoEntity> GetParentEntities(int[] childIds, UmbracoObjectTypes entityType)
        => childIds
            .InGroupsOf(Umbraco.Cms.Core.Constants.Sql.MaxParameterCount)
            .SelectMany(batch => _relationService.GetParentEntitiesByChildIds(
                batch,
                [Umbraco.Cms.Core.Constants.Conventions.RelationTypes.RelatedExternalBlockElementAlias],
                entityType));
}
