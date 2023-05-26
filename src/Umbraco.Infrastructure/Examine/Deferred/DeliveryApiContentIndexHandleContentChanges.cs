using Examine;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Infrastructure.HostedServices;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Examine.Deferred;

internal sealed class DeliveryApiContentIndexHandleContentChanges : DeliveryApiContentIndexDeferredBase, IDeferredAction
{
    private readonly IList<KeyValuePair<int, TreeChangeTypes>> _changes;
    private readonly IContentService _contentService;
    private readonly DeliveryApiIndexingHandler _deliveryApiIndexingHandler;
    private readonly IDeliveryApiContentIndexValueSetBuilder _deliveryApiContentIndexValueSetBuilder;
    private readonly IDeliveryApiContentIndexHelper _deliveryApiContentIndexHelper;
    private readonly IBackgroundTaskQueue _backgroundTaskQueue;

    public DeliveryApiContentIndexHandleContentChanges(
        IList<KeyValuePair<int, TreeChangeTypes>> changes,
        DeliveryApiIndexingHandler deliveryApiIndexingHandler,
        IContentService contentService,
        IDeliveryApiContentIndexValueSetBuilder deliveryApiContentIndexValueSetBuilder,
        IDeliveryApiContentIndexHelper deliveryApiContentIndexHelper,
        IBackgroundTaskQueue backgroundTaskQueue)
    {
        _changes = changes;
        _deliveryApiIndexingHandler = deliveryApiIndexingHandler;
        _contentService = contentService;
        _backgroundTaskQueue = backgroundTaskQueue;
        _deliveryApiContentIndexValueSetBuilder = deliveryApiContentIndexValueSetBuilder;
        _deliveryApiContentIndexHelper = deliveryApiContentIndexHelper;
    }

    public void Execute() => _backgroundTaskQueue.QueueBackgroundWorkItem(_ =>
    {
        IIndex index = _deliveryApiIndexingHandler.GetIndex()
                       ?? throw new InvalidOperationException("Could not obtain the delivery API content index");

        var pendingRemovals = new List<int>();
        foreach ((int contentId, TreeChangeTypes changeTypes) in _changes)
        {
            var remove = changeTypes.HasType(TreeChangeTypes.Remove);
            var reindex = changeTypes.HasType(TreeChangeTypes.RefreshNode) || changeTypes.HasType(TreeChangeTypes.RefreshBranch);

            if (remove)
            {
                pendingRemovals.Add(contentId);
            }
            else if (reindex)
            {
                IContent? content = _contentService.GetById(contentId);
                if (content == null || content.Trashed)
                {
                    pendingRemovals.Add(contentId);
                    continue;
                }

                RemoveFromIndex(pendingRemovals, index);
                pendingRemovals.Clear();

                Reindex(content, index);
            }
        }

        RemoveFromIndex(pendingRemovals, index);

        return Task.CompletedTask;
    });

    private void Reindex(IContent content, IIndex index)
    {
        // get the currently indexed cultures for the content
        var existingIndexCultures = index
            .Searcher
            .CreateQuery()
            .Field(UmbracoExamineFieldNames.DeliveryApiContentIndex.Id, content.Id.ToString())
            .SelectField(UmbracoExamineFieldNames.DeliveryApiContentIndex.Culture)
            .Execute()
            .SelectMany(f => f.GetValues(UmbracoExamineFieldNames.DeliveryApiContentIndex.Culture))
            .ToArray();

        // index the content
        var indexedCultures = UpdateIndex(content, index);
        if (indexedCultures.Any() is false)
        {
            // we likely got here because unpublishing triggered a "refresh branch" notification, now we
            // need to delete every last culture of this content and all descendants
            RemoveFromIndex(content.Id, index);
            return;
        }

        // if any of the content cultures did not exist in the index before, nor will any of its published descendants
        // in those cultures be at this point, so make sure those are added as well
        if (indexedCultures.Except(existingIndexCultures).Any())
        {
            ReindexDescendants(content, index);
        }

        // ensure that any unpublished cultures are removed from the index
        var unpublishedCultures = existingIndexCultures.Except(indexedCultures).ToArray();
        if (unpublishedCultures.Any() is false)
        {
            return;
        }

        var idsToDelete = unpublishedCultures
            .Select(culture => DeliveryApiContentIndexUtilites.IndexId(content, culture)).ToArray();
        RemoveFromIndex(idsToDelete, index);
    }

    private string[] UpdateIndex(IContent content, IIndex index)
    {
        ValueSet[] valueSets = _deliveryApiContentIndexValueSetBuilder.GetValueSets(content).ToArray();
        if (valueSets.Any() is false)
        {
            return Array.Empty<string>();
        }

        index.IndexItems(valueSets);
        return valueSets
            .SelectMany(v => v.GetValues("culture").Select(c => c.ToString()))
            .WhereNotNull()
            .ToArray();
    }

    private void ReindexDescendants(IContent content, IIndex index)
        => _deliveryApiContentIndexHelper.EnumerateApplicableDescendantsForContentIndex(
            content.Id,
            descendants =>
            {
                foreach (IContent descendant in descendants)
                {
                    UpdateIndex(descendant, index);
                }
            });
}
