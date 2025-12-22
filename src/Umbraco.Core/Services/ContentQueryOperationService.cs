using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Implements content query operations (counting, filtering by type/level).
/// </summary>
public class ContentQueryOperationService : ContentServiceBase, IContentQueryOperationService
{
    /// <summary>
    /// Default ordering for paged queries.
    /// </summary>
    private static readonly Ordering DefaultSortOrdering = Ordering.By("sortOrder");

    /// <summary>
    /// Logger for this service (for debugging, performance monitoring, or error tracking).
    /// </summary>
    private readonly ILogger<ContentQueryOperationService> _logger;

    public ContentQueryOperationService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IDocumentRepository documentRepository,
        IAuditService auditService,
        IUserIdKeyResolver userIdKeyResolver)
        : base(provider, loggerFactory, eventMessagesFactory, documentRepository, auditService, userIdKeyResolver)
    {
        _logger = loggerFactory.CreateLogger<ContentQueryOperationService>();
    }

    #region Count Operations

    /// <inheritdoc />
    public int Count(string? contentTypeAlias = null)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(Constants.Locks.ContentTree);
        return DocumentRepository.Count(contentTypeAlias);
    }

    /// <inheritdoc />
    public int CountPublished(string? contentTypeAlias = null)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(Constants.Locks.ContentTree);
        return DocumentRepository.CountPublished(contentTypeAlias);
    }

    /// <inheritdoc />
    public int CountChildren(int parentId, string? contentTypeAlias = null)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(Constants.Locks.ContentTree);
        return DocumentRepository.CountChildren(parentId, contentTypeAlias);
    }

    /// <inheritdoc />
    public int CountDescendants(int parentId, string? contentTypeAlias = null)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(Constants.Locks.ContentTree);
        return DocumentRepository.CountDescendants(parentId, contentTypeAlias);
    }

    #endregion

    #region Hierarchy Queries

    /// <inheritdoc />
    /// <remarks>
    /// The returned enumerable may be lazily evaluated. Callers should materialize
    /// results (e.g., call ToList()) if they need to access them after the scope is disposed.
    /// This is consistent with the existing ContentService.GetByLevel implementation.
    /// </remarks>
    public IEnumerable<IContent> GetByLevel(int level)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(Constants.Locks.ContentTree);
        IQuery<IContent>? query = Query<IContent>().Where(x => x.Level == level && x.Trashed == false);
        return DocumentRepository.Get(query);
    }

    #endregion

    #region Paged Type Queries

    /// <inheritdoc />
    public IEnumerable<IContent> GetPagedOfType(
        int contentTypeId,
        long pageIndex,
        int pageSize,
        out long totalRecords,
        IQuery<IContent>? filter = null,
        Ordering? ordering = null)
    {
        if (pageIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageIndex));
        }

        if (pageSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize));
        }

        ordering ??= DefaultSortOrdering;

        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(Constants.Locks.ContentTree);

        // Note: filter=null is valid and means no additional filtering beyond the content type
        return DocumentRepository.GetPage(
            Query<IContent>()?.Where(x => x.ContentTypeId == contentTypeId),
            pageIndex,
            pageSize,
            out totalRecords,
            filter,
            ordering);
    }

    /// <inheritdoc />
    public IEnumerable<IContent> GetPagedOfTypes(
        int[] contentTypeIds,
        long pageIndex,
        int pageSize,
        out long totalRecords,
        IQuery<IContent>? filter = null,
        Ordering? ordering = null)
    {
        ArgumentNullException.ThrowIfNull(contentTypeIds);

        if (pageIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageIndex));
        }

        if (pageSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize));
        }

        ordering ??= DefaultSortOrdering;

        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);

        // Expression trees require a List for Contains() - array not supported.
        // This O(n) copy is unavoidable but contentTypeIds is typically small.
        List<int> contentTypeIdsAsList = [.. contentTypeIds];

        scope.ReadLock(Constants.Locks.ContentTree);

        // Note: filter=null is valid and means no additional filtering beyond the content types
        return DocumentRepository.GetPage(
            Query<IContent>()?.Where(x => contentTypeIdsAsList.Contains(x.ContentTypeId)),
            pageIndex,
            pageSize,
            out totalRecords,
            filter,
            ordering);
    }

    #endregion
}
