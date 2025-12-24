// src/Umbraco.Core/Services/ContentMoveOperationService.cs
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Implements content move, copy, sort, and recycle bin operations.
/// </summary>
public class ContentMoveOperationService : ContentServiceBase, IContentMoveOperationService
{
    // v1.1: Extracted constants for page size and iteration limits
    private const int DefaultPageSize = 500;
    private const int MaxDeleteIterations = 10000;

    private readonly ILogger<ContentMoveOperationService> _logger;
    private readonly IEntityRepository _entityRepository;
    private readonly IContentCrudService _crudService;
    private readonly IIdKeyMap _idKeyMap;
    private readonly IRelationService _relationService;
    private readonly IUserIdKeyResolver _userIdKeyResolver;
    private ContentSettings _contentSettings;

    public ContentMoveOperationService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IDocumentRepository documentRepository,
        IAuditService auditService,
        IUserIdKeyResolver userIdKeyResolver,
        IEntityRepository entityRepository,
        IContentCrudService crudService,
        IIdKeyMap idKeyMap,
        IRelationService relationService,
        IOptionsMonitor<ContentSettings> contentSettings)
        : base(provider, loggerFactory, eventMessagesFactory, documentRepository, auditService, userIdKeyResolver)
    {
        _logger = loggerFactory.CreateLogger<ContentMoveOperationService>();
        _entityRepository = entityRepository ?? throw new ArgumentNullException(nameof(entityRepository));
        _crudService = crudService ?? throw new ArgumentNullException(nameof(crudService));
        _idKeyMap = idKeyMap ?? throw new ArgumentNullException(nameof(idKeyMap));
        _relationService = relationService ?? throw new ArgumentNullException(nameof(relationService));
        _userIdKeyResolver = userIdKeyResolver ?? throw new ArgumentNullException(nameof(userIdKeyResolver));
        _contentSettings = contentSettings?.CurrentValue ?? throw new ArgumentNullException(nameof(contentSettings));
        contentSettings.OnChange(settings => _contentSettings = settings);
    }

    #region Move Operations

    /// <inheritdoc />
    public OperationResult Move(IContent content, int parentId, int userId = Constants.Security.SuperUserId)
    {
        EventMessages eventMessages = EventMessagesFactory.Get();

        if (content.ParentId == parentId)
        {
            return OperationResult.Succeed(eventMessages);
        }

        // If moving to recycle bin, this should be called via facade's MoveToRecycleBin instead
        // But we handle it for API consistency - just perform a move without unpublish
        var isMovingToRecycleBin = parentId == Constants.System.RecycleBinContent;

        var moves = new List<(IContent, string)>();

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            scope.WriteLock(Constants.Locks.ContentTree);

            // v1.1: Use GetByIds pattern since IContentCrudService.GetById takes Guid, not int
            IContent? parent = parentId == Constants.System.Root
                ? null
                : _crudService.GetByIds(new[] { parentId }).FirstOrDefault();
            if (parentId != Constants.System.Root && parentId != Constants.System.RecycleBinContent && (parent == null || parent.Trashed))
            {
                throw new InvalidOperationException("Parent does not exist or is trashed.");
            }

            TryGetParentKey(parentId, out Guid? parentKey);
            var moveEventInfo = new MoveEventInfo<IContent>(content, content.Path, parentId, parentKey);

            var movingNotification = new ContentMovingNotification(moveEventInfo, eventMessages);
            if (scope.Notifications.PublishCancelable(movingNotification))
            {
                scope.Complete();
                return OperationResult.Cancel(eventMessages);
            }

            // Determine trash state change
            // If content was trashed and we're not moving to recycle bin, untrash it
            // If moving to recycle bin, set trashed = true
            bool? trashed = isMovingToRecycleBin ? true : (content.Trashed ? false : null);

            // If content was trashed and published, it needs to be unpublished when restored
            if (content.Trashed && content.Published && !isMovingToRecycleBin)
            {
                content.PublishedState = PublishedState.Unpublishing;
            }

            PerformMoveLockedInternal(content, parentId, parent, userId, moves, trashed);

            scope.Notifications.Publish(
                new ContentTreeChangeNotification(content, TreeChangeTypes.RefreshBranch, eventMessages));

            MoveEventInfo<IContent>[] moveInfo = moves
                .Select(x =>
                {
                    TryGetParentKey(x.Item1.ParentId, out Guid? itemParentKey);
                    return new MoveEventInfo<IContent>(x.Item1, x.Item2, x.Item1.ParentId, itemParentKey);
                })
                .ToArray();

            scope.Notifications.Publish(
                new ContentMovedNotification(moveInfo, eventMessages).WithStateFrom(movingNotification));

            Audit(AuditType.Move, userId, content.Id);

            scope.Complete();
            return OperationResult.Succeed(eventMessages);
        }
    }

    /// <inheritdoc />
    public IReadOnlyCollection<(IContent Content, string OriginalPath)> PerformMoveLocked(
        IContent content, int parentId, IContent? parent, int userId, bool? trash)
    {
        var moves = new List<(IContent, string)>();
        PerformMoveLockedInternal(content, parentId, parent, userId, moves, trash);
        return moves.AsReadOnly();
    }

    /// <summary>
    /// Performs the actual move operation within an existing write lock.
    /// </summary>
    private void PerformMoveLockedInternal(IContent content, int parentId, IContent? parent, int userId, ICollection<(IContent, string)> moves, bool? trash)
    {
        content.WriterId = userId;
        content.ParentId = parentId;

        // Get the level delta (old pos to new pos)
        // Note that recycle bin (id:-20) level is 0
        var levelDelta = 1 - content.Level + (parent?.Level ?? 0);

        var paths = new Dictionary<int, string>();

        moves.Add((content, content.Path)); // Capture original path

        var originalPath = content.Path;

        // Save the content (path, level, sortOrder will be updated by repository)
        PerformMoveContentLocked(content, userId, trash);

        // Calculate new path for descendants lookup
        paths[content.Id] =
            (parent == null
                ? parentId == Constants.System.RecycleBinContent ? "-1,-20" : Constants.System.RootString
                : parent.Path) + "," + content.Id;

        // v1.1: Using class-level constant
        IQuery<IContent>? query = GetPagedDescendantQuery(originalPath);
        long total;
        do
        {
            // Always page 0 because each page we move the result, reducing total
            IEnumerable<IContent> descendants =
                GetPagedLocked(query, 0, DefaultPageSize, out total, null, Ordering.By("Path"));

            foreach (IContent descendant in descendants)
            {
                moves.Add((descendant, descendant.Path)); // Capture original path

                // Update path and level since we don't update parentId for descendants
                descendant.Path = paths[descendant.Id] = paths[descendant.ParentId] + "," + descendant.Id;
                descendant.Level += levelDelta;
                PerformMoveContentLocked(descendant, userId, trash);
            }
        }
        while (total > DefaultPageSize);
    }

    private void PerformMoveContentLocked(IContent content, int userId, bool? trash)
    {
        if (trash.HasValue)
        {
            ((ContentBase)content).Trashed = trash.Value;
        }

        content.WriterId = userId;
        DocumentRepository.Save(content);
    }

    private bool TryGetParentKey(int parentId, [NotNullWhen(true)] out Guid? parentKey)
    {
        Attempt<Guid> parentKeyAttempt = _idKeyMap.GetKeyForId(parentId, UmbracoObjectTypes.Document);
        parentKey = parentKeyAttempt.Success ? parentKeyAttempt.Result : null;
        return parentKeyAttempt.Success;
    }

    #endregion

    #region Recycle Bin Operations

    /// <inheritdoc />
    public async Task<OperationResult> EmptyRecycleBinAsync(Guid userId)
        => EmptyRecycleBin(await _userIdKeyResolver.GetAsync(userId));

    /// <inheritdoc />
    public OperationResult EmptyRecycleBin(int userId = Constants.Security.SuperUserId)
    {
        var deleted = new List<IContent>();
        EventMessages eventMessages = EventMessagesFactory.Get();

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            scope.WriteLock(Constants.Locks.ContentTree);

            // Get all root items in recycle bin
            IQuery<IContent>? query = Query<IContent>().Where(x => x.ParentId == Constants.System.RecycleBinContent);
            IContent[] contents = DocumentRepository.Get(query).ToArray();

            var emptyingRecycleBinNotification = new ContentEmptyingRecycleBinNotification(contents, eventMessages);
            var deletingContentNotification = new ContentDeletingNotification(contents, eventMessages);
            if (scope.Notifications.PublishCancelable(emptyingRecycleBinNotification) ||
                scope.Notifications.PublishCancelable(deletingContentNotification))
            {
                scope.Complete();
                return OperationResult.Cancel(eventMessages);
            }

            if (contents is not null)
            {
                foreach (IContent content in contents)
                {
                    if (_contentSettings.DisableDeleteWhenReferenced &&
                        _relationService.IsRelated(content.Id, RelationDirectionFilter.Child))
                    {
                        continue;
                    }

                    _crudService.DeleteLocked(scope, content, eventMessages);
                    deleted.Add(content);
                }
            }

            scope.Notifications.Publish(
                new ContentEmptiedRecycleBinNotification(deleted, eventMessages)
                    .WithStateFrom(emptyingRecycleBinNotification));
            scope.Notifications.Publish(
                new ContentTreeChangeNotification(deleted, TreeChangeTypes.Remove, eventMessages));
            Audit(AuditType.Delete, userId, Constants.System.RecycleBinContent, "Recycle bin emptied");

            scope.Complete();
        }

        return OperationResult.Succeed(eventMessages);
    }

    /// <inheritdoc />
    public bool RecycleBinSmells()
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(Constants.Locks.ContentTree);
            return DocumentRepository.RecycleBinSmells();
        }
    }

    /// <inheritdoc />
    public IEnumerable<IContent> GetPagedContentInRecycleBin(
        long pageIndex,
        int pageSize,
        out long totalRecords,
        IQuery<IContent>? filter = null,
        Ordering? ordering = null)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            ordering ??= Ordering.By("Path");

            scope.ReadLock(Constants.Locks.ContentTree);
            IQuery<IContent>? query = Query<IContent>()?
                .Where(x => x.Path.StartsWith(Constants.System.RecycleBinContentPathPrefix));
            return DocumentRepository.GetPage(query, pageIndex, pageSize, out totalRecords, filter, ordering);
        }
    }

    #endregion

    #region Copy Operations

    /// <inheritdoc />
    public IContent? Copy(IContent content, int parentId, bool relateToOriginal, int userId = Constants.Security.SuperUserId)
        => Copy(content, parentId, relateToOriginal, true, userId);

    /// <inheritdoc />
    public IContent? Copy(IContent content, int parentId, bool relateToOriginal, bool recursive, int userId = Constants.Security.SuperUserId)
    {
        EventMessages eventMessages = EventMessagesFactory.Get();

        // v1.1: Removed unused navigationUpdates variable (critical review 2.2)
        // Navigation cache updates are handled by ContentTreeChangeNotification

        IContent copy = content.DeepCloneWithResetIdentities();
        copy.ParentId = parentId;

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            TryGetParentKey(parentId, out Guid? parentKey);
            if (scope.Notifications.PublishCancelable(new ContentCopyingNotification(content, copy, parentId, parentKey, eventMessages)))
            {
                scope.Complete();
                return null;
            }

            var copies = new List<Tuple<IContent, IContent>>();

            scope.WriteLock(Constants.Locks.ContentTree);

            // A copy is not published
            if (copy.Published)
            {
                copy.Published = false;
            }

            copy.CreatorId = userId;
            copy.WriterId = userId;

            // v1.1: Inlined GetPermissions to avoid nested scope issue (critical review 2.1)
            // The write lock is already held, so we can call the repository directly
            EntityPermissionCollection currentPermissions = DocumentRepository.GetPermissionsForEntity(content.Id);
            currentPermissions.RemoveWhere(p => p.IsDefaultPermissions);

            // Save and flush for ID
            DocumentRepository.Save(copy);

            // Copy permissions
            if (currentPermissions.Count > 0)
            {
                var permissionSet = new ContentPermissionSet(copy, currentPermissions);
                DocumentRepository.AddOrUpdatePermissions(permissionSet);
            }

            copies.Add(Tuple.Create(content, copy));
            var idmap = new Dictionary<int, int> { [content.Id] = copy.Id };

            // Process descendants
            if (recursive)
            {
                // v1.1: Using class-level constant
                var page = 0;
                var total = long.MaxValue;
                while (page * DefaultPageSize < total)
                {
                    IEnumerable<IContent> descendants =
                        _crudService.GetPagedDescendants(content.Id, page++, DefaultPageSize, out total);
                    foreach (IContent descendant in descendants)
                    {
                        // Skip if this is the copy itself
                        if (descendant.Id == copy.Id)
                        {
                            continue;
                        }

                        // Skip if parent was not copied
                        if (idmap.TryGetValue(descendant.ParentId, out var newParentId) == false)
                        {
                            continue;
                        }

                        IContent descendantCopy = descendant.DeepCloneWithResetIdentities();
                        descendantCopy.ParentId = newParentId;

                        // v1.1: Note - parentKey is the original operation's target parent, not each descendant's
                        // immediate parent. This matches original ContentService behavior for backwards compatibility
                        // with existing notification handlers (see critical review 2.4).
                        if (scope.Notifications.PublishCancelable(new ContentCopyingNotification(descendant, descendantCopy, newParentId, parentKey, eventMessages)))
                        {
                            continue;
                        }

                        if (descendantCopy.Published)
                        {
                            descendantCopy.Published = false;
                        }

                        descendantCopy.CreatorId = userId;
                        descendantCopy.WriterId = userId;

                        // Mark dirty to update sort order
                        descendantCopy.SortOrder = descendantCopy.SortOrder;

                        DocumentRepository.Save(descendantCopy);

                        copies.Add(Tuple.Create(descendant, descendantCopy));
                        idmap[descendant.Id] = descendantCopy.Id;
                    }
                }
            }

            scope.Notifications.Publish(
                new ContentTreeChangeNotification(copy, TreeChangeTypes.RefreshBranch, eventMessages));
            foreach (Tuple<IContent, IContent> x in CollectionsMarshal.AsSpan(copies))
            {
                // v1.1: parentKey is the original operation's target, maintaining backwards compatibility
                scope.Notifications.Publish(new ContentCopiedNotification(x.Item1, x.Item2, parentId, parentKey, relateToOriginal, eventMessages));
            }

            Audit(AuditType.Copy, userId, content.Id);

            scope.Complete();
        }

        return copy;
    }

    // v1.1: GetPermissions method removed - inlined into Copy method to avoid nested scope issue

    #endregion

    #region Sort Operations

    /// <inheritdoc />
    public OperationResult Sort(IEnumerable<IContent> items, int userId = Constants.Security.SuperUserId)
    {
        EventMessages evtMsgs = EventMessagesFactory.Get();

        IContent[] itemsA = items.ToArray();
        if (itemsA.Length == 0)
        {
            return new OperationResult(OperationResultType.NoOperation, evtMsgs);
        }

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            scope.WriteLock(Constants.Locks.ContentTree);

            OperationResult ret = SortLocked(scope, itemsA, userId, evtMsgs);
            scope.Complete();
            return ret;
        }
    }

    /// <inheritdoc />
    public OperationResult Sort(IEnumerable<int>? ids, int userId = Constants.Security.SuperUserId)
    {
        EventMessages evtMsgs = EventMessagesFactory.Get();

        var idsA = ids?.ToArray();
        if (idsA is null || idsA.Length == 0)
        {
            return new OperationResult(OperationResultType.NoOperation, evtMsgs);
        }

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            scope.WriteLock(Constants.Locks.ContentTree);
            IContent[] itemsA = _crudService.GetByIds(idsA).ToArray();

            OperationResult ret = SortLocked(scope, itemsA, userId, evtMsgs);
            scope.Complete();
            return ret;
        }
    }

    private OperationResult SortLocked(ICoreScope scope, IContent[] itemsA, int userId, EventMessages eventMessages)
    {
        var sortingNotification = new ContentSortingNotification(itemsA, eventMessages);
        var savingNotification = new ContentSavingNotification(itemsA, eventMessages);

        if (scope.Notifications.PublishCancelable(sortingNotification))
        {
            return OperationResult.Cancel(eventMessages);
        }

        if (scope.Notifications.PublishCancelable(savingNotification))
        {
            return OperationResult.Cancel(eventMessages);
        }

        var published = new List<IContent>();
        var saved = new List<IContent>();
        var sortOrder = 0;

        foreach (IContent content in itemsA)
        {
            if (content.SortOrder == sortOrder)
            {
                sortOrder++;
                continue;
            }

            content.SortOrder = sortOrder++;
            content.WriterId = userId;

            if (content.Published)
            {
                published.Add(content);
            }

            saved.Add(content);
            DocumentRepository.Save(content);
            Audit(AuditType.Sort, userId, content.Id, "Sorting content performed by user");
        }

        // v1.1: Added performance logging (critical review 3.4)
        _logger.LogDebug("Sort completed: {Modified}/{Total} items updated", saved.Count, itemsA.Length);

        scope.Notifications.Publish(
            new ContentSavedNotification(itemsA, eventMessages).WithStateFrom(savingNotification));
        scope.Notifications.Publish(
            new ContentSortedNotification(itemsA, eventMessages).WithStateFrom(sortingNotification));

        scope.Notifications.Publish(
            new ContentTreeChangeNotification(saved, TreeChangeTypes.RefreshNode, eventMessages));

        if (published.Any())
        {
            scope.Notifications.Publish(new ContentPublishedNotification(published, eventMessages));
        }

        return OperationResult.Succeed(eventMessages);
    }

    #endregion

    #region Helper Methods

    private IQuery<IContent>? GetPagedDescendantQuery(string contentPath)
    {
        IQuery<IContent>? query = Query<IContent>();
        if (!contentPath.IsNullOrWhiteSpace())
        {
            query?.Where(x => x.Path.SqlStartsWith($"{contentPath},", TextColumnType.NVarchar));
        }

        return query;
    }

    private IEnumerable<IContent> GetPagedLocked(IQuery<IContent>? query, long pageIndex, int pageSize, out long totalChildren, IQuery<IContent>? filter, Ordering? ordering)
    {
        if (pageIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageIndex));
        }

        if (pageSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize));
        }

        ordering ??= Ordering.By("sortOrder");

        return DocumentRepository.GetPage(query, pageIndex, pageSize, out totalChildren, filter, ordering);
    }

    private IEnumerable<IContent> GetPagedDescendantsLocked(int id, long pageIndex, int pageSize, out long totalChildren, IQuery<IContent>? filter = null, Ordering? ordering = null)
    {
        if (pageIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageIndex));
        }

        if (pageSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize));
        }

        if (ordering == null)
        {
            throw new ArgumentNullException(nameof(ordering));
        }

        if (id != Constants.System.Root)
        {
            TreeEntityPath[] contentPath =
                _entityRepository.GetAllPaths(Constants.ObjectTypes.Document, id).ToArray();
            if (contentPath.Length == 0)
            {
                totalChildren = 0;
                return Enumerable.Empty<IContent>();
            }

            IQuery<IContent>? query = GetPagedDescendantQuery(contentPath[0].Path);
            return DocumentRepository.GetPage(query, pageIndex, pageSize, out totalChildren, filter, ordering);
        }

        return DocumentRepository.GetPage(null, pageIndex, pageSize, out totalChildren, filter, ordering);
    }

    #endregion
}
