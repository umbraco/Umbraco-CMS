using Examine;
using Examine.Search;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Infrastructure.HostedServices;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Examine.Deferred;

internal sealed class DeliveryApiContentIndexHandleContentTypeChanges : DeliveryApiContentIndexDeferredBase, IDeferredAction
{
    private const int PageSize = 500;

    private readonly IList<KeyValuePair<int, ContentTypeChangeTypes>> _changes;
    private readonly DeliveryApiIndexingHandler _deliveryApiIndexingHandler;
    private readonly IDeliveryApiContentIndexValueSetBuilder _deliveryApiContentIndexValueSetBuilder;
    private readonly IContentService _contentService;
    private readonly IBackgroundTaskQueue _backgroundTaskQueue;
    private readonly IDeliveryApiCompositeIdHandler _deliveryApiCompositeIdHandler;

    [Obsolete("Use the constructor that takes an IDeliveryApiCompositeIdHandler instead, scheduled for removal in v15")]
    public DeliveryApiContentIndexHandleContentTypeChanges(
        IList<KeyValuePair<int, ContentTypeChangeTypes>> changes,
        DeliveryApiIndexingHandler deliveryApiIndexingHandler,
        IDeliveryApiContentIndexValueSetBuilder deliveryApiContentIndexValueSetBuilder,
        IContentService contentService,
        IBackgroundTaskQueue backgroundTaskQueue)
    : this(changes, deliveryApiIndexingHandler, deliveryApiContentIndexValueSetBuilder, contentService, backgroundTaskQueue, StaticServiceProvider.Instance.GetRequiredService<IDeliveryApiCompositeIdHandler>())
    {
    }

    public DeliveryApiContentIndexHandleContentTypeChanges(
        IList<KeyValuePair<int, ContentTypeChangeTypes>> changes,
        DeliveryApiIndexingHandler deliveryApiIndexingHandler,
        IDeliveryApiContentIndexValueSetBuilder deliveryApiContentIndexValueSetBuilder,
        IContentService contentService,
        IBackgroundTaskQueue backgroundTaskQueue,
        IDeliveryApiCompositeIdHandler deliveryApiCompositeIdHandler)
    {
        _changes = changes;
        _deliveryApiIndexingHandler = deliveryApiIndexingHandler;
        _deliveryApiContentIndexValueSetBuilder = deliveryApiContentIndexValueSetBuilder;
        _contentService = contentService;
        _backgroundTaskQueue = backgroundTaskQueue;
        _deliveryApiCompositeIdHandler = deliveryApiCompositeIdHandler;
    }

    public void Execute() => _backgroundTaskQueue.QueueBackgroundWorkItem(_ =>
    {
        var updatedContentTypeIds = new List<int>();

        // this looks a bit cumbersome, but we must iterate the changes in turn because the order matter; i.e. if a
        // content type is first changed, then deleted, we should not attempt to apply content type changes
        // NOTE: clean-up after content type deletion is performed by individual content cache refresh notifications for all deleted items
        foreach (KeyValuePair<int, ContentTypeChangeTypes> change in _changes)
        {
            if (change.Value.HasType(ContentTypeChangeTypes.Remove))
            {
                updatedContentTypeIds.Remove(change.Key);
            }
            else if (change.Value.HasType(ContentTypeChangeTypes.RefreshMain))
            {
                updatedContentTypeIds.Add(change.Key);
            }
        }

        if (updatedContentTypeIds.Any() is false)
        {
            return Task.CompletedTask;
        }

        IIndex index = _deliveryApiIndexingHandler.GetIndex() ??
                       throw new InvalidOperationException("Could not obtain the delivery API content index");

        HandleUpdatedContentTypes(updatedContentTypeIds, index);

        return Task.CompletedTask;
    });

    private void HandleUpdatedContentTypes(IEnumerable<int> updatedContentTypesIds, IIndex index)
    {
        foreach (var contentTypeId in updatedContentTypesIds)
        {
            List<string> indexIds = FindIdsForContentType(contentTypeId, index);

            // the index can contain multiple documents per content (for culture variant content). when reindexing below,
            // all documents are created "in one go", so we don't need to index the same document multiple times.
            // however, we need to keep track of the mapping between content IDs and their current (composite) index
            // IDs, since the index IDs can change here (if the content type culture variance is changed), and thus
            // we may have to clean up the current documents after reindexing.
            var indexIdsByContentIds = indexIds
                .Select(id =>
                {
                    DeliveryApiIndexCompositeIdModel compositeIdModel = _deliveryApiCompositeIdHandler.Decompose(id);
                    if (compositeIdModel.Id is null)
                    {
                        throw new InvalidOperationException($"Delivery API identifier should be composite of ID and culture, got: {id}");
                    }

                    return (ContentId: compositeIdModel.Id.Value, IndexId: compositeIdModel.Culture!);
                })
                .GroupBy(tuple => tuple.ContentId)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(t => t.IndexId).ToArray());

            // keep track of the IDs of the documents that must be removed, so we can remove them all in one go
            var indexIdsToRemove = new List<string>();

            foreach (KeyValuePair<int, string[]> indexIdsByContentId in indexIdsByContentIds)
            {
                IContent? content = _contentService.GetById(indexIdsByContentId.Key);
                if (content == null)
                {
                    // this should not happen if the rest of the indexing works as intended, but for good measure
                    // let's make sure we clean up all documents if the content does not exist
                    indexIdsToRemove.AddRange(indexIdsByContentId.Value);
                    continue;
                }

                // reindex the documents for this content
                ValueSet[] valueSets = _deliveryApiContentIndexValueSetBuilder.GetValueSets(content).ToArray();
                if (valueSets.Any())
                {
                    index.IndexItems(valueSets);
                }

                // if any of the document IDs have changed, make sure we clean up the previous ones
                indexIdsToRemove.AddRange(indexIdsByContentId.Value.Except(valueSets.Select(set => set.Id)));
            }

            RemoveFromIndex(indexIdsToRemove, index);
        }
    }

    private List<string> FindIdsForContentType(int contentTypeId, IIndex index)
    {
        var ids = new List<string>();

        var page = 0;
        var total = long.MaxValue;
        while (page * PageSize < total)
        {
            ISearchResults? results = index.Searcher
                .CreateQuery()
                .Field(UmbracoExamineFieldNames.DeliveryApiContentIndex.ContentTypeId, contentTypeId.ToString())
                // NOTE: we need to be explicit about fetching ItemIdFieldName here, otherwise Examine will try to be
                // clever and use the "id" field of the document (which we can't use for deletion)
                .SelectField(UmbracoExamineFieldNames.ItemIdFieldName)
                .Execute(QueryOptions.SkipTake(page * PageSize, PageSize));
            total = results.TotalItemCount;

            ids.AddRange(results.Select(result => result.Id));

            page++;
        }

        return ids;
    }
}
