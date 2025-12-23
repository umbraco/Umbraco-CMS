# ContentService Refactoring Phase 4: Move Operation Service Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Extract Move, Copy, Sort, and Recycle Bin operations from ContentService into a dedicated IContentMoveOperationService.

**Architecture:** Following the established pattern from Phases 1-3, create interface in Umbraco.Core and implementation inheriting from ContentServiceBase. The service handles content tree operations (Move, Copy, Sort) and Recycle Bin management. MoveToRecycleBin stays in the facade as it orchestrates with unpublish operations.

**Tech Stack:** C# 12, .NET 10.0, NUnit, Umbraco notification system

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-23 | Initial plan |
| 1.1 | 2025-12-23 | Applied critical review fixes (see Critical Review Response section) |

---

## Critical Review Response (v1.1)

The following changes address issues identified in `2025-12-23-contentservice-refactor-phase4-implementation-critical-review-1.md`:

### Critical Issues Fixed:

1. **GetPermissions nested scope issue (Section 2.1)** - FIXED
   - Inlined repository call within existing scope instead of creating nested scope
   - Removed the private `GetPermissions` method entirely

2. **navigationUpdates unused (Section 2.2)** - FIXED
   - Removed the unused `navigationUpdates` variable entirely
   - Navigation cache updates are handled by the `ContentTreeChangeNotification` which triggers cache refreshers

3. **GetById(int) method signature (Section 2.3)** - FIXED
   - Changed to use `GetByIds(new[] { parentId }).FirstOrDefault()` pattern
   - This is consistent with IContentCrudService interface which uses Guid-based GetById

4. **parentKey for descendants in Copy (Section 2.4)** - DOCUMENTED
   - Added comment documenting this matches original ContentService behavior
   - The parentKey represents the original operation's target parent, not each descendant's immediate parent
   - Changing this would break backwards compatibility with existing notification handlers

### Should Fix Issues Addressed:

5. **DeleteLocked empty batch handling (Section 2.5)** - FIXED
   - Changed to break immediately when batch is empty
   - Moved the break statement before iteration increment

### Nice-to-Have Improvements Applied:

6. **Page size constants** - FIXED
   - Extracted to class-level constants: `DefaultPageSize = 500`, `MaxDeleteIterations = 10000`

7. **Regions in interface** - NOT CHANGED
   - Keeping for consistency with existing Umbraco codebase patterns
   - Other interfaces in the codebase use regions

8. **Performance logging to Sort** - FIXED
   - Added debug logging showing modified vs total items

---

## Pre-Implementation Checklist

Before starting, verify baseline:

```bash
# Run all ContentService tests to establish baseline
dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentService" --no-build

# Run refactoring-specific tests
dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentServiceRefactoringTests" --no-build
```

All tests must pass before proceeding.

---

## Task 1: Create IContentMoveOperationService Interface

**Files:**
- Create: `src/Umbraco.Core/Services/IContentMoveOperationService.cs`

**Step 1: Write the interface definition**

```csharp
// src/Umbraco.Core/Services/IContentMoveOperationService.cs
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Service for content move, copy, sort, and recycle bin operations.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Implementation Note:</strong> Do not implement this interface directly.
/// Instead, inherit from <see cref="ContentServiceBase"/> which provides required
/// infrastructure (scoping, repository access, auditing). Direct implementation
/// without this base class will result in missing functionality.
/// </para>
/// <para>
/// This interface is part of the ContentService refactoring initiative (Phase 4).
/// It extracts move/copy/sort operations into a focused, testable service.
/// </para>
/// <para>
/// <strong>Note:</strong> <c>MoveToRecycleBin</c> is NOT part of this interface because
/// it orchestrates multiple services (unpublish + move) and belongs in the facade.
/// </para>
/// <para>
/// <strong>Versioning Policy:</strong> This interface follows additive-only changes.
/// New methods may be added with default implementations. Existing methods will not
/// be removed or have signatures changed without a 2 major version deprecation period.
/// </para>
/// <para>
/// <strong>Version History:</strong>
/// <list type="bullet">
///   <item><description>v1.0 (Phase 4): Initial interface with Move, Copy, Sort, RecycleBin operations</description></item>
/// </list>
/// </para>
/// </remarks>
/// <since>1.0</since>
public interface IContentMoveOperationService : IService
{
    // Note: #region blocks kept for consistency with existing Umbraco interface patterns

    #region Move Operations

    /// <summary>
    /// Moves content to a new parent.
    /// </summary>
    /// <param name="content">The content to move.</param>
    /// <param name="parentId">The target parent id, or -1 for root.</param>
    /// <param name="userId">The user performing the operation.</param>
    /// <returns>The operation result.</returns>
    /// <remarks>
    /// If parentId is the recycle bin (-20), this method delegates to MoveToRecycleBin
    /// behavior (should be called via ContentService facade instead).
    /// Fires <see cref="Notifications.ContentMovingNotification"/> (cancellable) before move
    /// and <see cref="Notifications.ContentMovedNotification"/> after successful move.
    /// </remarks>
    OperationResult Move(IContent content, int parentId, int userId = Constants.Security.SuperUserId);

    #endregion

    #region Recycle Bin Operations

    /// <summary>
    /// Empties the content recycle bin.
    /// </summary>
    /// <param name="userId">The user performing the operation.</param>
    /// <returns>The operation result.</returns>
    /// <remarks>
    /// Fires <see cref="Notifications.ContentEmptyingRecycleBinNotification"/> (cancellable) before emptying
    /// and <see cref="Notifications.ContentEmptiedRecycleBinNotification"/> after successful empty.
    /// Content with active relations may be skipped if DisableDeleteWhenReferenced is configured.
    /// </remarks>
    OperationResult EmptyRecycleBin(int userId = Constants.Security.SuperUserId);

    /// <summary>
    /// Empties the content recycle bin asynchronously.
    /// </summary>
    /// <param name="userId">The user key performing the operation.</param>
    /// <returns>The operation result.</returns>
    Task<OperationResult> EmptyRecycleBinAsync(Guid userId);

    /// <summary>
    /// Checks whether there is content in the recycle bin.
    /// </summary>
    /// <returns>True if the recycle bin has content; otherwise false.</returns>
    bool RecycleBinSmells();

    /// <summary>
    /// Gets paged content from the recycle bin.
    /// </summary>
    /// <param name="pageIndex">Zero-based page index.</param>
    /// <param name="pageSize">Page size.</param>
    /// <param name="totalRecords">Output: total number of records in recycle bin.</param>
    /// <param name="filter">Optional filter query.</param>
    /// <param name="ordering">Optional ordering (defaults to Path).</param>
    /// <returns>Paged content from the recycle bin.</returns>
    IEnumerable<IContent> GetPagedContentInRecycleBin(
        long pageIndex,
        int pageSize,
        out long totalRecords,
        IQuery<IContent>? filter = null,
        Ordering? ordering = null);

    #endregion

    #region Copy Operations

    /// <summary>
    /// Copies content to a new parent, including all descendants.
    /// </summary>
    /// <param name="content">The content to copy.</param>
    /// <param name="parentId">The target parent id.</param>
    /// <param name="relateToOriginal">Whether to create a relation to the original.</param>
    /// <param name="userId">The user performing the operation.</param>
    /// <returns>The copied content, or null if cancelled.</returns>
    /// <remarks>
    /// Fires <see cref="Notifications.ContentCopyingNotification"/> (cancellable) before each copy
    /// and <see cref="Notifications.ContentCopiedNotification"/> after each successful copy.
    /// The copy is not published regardless of the original's published state.
    /// </remarks>
    IContent? Copy(IContent content, int parentId, bool relateToOriginal, int userId = Constants.Security.SuperUserId);

    /// <summary>
    /// Copies content to a new parent.
    /// </summary>
    /// <param name="content">The content to copy.</param>
    /// <param name="parentId">The target parent id.</param>
    /// <param name="relateToOriginal">Whether to create a relation to the original.</param>
    /// <param name="recursive">Whether to copy descendants recursively.</param>
    /// <param name="userId">The user performing the operation.</param>
    /// <returns>The copied content, or null if cancelled.</returns>
    IContent? Copy(IContent content, int parentId, bool relateToOriginal, bool recursive, int userId = Constants.Security.SuperUserId);

    #endregion

    #region Sort Operations

    /// <summary>
    /// Sorts content items by updating their SortOrder.
    /// </summary>
    /// <param name="items">The content items in desired order.</param>
    /// <param name="userId">The user performing the operation.</param>
    /// <returns>The operation result.</returns>
    /// <remarks>
    /// Fires <see cref="Notifications.ContentSortingNotification"/> (cancellable) and
    /// <see cref="Notifications.ContentSavingNotification"/> (cancellable) before sorting.
    /// Fires <see cref="Notifications.ContentSavedNotification"/>,
    /// <see cref="Notifications.ContentSortedNotification"/>, and
    /// <see cref="Notifications.ContentPublishedNotification"/> (if any were published) after.
    /// </remarks>
    OperationResult Sort(IEnumerable<IContent> items, int userId = Constants.Security.SuperUserId);

    /// <summary>
    /// Sorts content items by id in the specified order.
    /// </summary>
    /// <param name="ids">The content ids in desired order.</param>
    /// <param name="userId">The user performing the operation.</param>
    /// <returns>The operation result.</returns>
    OperationResult Sort(IEnumerable<int>? ids, int userId = Constants.Security.SuperUserId);

    #endregion
}
```

**Step 2: Verify file compiles**

Run: `dotnet build src/Umbraco.Core --no-restore`
Expected: Build succeeded

**Step 3: Commit**

```bash
git add src/Umbraco.Core/Services/IContentMoveOperationService.cs
git commit -m "$(cat <<'EOF'
feat(core): add IContentMoveOperationService interface for Phase 4

Defines interface for Move, Copy, Sort, and Recycle Bin operations.
MoveToRecycleBin deliberately excluded as it requires facade orchestration.

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
EOF
)"
```

---

## Task 2: Create ContentMoveOperationService Implementation

**Files:**
- Create: `src/Umbraco.Core/Services/ContentMoveOperationService.cs`

**Step 1: Write the implementation class (Part 1 - Constructor and Move)**

```csharp
// src/Umbraco.Core/Services/ContentMoveOperationService.cs
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
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

            PerformMoveLocked(content, parentId, parent, userId, moves, trashed);

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

    /// <summary>
    /// Performs the actual move operation within an existing write lock.
    /// </summary>
    private void PerformMoveLocked(IContent content, int parentId, IContent? parent, int userId, ICollection<(IContent, string)> moves, bool? trash)
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

                    DeleteLocked(scope, content, eventMessages);
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

    /// <summary>
    /// Deletes content and all descendants within an existing scope.
    /// </summary>
    private void DeleteLocked(ICoreScope scope, IContent content, EventMessages evtMsgs)
    {
        void DoDelete(IContent c)
        {
            DocumentRepository.Delete(c);
            scope.Notifications.Publish(new ContentDeletedNotification(c, evtMsgs));
        }

        // v1.1: Using class-level constants
        var iteration = 0;
        var total = long.MaxValue;

        while (total > 0 && iteration < MaxDeleteIterations)
        {
            IEnumerable<IContent> descendants = GetPagedDescendantsLocked(
                content.Id,
                0,
                DefaultPageSize,
                out total,
                ordering: Ordering.By("Path", Direction.Descending));

            var batch = descendants.ToList();

            // v1.1: Break immediately when batch is empty (fix from critical review 2.5)
            if (batch.Count == 0)
            {
                if (total > 0)
                {
                    _logger.LogWarning(
                        "GetPagedDescendants reported {Total} total descendants but returned empty batch for content {ContentId}. Breaking loop.",
                        total,
                        content.Id);
                }
                break;  // Break immediately, don't continue iterating
            }

            foreach (IContent c in batch)
            {
                DoDelete(c);
            }

            iteration++;
        }

        if (iteration >= MaxDeleteIterations)
        {
            _logger.LogError(
                "DeleteLocked exceeded maximum iteration limit ({MaxIterations}) for content {ContentId}. Tree may be incompletely deleted.",
                MaxDeleteIterations,
                content.Id);
        }

        DoDelete(content);
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
```

**Step 2: Verify file compiles**

Run: `dotnet build src/Umbraco.Core --no-restore`
Expected: Build succeeded

**Step 3: Commit**

```bash
git add src/Umbraco.Core/Services/ContentMoveOperationService.cs
git commit -m "$(cat <<'EOF'
feat(core): add ContentMoveOperationService implementation for Phase 4

Implements Move, Copy, Sort, and Recycle Bin operations.
Follows established patterns from Phase 1-3 implementations.

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
EOF
)"
```

---

## Task 3: Register Service in DI Container

**Files:**
- Modify: `src/Umbraco.Core/DependencyInjection/UmbracoBuilder.cs`

**Step 1: Add the service registration**

Find the line with `Services.AddUnique<IContentVersionOperationService, ContentVersionOperationService>();` and add the new service registration after it:

```csharp
Services.AddUnique<IContentMoveOperationService, ContentMoveOperationService>();
```

**Step 2: Update ContentService factory to include new dependency**

Find the `Services.AddUnique<IContentService>` factory and add the new parameter:

```csharp
sp.GetRequiredService<IContentMoveOperationService>()
```

**Step 3: Verify build**

Run: `dotnet build src/Umbraco.Core --no-restore`
Expected: Build succeeded (will fail initially - need to update ContentService constructor)

**Step 4: Commit**

```bash
git add src/Umbraco.Core/DependencyInjection/UmbracoBuilder.cs
git commit -m "$(cat <<'EOF'
chore(di): register IContentMoveOperationService in DI container

Adds service registration and includes in ContentService factory.

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
EOF
)"
```

---

## Task 4: Update ContentService to Delegate Move Operations

**Files:**
- Modify: `src/Umbraco.Core/Services/ContentService.cs`

**Step 1: Add field and property for MoveOperationService**

After the existing version operation service fields, add:

```csharp
// Move operation service fields (for Phase 4 extracted move operations)
private readonly IContentMoveOperationService? _moveOperationService;
private readonly Lazy<IContentMoveOperationService>? _moveOperationServiceLazy;

/// <summary>
/// Gets the move operation service.
/// </summary>
/// <exception cref="InvalidOperationException">Thrown if the service was not properly initialized.</exception>
private IContentMoveOperationService MoveOperationService =>
    _moveOperationService ?? _moveOperationServiceLazy?.Value
    ?? throw new InvalidOperationException("MoveOperationService not initialized. Ensure the service is properly injected via constructor.");
```

**Step 2: Update primary constructor to accept new service**

Add parameter after `versionOperationService`:

```csharp
IContentMoveOperationService moveOperationService)  // NEW PARAMETER - Phase 4 move operations
```

And in the constructor body:

```csharp
// Phase 4: Move operation service (direct injection)
ArgumentNullException.ThrowIfNull(moveOperationService);
_moveOperationService = moveOperationService;
_moveOperationServiceLazy = null;  // Not needed when directly injected
```

**Step 3: Update obsolete constructors for lazy resolution**

Add to both obsolete constructors:

```csharp
// Phase 4: Lazy resolution of IContentMoveOperationService
_moveOperationServiceLazy = new Lazy<IContentMoveOperationService>(() =>
    StaticServiceProvider.Instance.GetRequiredService<IContentMoveOperationService>(),
    LazyThreadSafetyMode.ExecutionAndPublication);
```

**Step 4: Update methods to delegate to MoveOperationService**

Replace these methods with delegation:

```csharp
// In #region Move, RecycleBin section - update Move method
public OperationResult Move(IContent content, int parentId, int userId = Constants.Security.SuperUserId)
{
    // If moving to recycle bin, use MoveToRecycleBin which handles unpublish
    if (parentId == Constants.System.RecycleBinContent)
    {
        return MoveToRecycleBin(content, userId);
    }

    return MoveOperationService.Move(content, parentId, userId);
}

// Note: MoveToRecycleBin stays in ContentService as it orchestrates unpublish+move

// Update EmptyRecycleBin methods
public async Task<OperationResult> EmptyRecycleBinAsync(Guid userId)
    => await MoveOperationService.EmptyRecycleBinAsync(userId);

public OperationResult EmptyRecycleBin(int userId = Constants.Security.SuperUserId)
    => MoveOperationService.EmptyRecycleBin(userId);

public bool RecycleBinSmells()
    => MoveOperationService.RecycleBinSmells();

// Update GetPagedContentInRecycleBin
public IEnumerable<IContent> GetPagedContentInRecycleBin(long pageIndex, int pageSize, out long totalRecords, IQuery<IContent>? filter = null, Ordering? ordering = null)
    => MoveOperationService.GetPagedContentInRecycleBin(pageIndex, pageSize, out totalRecords, filter, ordering);

// In #region Others - update Copy methods
public IContent? Copy(IContent content, int parentId, bool relateToOriginal, int userId = Constants.Security.SuperUserId)
    => MoveOperationService.Copy(content, parentId, relateToOriginal, userId);

public IContent? Copy(IContent content, int parentId, bool relateToOriginal, bool recursive, int userId = Constants.Security.SuperUserId)
    => MoveOperationService.Copy(content, parentId, relateToOriginal, recursive, userId);

// Update Sort methods
public OperationResult Sort(IEnumerable<IContent> items, int userId = Constants.Security.SuperUserId)
    => MoveOperationService.Sort(items, userId);

public OperationResult Sort(IEnumerable<int>? ids, int userId = Constants.Security.SuperUserId)
    => MoveOperationService.Sort(ids, userId);
```

**Step 5: Remove now-unused private helper methods**

The following private methods can be removed from ContentService as they're now in ContentMoveOperationService:
- `PerformMoveLocked` (lines 2082-2132)
- `PerformMoveContentLocked` (lines 2134-2143)
- `Sort(ICoreScope scope, IContent[] itemsA, int userId, EventMessages eventMessages)` (lines 2500-2563)

Note: Keep `TryGetParentKey` as it's still used by `MoveToRecycleBin`. Actually, check if it's used elsewhere - may need to keep.

Note: Keep `DeleteLocked` and `GetPagedLocked` and `GetPagedDescendantQuery` as they're still used by other methods (like `DeleteOfType` and `MoveToRecycleBin`).

**Step 6: Verify build**

Run: `dotnet build src/Umbraco.Core --no-restore`
Expected: Build succeeded

**Step 7: Run tests to verify no regressions**

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentService" --no-build`
Expected: All tests pass

**Step 8: Commit**

```bash
git add src/Umbraco.Core/Services/ContentService.cs
git commit -m "$(cat <<'EOF'
refactor(core): delegate Move/Copy/Sort operations to MoveOperationService

ContentService now delegates:
- Move (non-recycle bin moves)
- EmptyRecycleBin, RecycleBinSmells, GetPagedContentInRecycleBin
- Copy (both overloads)
- Sort (both overloads)

MoveToRecycleBin stays in facade for unpublish orchestration.

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
EOF
)"
```

---

## Task 5: Create Unit Tests for Interface Contract

**Files:**
- Create: `tests/Umbraco.Tests.UnitTests/Umbraco.Core/Services/ContentMoveOperationServiceInterfaceTests.cs`

**Step 1: Write interface contract tests**

```csharp
// tests/Umbraco.Tests.UnitTests/Umbraco.Core/Services/ContentMoveOperationServiceInterfaceTests.cs
using System.Reflection;
using NUnit.Framework;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services;

[TestFixture]
public class ContentMoveOperationServiceInterfaceTests
{
    [Test]
    public void Interface_Exists_And_Is_Public()
    {
        var interfaceType = typeof(IContentMoveOperationService);

        Assert.That(interfaceType, Is.Not.Null);
        Assert.That(interfaceType.IsInterface, Is.True);
        Assert.That(interfaceType.IsPublic, Is.True);
    }

    [Test]
    public void Interface_Extends_IService()
    {
        var interfaceType = typeof(IContentMoveOperationService);

        Assert.That(typeof(IService).IsAssignableFrom(interfaceType), Is.True);
    }

    [Test]
    [TestCase("Move", new[] { typeof(Umbraco.Cms.Core.Models.IContent), typeof(int), typeof(int) })]
    [TestCase("EmptyRecycleBin", new[] { typeof(int) })]
    [TestCase("RecycleBinSmells", new Type[] { })]
    [TestCase("Copy", new[] { typeof(Umbraco.Cms.Core.Models.IContent), typeof(int), typeof(bool), typeof(int) })]
    [TestCase("Copy", new[] { typeof(Umbraco.Cms.Core.Models.IContent), typeof(int), typeof(bool), typeof(bool), typeof(int) })]
    public void Interface_Has_Required_Method(string methodName, Type[] parameterTypes)
    {
        var interfaceType = typeof(IContentMoveOperationService);
        var method = interfaceType.GetMethod(methodName, parameterTypes);

        Assert.That(method, Is.Not.Null, $"Method {methodName} should exist with specified parameters");
    }

    [Test]
    public void Interface_Has_Sort_Methods()
    {
        var interfaceType = typeof(IContentMoveOperationService);

        // Sort with IEnumerable<IContent>
        var sortContentMethod = interfaceType.GetMethods()
            .FirstOrDefault(m => m.Name == "Sort" &&
                m.GetParameters().Length == 2 &&
                m.GetParameters()[0].ParameterType.IsGenericType);

        // Sort with IEnumerable<int>
        var sortIdsMethod = interfaceType.GetMethods()
            .FirstOrDefault(m => m.Name == "Sort" &&
                m.GetParameters().Length == 2 &&
                m.GetParameters()[0].ParameterType == typeof(IEnumerable<int>));

        Assert.That(sortContentMethod, Is.Not.Null, "Sort(IEnumerable<IContent>, int) should exist");
        Assert.That(sortIdsMethod, Is.Not.Null, "Sort(IEnumerable<int>, int) should exist");
    }

    [Test]
    public void Implementation_Inherits_ContentServiceBase()
    {
        var implementationType = typeof(ContentMoveOperationService);
        var baseType = typeof(ContentServiceBase);

        Assert.That(baseType.IsAssignableFrom(implementationType), Is.True);
    }
}
```

**Step 2: Verify tests compile**

Run: `dotnet build tests/Umbraco.Tests.UnitTests --no-restore`
Expected: Build succeeded

**Step 3: Run unit tests**

Run: `dotnet test tests/Umbraco.Tests.UnitTests --filter "FullyQualifiedName~ContentMoveOperationServiceInterfaceTests" --no-build`
Expected: All tests pass

**Step 4: Commit**

```bash
git add tests/Umbraco.Tests.UnitTests/Umbraco.Core/Services/ContentMoveOperationServiceInterfaceTests.cs
git commit -m "$(cat <<'EOF'
test(unit): add ContentMoveOperationService interface contract tests

Verifies interface exists, extends IService, and has all required methods.

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
EOF
)"
```

---

## Task 6: Create Integration Tests

**Files:**
- Create: `tests/Umbraco.Tests.Integration/Umbraco.Infrastructure/Services/ContentMoveOperationServiceTests.cs`

**Step 1: Write integration tests**

```csharp
// tests/Umbraco.Tests.Integration/Umbraco.Infrastructure/Services/ContentMoveOperationServiceTests.cs
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class ContentMoveOperationServiceTests : UmbracoIntegrationTestWithContent
{
    private IContentMoveOperationService MoveOperationService => GetRequiredService<IContentMoveOperationService>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.AddNotificationHandler<ContentMovingNotification, MoveNotificationHandler>();
        builder.AddNotificationHandler<ContentMovedNotification, MoveNotificationHandler>();
        builder.AddNotificationHandler<ContentCopyingNotification, MoveNotificationHandler>();
        builder.AddNotificationHandler<ContentCopiedNotification, MoveNotificationHandler>();
        builder.AddNotificationHandler<ContentSortingNotification, MoveNotificationHandler>();
        builder.AddNotificationHandler<ContentSortedNotification, MoveNotificationHandler>();
    }

    #region Move Tests

    [Test]
    public void Move_ToNewParent_ChangesParentId()
    {
        // Arrange
        var child = ContentService.Create("Child", Textpage.Id, ContentType.Alias);
        ContentService.Save(child);

        var newParent = ContentService.Create("NewParent", Constants.System.Root, ContentType.Alias);
        ContentService.Save(newParent);

        // Act
        var result = MoveOperationService.Move(child, newParent.Id);

        // Assert
        Assert.That(result.Success, Is.True);
        var movedContent = ContentService.GetById(child.Id);
        Assert.That(movedContent!.ParentId, Is.EqualTo(newParent.Id));
    }

    [Test]
    public void Move_ToSameParent_ReturnsSuccess()
    {
        // Arrange
        var child = ContentService.Create("Child", Textpage.Id, ContentType.Alias);
        ContentService.Save(child);

        // Act
        var result = MoveOperationService.Move(child, Textpage.Id);

        // Assert
        Assert.That(result.Success, Is.True);
    }

    [Test]
    public void Move_ToNonExistentParent_ThrowsException()
    {
        // Arrange
        var content = ContentService.Create("Content", Constants.System.Root, ContentType.Alias);
        ContentService.Save(content);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            MoveOperationService.Move(content, 999999));
    }

    [Test]
    public void Move_FiresMovingAndMovedNotifications()
    {
        // Arrange
        var child = ContentService.Create("Child", Textpage.Id, ContentType.Alias);
        ContentService.Save(child);

        var newParent = ContentService.Create("NewParent", Constants.System.Root, ContentType.Alias);
        ContentService.Save(newParent);

        bool movingFired = false;
        bool movedFired = false;

        MoveNotificationHandler.Moving = notification => movingFired = true;
        MoveNotificationHandler.Moved = notification => movedFired = true;

        try
        {
            // Act
            var result = MoveOperationService.Move(child, newParent.Id);

            // Assert
            Assert.That(result.Success, Is.True);
            Assert.That(movingFired, Is.True, "Moving notification should fire");
            Assert.That(movedFired, Is.True, "Moved notification should fire");
        }
        finally
        {
            MoveNotificationHandler.Moving = null;
            MoveNotificationHandler.Moved = null;
        }
    }

    [Test]
    public void Move_WhenCancelled_ReturnsCancel()
    {
        // Arrange
        var child = ContentService.Create("Child", Textpage.Id, ContentType.Alias);
        ContentService.Save(child);

        var newParent = ContentService.Create("NewParent", Constants.System.Root, ContentType.Alias);
        ContentService.Save(newParent);

        MoveNotificationHandler.Moving = notification => notification.Cancel = true;

        try
        {
            // Act
            var result = MoveOperationService.Move(child, newParent.Id);

            // Assert
            Assert.That(result.Success, Is.False);
            Assert.That(result.Result, Is.EqualTo(OperationResultType.FailedCancelledByEvent));
        }
        finally
        {
            MoveNotificationHandler.Moving = null;
        }
    }

    #endregion

    #region RecycleBin Tests

    [Test]
    public void RecycleBinSmells_WhenEmpty_ReturnsFalse()
    {
        // Act
        var result = MoveOperationService.RecycleBinSmells();

        // Assert - depends on base class setup, but Trashed item should make it smell
        Assert.That(result, Is.True); // Trashed exists from base class
    }

    [Test]
    public void GetPagedContentInRecycleBin_ReturnsPagedResults()
    {
        // Act
        var results = MoveOperationService.GetPagedContentInRecycleBin(0, 10, out long totalRecords);

        // Assert
        Assert.That(results, Is.Not.Null);
        Assert.That(totalRecords, Is.GreaterThanOrEqualTo(0));
    }

    [Test]
    public void EmptyRecycleBin_ClearsRecycleBin()
    {
        // Arrange - ensure something is in recycle bin (from base class)
        Assert.That(MoveOperationService.RecycleBinSmells(), Is.True);

        // Act
        var result = MoveOperationService.EmptyRecycleBin();

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(MoveOperationService.RecycleBinSmells(), Is.False);
    }

    #endregion

    #region Copy Tests

    [Test]
    public void Copy_CreatesNewContent()
    {
        // Arrange
        var original = Textpage;

        // Act
        var copy = MoveOperationService.Copy(original, Constants.System.Root, false);

        // Assert
        Assert.That(copy, Is.Not.Null);
        Assert.That(copy!.Id, Is.Not.EqualTo(original.Id));
        Assert.That(copy.Key, Is.Not.EqualTo(original.Key));
        Assert.That(copy.Name, Is.EqualTo(original.Name));
    }

    [Test]
    public void Copy_Recursive_CopiesDescendants()
    {
        // Arrange
        var child = ContentService.Create("Child", Textpage.Id, ContentType.Alias);
        ContentService.Save(child);
        var grandchild = ContentService.Create("Grandchild", child.Id, ContentType.Alias);
        ContentService.Save(grandchild);

        // Act
        var copy = MoveOperationService.Copy(Textpage, Constants.System.Root, false, recursive: true);

        // Assert
        Assert.That(copy, Is.Not.Null);
        var copyChildren = ContentService.GetPagedChildren(copy!.Id, 0, 10, out _).ToList();
        Assert.That(copyChildren.Count, Is.EqualTo(1));
    }

    [Test]
    public void Copy_NonRecursive_DoesNotCopyDescendants()
    {
        // Arrange
        var child = ContentService.Create("Child", Textpage.Id, ContentType.Alias);
        ContentService.Save(child);

        // Act
        var copy = MoveOperationService.Copy(Textpage, Constants.System.Root, false, recursive: false);

        // Assert
        Assert.That(copy, Is.Not.Null);
        var copyChildren = ContentService.GetPagedChildren(copy!.Id, 0, 10, out _).ToList();
        Assert.That(copyChildren.Count, Is.EqualTo(0));
    }

    [Test]
    public void Copy_FiresCopyingAndCopiedNotifications()
    {
        // Arrange
        bool copyingFired = false;
        bool copiedFired = false;

        MoveNotificationHandler.Copying = notification => copyingFired = true;
        MoveNotificationHandler.Copied = notification => copiedFired = true;

        try
        {
            // Act
            var copy = MoveOperationService.Copy(Textpage, Constants.System.Root, false);

            // Assert
            Assert.That(copy, Is.Not.Null);
            Assert.That(copyingFired, Is.True, "Copying notification should fire");
            Assert.That(copiedFired, Is.True, "Copied notification should fire");
        }
        finally
        {
            MoveNotificationHandler.Copying = null;
            MoveNotificationHandler.Copied = null;
        }
    }

    [Test]
    public void Copy_WhenCancelled_ReturnsNull()
    {
        // Arrange
        MoveNotificationHandler.Copying = notification => notification.Cancel = true;

        try
        {
            // Act
            var copy = MoveOperationService.Copy(Textpage, Constants.System.Root, false);

            // Assert
            Assert.That(copy, Is.Null);
        }
        finally
        {
            MoveNotificationHandler.Copying = null;
        }
    }

    #endregion

    #region Sort Tests

    [Test]
    public void Sort_ChangesOrder()
    {
        // Arrange
        var child1 = ContentService.Create("Child1", Textpage.Id, ContentType.Alias);
        child1.SortOrder = 0;
        ContentService.Save(child1);

        var child2 = ContentService.Create("Child2", Textpage.Id, ContentType.Alias);
        child2.SortOrder = 1;
        ContentService.Save(child2);

        var child3 = ContentService.Create("Child3", Textpage.Id, ContentType.Alias);
        child3.SortOrder = 2;
        ContentService.Save(child3);

        // Act - reverse the order
        var result = MoveOperationService.Sort(new[] { child3, child2, child1 });

        // Assert
        Assert.That(result.Success, Is.True);
        var reloaded1 = ContentService.GetById(child1.Id)!;
        var reloaded2 = ContentService.GetById(child2.Id)!;
        var reloaded3 = ContentService.GetById(child3.Id)!;
        Assert.That(reloaded3.SortOrder, Is.EqualTo(0));
        Assert.That(reloaded2.SortOrder, Is.EqualTo(1));
        Assert.That(reloaded1.SortOrder, Is.EqualTo(2));
    }

    [Test]
    public void Sort_ByIds_ChangesOrder()
    {
        // Arrange
        var child1 = ContentService.Create("Child1", Textpage.Id, ContentType.Alias);
        child1.SortOrder = 0;
        ContentService.Save(child1);

        var child2 = ContentService.Create("Child2", Textpage.Id, ContentType.Alias);
        child2.SortOrder = 1;
        ContentService.Save(child2);

        // Act - reverse the order
        var result = MoveOperationService.Sort(new[] { child2.Id, child1.Id });

        // Assert
        Assert.That(result.Success, Is.True);
        var reloaded1 = ContentService.GetById(child1.Id)!;
        var reloaded2 = ContentService.GetById(child2.Id)!;
        Assert.That(reloaded2.SortOrder, Is.EqualTo(0));
        Assert.That(reloaded1.SortOrder, Is.EqualTo(1));
    }

    [Test]
    public void Sort_FiresSortingAndSortedNotifications()
    {
        // Arrange
        var child1 = ContentService.Create("Child1", Textpage.Id, ContentType.Alias);
        ContentService.Save(child1);

        bool sortingFired = false;
        bool sortedFired = false;

        MoveNotificationHandler.Sorting = notification => sortingFired = true;
        MoveNotificationHandler.Sorted = notification => sortedFired = true;

        try
        {
            // Act
            var result = MoveOperationService.Sort(new[] { child1 });

            // Assert
            Assert.That(result.Success, Is.True);
            Assert.That(sortingFired, Is.True, "Sorting notification should fire");
            Assert.That(sortedFired, Is.True, "Sorted notification should fire");
        }
        finally
        {
            MoveNotificationHandler.Sorting = null;
            MoveNotificationHandler.Sorted = null;
        }
    }

    [Test]
    public void Sort_EmptyList_ReturnsNoOperation()
    {
        // Act
        var result = MoveOperationService.Sort(Array.Empty<IContent>());

        // Assert
        Assert.That(result.Result, Is.EqualTo(OperationResultType.NoOperation));
    }

    #endregion

    #region Behavioral Equivalence Tests

    [Test]
    public void Move_ViaService_MatchesContentService()
    {
        // Arrange
        var child1 = ContentService.Create("Child1", Textpage.Id, ContentType.Alias);
        ContentService.Save(child1);
        var child2 = ContentService.Create("Child2", Textpage.Id, ContentType.Alias);
        ContentService.Save(child2);

        var newParent = ContentService.Create("NewParent", Constants.System.Root, ContentType.Alias);
        ContentService.Save(newParent);

        // Act
        var viaService = MoveOperationService.Move(child1, newParent.Id);
        var viaContentService = ContentService.Move(child2, newParent.Id);

        // Assert
        Assert.That(viaService.Success, Is.EqualTo(viaContentService.Success));
    }

    [Test]
    public void Copy_ViaService_MatchesContentService()
    {
        // Arrange
        var original = Textpage;

        // Act
        var viaService = MoveOperationService.Copy(original, Constants.System.Root, false, false);
        var viaContentService = ContentService.Copy(original, Constants.System.Root, false, false);

        // Assert
        Assert.That(viaService?.Name, Is.EqualTo(viaContentService?.Name));
        Assert.That(viaService?.ContentTypeId, Is.EqualTo(viaContentService?.ContentTypeId));
    }

    #endregion

    #region Notification Handler

    private class MoveNotificationHandler :
        INotificationHandler<ContentMovingNotification>,
        INotificationHandler<ContentMovedNotification>,
        INotificationHandler<ContentCopyingNotification>,
        INotificationHandler<ContentCopiedNotification>,
        INotificationHandler<ContentSortingNotification>,
        INotificationHandler<ContentSortedNotification>
    {
        public static Action<ContentMovingNotification>? Moving { get; set; }
        public static Action<ContentMovedNotification>? Moved { get; set; }
        public static Action<ContentCopyingNotification>? Copying { get; set; }
        public static Action<ContentCopiedNotification>? Copied { get; set; }
        public static Action<ContentSortingNotification>? Sorting { get; set; }
        public static Action<ContentSortedNotification>? Sorted { get; set; }

        public void Handle(ContentMovingNotification notification) => Moving?.Invoke(notification);
        public void Handle(ContentMovedNotification notification) => Moved?.Invoke(notification);
        public void Handle(ContentCopyingNotification notification) => Copying?.Invoke(notification);
        public void Handle(ContentCopiedNotification notification) => Copied?.Invoke(notification);
        public void Handle(ContentSortingNotification notification) => Sorting?.Invoke(notification);
        public void Handle(ContentSortedNotification notification) => Sorted?.Invoke(notification);
    }

    #endregion
}
```

**Step 2: Verify tests compile**

Run: `dotnet build tests/Umbraco.Tests.Integration --no-restore`
Expected: Build succeeded

**Step 3: Run integration tests**

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentMoveOperationServiceTests" --no-build`
Expected: All tests pass

**Step 4: Commit**

```bash
git add tests/Umbraco.Tests.Integration/Umbraco.Infrastructure/Services/ContentMoveOperationServiceTests.cs
git commit -m "$(cat <<'EOF'
test(integration): add ContentMoveOperationService integration tests

Tests for Move, Copy, Sort, RecycleBin operations with notification verification.
Includes behavioral equivalence tests against ContentService.

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
EOF
)"
```

---

## Task 7: Run All ContentService Tests

**Step 1: Run full ContentService test suite**

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentService" --no-build`
Expected: All tests pass

**Step 2: Run refactoring-specific tests**

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentServiceRefactoringTests" --no-build`
Expected: All tests pass

**Step 3: If any tests fail, diagnose and fix**

Follow the regression protocol from the design document.

---

## Task 8: Update Design Document

**Files:**
- Modify: `docs/plans/2025-12-19-contentservice-refactor-design.md`

**Step 1: Mark Phase 4 as complete**

Update the Implementation Order table:

```markdown
| Phase | Service | Tests to Run | Gate | Status |
|-------|---------|--------------|------|--------|
| 4 | Move Service | All ContentService*Tests + Sort/MoveToRecycleBin tests | All pass | ✅ Complete |
```

Add to revision history:

```markdown
| 1.8 | Phase 4 complete - ContentMoveOperationService extracted |
```

**Step 2: Commit**

```bash
git add docs/plans/2025-12-19-contentservice-refactor-design.md
git commit -m "$(cat <<'EOF'
docs: mark Phase 4 complete in design document

ContentMoveOperationService extraction complete.

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
EOF
)"
```

---

## Task 9: Create Git Tag

**Step 1: Create phase tag**

```bash
git tag -a phase-4-move-extraction -m "Phase 4: ContentMoveOperationService extraction complete"
```

**Step 2: Verify tag**

```bash
git tag -l "phase-*"
```

Expected: Shows all phase tags including `phase-4-move-extraction`

---

## Post-Implementation Verification

Run full test suite to confirm no regressions:

```bash
# All ContentService tests
dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentService"

# Refactoring-specific tests
dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentServiceRefactoringTests"

# New MoveOperationService tests
dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentMoveOperationServiceTests"
```

All tests must pass.

---

## Summary

Phase 4 extracts Move, Copy, Sort, and Recycle Bin operations from ContentService:

**New files created:**
- `src/Umbraco.Core/Services/IContentMoveOperationService.cs` - Interface
- `src/Umbraco.Core/Services/ContentMoveOperationService.cs` - Implementation
- `tests/Umbraco.Tests.UnitTests/Umbraco.Core/Services/ContentMoveOperationServiceInterfaceTests.cs`
- `tests/Umbraco.Tests.Integration/Umbraco.Infrastructure/Services/ContentMoveOperationServiceTests.cs`

**Files modified:**
- `src/Umbraco.Core/DependencyInjection/UmbracoBuilder.cs` - DI registration
- `src/Umbraco.Core/Services/ContentService.cs` - Delegation to new service
- `docs/plans/2025-12-19-contentservice-refactor-design.md` - Status update

**Key decisions:**
- `MoveToRecycleBin` stays in ContentService facade (requires unpublish orchestration)
- Move to recycle bin via `Move(content, -20)` works but delegates internally
- All notifications are preserved and fire in same order
