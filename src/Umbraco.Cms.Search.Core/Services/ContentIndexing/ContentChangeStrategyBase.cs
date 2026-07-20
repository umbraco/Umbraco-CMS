using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Search.Core.Models.Indexing;

namespace Umbraco.Cms.Search.Core.Services.ContentIndexing;

internal abstract class ContentChangeStrategyBase
{
    private readonly IUmbracoDatabaseFactory _umbracoDatabaseFactory;
    private readonly IIdKeyMap _idKeyMap;
    private readonly ILogger<ContentChangeStrategyBase> _logger;

    protected abstract bool SupportsTrashedContent { get; }

    internal const int ContentEnumerationPageSize = 1000;

    protected ContentChangeStrategyBase(
        IUmbracoDatabaseFactory umbracoDatabaseFactory,
        IIdKeyMap idKeyMap,
        ILogger<ContentChangeStrategyBase> logger)
    {
        _umbracoDatabaseFactory = umbracoDatabaseFactory;
        _idKeyMap = idKeyMap;
        _logger = logger;
    }

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

    protected void LogIndexRebuildCancellation(ContentIndexInfo indexInfo)
        => _logger.LogInformation("Cancellation requested for rebuild of index: {indexAlias}", indexInfo.IndexAlias);
}
