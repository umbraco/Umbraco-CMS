using Examine;
using Examine.Lucene;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Examine;

public class DeliveryApiContentIndex : UmbracoExamineIndex
{
    private readonly ILogger<DeliveryApiContentIndex> _logger;

    public DeliveryApiContentIndex(
        ILoggerFactory loggerFactory,
        string name,
        IOptionsMonitor<LuceneDirectoryIndexOptions> indexOptions,
        IHostingEnvironment hostingEnvironment,
        IRuntimeState runtimeState)
        : base(loggerFactory, name, indexOptions, hostingEnvironment, runtimeState)
    {
        PublishedValuesOnly = true;
        EnableDefaultEventHandler = false;

        _logger = loggerFactory.CreateLogger<DeliveryApiContentIndex>();

        // so... Examine lazily resolves the field value types, and incidentally this currently only happens at indexing time.
        // however, we really must have the correct value types at boot time, so we'll forcefully resolve the value types here.
        // this is, in other words, a workaround.
        if (FieldValueTypeCollection.ValueTypes.Any() is false)
        {
            // we should never ever get here
            _logger.LogError("No value types defined for the delivery API content index");
        }
    }

    /// <inheritdoc />
    /// <summary>
    ///     Deletes a node from the index.
    /// </summary>
    /// <remarks>
    ///     When a content node is deleted, we also need to delete it's children from the index so we need to perform a
    ///     custom Lucene search to find all decendents and create Delete item queues for them too.
    /// </remarks>
    /// <param name="itemIds">ID of the node to delete</param>
    /// <param name="onComplete"></param>
    protected override void PerformDeleteFromIndex(IEnumerable<string> itemIds, Action<IndexOperationEventArgs>? onComplete)
    {
        var removedContentIds = new List<string>();
        foreach (var itemId in itemIds)
        {
            // an item ID passed to this method can be a composite of content ID and culture (like "1234|da-DK") or simply a content ID
            // - when it's a composite ID, only the supplied culture of the given item should be deleted from the index
            // - when it's an content ID, all cultures of the of the given item should be deleted from the index
            var (contentId, culture) = ParseItemId(itemId);
            if (contentId == null || removedContentIds.Contains(contentId))
            {
                _logger.LogWarning("Could not parse item ID; expected integer or composite ID, got: {itemId}", itemId);
                continue;
            }

            // find descendants-or-self based on path and optional culture
            var rawQuery = $"({UmbracoExamineFieldNames.IndexPathFieldName}:\\-1*,{contentId} OR {UmbracoExamineFieldNames.IndexPathFieldName}:\\-1*,{contentId},*)";
            if (culture != null)
            {
                rawQuery = $"{rawQuery} AND culture:{culture}";
            }

            ISearchResults results = Searcher
                .CreateQuery()
                .NativeQuery(rawQuery)
                // NOTE: we need to be explicit about fetching ItemIdFieldName here, otherwise Examine will try to be
                // clever and use the "id" field of the document (which we can't use for deletion)
                .SelectField(UmbracoExamineFieldNames.ItemIdFieldName)
                .Execute();

            _logger.LogDebug("DeleteFromIndex with query: {Query} (found {TotalItems} results)", rawQuery, results.TotalItemCount);

            // grab the index IDs from the index (the composite IDs)
            var indexIds = results.Select(x => x.Id).ToList();

            // remember which items we removed, so we can skip those later
            removedContentIds.AddRange(indexIds.Select(indexId => ParseItemId(indexId).ContentId).WhereNotNull());

            // delete the resulting items from the index
            base.PerformDeleteFromIndex(indexIds, null);
        }
    }

    private (string? ContentId, string? Culture) ParseItemId(string id)
    {
        if (int.TryParse(id, out _))
        {
            return (id, null);
        }

        var parts = id.Split(Constants.CharArrays.VerticalTab);
        if (parts.Length == 2 && int.TryParse(parts[0], out _))
        {
            return (parts[0], parts[1]);
        }

        return (null, null);
    }

    protected override void OnTransformingIndexValues(IndexingItemEventArgs e)
    {
        // UmbracoExamineIndex (base class down the hierarchy) performs some magic transformations here for paths and icons;
        // we don't want that for the Delivery API, so we'll have to override this method and simply do nothing.
    }
}
