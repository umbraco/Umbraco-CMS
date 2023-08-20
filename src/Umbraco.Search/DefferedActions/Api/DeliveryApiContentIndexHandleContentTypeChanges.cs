using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Search;
using Umbraco.Cms.Core.Search;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Infrastructure.HostedServices;
using Umbraco.Extensions;

namespace Umbraco.Search.DefferedActions.Api;

internal sealed class DeliveryApiContentIndexHandleContentTypeChanges : DeliveryApiContentIndexDeferredBase,
    IDeferredAction
{
    private const int PageSize = 500;

    private readonly IList<KeyValuePair<int, ContentTypeChangeTypes>> _changes;
    private readonly DeliveryApiIndexingHandler _deliveryApiIndexingHandler;
    private readonly IContentService _contentService;
    private readonly IBackgroundTaskQueue _backgroundTaskQueue;

    public DeliveryApiContentIndexHandleContentTypeChanges(
        IList<KeyValuePair<int, ContentTypeChangeTypes>> changes,
        DeliveryApiIndexingHandler deliveryApiIndexingHandler,
        IContentService contentService,
        ISearchProvider searchProvider,
        IBackgroundTaskQueue backgroundTaskQueue) : base(searchProvider)
    {
        _changes = changes;
        _deliveryApiIndexingHandler = deliveryApiIndexingHandler;
        _contentService = contentService;
        _backgroundTaskQueue = backgroundTaskQueue;
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

        IUmbracoIndex<IContentBase> index = _deliveryApiIndexingHandler.GetIndex() ??
                                        throw new InvalidOperationException(
                                            "Could not obtain the delivery API content index");

        IUmbracoSearcher searcher = _deliveryApiIndexingHandler.GetSearcher() ??
                                    throw new InvalidOperationException(
                                        "Could not obtain the delivery API content searcher");

        HandleUpdatedContentTypes(updatedContentTypeIds, index, searcher);

        return Task.CompletedTask;
    });

    private void HandleUpdatedContentTypes(IEnumerable<int> updatedContentTypesIds, IUmbracoIndex<IContentBase> index,
        IUmbracoSearcher searcher)
    {
        foreach (var contentTypeId in updatedContentTypesIds)
        {
            List<string> indexIds = FindIdsForContentType(contentTypeId, searcher);

            // the index can contain multiple documents per content (for culture variant content). when reindexing below,
            // all documents are created "in one go", so we don't need to index the same document multiple times.
            // however, we need to keep track of the mapping between content IDs and their current (composite) index
            // IDs, since the index IDs can change here (if the content type culture variance is changed), and thus
            // we may have to clean up the current documents after reindexing.
            var indexIdsByContentIds = GetIndexIdsbyContenIds(indexIds, index);

            // keep track of the IDs of the documents that must be removed, so we can remove them all in one go
            var indexIdsToRemove = new List<string>();
            List<IContent> contentToBeIndex = new List<IContent>();
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
                contentToBeIndex.Add(content);
                // if any of the document IDs have changed, make sure we clean up the previous ones
                indexIdsToRemove.AddRange(
                    indexIdsByContentId.Value.Except(contentToBeIndex.Select(set => set.Id.ToString())));
            }

            if (contentToBeIndex.Any())
            {
                index.IndexItems(contentToBeIndex.ToArray());
            }

            RemoveFromIndex(indexIdsToRemove, index);
        }
    }

    private IEnumerable<KeyValuePair<int, string[]>> GetIndexIdsbyContenIds(List<string> indexIds,
        IUmbracoIndex<IContentBase> index)
    {
        return indexIds
            .Select(id =>
            {
                var parts = id.Split(Constants.CharArrays.VerticalTab);
                return parts.Length == 2 && int.TryParse(parts[0], out var contentId)
                    ? (ContentId: contentId, IndexId: id)
                    : throw new InvalidOperationException(
                        $"Delivery API identifier should be composite of ID and culture, got: {id}");
            })
            .GroupBy(tuple => tuple.ContentId)
            .ToDictionary(
                group => group.Key,
                group => group.Select(t => t.IndexId).ToArray());
    }

    private List<string> FindIdsForContentType(int contentTypeId, IUmbracoSearcher index)
    {
        var ids = new List<string>();

        var page = 0;
        var total = long.MaxValue;
        while (page * PageSize < total)
        {
            var searchRequest = index.CreateSearchRequest();
            searchRequest.CreateFilter(UmbracoSearchFieldNames.DeliveryApiContentIndex.ContentTypeId,
                new[] { contentTypeId.ToString() }.ToList(), LogicOperator.OR);
            searchRequest.Page = page;
            searchRequest.PageSize = PageSize;
            IUmbracoSearchResults? results =
                index.Search(searchRequest
                );
            total = results.TotalRecords;

            if (results.Results != null)
            {
                ids.AddRange(results.Results.Select(result => result.Id));
            }

            page++;
        }

        return ids;
    }
}
