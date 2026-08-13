using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Search;

/// <summary>
/// Queues reindexing of documents whose search index depends on an element whenever that element's cache is refreshed
/// (publish, unpublish, trash or delete).
/// </summary>
/// <remarks>
/// Bound to the element cache-refresher notification, which is broadcast to every server via the distributed cache,
/// so each server reindexes its own local search index. Entity notifications (e.g. published/unpublished) would only
/// fire on the server that handled the request.
/// </remarks>
internal sealed class ElementIndexingNotificationHandler : INotificationHandler<ElementCacheRefresherNotification>
{
    private readonly IDeferredSearchReindexService _deferredSearchReindexService;
    private readonly IUmbracoIndexingHandler _umbracoIndexingHandler;
    private readonly IOptionsMonitor<IndexingSettings> _indexingSettings;

    public ElementIndexingNotificationHandler(
        IDeferredSearchReindexService deferredSearchReindexService,
        IUmbracoIndexingHandler umbracoIndexingHandler,
        IOptionsMonitor<IndexingSettings> indexingSettings)
    {
        _deferredSearchReindexService = deferredSearchReindexService;
        _umbracoIndexingHandler = umbracoIndexingHandler;
        _indexingSettings = indexingSettings;
    }

    public void Handle(ElementCacheRefresherNotification notification)
    {
        // Nothing to reindex when the feature is disabled; skip early to avoid waking the background worker.
        if (_indexingSettings.CurrentValue.IndexExternalBlockElements is false)
        {
            return;
        }

        if (_umbracoIndexingHandler.Enabled is false || Suspendable.ExamineEvents.CanIndex is false)
        {
            return;
        }

        if (notification.MessageType != MessageType.RefreshByPayload)
        {
            return;
        }

        // Reindex on publish/unpublish (which carry the affected cultures), trash (a branch refresh) and delete (a
        // Remove). A plain draft save is only a cultureless RefreshNode, so it is ignored - drafts never enter the index.
        var elementIds = ((ElementCacheRefresher.JsonPayload[])notification.MessageObject)
            .Where(payload => payload.ChangeTypes.HasType(TreeChangeTypes.Remove)
                || payload.ChangeTypes.HasType(TreeChangeTypes.RefreshBranch)
                || payload.PublishedCultures?.Length > 0
                || payload.UnpublishedCultures?.Length > 0)
            .Select(payload => payload.Id)
            .Distinct()
            .ToArray();

        if (elementIds.Length == 0)
        {
            return;
        }

        _deferredSearchReindexService.QueueReindexOnElementChange(elementIds);
    }
}
