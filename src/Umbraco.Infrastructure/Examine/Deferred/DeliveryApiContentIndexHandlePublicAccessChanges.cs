using Examine;
using Examine.Search;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.HostedServices;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Examine.Deferred;

internal sealed class DeliveryApiContentIndexHandlePublicAccessChanges : DeliveryApiContentIndexDeferredBase, IDeferredAction
{
    private readonly IPublicAccessService _publicAccessService;
    private readonly DeliveryApiIndexingHandler _deliveryApiIndexingHandler;
    private readonly IDeliveryApiContentIndexHelper _deliveryApiContentIndexHelper;
    private readonly IBackgroundTaskQueue _backgroundTaskQueue;
    private readonly DeliveryApiSettings _deliveryApiSettings;
    private readonly IContentService _contentService;
    private readonly IDeliveryApiContentIndexValueSetBuilder _deliveryApiContentIndexValueSetBuilder;

    public DeliveryApiContentIndexHandlePublicAccessChanges(
        IPublicAccessService publicAccessService,
        DeliveryApiIndexingHandler deliveryApiIndexingHandler,
        IContentService contentService,
        IDeliveryApiContentIndexValueSetBuilder deliveryApiContentIndexValueSetBuilder,
        IDeliveryApiContentIndexHelper deliveryApiContentIndexHelper,
        DeliveryApiSettings deliveryApiSettings,
        IBackgroundTaskQueue backgroundTaskQueue)
    {
        _publicAccessService = publicAccessService;
        _deliveryApiIndexingHandler = deliveryApiIndexingHandler;
        _contentService = contentService;
        _deliveryApiContentIndexValueSetBuilder = deliveryApiContentIndexValueSetBuilder;
        _deliveryApiContentIndexHelper = deliveryApiContentIndexHelper;
        _deliveryApiSettings = deliveryApiSettings;
        _backgroundTaskQueue = backgroundTaskQueue;
    }

    // NOTE: at the time of implementing this, the distributed notifications for public access changes only ever
    //       sends out "refresh all" notifications, which means we can't be clever about minimizing the work
    //       effort to handle public access changes. instead we have to grab all protected content definitions
    //       and handle every last one with every notification.
    public void Execute() => _backgroundTaskQueue.QueueBackgroundWorkItem(_ =>
    {
        IIndex index = _deliveryApiIndexingHandler.GetIndex() ??
                       throw new InvalidOperationException("Could not obtain the delivery API content index");

        if (_deliveryApiSettings.MemberAuthorizationIsEnabled() is false)
        {
            EnsureProtectedContentIsRemovedFromIndex(index);
            return Task.CompletedTask;
        }

        EnsureProtectedContentIsUpToDateInIndex(index);
        return Task.CompletedTask;
    });

    private void EnsureProtectedContentIsRemovedFromIndex(IIndex index)
    {
        var protectedContentIds = _publicAccessService.GetAll().Select(entry => entry.ProtectedNodeId).ToArray();
        if (protectedContentIds.Any() is false)
        {
            return;
        }

        List<string> indexIds = FindIndexIdsForContentIds(protectedContentIds, index);
        if (indexIds.Any() is false)
        {
            return;
        }

        RemoveFromIndex(indexIds, index);
    }

    private void EnsureProtectedContentIsUpToDateInIndex(IIndex index)
    {
        // first we need to re-index all the content items that are currently known to be protected in the index,
        // as their protection might have been revoked or altered.
        var protectedContentIdsInIndex = FindContentIdsForProtectedContent(index);
        foreach (var contentId in protectedContentIdsInIndex)
        {
            UpdateIndex(contentId, index);
        }

        // then we have to re-index any protected content items that were not part of the first operation.
        var unhandledProtectedContentIds = _publicAccessService
            .GetAll()
            .Select(entry => entry.ProtectedNodeId)
            .Except(protectedContentIdsInIndex)
            .ToArray();

        foreach (var contentId in unhandledProtectedContentIds)
        {
            UpdateIndexWithDescendants(contentId, index);
        }
    }

    private void UpdateIndexWithDescendants(int contentId, IIndex index)
    {
        if (UpdateIndex(contentId, index))
        {
            _deliveryApiContentIndexHelper.EnumerateApplicableDescendantsForContentIndex(
                contentId,
                descendants =>
                {
                    foreach (IContent descendant in descendants)
                    {
                        UpdateIndex(descendant, index);
                    }
                });
        }
    }

    private bool UpdateIndex(int contentId, IIndex index)
    {
        IContent? content = _contentService.GetById(contentId);
        return content is not null && UpdateIndex(content, index);
    }

    private bool UpdateIndex(IContent content, IIndex index)
    {
        if (content.Trashed)
        {
            return false;
        }

        ValueSet[] valueSets = _deliveryApiContentIndexValueSetBuilder.GetValueSets(content).ToArray();
        if (valueSets.Any())
        {
            index.IndexItems(valueSets);
        }

        return true;
    }

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

    private int[] FindContentIdsForProtectedContent(IIndex index)
    {
        const int pageSize = 500;

        var ids = new List<int>();

        var page = 0;
        var total = long.MaxValue;

        while (page * pageSize < total)
        {
            ISearchResults? results = index.Searcher
                .CreateQuery()
                .Field(UmbracoExamineFieldNames.DeliveryApiContentIndex.Protected, "y")
                .SelectField(UmbracoExamineFieldNames.DeliveryApiContentIndex.Id)
                .Execute(QueryOptions.SkipTake(page * pageSize, pageSize));
            total = results.TotalItemCount;

            ids.AddRange(results.Select(result => int.Parse(result[UmbracoExamineFieldNames.DeliveryApiContentIndex.Id])));

            page++;
        }

        return ids.Distinct().ToArray();
    }
}
