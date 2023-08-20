using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Search;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Infrastructure.HostedServices;
using Umbraco.Extensions;
using Umbraco.Search.Services;

namespace Umbraco.Search.DefferedActions.Api;

internal sealed class DeliveryApiContentIndexHandleContentChanges : DeliveryApiContentIndexDeferredBase, IDeferredAction
{
    private readonly IList<KeyValuePair<int, TreeChangeTypes>> _changes;
    private readonly IContentService _contentService;
    private readonly DeliveryApiIndexingHandler _deliveryApiIndexingHandler;
    private readonly IDeliveryApiContentIndexHelper _deliveryApiContentIndexHelper;
    private readonly IBackgroundTaskQueue _backgroundTaskQueue;

    public DeliveryApiContentIndexHandleContentChanges(
        IList<KeyValuePair<int, TreeChangeTypes>> changes,
        DeliveryApiIndexingHandler deliveryApiIndexingHandler,
        IDeliveryApiContentIndexHelper deliveryApiContentIndexHelper,
        IContentService contentService,
        ISearchProvider searchProvider,
        IBackgroundTaskQueue backgroundTaskQueue) : base(searchProvider)
    {
        _changes = changes;
        _deliveryApiIndexingHandler = deliveryApiIndexingHandler;
        _deliveryApiContentIndexHelper = deliveryApiContentIndexHelper;
        _contentService = contentService;
        _backgroundTaskQueue = backgroundTaskQueue;
    }

    public void Execute() => _backgroundTaskQueue.QueueBackgroundWorkItem(_ =>
    {
        IUmbracoIndex<IContentBase> index = _deliveryApiIndexingHandler.GetIndex()
                       ?? throw new InvalidOperationException("Could not obtain the delivery API content index");
        IUmbracoSearcher searcher = _deliveryApiIndexingHandler.GetSearcher()
                              ?? throw new InvalidOperationException("Could not obtain the delivery API content searcher");


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

                Reindex(content, index, searcher);
            }
        }

        RemoveFromIndex(pendingRemovals, index);

        return Task.CompletedTask;
    });

    private void Reindex(IContent content, IUmbracoIndex<IContentBase> index, IUmbracoSearcher searcher)
    {
        var searchRequest = searcher.CreateSearchRequest();
        searchRequest.CreateFilter(UmbracoSearchFieldNames.DeliveryApiContentIndex.Id,new[] { content.Id.ToString() }.ToList(), LogicOperator.OR);
        searchRequest.Page = 0;
        searchRequest.PageSize = 10000;
        // get the currently indexed cultures for the content
        var umbracoSearchResults = searcher.Search(searchRequest).Results;
        if (umbracoSearchResults != null)
        {
            var existingIndexCultures = umbracoSearchResults
                .SelectMany(f => f.Values[UmbracoSearchFieldNames.DeliveryApiContentIndex.Culture])
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
                .Select(culture => DeliveryApiContentIndexUtilites.IndexId(content, culture.ToString())).ToArray();
            RemoveFromIndex(idsToDelete, index);
        }
    }

    private string[] UpdateIndex(IContent content, IUmbracoIndex<IContentBase> index)
    {

        index.IndexItems(new []{content});
        return content.PublishedCultures.ToArray();
    }

    private void ReindexDescendants(IContent content, IUmbracoIndex<IContentBase>  index)
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
