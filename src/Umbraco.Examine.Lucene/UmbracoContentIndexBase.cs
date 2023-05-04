using Examine;
using Examine.Lucene;
using Examine.Search;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.Examine;

public abstract class UmbracoContentIndexBase : UmbracoExamineIndex
{
    private readonly ISet<string> _idOnlyFieldSet = new HashSet<string> { "id" };
    private readonly ILogger<UmbracoContentIndex> _logger;

    protected UmbracoContentIndexBase(
        ILoggerFactory loggerFactory,
        string name,
        IOptionsMonitor<LuceneDirectoryIndexOptions> indexOptions,
        IHostingEnvironment hostingEnvironment,
        IRuntimeState runtimeState)
        : base(loggerFactory, name, indexOptions, hostingEnvironment, runtimeState) =>
        _logger = loggerFactory.CreateLogger<UmbracoContentIndex>();

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
        var idsAsList = itemIds.ToList();

        for (var i = 0; i < idsAsList.Count; i++)
        {
            var nodeId = idsAsList[i];

            //find all descendants based on path
            var descendantPath = $@"\-1\,*{nodeId}\,*";
            var rawQuery = $"{UmbracoExamineFieldNames.IndexPathFieldName}:{descendantPath}";
            IQuery? c = Searcher.CreateQuery();
            IBooleanOperation? filtered = c.NativeQuery(rawQuery);
            IOrdering? selectedFields = filtered.SelectFields(_idOnlyFieldSet);
            ISearchResults? results = selectedFields.Execute();

            _logger.LogDebug("DeleteFromIndex with query: {Query} (found {TotalItems} results)", rawQuery, results.TotalItemCount);

            var toRemove = results.Select(x => x.Id).ToList();
            // delete those descendants (ensure base. is used here so we aren't calling ourselves!)
            base.PerformDeleteFromIndex(toRemove, null);

            // remove any ids from our list that were part of the descendants
            idsAsList.RemoveAll(x => toRemove.Contains(x));
        }

        base.PerformDeleteFromIndex(idsAsList, onComplete);
    }
}
