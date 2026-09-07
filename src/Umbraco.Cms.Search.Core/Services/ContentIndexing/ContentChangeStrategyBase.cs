using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Search.Core.Models.Indexing;

namespace Umbraco.Cms.Search.Core.Services.ContentIndexing;

/// <summary>
/// Provides shared helpers for <see cref="IContentChangeStrategy"/> implementations: paged descendant enumeration and rebuild-cancellation logging.
/// </summary>
internal abstract class ContentChangeStrategyBase
{
    private readonly IUmbracoDatabaseFactory _umbracoDatabaseFactory;
    private readonly IIdKeyMap _idKeyMap;
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
    /// <param name="umbracoDatabaseFactory">The database factory used to build queries for paged descendant enumeration.</param>
    /// <param name="idKeyMap">The map used to resolve a root item's key to its numeric ID.</param>
    /// <param name="logger">The logger used to record unresolvable root IDs and rebuild cancellations.</param>
    protected ContentChangeStrategyBase(
        IUmbracoDatabaseFactory umbracoDatabaseFactory,
        IIdKeyMap idKeyMap,
        ILogger<ContentChangeStrategyBase> logger)
    {
        _umbracoDatabaseFactory = umbracoDatabaseFactory;
        _idKeyMap = idKeyMap;
        _logger = logger;
    }

    /// <summary>
    /// Pages through the descendants of a root item ordered by path, invoking <paramref name="actionToPerform"/> for each page.
    /// </summary>
    /// <typeparam name="T">The content type being enumerated.</typeparam>
    /// <param name="objectType">The object type of the root item, used to resolve its numeric ID.</param>
    /// <param name="rootId">The key of the root item to enumerate descendants of.</param>
    /// <param name="getPagedDescendants">Fetches a page of descendants given the root's numeric ID, page index, page size, query and ordering.</param>
    /// <param name="actionToPerform">The action to invoke for each page of descendants.</param>
    protected async Task EnumerateDescendantsByPath<T>(
        UmbracoObjectTypes objectType,
        Guid rootId,
        Func<int, int, int, IQuery<T>, Ordering, T[]> getPagedDescendants,
        Func<T[], Task> actionToPerform)
        where T : IContentBase
    {
        Attempt<int> rootIdAttempt = _idKeyMap.GetIdForKey(rootId, objectType);
        if (rootIdAttempt.Success is false)
        {
            _logger.LogWarning("Could not resolve ID for {objectType} item {rootId} - aborting enumerations of descendants.", objectType, rootId);
            return;
        }

        var pageIndex = 0;

        T[] descendants;

        IQuery<T> query = _umbracoDatabaseFactory.SqlContext.Query<T>();
        if (SupportsTrashedContent is false)
        {
            query = query.Where(content => content.Trashed == false);
        }

        do
        {
            descendants = getPagedDescendants(rootIdAttempt.Result, pageIndex, ContentEnumerationPageSize, query, Ordering.By("Path"));

            await actionToPerform(descendants.ToArray());

            pageIndex++;
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
