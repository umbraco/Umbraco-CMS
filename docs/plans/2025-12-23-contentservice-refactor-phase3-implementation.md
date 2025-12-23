# ContentService Version Operations Extraction - Phase 3 Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Extract version operations (GetVersion, GetVersions, GetVersionsSlim, GetVersionIds, Rollback, DeleteVersions, DeleteVersion) from ContentService into a dedicated `IContentVersionOperationService` interface and `ContentVersionOperationService` implementation.

**Architecture:** `ContentVersionOperationService` implements `IContentVersionOperationService` and inherits from `ContentServiceBase`. The existing `ContentService` facade will delegate to `ContentVersionOperationService` for version operations. The new service handles synchronous version operations extracted from `IContentService`, separate from the existing async `IContentVersionService` which provides API-layer functionality.

**Tech Stack:** .NET 10.0, NUnit, Microsoft.Extensions.DependencyInjection

---

## Plan Version History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2025-12-23 | Claude | Initial plan creation |
| 1.1 | 2025-12-23 | Claude | Applied critical review fixes (Issues 2.1-2.5, 3.2-3.4) |
| 1.2 | 2025-12-23 | Claude | Applied critical review v2 fixes (Issues 2.1-2.2, 3.1-3.4) |
| 1.3 | 2025-12-23 | Claude | Applied critical review v3 fixes (Issues 3.1-3.4, 3.6) |

---

## Changes in Version 1.1

**Summary:** Applied fixes from critical implementation review (2025-12-23-contentservice-refactor-phase3-implementation-critical-review-1.md)

### Critical Fixes Applied:

1. **Issue 2.1 - Rollback TOCTOU Race Condition**: Consolidated Rollback into a single scope to eliminate race condition between read and write operations. Removed `PerformRollback` helper method; all operations now occur within one scope.

2. **Issue 2.2 - Rollback Error Handling**: Added try-catch around save operation with proper error logging ("was unable to rollback") and conditional notification (only fires `ContentRolledBackNotification` on success).

3. **Issue 2.3 - GetVersionIds Missing ReadLock**: Added `scope.ReadLock(Constants.Locks.ContentTree)` for consistency with other read operations. Added code comment explaining this is a bug fix.

4. **Issue 2.4 - DeleteVersion Nested Scope**: Changed `deletePriorVersions` branch to use `DocumentRepository.GetVersion()` and `DocumentRepository.DeleteVersions()` directly instead of calling the service methods that create nested scopes.

5. **Issue 2.5 - Flaky Test Pattern**: Replaced `Thread.Sleep(100)` with deterministic date comparison using actual version update dates.

### Minor Improvements Applied:

6. **Issue 3.2 - Cancellation Test**: Added `Rollback_WhenNotificationCancelled_ReturnsCancelledResult` test.

7. **Issue 3.3 - Published Version Protection Test**: Added `DeleteVersion_PublishedVersion_DoesNotDelete` test.

8. **Issue 3.4 - Interface Documentation**: Enhanced `GetVersionIds` XML documentation to specify behavior for non-existent content and maxRows edge cases.

---

## Changes in Version 1.2

**Summary:** Applied fixes from critical implementation review v2 (2025-12-23-contentservice-refactor-phase3-implementation-critical-review-2.md)

### Critical Fixes Applied:

1. **Issue 2.1 - Rollback Notification Preservation**: Changed Rollback to use `CrudService.Save` instead of `DocumentRepository.Save` to preserve `ContentSavingNotification` and `ContentSavedNotification` firing. This maintains backward compatibility with existing notification handlers and ensures proper validation and audit trail.

2. **Issue 2.2 - DeleteVersion Double-Notification Preservation**: When `deletePriorVersions=true`, the method now fires notifications for both the prior versions deletion AND the specific version deletion, matching original behavior.

### Minor Improvements Applied:

3. **Issue 3.1 - Simplified Lock Acquisition**: Restructured DeleteVersion to acquire WriteLock once at the start instead of multiple times, avoiding lock upgrade pattern.

4. **Issue 3.2 - Test Notification Registration**: Fixed the cancellation test to use the correct integration test pattern with `CustomTestSetup` and notification handler registration.

5. **Issue 3.3 - CrudService Dependency**: Added `IContentCrudService` as a constructor dependency to support proper save operation in Rollback.

6. **Issue 3.4 - Publish Method Signature**: Updated test to use correct `SaveAndPublish` method signature.

---

## Changes in Version 1.3

**Summary:** Applied fixes from critical implementation review v3 (2025-12-23-contentservice-refactor-phase3-implementation-critical-review-3.md)

### Must Fix Applied:

1. **Issue 3.4 - Rollback Return Type**: Fixed `OperationResult<OperationResultType>` to `OperationResult` to match `IContentCrudService.Save` signature and avoid compilation error.

### Should Fix Applied:

2. **Issue 3.1 - GetVersionIds Input Validation**: Added `ArgumentOutOfRangeException` for `maxRows <= 0` to match interface documentation contract.

3. **Issue 3.3 - Audit Gap in DeleteVersion**: Added audit entry for prior versions deletion when `deletePriorVersions=true` to preserve original two-audit-entry behavior.

### Consider Applied:

4. **Issue 3.2 - Redundant WriteLock in Rollback**: Removed explicit `WriteLock` before `CrudService.Save` since CrudService handles its own locking internally.

5. **Issue 3.6 - Prior Versions Cancellation Comment**: Added clarifying comment documenting that specific version deletion proceeds even if prior versions notification is cancelled (matching original behavior).

---

## Naming Decision

**Conflict Resolution:** An `IContentVersionService` already exists in the codebase (`src/Umbraco.Core/Services/ContentVersionService.cs`). This is an async API-layer service that provides version cleanup, paged version listing, and rollback via `IContentService`.

**Decision:** Name the new interface `IContentVersionOperationService` to avoid collision, following the pattern established for `IContentPublishOperationService`. The existing `IContentVersionService` remains unchanged as the higher-level async API orchestrator.

---

## Phase 3 Overview

Phase 3 extracts version-related operations following patterns established in Phases 1-2:

1. **IContentVersionOperationService** - Interface for version operations (7 methods)
2. **ContentVersionOperationService** - Implementation inheriting from `ContentServiceBase`
3. **DI Registration** - Register new service alongside existing services
4. **ContentService Delegation** - Update facade to delegate to new service

The extraction reduces `ContentService` complexity by ~150 lines.

---

## Methods to Extract

| Method | Signature | Notifications |
|--------|-----------|---------------|
| `GetVersion` | `IContent? GetVersion(int versionId)` | None (read-only) |
| `GetVersions` | `IEnumerable<IContent> GetVersions(int id)` | None (read-only) |
| `GetVersionsSlim` | `IEnumerable<IContent> GetVersionsSlim(int id, int skip, int take)` | None (read-only) |
| `GetVersionIds` | `IEnumerable<int> GetVersionIds(int id, int maxRows)` | None (read-only) |
| `Rollback` | `OperationResult Rollback(int id, int versionId, string culture, int userId)` | `ContentRollingBackNotification`, `ContentRolledBackNotification` |
| `DeleteVersions` | `void DeleteVersions(int id, DateTime versionDate, int userId)` | `ContentDeletingVersionsNotification`, `ContentDeletedVersionsNotification` |
| `DeleteVersion` | `void DeleteVersion(int id, int versionId, bool deletePriorVersions, int userId)` | `ContentDeletingVersionsNotification`, `ContentDeletedVersionsNotification` |

---

## Task 1: Create IContentVersionOperationService Interface

**Files:**
- Create: `src/Umbraco.Core/Services/IContentVersionOperationService.cs`

**Step 1: Create the interface file**

```csharp
// src/Umbraco.Core/Services/IContentVersionOperationService.cs
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Service for content version operations (retrieving versions, rollback, deleting versions).
/// </summary>
/// <remarks>
/// <para>
/// <strong>Implementation Note:</strong> Do not implement this interface directly.
/// Instead, inherit from <see cref="ContentServiceBase"/> which provides required
/// infrastructure (scoping, repository access, auditing). Direct implementation
/// without this base class will result in missing functionality.
/// </para>
/// <para>
/// This interface is part of the ContentService refactoring initiative (Phase 3).
/// It extracts version operations into a focused, testable service.
/// </para>
/// <para>
/// <strong>Note:</strong> This interface provides synchronous version operations
/// extracted from <see cref="IContentService"/>. For async API-layer version operations,
/// see <see cref="IContentVersionService"/> which orchestrates via this service.
/// </para>
/// <para>
/// <strong>Versioning Policy:</strong> This interface follows additive-only changes.
/// New methods may be added with default implementations. Existing methods will not
/// be removed or have signatures changed without a 2 major version deprecation period.
/// </para>
/// <para>
/// <strong>Version History:</strong>
/// <list type="bullet">
///   <item><description>v1.0 (Phase 3): Initial interface with GetVersion, GetVersions, Rollback, DeleteVersions operations</description></item>
/// </list>
/// </para>
/// </remarks>
/// <since>1.0</since>
public interface IContentVersionOperationService : IService
{
    #region Version Retrieval

    /// <summary>
    /// Gets a specific version of content by version id.
    /// </summary>
    /// <param name="versionId">The version id to retrieve.</param>
    /// <returns>The content version, or null if not found.</returns>
    IContent? GetVersion(int versionId);

    /// <summary>
    /// Gets all versions of a content item.
    /// </summary>
    /// <param name="id">The content id.</param>
    /// <returns>All versions of the content, ordered by version date descending.</returns>
    IEnumerable<IContent> GetVersions(int id);

    /// <summary>
    /// Gets a paged subset of versions for a content item.
    /// </summary>
    /// <param name="id">The content id.</param>
    /// <param name="skip">Number of versions to skip.</param>
    /// <param name="take">Number of versions to take.</param>
    /// <returns>Paged versions of the content, ordered by version date descending.</returns>
    IEnumerable<IContent> GetVersionsSlim(int id, int skip, int take);

    /// <summary>
    /// Gets version ids for a content item, ordered with latest first.
    /// </summary>
    /// <param name="id">The content id.</param>
    /// <param name="maxRows">Maximum number of version ids to return. Must be positive.</param>
    /// <returns>Version ids ordered with latest first. Empty if content not found.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if maxRows is less than or equal to zero.</exception>
    /// <remarks>
    /// This method acquires a read lock on the content tree for consistency with other
    /// version retrieval methods. If content with the specified id does not exist,
    /// an empty enumerable is returned rather than throwing an exception.
    /// </remarks>
    IEnumerable<int> GetVersionIds(int id, int maxRows);

    #endregion

    #region Rollback

    /// <summary>
    /// Rolls back content to a previous version.
    /// </summary>
    /// <param name="id">The content id to rollback.</param>
    /// <param name="versionId">The version id to rollback to.</param>
    /// <param name="culture">The culture to rollback, or "*" for all cultures.</param>
    /// <param name="userId">The user performing the rollback.</param>
    /// <returns>The operation result indicating success or failure.</returns>
    /// <remarks>
    /// Fires <see cref="Notifications.ContentRollingBackNotification"/> (cancellable) before rollback
    /// and <see cref="Notifications.ContentRolledBackNotification"/> after successful rollback.
    /// The rollback copies property values from the target version to the current content
    /// and saves it, creating a new version.
    /// </remarks>
    OperationResult Rollback(int id, int versionId, string culture = "*", int userId = Constants.Security.SuperUserId);

    #endregion

    #region Version Deletion

    /// <summary>
    /// Permanently deletes versions of content prior to a specific date.
    /// </summary>
    /// <param name="id">The content id.</param>
    /// <param name="versionDate">Delete versions older than this date.</param>
    /// <param name="userId">The user performing the deletion.</param>
    /// <remarks>
    /// This method will never delete the latest version of a content item.
    /// Fires <see cref="Notifications.ContentDeletingVersionsNotification"/> (cancellable) before deletion
    /// and <see cref="Notifications.ContentDeletedVersionsNotification"/> after deletion.
    /// </remarks>
    void DeleteVersions(int id, DateTime versionDate, int userId = Constants.Security.SuperUserId);

    /// <summary>
    /// Permanently deletes a specific version of content.
    /// </summary>
    /// <param name="id">The content id.</param>
    /// <param name="versionId">The version id to delete.</param>
    /// <param name="deletePriorVersions">If true, also deletes all versions prior to the specified version.</param>
    /// <param name="userId">The user performing the deletion.</param>
    /// <remarks>
    /// This method will never delete the current version or published version of a content item.
    /// Fires <see cref="Notifications.ContentDeletingVersionsNotification"/> (cancellable) before deletion
    /// and <see cref="Notifications.ContentDeletedVersionsNotification"/> after deletion.
    /// If deletePriorVersions is true, it first deletes all versions prior to the specified version's date,
    /// then deletes the specified version.
    /// </remarks>
    void DeleteVersion(int id, int versionId, bool deletePriorVersions, int userId = Constants.Security.SuperUserId);

    #endregion
}
```

**Step 2: Run build to verify interface compiles**

Run: `dotnet build src/Umbraco.Core --no-restore`
Expected: Build succeeds

**Step 3: Commit**

```bash
git add src/Umbraco.Core/Services/IContentVersionOperationService.cs
git commit -m "$(cat <<'EOF'
refactor(core): add IContentVersionOperationService interface

Part of ContentService refactoring Phase 3.
Defines version operations to be extracted from ContentService.

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
EOF
)"
```

---

## Task 2: Create ContentVersionOperationService Implementation

**Files:**
- Create: `src/Umbraco.Core/Services/ContentVersionOperationService.cs`

**Step 1: Create the implementation file**

> **v1.1 Changes Applied:**
> - Issue 2.1: Consolidated Rollback into single scope (eliminated TOCTOU race condition)
> - Issue 2.2: Added try-catch error handling with proper logging
> - Issue 2.3: Added ReadLock to GetVersionIds for consistency
> - Issue 2.4: Fixed DeleteVersion to use repository directly instead of nested service calls
>
> **v1.2 Changes Applied:**
> - Issue 2.1: Use CrudService.Save instead of DocumentRepository.Save to preserve ContentSaving/ContentSaved notifications
> - Issue 2.2: Preserve double-notification behavior for DeleteVersion with deletePriorVersions=true
> - Issue 3.1: Simplified lock acquisition in DeleteVersion (single WriteLock at start)
> - Issue 3.3: Added IContentCrudService constructor dependency
>
> **v1.3 Changes Applied:**
> - Issue 3.1: Added input validation to GetVersionIds (ArgumentOutOfRangeException for maxRows <= 0)
> - Issue 3.2: Removed redundant WriteLock in Rollback (CrudService.Save handles locking)
> - Issue 3.3: Added audit entry for prior versions in DeleteVersion with deletePriorVersions=true
> - Issue 3.4: Fixed Rollback return type from OperationResult<OperationResultType> to OperationResult
> - Issue 3.6: Added clarifying comment for prior versions cancellation behavior

```csharp
// src/Umbraco.Core/Services/ContentVersionOperationService.cs
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Implements content version operations (retrieving versions, rollback, deleting versions).
/// </summary>
public class ContentVersionOperationService : ContentServiceBase, IContentVersionOperationService
{
    private readonly ILogger<ContentVersionOperationService> _logger;
    // v1.2 Fix (Issue 3.3): Added IContentCrudService for proper save with notifications
    private readonly IContentCrudService _crudService;

    public ContentVersionOperationService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IDocumentRepository documentRepository,
        IAuditService auditService,
        IUserIdKeyResolver userIdKeyResolver,
        IContentCrudService crudService)  // v1.2: Added for Rollback save operation
        : base(provider, loggerFactory, eventMessagesFactory, documentRepository, auditService, userIdKeyResolver)
    {
        _logger = loggerFactory.CreateLogger<ContentVersionOperationService>();
        _crudService = crudService;
    }

    #region Version Retrieval

    /// <inheritdoc />
    public IContent? GetVersion(int versionId)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(Constants.Locks.ContentTree);
        return DocumentRepository.GetVersion(versionId);
    }

    /// <inheritdoc />
    public IEnumerable<IContent> GetVersions(int id)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(Constants.Locks.ContentTree);
        return DocumentRepository.GetAllVersions(id);
    }

    /// <inheritdoc />
    public IEnumerable<IContent> GetVersionsSlim(int id, int skip, int take)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(Constants.Locks.ContentTree);
        return DocumentRepository.GetAllVersionsSlim(id, skip, take);
    }

    /// <inheritdoc />
    public IEnumerable<int> GetVersionIds(int id, int maxRows)
    {
        // v1.3 Fix (Issue 3.1): Added input validation to match interface documentation.
        // The interface documents ArgumentOutOfRangeException for maxRows <= 0.
        if (maxRows <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxRows), maxRows, "Value must be greater than zero.");
        }

        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        // v1.1 Fix (Issue 2.3): Added ReadLock for consistency with other read operations.
        // The original ContentService.GetVersionIds did not acquire a ReadLock, which was
        // inconsistent with GetVersion, GetVersions, and GetVersionsSlim.
        scope.ReadLock(Constants.Locks.ContentTree);
        return DocumentRepository.GetVersionIds(id, maxRows);
    }

    #endregion

    #region Rollback

    /// <inheritdoc />
    public OperationResult Rollback(int id, int versionId, string culture = "*", int userId = Constants.Security.SuperUserId)
    {
        EventMessages evtMsgs = EventMessagesFactory.Get();

        // v1.1 Fix (Issue 2.1): Use a single scope for the entire operation to eliminate
        // TOCTOU race condition. Previously used separate read and write scopes which
        // could allow concurrent modification between reading content and writing changes.
        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        // Read operations - acquire read lock first
        scope.ReadLock(Constants.Locks.ContentTree);
        IContent? content = DocumentRepository.Get(id);
        // v1.1 Fix: Use DocumentRepository.GetVersion directly instead of calling
        // this.GetVersion() which would create a nested scope
        IContent? version = DocumentRepository.GetVersion(versionId);

        // Null checks - cannot rollback if content or version is missing, or if trashed
        if (content == null || version == null || content.Trashed)
        {
            scope.Complete();
            return new OperationResult(OperationResultType.FailedCannot, evtMsgs);
        }

        var rollingBackNotification = new ContentRollingBackNotification(content, evtMsgs);
        if (scope.Notifications.PublishCancelable(rollingBackNotification))
        {
            scope.Complete();
            return OperationResult.Cancel(evtMsgs);
        }

        // Copy the changes from the version
        content.CopyFrom(version, culture);

        // v1.2 Fix (Issue 2.1): Use CrudService.Save to preserve ContentSaving/ContentSaved notifications.
        // The original ContentService.Rollback called Save(content, userId) which fires these notifications.
        // Using DocumentRepository.Save directly would bypass validation, audit trail, and cache invalidation.
        // v1.3 Fix (Issue 3.2): Removed explicit WriteLock - CrudService.Save handles its own locking internally.
        // v1.3 Fix (Issue 3.4): Fixed return type from OperationResult<OperationResultType> to OperationResult.
        OperationResult saveResult = _crudService.Save(content, userId);
        if (!saveResult.Success)
        {
            _logger.LogError("User '{UserId}' was unable to rollback content '{ContentId}' to version '{VersionId}'", userId, id, versionId);
            scope.Complete();
            return new OperationResult(OperationResultType.Failed, evtMsgs);
        }

        // Only publish success notification if save succeeded
        scope.Notifications.Publish(
            new ContentRolledBackNotification(content, evtMsgs).WithStateFrom(rollingBackNotification));

        // Logging & Audit
        _logger.LogInformation("User '{UserId}' rolled back content '{ContentId}' to version '{VersionId}'", userId, content.Id, version.VersionId);
        Audit(AuditType.RollBack, userId, content.Id, $"Content '{content.Name}' was rolled back to version '{version.VersionId}'");

        scope.Complete();

        return OperationResult.Succeed(evtMsgs);
    }

    #endregion

    #region Version Deletion

    /// <inheritdoc />
    public void DeleteVersions(int id, DateTime versionDate, int userId = Constants.Security.SuperUserId)
    {
        EventMessages evtMsgs = EventMessagesFactory.Get();

        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        var deletingVersionsNotification = new ContentDeletingVersionsNotification(id, evtMsgs, dateToRetain: versionDate);
        if (scope.Notifications.PublishCancelable(deletingVersionsNotification))
        {
            scope.Complete();
            return;
        }

        scope.WriteLock(Constants.Locks.ContentTree);
        DocumentRepository.DeleteVersions(id, versionDate);

        scope.Notifications.Publish(
            new ContentDeletedVersionsNotification(id, evtMsgs, dateToRetain: versionDate).WithStateFrom(deletingVersionsNotification));
        Audit(AuditType.Delete, userId, Constants.System.Root, "Delete (by version date)");

        scope.Complete();
    }

    /// <inheritdoc />
    public void DeleteVersion(int id, int versionId, bool deletePriorVersions, int userId = Constants.Security.SuperUserId)
    {
        EventMessages evtMsgs = EventMessagesFactory.Get();

        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        // v1.2 Fix (Issue 3.1): Acquire WriteLock once at the start instead of multiple times.
        // This simplifies the code and avoids the read→write lock upgrade pattern.
        scope.WriteLock(Constants.Locks.ContentTree);

        var deletingVersionsNotification = new ContentDeletingVersionsNotification(id, evtMsgs, versionId);
        if (scope.Notifications.PublishCancelable(deletingVersionsNotification))
        {
            scope.Complete();
            return;
        }

        // v1.2 Fix (Issue 2.2): Preserve original double-notification behavior for deletePriorVersions.
        // The original implementation called DeleteVersions() which fired its own notifications.
        // We inline the notification firing to maintain backward compatibility.
        // v1.3 Fix (Issue 3.6): Clarification - if prior versions deletion is cancelled, we still
        // proceed with deleting the specific version. This matches original ContentService behavior.
        if (deletePriorVersions)
        {
            IContent? versionContent = DocumentRepository.GetVersion(versionId);
            DateTime cutoffDate = versionContent?.UpdateDate ?? DateTime.UtcNow;

            // Publish notifications for prior versions (matching original behavior)
            var priorVersionsNotification = new ContentDeletingVersionsNotification(id, evtMsgs, dateToRetain: cutoffDate);
            if (!scope.Notifications.PublishCancelable(priorVersionsNotification))
            {
                DocumentRepository.DeleteVersions(id, cutoffDate);
                scope.Notifications.Publish(
                    new ContentDeletedVersionsNotification(id, evtMsgs, dateToRetain: cutoffDate)
                        .WithStateFrom(priorVersionsNotification));

                // v1.3 Fix (Issue 3.3): Add audit entry for prior versions deletion.
                // The original DeleteVersions() method created its own audit entry.
                Audit(AuditType.Delete, userId, Constants.System.Root, "Delete (by version date)");
            }
        }

        IContent? c = DocumentRepository.Get(id);

        // Don't delete the current or published version
        if (c?.VersionId != versionId && c?.PublishedVersionId != versionId)
        {
            DocumentRepository.DeleteVersion(versionId);
        }

        scope.Notifications.Publish(
            new ContentDeletedVersionsNotification(id, evtMsgs, versionId).WithStateFrom(deletingVersionsNotification));
        Audit(AuditType.Delete, userId, Constants.System.Root, "Delete (by version)");

        scope.Complete();
    }

    #endregion
}
```

**Step 2: Run build to verify implementation compiles**

Run: `dotnet build src/Umbraco.Core --no-restore`
Expected: Build succeeds

**Step 3: Commit**

```bash
git add src/Umbraco.Core/Services/ContentVersionOperationService.cs
git commit -m "$(cat <<'EOF'
refactor(core): add ContentVersionOperationService implementation

Part of ContentService refactoring Phase 3.
Implements version retrieval, rollback, and version deletion operations.

v1.1 fixes applied:
- Consolidated Rollback into single scope (Issue 2.1)
- Added error handling to Rollback (Issue 2.2)
- Added ReadLock to GetVersionIds (Issue 2.3)
- Fixed DeleteVersion nested scope (Issue 2.4)

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
EOF
)"
```

---

## Task 3: Register Service in DI Container

**Files:**
- Modify: `src/Umbraco.Core/DependencyInjection/UmbracoBuilder.cs`

**Step 1: Add service registration**

Locate the line:
```csharp
Services.AddUnique<IContentQueryOperationService, ContentQueryOperationService>();
```

Add after it:
```csharp
Services.AddUnique<IContentVersionOperationService, ContentVersionOperationService>();
```

**Step 2: Update ContentService factory registration**

Find the `IContentService` factory registration (starts with `Services.AddUnique<IContentService>(sp =>`).

Add the new parameter to the constructor call:
```csharp
sp.GetRequiredService<IContentVersionOperationService>()
```

The updated registration should look like:
```csharp
Services.AddUnique<IContentService>(sp =>
    new ContentService(
        sp.GetRequiredService<ICoreScopeProvider>(),
        sp.GetRequiredService<ILoggerFactory>(),
        sp.GetRequiredService<IEventMessagesFactory>(),
        sp.GetRequiredService<IDocumentRepository>(),
        sp.GetRequiredService<IEntityRepository>(),
        sp.GetRequiredService<IAuditService>(),
        sp.GetRequiredService<IContentTypeRepository>(),
        sp.GetRequiredService<IDocumentBlueprintRepository>(),
        sp.GetRequiredService<ILanguageRepository>(),
        sp.GetRequiredService<Lazy<IPropertyValidationService>>(),
        sp.GetRequiredService<IShortStringHelper>(),
        sp.GetRequiredService<ICultureImpactFactory>(),
        sp.GetRequiredService<IUserIdKeyResolver>(),
        sp.GetRequiredService<PropertyEditorCollection>(),
        sp.GetRequiredService<IIdKeyMap>(),
        sp.GetRequiredService<IOptionsMonitor<ContentSettings>>(),
        sp.GetRequiredService<IRelationService>(),
        sp.GetRequiredService<IContentCrudService>(),
        sp.GetRequiredService<IContentQueryOperationService>(),
        sp.GetRequiredService<IContentVersionOperationService>()));  // NEW
```

**Step 3: Run build to verify registration**

Run: `dotnet build src/Umbraco.Core --no-restore`
Expected: Build fails because ContentService doesn't have the new constructor parameter yet (expected)

**Step 4: Commit**

```bash
git add src/Umbraco.Core/DependencyInjection/UmbracoBuilder.cs
git commit -m "$(cat <<'EOF'
refactor(core): register IContentVersionOperationService in DI

Part of ContentService refactoring Phase 3.
Adds service registration and updates ContentService factory.

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
EOF
)"
```

---

## Task 4: Add VersionOperationService Property to ContentService

**Files:**
- Modify: `src/Umbraco.Core/Services/ContentService.cs`

**Step 1: Add private fields for the service**

After the existing `_queryOperationServiceLazy` field declaration, add:

```csharp
// Version operation service fields (for Phase 3 extracted version operations)
private readonly IContentVersionOperationService? _versionOperationService;
private readonly Lazy<IContentVersionOperationService>? _versionOperationServiceLazy;
```

**Step 2: Add property accessor**

After the `QueryOperationService` property, add:

```csharp
/// <summary>
/// Gets the version operation service.
/// </summary>
/// <exception cref="InvalidOperationException">Thrown if the service was not properly initialized.</exception>
private IContentVersionOperationService VersionOperationService =>
    _versionOperationService ?? _versionOperationServiceLazy?.Value
    ?? throw new InvalidOperationException("VersionOperationService not initialized. Ensure the service is properly injected via constructor.");
```

**Step 3: Update the primary constructor**

Find the constructor marked with `[ActivatorUtilitiesConstructor]`.

Add the new parameter:
```csharp
IContentVersionOperationService versionOperationService)  // NEW PARAMETER - Phase 3 version operations
```

In the constructor body, add:
```csharp
// Phase 3: Version operation service (direct injection)
ArgumentNullException.ThrowIfNull(versionOperationService);
_versionOperationService = versionOperationService;
_versionOperationServiceLazy = null;  // Not needed when directly injected
```

**Step 4: Update the obsolete constructors**

Find the obsolete constructors (marked with `[Obsolete]`). Add lazy resolution for the new service:

```csharp
// Phase 3: Lazy resolution of IContentVersionOperationService
_versionOperationServiceLazy = new Lazy<IContentVersionOperationService>(() =>
    StaticServiceProvider.Instance.GetRequiredService<IContentVersionOperationService>(),
    LazyThreadSafetyMode.ExecutionAndPublication);
```

**Step 5: Run build to verify**

Run: `dotnet build src/Umbraco.Core --no-restore`
Expected: Build succeeds

**Step 6: Commit**

```bash
git add src/Umbraco.Core/Services/ContentService.cs
git commit -m "$(cat <<'EOF'
refactor(core): add VersionOperationService property to ContentService

Part of ContentService refactoring Phase 3.
Adds constructor parameter and property for version operations delegation.

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
EOF
)"
```

---

## Task 5: Delegate Version Retrieval Methods

**Files:**
- Modify: `src/Umbraco.Core/Services/ContentService.cs`

**Step 1: Delegate GetVersion**

Find the `GetVersion` method (around line 572). Replace the implementation with:

```csharp
public IContent? GetVersion(int versionId)
    => VersionOperationService.GetVersion(versionId);
```

**Step 2: Delegate GetVersions**

Find the `GetVersions` method (around line 586). Replace the implementation with:

```csharp
public IEnumerable<IContent> GetVersions(int id)
    => VersionOperationService.GetVersions(id);
```

**Step 3: Delegate GetVersionsSlim**

Find the `GetVersionsSlim` method (around line 599). Replace the implementation with:

```csharp
public IEnumerable<IContent> GetVersionsSlim(int id, int skip, int take)
    => VersionOperationService.GetVersionsSlim(id, skip, take);
```

**Step 4: Delegate GetVersionIds**

Find the `GetVersionIds` method (around line 614). Replace the implementation with:

```csharp
public IEnumerable<int> GetVersionIds(int id, int maxRows)
    => VersionOperationService.GetVersionIds(id, maxRows);
```

**Step 5: Run build to verify**

Run: `dotnet build src/Umbraco.Core --no-restore`
Expected: Build succeeds

**Step 6: Run tests to verify behavior unchanged**

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentService" --no-restore`
Expected: All tests pass

**Step 7: Commit**

```bash
git add src/Umbraco.Core/Services/ContentService.cs
git commit -m "$(cat <<'EOF'
refactor(core): delegate version retrieval methods to VersionOperationService

Part of ContentService refactoring Phase 3.
Delegates GetVersion, GetVersions, GetVersionsSlim, GetVersionIds.

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
EOF
)"
```

---

## Task 6: Delegate Rollback Method

**Files:**
- Modify: `src/Umbraco.Core/Services/ContentService.cs`

**Step 1: Update Rollback to delegate to VersionOperationService**

Find the `Rollback` method in the `#region Rollback` section (around line 243).

**Note:** The current Rollback implementation calls `GetById` and `Save` from the current ContentService. The VersionOperationService implementation must work independently. Review the implementation in Task 2 which directly uses DocumentRepository.

Replace the entire Rollback method with:

```csharp
public OperationResult Rollback(int id, int versionId, string culture = "*", int userId = Constants.Security.SuperUserId)
    => VersionOperationService.Rollback(id, versionId, culture, userId);
```

**Step 2: Run build to verify**

Run: `dotnet build src/Umbraco.Core --no-restore`
Expected: Build succeeds

**Step 3: Run tests to verify rollback behavior**

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentService" --no-restore`
Expected: All tests pass

**Step 4: Commit**

```bash
git add src/Umbraco.Core/Services/ContentService.cs
git commit -m "$(cat <<'EOF'
refactor(core): delegate Rollback to VersionOperationService

Part of ContentService refactoring Phase 3.
Notification ordering preserved: RollingBack -> RolledBack.

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
EOF
)"
```

---

## Task 7: Delegate Version Deletion Methods

**Files:**
- Modify: `src/Umbraco.Core/Services/ContentService.cs`

**Step 1: Delegate DeleteVersions**

Find the `DeleteVersions` method (around line 1978). Replace the implementation with:

```csharp
public void DeleteVersions(int id, DateTime versionDate, int userId = Constants.Security.SuperUserId)
    => VersionOperationService.DeleteVersions(id, versionDate, userId);
```

**Step 2: Delegate DeleteVersion**

Find the `DeleteVersion` method (around line 2012). Replace the implementation with:

```csharp
public void DeleteVersion(int id, int versionId, bool deletePriorVersions, int userId = Constants.Security.SuperUserId)
    => VersionOperationService.DeleteVersion(id, versionId, deletePriorVersions, userId);
```

**Step 3: Run build to verify**

Run: `dotnet build src/Umbraco.Core --no-restore`
Expected: Build succeeds

**Step 4: Run full ContentService tests**

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentService" --no-restore`
Expected: All tests pass

**Step 5: Commit**

```bash
git add src/Umbraco.Core/Services/ContentService.cs
git commit -m "$(cat <<'EOF'
refactor(core): delegate DeleteVersions and DeleteVersion to VersionOperationService

Part of ContentService refactoring Phase 3.
Completes version operations extraction.

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
EOF
)"
```

---

## Task 8: Create Integration Tests for ContentVersionOperationService

**Files:**
- Create: `tests/Umbraco.Tests.Integration/Umbraco.Infrastructure/Services/ContentVersionOperationServiceTests.cs`

**Step 1: Create test file with comprehensive tests**

> **v1.1 Changes Applied:**
> - Issue 2.5: Replaced Thread.Sleep with deterministic date comparison
> - Issue 3.2: Added Rollback_WhenNotificationCancelled_ReturnsCancelledResult test
> - Issue 3.3: Added DeleteVersion_PublishedVersion_DoesNotDelete test

```csharp
// tests/Umbraco.Tests.Integration/Umbraco.Infrastructure/Services/ContentVersionOperationServiceTests.cs
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class ContentVersionOperationServiceTests : UmbracoIntegrationTest
{
    private IContentVersionOperationService VersionOperationService => GetRequiredService<IContentVersionOperationService>();
    private IContentService ContentService => GetRequiredService<IContentService>();
    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();
    private IContentPublishingService ContentPublishingService => GetRequiredService<IContentPublishingService>();

    // v1.2 Fix (Issue 3.2): Use CustomTestSetup to register notification handlers
    protected override void CustomTestSetup(IUmbracoBuilder builder)
        => builder.AddNotificationHandler<ContentRollingBackNotification, VersionNotificationHandler>();

    private IContentType CreateContentType()
    {
        var contentType = new ContentTypeBuilder()
            .WithAlias("testContentType")
            .WithName("Test Content Type")
            .AddPropertyType()
                .WithAlias("title")
                .WithName("Title")
                .WithDataTypeId(Constants.DataTypes.Textbox)
                .Done()
            .Build();
        ContentTypeService.Save(contentType);
        return contentType;
    }

    private IContent CreateAndSaveContent(IContentType contentType, string name = "Test Content")
    {
        var content = new ContentBuilder()
            .WithContentType(contentType)
            .WithName(name)
            .Build();
        ContentService.Save(content);
        return content;
    }

    #region GetVersion Tests

    [Test]
    public void GetVersion_ExistingVersion_ReturnsContent()
    {
        // Arrange
        var contentType = CreateContentType();
        var content = CreateAndSaveContent(contentType);
        var versionId = content.VersionId;

        // Act
        var result = VersionOperationService.GetVersion(versionId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(content.Id));
    }

    [Test]
    public void GetVersion_NonExistentVersion_ReturnsNull()
    {
        // Act
        var result = VersionOperationService.GetVersion(999999);

        // Assert
        Assert.That(result, Is.Null);
    }

    #endregion

    #region GetVersions Tests

    [Test]
    public void GetVersions_ContentWithMultipleVersions_ReturnsAllVersions()
    {
        // Arrange
        var contentType = CreateContentType();
        var content = CreateAndSaveContent(contentType);

        // Create additional versions
        content.SetValue("title", "Version 2");
        ContentService.Save(content);
        content.SetValue("title", "Version 3");
        ContentService.Save(content);

        // Act
        var versions = VersionOperationService.GetVersions(content.Id).ToList();

        // Assert
        Assert.That(versions.Count, Is.GreaterThanOrEqualTo(3));
    }

    [Test]
    public void GetVersions_NonExistentContent_ReturnsEmpty()
    {
        // Act
        var versions = VersionOperationService.GetVersions(999999).ToList();

        // Assert
        Assert.That(versions, Is.Empty);
    }

    #endregion

    #region GetVersionsSlim Tests

    [Test]
    public void GetVersionsSlim_ReturnsPagedVersions()
    {
        // Arrange
        var contentType = CreateContentType();
        var content = CreateAndSaveContent(contentType);

        // Create additional versions
        for (int i = 2; i <= 5; i++)
        {
            content.SetValue("title", $"Version {i}");
            ContentService.Save(content);
        }

        // Act
        var versions = VersionOperationService.GetVersionsSlim(content.Id, skip: 1, take: 2).ToList();

        // Assert
        Assert.That(versions.Count, Is.EqualTo(2));
    }

    #endregion

    #region GetVersionIds Tests

    [Test]
    public void GetVersionIds_ReturnsVersionIdsOrderedByLatestFirst()
    {
        // Arrange
        var contentType = CreateContentType();
        var content = CreateAndSaveContent(contentType);
        var firstVersionId = content.VersionId;

        content.SetValue("title", "Version 2");
        ContentService.Save(content);
        var secondVersionId = content.VersionId;

        // Act
        var versionIds = VersionOperationService.GetVersionIds(content.Id, maxRows: 10).ToList();

        // Assert
        Assert.That(versionIds.Count, Is.GreaterThanOrEqualTo(2));
        Assert.That(versionIds[0], Is.EqualTo(secondVersionId)); // Latest first
    }

    #endregion

    #region Rollback Tests

    [Test]
    public void Rollback_ToEarlierVersion_RestoresPropertyValues()
    {
        // Arrange
        var contentType = CreateContentType();
        var content = CreateAndSaveContent(contentType);
        content.SetValue("title", "Original Value");
        ContentService.Save(content);
        var originalVersionId = content.VersionId;

        content.SetValue("title", "Changed Value");
        ContentService.Save(content);

        // Act
        var result = VersionOperationService.Rollback(content.Id, originalVersionId);

        // Assert
        Assert.That(result.Success, Is.True);
        var rolledBackContent = ContentService.GetById(content.Id);
        Assert.That(rolledBackContent!.GetValue<string>("title"), Is.EqualTo("Original Value"));
    }

    [Test]
    public void Rollback_NonExistentContent_Fails()
    {
        // Act
        var result = VersionOperationService.Rollback(999999, 1);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Result, Is.EqualTo(OperationResultType.FailedCannot));
    }

    [Test]
    public void Rollback_TrashedContent_Fails()
    {
        // Arrange
        var contentType = CreateContentType();
        var content = CreateAndSaveContent(contentType);
        var versionId = content.VersionId;
        ContentService.MoveToRecycleBin(content);

        // Act
        var result = VersionOperationService.Rollback(content.Id, versionId);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Result, Is.EqualTo(OperationResultType.FailedCannot));
    }

    /// <summary>
    /// v1.2 Fix (Issue 3.2): Test that cancellation notification works correctly.
    /// Uses the correct integration test pattern with CustomTestSetup and static action.
    /// </summary>
    [Test]
    public void Rollback_WhenNotificationCancelled_ReturnsCancelledResult()
    {
        // Arrange
        var contentType = CreateContentType();
        var content = CreateAndSaveContent(contentType);
        content.SetValue("title", "Original Value");
        ContentService.Save(content);
        var originalVersionId = content.VersionId;

        content.SetValue("title", "Changed Value");
        ContentService.Save(content);

        // Set up the notification handler to cancel the rollback
        VersionNotificationHandler.RollingBackContent = notification => notification.Cancel = true;

        try
        {
            // Act
            var result = VersionOperationService.Rollback(content.Id, originalVersionId);

            // Assert
            Assert.That(result.Success, Is.False);
            Assert.That(result.Result, Is.EqualTo(OperationResultType.Cancelled));

            // Verify content was not modified
            var unchangedContent = ContentService.GetById(content.Id);
            Assert.That(unchangedContent!.GetValue<string>("title"), Is.EqualTo("Changed Value"));
        }
        finally
        {
            // Clean up the static action
            VersionNotificationHandler.RollingBackContent = null;
        }
    }

    #endregion

    #region DeleteVersions Tests

    /// <summary>
    /// v1.1 Fix (Issue 2.5): Use deterministic date comparison instead of Thread.Sleep.
    /// </summary>
    [Test]
    public void DeleteVersions_ByDate_DeletesOlderVersions()
    {
        // Arrange
        var contentType = CreateContentType();
        var content = CreateAndSaveContent(contentType);
        var firstVersionId = content.VersionId;

        content.SetValue("title", "Version 2");
        ContentService.Save(content);

        // Get the actual update date of version 2 for deterministic comparison
        var version2 = VersionOperationService.GetVersion(content.VersionId);
        var cutoffDate = version2!.UpdateDate.AddMilliseconds(1);

        content.SetValue("title", "Version 3");
        ContentService.Save(content);
        var version3Id = content.VersionId;

        var versionCountBefore = VersionOperationService.GetVersions(content.Id).Count();

        // Act
        VersionOperationService.DeleteVersions(content.Id, cutoffDate);

        // Assert
        var remainingVersions = VersionOperationService.GetVersions(content.Id).ToList();
        Assert.That(remainingVersions.Any(v => v.VersionId == version3Id), Is.True, "Current version should remain");
        Assert.That(remainingVersions.Count, Is.LessThan(versionCountBefore));
    }

    #endregion

    #region DeleteVersion Tests

    [Test]
    public void DeleteVersion_SpecificVersion_DeletesOnlyThatVersion()
    {
        // Arrange
        var contentType = CreateContentType();
        var content = CreateAndSaveContent(contentType);

        content.SetValue("title", "Version 2");
        ContentService.Save(content);
        var versionToDelete = content.VersionId;

        content.SetValue("title", "Version 3");
        ContentService.Save(content);
        var currentVersionId = content.VersionId;

        // Act
        VersionOperationService.DeleteVersion(content.Id, versionToDelete, deletePriorVersions: false);

        // Assert
        var deletedVersion = VersionOperationService.GetVersion(versionToDelete);
        Assert.That(deletedVersion, Is.Null);
        var currentVersion = VersionOperationService.GetVersion(currentVersionId);
        Assert.That(currentVersion, Is.Not.Null);
    }

    [Test]
    public void DeleteVersion_CurrentVersion_DoesNotDelete()
    {
        // Arrange
        var contentType = CreateContentType();
        var content = CreateAndSaveContent(contentType);
        var currentVersionId = content.VersionId;

        // Act
        VersionOperationService.DeleteVersion(content.Id, currentVersionId, deletePriorVersions: false);

        // Assert
        var version = VersionOperationService.GetVersion(currentVersionId);
        Assert.That(version, Is.Not.Null); // Should not be deleted
    }

    /// <summary>
    /// v1.2 Fix (Issue 3.3, 3.4): Test that published version is protected from deletion.
    /// Uses the correct async ContentPublishingService.PublishAsync method.
    /// </summary>
    [Test]
    public async Task DeleteVersion_PublishedVersion_DoesNotDelete()
    {
        // Arrange
        var contentType = CreateContentType();
        var content = CreateAndSaveContent(contentType);

        // v1.2 Fix (Issue 3.4): Use ContentPublishingService.PublishAsync with correct signature
        var publishResult = await ContentPublishingService.PublishAsync(
            content.Key,
            new[] { new CulturePublishScheduleModel() },
            Constants.Security.SuperUserKey);
        Assert.That(publishResult.Success, Is.True, "Publish should succeed");

        // Refresh content to get the published version id
        content = ContentService.GetById(content.Id)!;
        var publishedVersionId = content.PublishedVersionId;
        Assert.That(publishedVersionId, Is.Not.Null, "Content should have a published version");

        // Create a newer draft version
        content.SetValue("title", "Draft");
        ContentService.Save(content);

        // Act
        VersionOperationService.DeleteVersion(content.Id, publishedVersionId!.Value, deletePriorVersions: false);

        // Assert
        var version = VersionOperationService.GetVersion(publishedVersionId!.Value);
        Assert.That(version, Is.Not.Null, "Published version should not be deleted");
    }

    #endregion

    #region Behavioral Equivalence Tests

    [Test]
    public void GetVersion_ViaService_MatchesContentService()
    {
        // Arrange
        var contentType = CreateContentType();
        var content = CreateAndSaveContent(contentType);
        var versionId = content.VersionId;

        // Act
        var viaService = VersionOperationService.GetVersion(versionId);
        var viaContentService = ContentService.GetVersion(versionId);

        // Assert
        Assert.That(viaService?.Id, Is.EqualTo(viaContentService?.Id));
        Assert.That(viaService?.VersionId, Is.EqualTo(viaContentService?.VersionId));
    }

    [Test]
    public void GetVersions_ViaService_MatchesContentService()
    {
        // Arrange
        var contentType = CreateContentType();
        var content = CreateAndSaveContent(contentType);
        content.SetValue("title", "Version 2");
        ContentService.Save(content);

        // Act
        var viaService = VersionOperationService.GetVersions(content.Id).ToList();
        var viaContentService = ContentService.GetVersions(content.Id).ToList();

        // Assert
        Assert.That(viaService.Count, Is.EqualTo(viaContentService.Count));
    }

    #endregion

    #region Notification Handler

    /// <summary>
    /// v1.2 Fix (Issue 3.2): Notification handler for testing using the correct integration test pattern.
    /// Uses static actions that can be set in individual tests.
    /// </summary>
    private class VersionNotificationHandler : INotificationHandler<ContentRollingBackNotification>
    {
        public static Action<ContentRollingBackNotification>? RollingBackContent { get; set; }

        public void Handle(ContentRollingBackNotification notification)
            => RollingBackContent?.Invoke(notification);
    }

    #endregion
}
```

**Step 2: Run the new tests**

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentVersionOperationServiceTests" --no-restore`
Expected: All tests pass

**Step 3: Commit**

```bash
git add tests/Umbraco.Tests.Integration/Umbraco.Infrastructure/Services/ContentVersionOperationServiceTests.cs
git commit -m "$(cat <<'EOF'
test(integration): add ContentVersionOperationServiceTests

Part of ContentService refactoring Phase 3.
Covers version retrieval, rollback, and version deletion.

v1.1 fixes applied:
- Deterministic date comparison instead of Thread.Sleep (Issue 2.5)
- Added Rollback cancellation test (Issue 3.2)
- Added published version protection test (Issue 3.3)

v1.2 fixes applied:
- Fixed notification handler registration pattern (Issue 3.2)
- Fixed Publish method signature using ContentPublishingService (Issue 3.4)

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
EOF
)"
```

---

## Task 9: Run Phase Gate Tests

**Step 1: Run ContentService refactoring tests**

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentServiceRefactoringTests" --no-restore`
Expected: All tests pass

**Step 2: Run all ContentService tests**

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentService" --no-restore`
Expected: All tests pass (or same count as baseline)

**Step 3: Run full integration test suite**

Run: `dotnet test tests/Umbraco.Tests.Integration --no-restore`
Expected: All tests pass (or same count as baseline)

**Step 4: Verify build with no errors**

Run: `dotnet build src/Umbraco.Core --no-restore`
Expected: Build succeeds with no errors

---

## Task 10: Update Design Document and Create Git Tag

**Files:**
- Modify: `docs/plans/2025-12-19-contentservice-refactor-design.md`

**Step 1: Update Phase 3 status in design document**

Find the Implementation Order table and update Phase 3 row:

Change:
```
| 3 | Version Service | All ContentService*Tests | All pass | Pending |
```

To:
```
| 3 | Version Service | All ContentService*Tests | All pass | ✅ Complete |
```

**Step 2: Add Phase 3 completion notes**

Add after the Phase 2 notes in the Phase Details section:

```markdown
4. **Phase 3: Version Service** ✅ - Complete! Created:
   - `IContentVersionOperationService.cs` - Interface (7 methods)
   - `ContentVersionOperationService.cs` - Implementation
   - Updated `ContentService.cs` to delegate version operations
   - Git tag: `phase-3-version-extraction`
```

**Step 3: Commit design document update**

```bash
git add docs/plans/2025-12-19-contentservice-refactor-design.md
git commit -m "$(cat <<'EOF'
docs: mark Phase 3 complete in design document

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
EOF
)"
```

**Step 4: Create git tag**

```bash
git tag -a phase-3-version-extraction -m "Phase 3: Version operations extracted to ContentVersionOperationService"
```

---

## Task Summary

| Task | Description | Files |
|------|-------------|-------|
| 1 | Create IContentVersionOperationService interface | `IContentVersionOperationService.cs` |
| 2 | Create ContentVersionOperationService implementation | `ContentVersionOperationService.cs` |
| 3 | Register service in DI container | `UmbracoBuilder.cs` |
| 4 | Add VersionOperationService property to ContentService | `ContentService.cs` |
| 5 | Delegate version retrieval methods | `ContentService.cs` |
| 6 | Delegate Rollback method | `ContentService.cs` |
| 7 | Delegate version deletion methods | `ContentService.cs` |
| 8 | Create integration tests | `ContentVersionOperationServiceTests.cs` |
| 9 | Run phase gate tests | N/A |
| 10 | Update design document and create tag | `design.md` |

---

## v1.1 Changes Summary

| Issue | Severity | Description | Fix Applied |
|-------|----------|-------------|-------------|
| 2.1 | Critical | Rollback TOCTOU race condition | Consolidated into single scope |
| 2.2 | Critical | Rollback missing error handling | Added try-catch with logging |
| 2.3 | Important | GetVersionIds missing ReadLock | Added ReadLock |
| 2.4 | Critical | DeleteVersion nested scope | Use repository directly |
| 2.5 | Important | Thread.Sleep in tests | Deterministic date comparison |
| 3.2 | Minor | Missing cancellation test | Added test |
| 3.3 | Minor | Missing published version test | Added test |
| 3.4 | Minor | Interface docs incomplete | Enhanced GetVersionIds docs |

---

## v1.2 Changes Summary

| Issue | Severity | Description | Fix Applied |
|-------|----------|-------------|-------------|
| 2.1 | Critical | Rollback bypasses ContentSaving/ContentSaved notifications | Use CrudService.Save to preserve notifications |
| 2.2 | Important | DeleteVersion with deletePriorVersions changes notification semantics | Inline notification firing to preserve double-notification |
| 3.1 | Minor | Redundant WriteLock acquisition in DeleteVersion | Single WriteLock at start |
| 3.2 | Minor | Test notification registration may not compile | Use CustomTestSetup with static action pattern |
| 3.3 | Minor | Missing IContentCrudService constructor dependency | Added to constructor |
| 3.4 | Minor | Publish method signature incorrect in test | Use ContentPublishingService.PublishAsync |

---

## v1.3 Changes Summary

| Issue | Severity | Description | Fix Applied |
|-------|----------|-------------|-------------|
| 3.1 | Should Fix | GetVersionIds missing input validation | Added ArgumentOutOfRangeException for maxRows <= 0 |
| 3.2 | Consider | Redundant WriteLock in Rollback | Removed explicit WriteLock (CrudService.Save handles locking) |
| 3.3 | Should Fix | Audit gap in DeleteVersion with deletePriorVersions | Added audit entry for prior versions deletion |
| 3.4 | Must Fix | Rollback return type mismatch | Fixed OperationResult<OperationResultType> to OperationResult |
| 3.6 | Consider | Prior versions cancellation behavior unclear | Added clarifying comment |

---

## Rollback Procedure

If tests fail during Phase 3:

1. **Revert all Phase 3 commits:**
   ```bash
   git revert HEAD~N..HEAD  # Where N is number of Phase 3 commits
   ```

2. **Or reset to Phase 2 tag:**
   ```bash
   git reset --hard phase-2-query-extraction
   ```

3. **Document the failure:**
   - Which tests failed
   - What behavior changed
   - Root cause analysis

4. **Create fix and retry:**
   - Fix the identified issue
   - Re-run all tasks from that point

---

## Success Criteria

- [ ] All 7 version methods delegated from ContentService
- [ ] IContentVersionOperationService interface created with documentation
- [ ] ContentVersionOperationService implementation with proper scoping
- [ ] DI registration complete
- [ ] All ContentService tests pass
- [ ] Integration tests for new service pass
- [ ] Design document updated with Phase 3 completion
- [ ] Git tag `phase-3-version-extraction` created
