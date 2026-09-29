using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Search.Core.Models.Indexing;

namespace Umbraco.Cms.Search.Core.Services.ContentIndexing;

/// <summary>
/// Provides shared helpers for <see cref="IContentChangeStrategy"/> implementations: paged descendant enumeration and rebuild-cancellation logging.
/// </summary>
internal abstract class ContentChangeStrategyBase
{
    private readonly ILogger<ContentChangeStrategyBase> _logger;

    /// <summary>
    /// Gets a value indicating whether this strategy indexes trashed (recycle bin) content.
    /// </summary>
    protected abstract bool SupportsTrashedContent { get; }

    /// <summary>
    /// The page size used when enumerating content for rebuilds and descendant traversal.
    /// </summary>
    internal const int ContentEnumerationPageSize = 1000;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentChangeStrategyBase"/> class.
    /// </summary>
    /// <param name="logger">The logger used to record rebuild cancellations.</param>
    protected ContentChangeStrategyBase(ILogger<ContentChangeStrategyBase> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Pages through the descendants of a root item ordered by path, invoking <paramref name="actionToPerform"/> for each page.
    /// </summary>
    /// <typeparam name="T">The content type being enumerated.</typeparam>
    /// <param name="rootId">The key of the root item to enumerate descendants of.</param>
    /// <param name="getPagedDescendants">Fetches a page of descendants given the root's key, skip, take and ordering.</param>
    /// <param name="actionToPerform">The action to invoke for each page of descendants.</param>
    protected async Task EnumerateDescendantsByPath<T>(
        Guid rootId,
        Func<Guid, int, int, Ordering, Task<T[]>> getPagedDescendants,
        Func<T[], Task> actionToPerform)
        where T : IContentBase
    {
        var skip = 0;
        T[] descendants;
        Ordering ordering = Ordering.By("Path");

        do
        {
            descendants = await getPagedDescendants(rootId, skip, ContentEnumerationPageSize, ordering);

            await actionToPerform(descendants);

            skip += ContentEnumerationPageSize;
        }
        while (descendants.Length == ContentEnumerationPageSize);
    }

    /// <summary>
    /// Logs that an index rebuild was cancelled before completing.
    /// </summary>
    /// <param name="indexInfo">The index whose rebuild was cancelled.</param>
    protected void LogIndexRebuildCancellation(ContentIndexInfo indexInfo)
        => _logger.LogInformation("Cancellation requested for rebuild of index: {indexAlias}", indexInfo.IndexAlias);
}
