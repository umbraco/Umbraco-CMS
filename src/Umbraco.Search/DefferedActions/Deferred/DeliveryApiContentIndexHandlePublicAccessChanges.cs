using Examine;
using Examine.Search;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.HostedServices;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Examine.Deferred;

internal sealed class DeliveryApiContentIndexHandlePublicAccessChanges : DeliveryApiContentIndexDeferredBase, IDeferredAction
{
    private readonly IPublicAccessService _publicAccessService;
    private readonly DeliveryApiIndexingHandler _deliveryApiIndexingHandler;
    private readonly IBackgroundTaskQueue _backgroundTaskQueue;

    public DeliveryApiContentIndexHandlePublicAccessChanges(
        IPublicAccessService publicAccessService,
        DeliveryApiIndexingHandler deliveryApiIndexingHandler,
        IBackgroundTaskQueue backgroundTaskQueue)
    {
        _publicAccessService = publicAccessService;
        _deliveryApiIndexingHandler = deliveryApiIndexingHandler;
        _backgroundTaskQueue = backgroundTaskQueue;
    }

    public void Execute() => _backgroundTaskQueue.QueueBackgroundWorkItem(_ =>
    {
        // NOTE: at the time of implementing this, the distributed notifications for public access changes only ever
        //       sends out "refresh all" notifications, which means we can't be clever about minimizing the work
        //       effort to handle public access changes. instead we have to grab all protected content definitions
        //       and handle every last one with every notification.

        // NOTE: eventually the Delivery API will support protected content, but for now we need to ensure that the
        //       index does not contain any protected content. this also means that whenever content is unprotected,
        //       one must trigger a manual republish of said content for it to be re-added to the index. not exactly
        //       an optimal solution, but it's the best we can do at this point, given the limitations outlined above
        //       and without prematurely assuming the future implementation details of protected content handling.

        var protectedContentIds = _publicAccessService.GetAll().Select(entry => entry.ProtectedNodeId).ToArray();
        if (protectedContentIds.Any() is false)
        {
            return Task.CompletedTask;
        }

        IIndex index = _deliveryApiIndexingHandler.GetIndex() ??
                       throw new InvalidOperationException("Could not obtain the delivery API content index");

        List<string> indexIds = FindIndexIdsForContentIds(protectedContentIds, index);
        if (indexIds.Any() is false)
        {
            return Task.CompletedTask;
        }

        RemoveFromIndex(indexIds, index);
        return Task.CompletedTask;
    });

    private List<string> FindIndexIdsForContentIds(int[] contentIds, IIndex index)
    {
        const int pageSize = 500;
        const int batchSize = 50;

        var ids = new List<string>();

        foreach (IEnumerable<int> batch in contentIds.InGroupsOf(batchSize))
        {
            IEnumerable<int> batchAsArray = batch as int[] ?? batch.ToArray();
            var page = 0;
            var total = long.MaxValue;

            while (page * pageSize < total)
            {
                ISearchResults? results = index.Searcher
                    .CreateQuery()
                    .GroupedOr(new[] { UmbracoExamineFieldNames.DeliveryApiContentIndex.Id }, batchAsArray.Select(id => id.ToString()).ToArray())
                    // NOTE: we need to be explicit about fetching ItemIdFieldName here, otherwise Examine will try to be
                    // clever and use the "id" field of the document (which we can't use for deletion)
                    .SelectField(UmbracoExamineFieldNames.ItemIdFieldName)
                    .Execute(QueryOptions.SkipTake(page * pageSize, pageSize));
                total = results.TotalItemCount;

                ids.AddRange(results.Select(result => result.Id));

                page++;
            }
        }

        return ids;
    }

}
