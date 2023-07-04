using Umbraco.Cms.Core.Models.Search;
using Umbraco.Cms.Core.Search;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.HostedServices;
using Umbraco.Extensions;

namespace Umbraco.Search.DefferedActions;

internal sealed class DeliveryApiContentIndexHandlePublicAccessChanges : DeliveryApiContentIndexDeferredBase, IDeferredAction
{
    private readonly IPublicAccessService _publicAccessService;
    private readonly DeliveryApiIndexingHandler _deliveryApiIndexingHandler;
    private readonly IBackgroundTaskQueue _backgroundTaskQueue;

    public DeliveryApiContentIndexHandlePublicAccessChanges(
        IPublicAccessService publicAccessService,
        DeliveryApiIndexingHandler deliveryApiIndexingHandler,
        ISearchProvider searchProvider,
        IBackgroundTaskQueue backgroundTaskQueue) : base(searchProvider)
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

        IUmbracoIndex index = _deliveryApiIndexingHandler.GetIndex() ??
                       throw new InvalidOperationException("Could not obtain the delivery API content index");
        IUmbracoSearcher searcher = _deliveryApiIndexingHandler.GetSearcher() ??
                              throw new InvalidOperationException("Could not obtain the delivery API content searcher");

        List<string> indexIds = FindIndexIdsForContentIds(protectedContentIds, searcher);
        if (indexIds.Any() is false)
        {
            return Task.CompletedTask;
        }

        RemoveFromIndex(indexIds, index);
        return Task.CompletedTask;
    });

    private List<string> FindIndexIdsForContentIds(int[] contentIds, IUmbracoSearcher searcher)
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
                var searchRequest = searcher.CreateSearchRequest();
                searchRequest.CreateFilter(UmbracoSearchFieldNames.DeliveryApiContentIndex.Id,batchAsArray.Select(id => id.ToString()).ToList(), LogicOperator.OR);
                ;
                searchRequest.Page = page;
                searchRequest.PageSize = pageSize;
                IUmbracoSearchResults? results =
                    searcher.Search(  searchRequest);

                total = results.TotalItemCount;

                ids.AddRange(results.Select(result => result.Id));

                page++;
            }
        }

        return ids;
    }

}
