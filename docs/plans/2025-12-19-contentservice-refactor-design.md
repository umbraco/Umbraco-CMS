# ContentService Refactoring Design

**Date**: 2025-12-19
**Status**: Revised (post-test strategy)
**Branch**: refactor/ContentService

### Revision History

| Rev | Description |
|-----|-------------|
| 1.0 | Initial design document |
| 1.1 | Post-architectural review clarifications |
| 1.2 | Added performance optimizations (database, memory, concurrency, caching) |
| 1.3 | Added detailed test strategy (15 tests for coverage gaps) |
| 1.4 | Added phase gates with test execution commands and regression protocol |
| 1.5 | Added performance benchmarks (33 tests for baseline comparison) |

## Overview

Refactor the `ContentService` class (~3800 lines) into smaller, focused components for improved maintainability, testability, and performance.

## Goals

1. **Code Organization** - Break monolithic class into logical units
2. **Testability** - Enable mocking of focused services
3. **Performance** - Maintain or improve performance with better patterns
4. **Backward Compatibility** - All existing `IContentService` consumers continue working

## Constraints

- Full backward compatibility required
- Existing public method signatures unchanged
- New interfaces can be added
- Obsolete constructors to be commented out (not removed)
- **Must avoid naming collision with existing `IContentPublishingService`** (see Naming Decisions)

## Naming Decisions

### Resolving IContentPublishingService Collision

An `IContentPublishingService` already exists in the codebase (`src/Umbraco.Core/Services/ContentPublishingService.cs`). This is an async API-layer service that orchestrates publishing via `IContentService`.

**Decision**: Rename the proposed publishing interface to `IContentPublishOperationService` to avoid collision. The existing `IContentPublishingService` remains unchanged as the higher-level async API orchestrator.

## Architecture

### Service Dependency Direction

Services follow a unidirectional dependency pattern (no circular dependencies):

```
                    ┌─────────────────────────────┐
                    │   ContentService (Facade)    │
                    │   - Delegates to all below   │
                    └─────────────┬───────────────┘
                                  │ depends on
          ┌───────────────────────┼───────────────────────┐
          │                       │                       │
          ▼                       ▼                       ▼
┌─────────────────┐   ┌─────────────────────┐   ┌─────────────────┐
│ IContentCrud-   │   │ IContentPublish-    │   │ IContentMove-   │
│ Service         │   │ OperationService    │   │ Service         │
│ (CRUD only)     │   │ (publish/schedule)  │   │ (move/copy/bin) │
└────────┬────────┘   └─────────┬───────────┘   └────────┬────────┘
         │                      │                        │
         │                      │ can call               │ can call
         │                      ▼                        ▼
         │            ┌─────────────────────────────────────────────┐
         └──────────► │           IContentCrudService               │
                      │   (Save/Delete operations as needed)        │
                      └─────────────────────────────────────────────┘
```

**Rules**:
- `IContentCrudService` has NO dependencies on other content services
- `IContentPublishOperationService` MAY call `IContentCrudService` for Save operations
- `IContentMoveService` MAY call `IContentCrudService` for Save/Delete operations
- The facade orchestrates cross-service operations when needed

### New Public Service Interfaces (3)

| Interface | Responsibility | Est. Lines |
|-----------|---------------|------------|
| `IContentCrudService` | Create, Get, Save, Delete | ~400 |
| `IContentPublishOperationService` | Publish, Unpublish, Scheduling, Branch | ~1000* |
| `IContentMoveService` | Move, RecycleBin, Copy, Sort | ~350 |

*Revised estimate: `CommitDocumentChangesInternal` alone is 330+ lines

### New Public Service Interfaces - Revised Classification (5)

Based on actual API usage patterns, Query and Versioning are promoted to public:

| Interface | Responsibility | Est. Lines | Visibility |
|-----------|---------------|------------|------------|
| `IContentCrudService` | Create, Get, Save, Delete | ~400 | **Public** |
| `IContentPublishOperationService` | Publish, Unpublish, Scheduling | ~1000 | **Public** |
| `IContentMoveService` | Move, RecycleBin, Copy, Sort | ~350 | **Public** |
| `IContentQueryService` | Count, Paged queries, Hierarchy | ~250 | **Public** |
| `IContentVersionService` | Versions, Rollback, DeleteVersions | ~200 | **Public** |

### Internal Helper Classes (2)

| Class | Responsibility | Est. Lines |
|-------|---------------|------------|
| `ContentPermissionManager` | Get/Set permissions | ~50 |
| `ContentBlueprintManager` | Blueprint CRUD | ~200 |

*Renamed from "Helper" to "Manager" per .NET naming conventions*

### ContentService Facade (~200 lines)

Thin wrapper delegating to services and managers for backward compatibility.

## Transaction/Scope Ownership

### Scope Creation Rules

**Pattern: Caller-Creates-Scope (Ambient Scope)**

All public service methods check for an existing ambient scope and use it if present. If no ambient scope exists, the method creates its own.

```csharp
public OperationResult Save(IContent content, int? userId = null)
{
    using ICoreScope scope = ScopeProvider.CreateCoreScope();
    // ... implementation
    scope.Complete();
    return result;
}
```

**Cross-Service Call Behavior**:
- When Service A calls Service B, Service B detects the ambient scope from A
- Nested scopes participate in the parent transaction
- Only the outermost scope's `Complete()` commits the transaction
- If any nested scope fails to complete, the entire transaction rolls back

**Facade Orchestration**:
For operations requiring multiple services (e.g., `MoveToRecycleBin` which may need to unpublish), the facade creates the scope:

```csharp
// In ContentService facade
public OperationResult MoveToRecycleBin(IContent content, int userId)
{
    using ICoreScope scope = ScopeProvider.CreateCoreScope();

    // Unpublish if published (uses ambient scope)
    if (content.Published)
    {
        _publishOperationService.Unpublish(content, "*", userId);
    }

    // Move to bin (uses same ambient scope)
    var result = _moveService.MoveToRecycleBinInternal(content, userId);

    scope.Complete();
    return result;
}
```

### Lock Coordination

Write locks (`scope.WriteLock(Constants.Locks.ContentTree)`) are acquired by the service performing the operation. When services call each other within an ambient scope, locks are already held by the parent.

**Rule**: Acquire locks at the highest level that creates the scope.

## File Structure

```
src/Umbraco.Core/Services/
├── IContentCrudService.cs
├── IContentPublishOperationService.cs   # Renamed to avoid collision
├── IContentMoveService.cs
├── IContentQueryService.cs              # Promoted to public
├── IContentVersionService.cs            # Promoted to public
└── ContentService.cs (facade - existing file, refactored)

src/Umbraco.Infrastructure/Services/
├── ContentCrudService.cs
├── ContentPublishOperationService.cs    # Renamed to avoid collision
├── ContentMoveService.cs
├── ContentQueryService.cs               # Promoted to public
├── ContentVersionService.cs             # Promoted to public
├── ContentServiceBase.cs (shared infrastructure)
└── Content/
    ├── ContentPermissionManager.cs      # Renamed from Helper
    └── ContentBlueprintManager.cs       # Renamed from Helper
```

## Detailed Interface Designs

### IContentCrudService

```csharp
public interface IContentCrudService
{
    // Create
    IContent Create(string name, int parentId, string contentTypeAlias, int userId = Constants.Security.SuperUserId);
    IContent Create(string name, Guid parentId, string contentTypeAlias, int userId = Constants.Security.SuperUserId);
    IContent Create(string name, int parentId, IContentType contentType, int userId = Constants.Security.SuperUserId);
    IContent Create(string name, IContent? parent, string contentTypeAlias, int userId = Constants.Security.SuperUserId);
    IContent CreateAndSave(string name, int parentId, string contentTypeAlias, int userId = Constants.Security.SuperUserId);
    IContent CreateAndSave(string name, IContent parent, string contentTypeAlias, int userId = Constants.Security.SuperUserId);

    // Read
    IContent? GetById(int id);
    IContent? GetById(Guid key);
    IEnumerable<IContent> GetByIds(IEnumerable<int> ids);
    IEnumerable<IContent> GetByIds(IEnumerable<Guid> ids);
    IEnumerable<IContent> GetRootContent();
    IContent? GetParent(int id);
    IContent? GetParent(IContent? content);

    // Save
    OperationResult Save(IContent content, int? userId = null, ContentScheduleCollection? contentSchedule = null);
    OperationResult Save(IEnumerable<IContent> contents, int userId = Constants.Security.SuperUserId);

    // Delete
    OperationResult Delete(IContent content, int userId = Constants.Security.SuperUserId);
}
```

### IContentPublishOperationService

*Renamed from `IContentPublishingService` to avoid collision with existing API-layer service.*

```csharp
public interface IContentPublishOperationService
{
    // Publishing
    PublishResult Publish(IContent content, string[] cultures, int userId = Constants.Security.SuperUserId);
    IEnumerable<PublishResult> PublishBranch(IContent content, PublishBranchFilter filter, string[] cultures, int userId = Constants.Security.SuperUserId);

    // Unpublishing
    PublishResult Unpublish(IContent content, string? culture = "*", int userId = Constants.Security.SuperUserId);

    // Scheduled Publishing
    IEnumerable<PublishResult> PerformScheduledPublish(DateTime date);
    IEnumerable<IContent> GetContentForExpiration(DateTime date);
    IEnumerable<IContent> GetContentForRelease(DateTime date);

    // Schedule Management
    ContentScheduleCollection GetContentScheduleByContentId(int contentId);
    ContentScheduleCollection GetContentScheduleByContentId(Guid contentId);
    void PersistContentSchedule(IContent content, ContentScheduleCollection contentSchedule);
    IDictionary<int, IEnumerable<ContentSchedule>> GetContentSchedulesByIds(Guid[] keys);

    // Path/Status Checks
    bool IsPathPublishable(IContent content);
    bool IsPathPublished(IContent? content);

    // Send to Publication (workflow)
    bool SendToPublication(IContent? content, int userId = Constants.Security.SuperUserId);
}
```

### IContentMoveService

```csharp
public interface IContentMoveService
{
    // Move operations
    OperationResult Move(IContent content, int parentId, int userId = Constants.Security.SuperUserId);
    OperationResult MoveToRecycleBin(IContent content, int userId = Constants.Security.SuperUserId);

    // Recycle bin
    OperationResult EmptyRecycleBin(int userId = Constants.Security.SuperUserId);
    Task<OperationResult> EmptyRecycleBinAsync(Guid userId);
    bool RecycleBinSmells();
    IEnumerable<IContent> GetPagedContentInRecycleBin(long pageIndex, int pageSize, out long totalRecords, IQuery<IContent>? filter = null, Ordering? ordering = null);

    // Copy
    IContent? Copy(IContent content, int parentId, bool relateToOriginal, int userId = Constants.Security.SuperUserId);
    IContent? Copy(IContent content, int parentId, bool relateToOriginal, bool recursive, int userId = Constants.Security.SuperUserId);

    // Sort
    OperationResult Sort(IEnumerable<IContent> items, int userId = Constants.Security.SuperUserId);
    OperationResult Sort(IEnumerable<int>? ids, int userId = Constants.Security.SuperUserId);
}
```

### IContentVersionService (Promoted to Public)

```csharp
public interface IContentVersionService
{
    IContent? GetVersion(int versionId);
    IEnumerable<IContent> GetVersions(int id);
    IEnumerable<IContent> GetVersionsSlim(int id, int skip, int take);
    IEnumerable<int> GetVersionIds(int id, int maxRows);
    OperationResult Rollback(int id, int versionId, string culture = "*", int userId = Constants.Security.SuperUserId);
    void DeleteVersions(int id, DateTime versionDate, int userId = Constants.Security.SuperUserId);
    void DeleteVersion(int id, int versionId, bool deletePriorVersions, int userId = Constants.Security.SuperUserId);
}
```

### IContentQueryService (Promoted to Public)

```csharp
public interface IContentQueryService
{
    // Counting
    int Count(string? contentTypeAlias = null);
    int CountPublished(string? contentTypeAlias = null);
    int CountChildren(int parentId, string? contentTypeAlias = null);
    int CountDescendants(int parentId, string? contentTypeAlias = null);
    bool HasChildren(int id);

    // Hierarchy
    IEnumerable<IContent> GetByLevel(int level);
    IEnumerable<IContent> GetAncestors(int id);
    IEnumerable<IContent> GetAncestors(IContent content);

    // Paged Queries
    IEnumerable<IContent> GetPagedChildren(int id, long pageIndex, int pageSize, out long totalRecords, IQuery<IContent>? filter = null, Ordering? ordering = null);
    IEnumerable<IContent> GetPagedDescendants(int id, long pageIndex, int pageSize, out long totalRecords, IQuery<IContent>? filter = null, Ordering? ordering = null);
    IEnumerable<IContent> GetPagedOfType(int contentTypeId, long pageIndex, int pageSize, out long totalRecords, IQuery<IContent> filter, Ordering? ordering = null);
    IEnumerable<IContent> GetPagedOfTypes(int[] contentTypeIds, long pageIndex, int pageSize, out long totalRecords, IQuery<IContent>? filter, Ordering? ordering = null);
}
```

### Internal Managers

```csharp
internal class ContentPermissionManager
{
    void SetPermissions(EntityPermissionSet permissionSet);
    void SetPermission(IContent entity, string permission, IEnumerable<int> groupIds);
    EntityPermissionCollection GetPermissions(IContent content);
}

internal class ContentBlueprintManager
{
    IContent? GetBlueprintById(int id);
    IContent? GetBlueprintById(Guid id);
    void SaveBlueprint(IContent content, IContent? createdFromContent, int userId);
    void DeleteBlueprint(IContent content, int userId);
    IContent CreateBlueprintFromContent(IContent blueprint, string name, int userId);
    IEnumerable<IContent> GetBlueprintsForContentTypes(params int[] contentTypeId);
    void DeleteBlueprintsOfType(int contentTypeId, int userId);
    void DeleteBlueprintsOfTypes(IEnumerable<int> contentTypeIds, int userId);
}
```

## Shared Infrastructure

```csharp
internal abstract class ContentServiceBase
{
    protected readonly ICoreScopeProvider ScopeProvider;
    protected readonly IDocumentRepository DocumentRepository;
    protected readonly IEventMessagesFactory EventMessagesFactory;
    protected readonly IAuditService AuditService;
    protected readonly ILogger Logger;

    protected void Audit(AuditType type, int userId, int objectId, string? message = null);
}
```

## Dependency Injection

```csharp
public class ContentServicesComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        // Public services
        builder.Services.AddScoped<IContentCrudService, ContentCrudService>();
        builder.Services.AddScoped<IContentPublishOperationService, ContentPublishOperationService>();
        builder.Services.AddScoped<IContentMoveService, ContentMoveService>();
        builder.Services.AddScoped<IContentQueryService, ContentQueryService>();
        builder.Services.AddScoped<IContentVersionService, ContentVersionService>();

        // Internal managers
        builder.Services.AddScoped<ContentPermissionManager>();
        builder.Services.AddScoped<ContentBlueprintManager>();

        // Facade (backward compatible)
        builder.Services.AddScoped<IContentService, ContentService>();
    }
}
```

## Implementation Order

Each phase MUST run tests before and after to verify no regressions.

| Phase | Service | Tests to Run | Gate | Status |
|-------|---------|--------------|------|--------|
| 0 | Write tests | `ContentServiceRefactoringTests` | All 15 pass | ✅ Complete |
| 1 | CRUD Service | All ContentService*Tests | All pass | ✅ Complete |
| 2 | Query Service | All ContentService*Tests | All pass | Pending |
| 3 | Version Service | All ContentService*Tests | All pass | Pending |
| 4 | Move Service | All ContentService*Tests + Sort/MoveToRecycleBin tests | All pass | Pending |
| 5 | Publish Operation Service | All ContentService*Tests + Notification ordering tests | All pass | Pending |
| 6 | Permission Manager | All ContentService*Tests + Permission tests | All pass | Pending |
| 7 | Blueprint Manager | All ContentService*Tests | All pass | Pending |
| 8 | Facade | **Full test suite** | All pass | Pending |

### Phase Details

1. **Phase 0: Write Tests** ✅ - Created `ContentServiceRefactoringTests.cs` with 16 tests (15 original + 1 DI test)
2. **Phase 1: CRUD Service** ✅ - Complete! Created:
   - `ContentServiceBase.cs` - Abstract base class with shared infrastructure
   - `ContentServiceConstants.cs` - Shared constants
   - `IContentCrudService.cs` - Interface (21 methods)
   - `ContentCrudService.cs` - Implementation (~750 lines)
   - Updated `ContentService.cs` to delegate CRUD operations (reduced from 3823 to 3497 lines)
   - Benchmark regression enforcement (20% threshold, CI-configurable)
   - Git tag: `phase-1-crud-extraction`
3. **Phase 2: Query Service** - Read-only operations, low risk
4. **Phase 3: Version Service** - Straightforward extraction
5. **Phase 4: Move Service** - Depends on CRUD; Sort and MoveToRecycleBin tests critical
6. **Phase 5: Publish Operation Service** - Most complex; notification ordering tests critical
7. **Phase 6: Permission Manager** - Small extraction; permission tests critical
8. **Phase 7: Blueprint Manager** - Final cleanup
9. **Phase 8: Facade** - Wire everything together, add async methods

### Test Execution Commands

```bash
# Run refactoring-specific tests (fast feedback)
dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentServiceRefactoringTests"

# Run all ContentService tests (phase gate)
dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentService"

# Run full integration test suite (final gate)
dotnet test tests/Umbraco.Tests.Integration
```

### Regression Protocol

If any test fails during a phase:
1. **STOP** - Do not proceed to next phase
2. **DIAGNOSE** - Identify which behavior changed
3. **FIX** - Restore expected behavior
4. **VERIFY** - Re-run all tests for current phase
5. **CONTINUE** - Only after all tests pass

## Complete Method Mapping

This table maps ALL methods from `IContentService` (and its base `IContentServiceBase<IContent>`) to the proposed services/managers.

### IContentCrudService Methods

| Current Method | Target | Notes |
|----------------|--------|-------|
| `Create(string name, Guid parentId, string docTypeAlias, int userId)` | IContentCrudService | |
| `Create(string name, int parentId, string docTypeAlias, int userId)` | IContentCrudService | |
| `Create(string name, int parentId, IContentType contentType, int userId)` | IContentCrudService | |
| `Create(string name, IContent? parent, string docTypeAlias, int userId)` | IContentCrudService | |
| `CreateAndSave(string name, int parentId, string contentTypeAlias, int userId)` | IContentCrudService | Calls Save internally |
| `CreateAndSave(string name, IContent parent, string contentTypeAlias, int userId)` | IContentCrudService | Calls Save internally |
| `GetById(int id)` | IContentCrudService | |
| `GetById(Guid key)` | IContentCrudService | |
| `GetByIds(IEnumerable<int> ids)` | IContentCrudService | |
| `GetByIds(IEnumerable<Guid> ids)` | IContentCrudService | |
| `GetRootContent()` | IContentCrudService | |
| `GetParent(int id)` | IContentCrudService | |
| `GetParent(IContent content)` | IContentCrudService | |
| `Save(IContent content, int? userId, ContentScheduleCollection? schedule)` | IContentCrudService | |
| `Save(IEnumerable<IContent> contents, int userId)` | IContentCrudService | |
| `Delete(IContent content, int userId)` | IContentCrudService | |
| `DeleteOfType(int documentTypeId, int userId)` | IContentCrudService | *Orchestration: moves descendants to bin first* |
| `DeleteOfTypes(IEnumerable<int> contentTypeIds, int userId)` | IContentCrudService | *Orchestration: moves descendants to bin first* |

### IContentPublishOperationService Methods

| Current Method | Target | Notes |
|----------------|--------|-------|
| `Publish(IContent content, string[] cultures, int userId)` | IContentPublishOperationService | |
| `PublishBranch(IContent content, PublishBranchFilter filter, string[] cultures, int userId)` | IContentPublishOperationService | Complex: ~200 lines |
| `Unpublish(IContent content, string? culture, int userId)` | IContentPublishOperationService | |
| `IsPathPublishable(IContent content)` | IContentPublishOperationService | Read-only |
| `IsPathPublished(IContent content)` | IContentPublishOperationService | Read-only |
| `SendToPublication(IContent? content, int userId)` | IContentPublishOperationService | Workflow trigger |
| `PerformScheduledPublish(DateTime date)` | IContentPublishOperationService | Scheduled job entry point |
| `GetContentForExpiration(DateTime date)` | IContentPublishOperationService | Schedule query |
| `GetContentForRelease(DateTime date)` | IContentPublishOperationService | Schedule query |
| `GetContentScheduleByContentId(int contentId)` | IContentPublishOperationService | |
| `GetContentScheduleByContentId(Guid contentId)` | IContentPublishOperationService | |
| `PersistContentSchedule(IContent content, ContentScheduleCollection schedule)` | IContentPublishOperationService | |
| `GetContentSchedulesByIds(Guid[] keys)` | IContentPublishOperationService | Bulk query |

### IContentMoveService Methods

| Current Method | Target | Notes |
|----------------|--------|-------|
| `Move(IContent content, int parentId, int userId)` | IContentMoveService | |
| `MoveToRecycleBin(IContent content, int userId)` | **Facade** | *Orchestration: may unpublish first* |
| `EmptyRecycleBin(int userId)` | IContentMoveService | |
| `EmptyRecycleBinAsync(Guid userId)` | IContentMoveService | Async variant |
| `RecycleBinSmells()` | IContentMoveService | |
| `GetPagedContentInRecycleBin(...)` | IContentMoveService | |
| `Copy(IContent content, int parentId, bool relateToOriginal, int userId)` | IContentMoveService | |
| `Copy(IContent content, int parentId, bool relateToOriginal, bool recursive, int userId)` | IContentMoveService | |
| `Sort(IEnumerable<IContent> items, int userId)` | IContentMoveService | |
| `Sort(IEnumerable<int>? ids, int userId)` | IContentMoveService | |

### IContentQueryService Methods

| Current Method | Target | Notes |
|----------------|--------|-------|
| `Count(string? documentTypeAlias)` | IContentQueryService | |
| `CountPublished(string? documentTypeAlias)` | IContentQueryService | |
| `CountChildren(int parentId, string? documentTypeAlias)` | IContentQueryService | |
| `CountDescendants(int parentId, string? documentTypeAlias)` | IContentQueryService | |
| `HasChildren(int id)` | IContentQueryService | |
| `GetByLevel(int level)` | IContentQueryService | |
| `GetAncestors(int id)` | IContentQueryService | |
| `GetAncestors(IContent content)` | IContentQueryService | |
| `GetPagedChildren(int id, ...)` | IContentQueryService | |
| `GetPagedDescendants(int id, ...)` | IContentQueryService | |
| `GetPagedOfType(int contentTypeId, ...)` | IContentQueryService | |
| `GetPagedOfTypes(int[] contentTypeIds, ...)` | IContentQueryService | |

### IContentVersionService Methods

| Current Method | Target | Notes |
|----------------|--------|-------|
| `GetVersion(int versionId)` | IContentVersionService | |
| `GetVersions(int id)` | IContentVersionService | |
| `GetVersionsSlim(int id, int skip, int take)` | IContentVersionService | |
| `GetVersionIds(int id, int topRows)` | IContentVersionService | |
| `Rollback(int id, int versionId, string culture, int userId)` | IContentVersionService | |
| `DeleteVersions(int id, DateTime date, int userId)` | IContentVersionService | |
| `DeleteVersion(int id, int versionId, bool deletePriorVersions, int userId)` | IContentVersionService | |

### ContentPermissionManager Methods (Internal)

| Current Method | Target | Notes |
|----------------|--------|-------|
| `GetPermissions(IContent content)` | ContentPermissionManager | |
| `SetPermissions(EntityPermissionSet permissionSet)` | ContentPermissionManager | |
| `SetPermission(IContent entity, string permission, IEnumerable<int> groupIds)` | ContentPermissionManager | |

### ContentBlueprintManager Methods (Internal)

| Current Method | Target | Notes |
|----------------|--------|-------|
| `GetBlueprintById(int id)` | ContentBlueprintManager | |
| `GetBlueprintById(Guid id)` | ContentBlueprintManager | |
| `GetBlueprintsForContentTypes(params int[] documentTypeId)` | ContentBlueprintManager | |
| `SaveBlueprint(IContent content, int userId)` | ContentBlueprintManager | Obsolete overload |
| `SaveBlueprint(IContent content, IContent? createdFromContent, int userId)` | ContentBlueprintManager | |
| `DeleteBlueprint(IContent content, int userId)` | ContentBlueprintManager | |
| `CreateBlueprintFromContent(IContent blueprint, string name, int userId)` | ContentBlueprintManager | |
| `CreateContentFromBlueprint(IContent blueprint, string name, int userId)` | ContentBlueprintManager | Obsolete |
| `DeleteBlueprintsOfType(int contentTypeId, int userId)` | ContentBlueprintManager | |
| `DeleteBlueprintsOfTypes(IEnumerable<int> contentTypeIds, int userId)` | ContentBlueprintManager | |

### Facade Orchestration Methods

These methods stay in the facade because they coordinate multiple services:

| Method | Why in Facade |
|--------|---------------|
| `MoveToRecycleBin` | Unpublishes content (IContentPublishOperationService) then moves (IContentMoveService) |
| `DeleteOfType` / `DeleteOfTypes` | Moves descendants to bin first, then deletes type content |

## Notification Responsibility Matrix

Each service fires only its own notifications. State is preserved via `WithStateFrom()` for notification continuity.

| Notification | Firing Service | Notes |
|--------------|----------------|-------|
| `ContentSavingNotification` | IContentCrudService | Cancellable |
| `ContentSavedNotification` | IContentCrudService | |
| `ContentDeletingNotification` | IContentCrudService | Cancellable |
| `ContentDeletedNotification` | IContentCrudService | |
| `ContentPublishingNotification` | IContentPublishOperationService | Cancellable |
| `ContentPublishedNotification` | IContentPublishOperationService | |
| `ContentUnpublishingNotification` | IContentPublishOperationService | Cancellable |
| `ContentUnpublishedNotification` | IContentPublishOperationService | |
| `ContentMovingNotification` | IContentMoveService | Cancellable |
| `ContentMovedNotification` | IContentMoveService | |
| `ContentMovingToRecycleBinNotification` | **Facade** | Orchestrated operation |
| `ContentMovedToRecycleBinNotification` | **Facade** | Orchestrated operation |
| `ContentCopyingNotification` | IContentMoveService | Cancellable |
| `ContentCopiedNotification` | IContentMoveService | |
| `ContentSortingNotification` | IContentMoveService | Cancellable |
| `ContentSortedNotification` | IContentMoveService | |
| `ContentEmptyingRecycleBinNotification` | IContentMoveService | Cancellable |
| `ContentEmptiedRecycleBinNotification` | IContentMoveService | |
| `ContentRollingBackNotification` | IContentVersionService | Cancellable |
| `ContentRolledBackNotification` | IContentVersionService | |
| `ContentSendingToPublishNotification` | IContentPublishOperationService | Cancellable |
| `ContentSentToPublishNotification` | IContentPublishOperationService | |
| `ContentSavingBlueprintNotification` | ContentBlueprintManager | Via IEventAggregator |
| `ContentSavedBlueprintNotification` | ContentBlueprintManager | Via IEventAggregator |
| `ContentDeletedBlueprintNotification` | ContentBlueprintManager | Via IEventAggregator |

**State Preservation Rule**: When a method chains operations (e.g., Save → Publish), the second notification carries state from the first using `WithStateFrom()`.

## Risk Mitigation

| Risk | Mitigation |
|------|------------|
| Circular dependencies | Unidirectional: Publish/Move → CRUD only; no reverse |
| Transaction boundaries | Ambient scope pattern (see Transaction/Scope Ownership) |
| Notification consistency | Notification responsibility matrix per service |
| Breaking changes | Extensive test coverage before refactor |
| Naming collision | Renamed to IContentPublishOperationService |
| Cross-service calls | Facade orchestrates complex multi-service operations |

## Async Considerations

Methods with async variants should define async-first interfaces where database I/O occurs:

| Current Method | Async Variant Needed |
|----------------|---------------------|
| `EmptyRecycleBin` | ✅ Already exists: `EmptyRecycleBinAsync` |
| `Delete` (bulk) | Consider for large deletions |
| `PublishBranch` | Consider for large trees |

**Decision**: New services should provide async overloads for operations that can process large datasets.

## Clarifications (from Architectural Review)

### Q1: Existing IContentPublishingService handling?
**A**: The existing `IContentPublishingService` (API-layer) remains unchanged. It continues to orchestrate via `IContentService`. After refactoring, it will indirectly use the new `IContentPublishOperationService` through the facade.

### Q2: PublishBranch ownership?
**A**: `PublishBranch` belongs entirely to `IContentPublishOperationService`. Tree traversal is part of publishing logic in this context.

### Q3: Locking strategy?
**A**: Locks are acquired by the scope creator (highest-level caller). Nested service calls within an ambient scope inherit the existing locks.

### Q4: Culture-variant complexity (CommitDocumentChangesInternal)?
**A**: The full complexity (~330 lines) stays in `ContentPublishOperationService`. No further splitting at this time; revisit if the class exceeds 1200 lines.

### Q5: RepositoryService base class?
**A**: New services inherit from `ContentServiceBase` (see Shared Infrastructure), which wraps scope/repository access similarly to `RepositoryService` but is specific to content operations.

## Performance Optimizations

Performance improvements to implement during the refactoring, organized into four areas with incremental steps.

### 1. Database Query Efficiency

**Approach**: Batch lookups to eliminate N+1 queries.

| Step | Change |
|------|--------|
| 1 | Add `IIdKeyMap.GetIdsForKeys(Guid[] keys, UmbracoObjectTypes type)` for batch key-to-id resolution |
| 2 | Add `GetSchedulesByContentIds(int[] ids)` to batch schedule lookups |
| 3 | Add `ArePathsPublished(int[] contentIds)` for batch path validation |
| 4 | Add `GetParents(int[] contentIds)` for batch ancestor lookups |

**Key locations to fix**:
- `GetContentSchedulesByIds` (line 1025-1049): N+1 in `_idKeyMap.GetIdForKey` calls
- `CommitDocumentChangesInternal` (line 1461+): Multiple repository calls in same scope
- `IsPathPublishable` (line 1070), `GetAncestors` (line 792): Repeated single-item lookups

### 2. Memory Allocation Patterns

**Approach**: Aggressive optimization, implemented incrementally.

| Step | Focus | Changes |
|------|-------|---------|
| 1 | Remove unnecessary materializations | Delete `.ToArray()`/`.ToList()` before `foreach` loops |
| 2 | Fix string allocations | Replace `+` concat in loops with `StringBuilder` |
| 3 | Add StringBuilder pooling | Use `StringBuilderCache` or `ObjectPool<StringBuilder>` |
| 4 | Eliminate closure allocations | Hoist lambdas, use static lambdas where possible |
| 5 | Span for string operations | Replace substrings with `ReadOnlySpan<char>` |
| 6 | ArrayPool for temp arrays | Pool arrays in batch operations |
| 7 | Pooled collections | Use `ArrayPoolList<T>` or similar for hot paths |

**Key locations to fix**:
- Lines 1170, 814, 2650: ToArray/ToList before iteration
- Lines 1201, 2596-2598: String concat in loops
- Lines 1125-1127: Lambda/closure on every save
- Lines 555-556: Dictionary recreation

### 3. Concurrency & Locking

**Approach**: Moderate optimization focused on lock duration and correctness.

| Step | Focus | Changes |
|------|-------|---------|
| 1 | Remove unnecessary read locks | Skip `ReadLock` when data is retrieved from cache |
| 2 | Lock before notification | Move `WriteLock` acquisition before `*SavingNotification` |
| 3 | Batch descendant operations | In `PerformMoveLocked`, collect changes then batch-save |
| 4 | Document lock contracts | Each new service documents its lock expectations |

**Current issues**:
- Single coarse-grained lock (`ContentTree`) for all 60+ lock calls
- Long lock hold in `PerformMoveLocked` (line 2600+) during descendant iteration
- Lock acquired after notification (line 1114) creates race window

### 4. Caching Strategies

**Approach**: Moderate optimization with clear invalidation contracts.

| Step | Focus | Changes |
|------|-------|---------|
| 1 | Audit RefreshBranch usage | Switch to `RefreshNode` where only single node affected |
| 2 | Parallelize cache clears | Run Memory, Navigation, Routing, Published status clears concurrently |
| 3 | Fix N+1 in HandleNavigation | Batch descendant lookups instead of per-item queries |
| 4 | Document invalidation contracts | Each service specifies which cache events it fires |

**Cache invalidation flow**:
```
ContentService operation
    → ContentTreeChangeNotification
        → ContentTreeChangeDistributedCacheNotificationHandler
            → ContentCacheRefresher.Refresh()
                → Repository cache, Elements cache, Navigation, URLs, Published status
```

**Known limitations (out of scope)**:
- Elements cache full clear: Requires architectural change (elements as entities, not JSON blobs)
- Sync-over-async in cache refresh: Cross-cutting concern for separate initiative

## Testing Strategy

### Overview

1. Ensure existing unit tests pass throughout refactor
2. Add new integration tests for identified coverage gaps (15 tests)
3. Integration tests validate end-to-end behavior unchanged
4. Benchmark critical paths before/after

### Test File Organization

Create a single new test file for refactoring-specific coverage:

```
tests/Umbraco.Tests.Integration/Umbraco.Infrastructure/Services/
    ContentServiceRefactoringTests.cs
```

**Rationale**: These tests exist specifically to validate the refactoring. Grouping them together:
- Makes clear which tests are refactoring-specific safety nets
- Can be retired or merged once refactoring is stable
- Avoids polluting existing test files

### Test Coverage Summary

| Gap Area | Test Count | Purpose |
|----------|------------|---------|
| Notification ordering | 2 | MoveToRecycleBin unpublish→move sequence |
| Sort operation | 3 | Sort(IContent), Sort(ids), notification firing |
| DeleteOfType | 3 | Descendants handling, mixed types, multiple types |
| Permissions | 4 | SetPermission, SetPermissions, GetPermissions |
| Transaction boundaries | 3 | Ambient scope across chained operations |
| **Total** | **15** | |

### Notification Handler Extension

The existing `ContentNotificationHandler` pattern needs extension for new notifications:

```csharp
internal sealed class ContentNotificationHandler :
    INotificationHandler<ContentSavingNotification>,
    INotificationHandler<ContentSavedNotification>,
    INotificationHandler<ContentPublishingNotification>,
    INotificationHandler<ContentPublishedNotification>,
    INotificationHandler<ContentUnpublishingNotification>,
    INotificationHandler<ContentUnpublishedNotification>,
    INotificationHandler<ContentMovingToRecycleBinNotification>,    // NEW
    INotificationHandler<ContentMovedToRecycleBinNotification>,     // NEW
    INotificationHandler<ContentSortingNotification>,               // NEW
    INotificationHandler<ContentSortedNotification>,                // NEW
    INotificationHandler<ContentTreeChangeNotification>
{
    // Existing delegates...

    // NEW delegates
    public static Action<ContentMovingToRecycleBinNotification> MovingToRecycleBin { get; set; }
    public static Action<ContentMovedToRecycleBinNotification> MovedToRecycleBin { get; set; }
    public static Action<ContentSortingNotification> Sorting { get; set; }
    public static Action<ContentSortedNotification> Sorted { get; set; }

    // NEW handlers
    public void Handle(ContentMovingToRecycleBinNotification notification)
        => MovingToRecycleBin?.Invoke(notification);
    public void Handle(ContentMovedToRecycleBinNotification notification)
        => MovedToRecycleBin?.Invoke(notification);
    public void Handle(ContentSortingNotification notification)
        => Sorting?.Invoke(notification);
    public void Handle(ContentSortedNotification notification)
        => Sorted?.Invoke(notification);
}
```

### Detailed Test Specifications

#### 1. Notification Ordering Tests (2 tests)

**Test 1: MoveToRecycleBin_PublishedContent_UnpublishesBeforeMoving**

Verifies that for published content, `MoveToRecycleBin` fires notifications in order:
1. `ContentUnpublishingNotification`
2. `ContentUnpublishedNotification`
3. `ContentMovingToRecycleBinNotification`
4. `ContentMovedToRecycleBinNotification`

**Test 2: MoveToRecycleBin_UnpublishedContent_OnlyFiresMoveNotifications**

Verifies that for unpublished content, only move notifications fire (no unpublish notifications).

#### 2. Sort Operation Tests (3 tests)

**Test 3: Sort_WithContentItems_ChangesSortOrder**

Verifies `Sort(IEnumerable<IContent>)` correctly reorders children.

**Test 4: Sort_WithIds_ChangesSortOrder**

Verifies `Sort(IEnumerable<int>)` correctly reorders children by ID.

**Test 5: Sort_FiresSortingAndSortedNotifications**

Verifies `ContentSortingNotification` and `ContentSortedNotification` fire in sequence with correct entities.

#### 3. DeleteOfType Tests (3 tests)

**Test 6: DeleteOfType_MovesDescendantsToRecycleBinFirst**

Verifies hierarchical content (parent → child → grandchild) is fully deleted and recycle bin is empty afterward.

**Test 7: DeleteOfType_WithMixedTypes_OnlyDeletesSpecifiedType**

Verifies only content of the specified type is deleted; other types remain.

**Test 8: DeleteOfTypes_DeletesMultipleTypesAtOnce**

Verifies `DeleteOfTypes(IEnumerable<int>)` deletes multiple content types in single operation.

#### 4. Permission Tests (4 tests)

**Test 9: SetPermission_AssignsPermissionToUserGroup**

Verifies `SetPermission` assigns a permission and `GetPermissions` retrieves it.

**Test 10: SetPermission_MultiplePermissionsForSameGroup**

Verifies multiple `SetPermission` calls accumulate permissions for a user group.

**Test 11: SetPermissions_AssignsPermissionSet**

Verifies `SetPermissions(EntityPermissionSet)` assigns a complete permission set.

**Test 12: SetPermission_AssignsToMultipleUserGroups**

Verifies single `SetPermission` call can assign to multiple user groups simultaneously.

#### 5. Transaction Boundary Tests (3 tests)

**Test 13: AmbientScope_NestedOperationsShareTransaction**

Verifies that multiple operations within an uncompleted scope all roll back together.

**Test 14: AmbientScope_CompletedScopeCommitsAllOperations**

Verifies that multiple operations within a completed scope all commit together.

**Test 15: AmbientScope_MoveToRecycleBinRollsBackCompletely**

Verifies that `MoveToRecycleBin` (which internally unpublishes then moves) rolls back both operations when scope is not completed.

## Success Criteria

- [ ] All existing tests pass
- [ ] No public API breaking changes
- [ ] ContentService reduced to ~200 lines
- [ ] Each new service independently testable
- [ ] Notification ordering matches current behavior
- [ ] All 80+ IContentService methods mapped to new services

### Test Coverage Criteria

- [ ] 15 new integration tests written and passing (ContentServiceRefactoringTests.cs)
- [ ] Notification ordering tests verify MoveToRecycleBin sequence
- [ ] Sort operation has full test coverage
- [ ] DeleteOfType/DeleteOfTypes descendant handling verified
- [ ] Permission operations (SetPermission, SetPermissions) tested
- [ ] Transaction boundary tests confirm ambient scope behavior

### Performance Criteria

- [ ] N+1 queries eliminated (batch lookups implemented)
- [ ] No unnecessary ToArray/ToList materializations in hot paths
- [ ] Lock duration reduced for bulk operations
- [ ] Each service documents its lock and cache invalidation contracts
- [ ] Benchmarks show no regression (target: 10%+ improvement on batch operations)

## Performance Benchmarks

Dedicated benchmark suite for capturing baseline metrics and comparing before/after refactoring.

### Benchmark File

```
tests/Umbraco.Tests.Integration/Umbraco.Infrastructure/Services/
    ContentServiceRefactoringBenchmarks.cs
```

### Benchmark Infrastructure

**BenchmarkResult model:**
```csharp
public record BenchmarkResult(
    string Name,
    long ElapsedMs,
    int ItemCount,
    double MsPerItem,
    DateTime Timestamp);
```

**Output format** (JSON for comparison):
```json
{
  "runDate": "2025-12-20T10:30:00Z",
  "branch": "refactor/ContentService",
  "commit": "f4a01ed",
  "results": {
    "Save_BatchOf100": { "elapsedMs": 245, "itemCount": 100, "msPerItem": 2.45 },
    "GetAncestors_DeepHierarchy": { "elapsedMs": 89, "itemCount": 10, "msPerItem": 8.9 }
  }
}
```

### CRUD Operation Benchmarks (7 tests)

| Test Name | Operation | Data Size | What It Measures |
|-----------|-----------|-----------|------------------|
| `Save_SingleItem` | `Save(IContent)` | 1 | Single save latency |
| `Save_BatchOf100` | `Save(IEnumerable<IContent>)` | 100 | Batch save throughput |
| `Save_BatchOf1000` | `Save(IEnumerable<IContent>)` | 1000 | Large batch scalability |
| `GetById_Single` | `GetById(int)` | 1 | Single retrieval latency |
| `GetByIds_BatchOf100` | `GetByIds(IEnumerable<int>)` | 100 | Batch retrieval (N+1 detection) |
| `Delete_SingleItem` | `Delete(IContent)` | 1 | Single delete latency |
| `Delete_WithDescendants` | `Delete(IContent)` | Parent + 50 children | Cascade delete performance |

### Query Operation Benchmarks (6 tests)

| Test Name | Operation | Data Size | What It Measures |
|-----------|-----------|-----------|------------------|
| `GetPagedChildren_100Items` | `GetPagedChildren()` | 100 children | Paged query performance |
| `GetPagedDescendants_DeepTree` | `GetPagedDescendants()` | 3 levels, 300 total | Tree traversal efficiency |
| `GetAncestors_DeepHierarchy` | `GetAncestors()` | 10-level deep item | Ancestor lookup (N+1 prone) |
| `Count_ByContentType` | `Count(alias)` | 500 items of type | Count query optimization |
| `CountDescendants_LargeTree` | `CountDescendants()` | 500 descendants | Descendant counting |
| `HasChildren_100Nodes` | `HasChildren()` × 100 | 100 parent nodes | Repeated single lookups |

### Publish Operation Benchmarks (7 tests)

| Test Name | Operation | Data Size | What It Measures |
|-----------|-----------|-----------|------------------|
| `Publish_SingleItem` | `Publish()` | 1 | Single publish latency |
| `Publish_BatchOf50` | `Publish()` × 50 | 50 items | Repeated publish overhead |
| `PublishBranch_ShallowTree` | `PublishBranch()` | 1 parent + 20 children | Branch publish (small) |
| `PublishBranch_DeepTree` | `PublishBranch()` | 5 levels, 100 items | Branch publish scalability |
| `Unpublish_SingleItem` | `Unpublish()` | 1 | Single unpublish latency |
| `PerformScheduledPublish` | `PerformScheduledPublish()` | 50 scheduled items | Scheduled job performance |
| `GetContentSchedulesByIds_100Items` | `GetContentSchedulesByIds()` | 100 items | N+1 issue (line 1025-1049) |

### Move Operation Benchmarks (8 tests)

| Test Name | Operation | Data Size | What It Measures |
|-----------|-----------|-----------|------------------|
| `Move_SingleItem` | `Move()` | 1 | Single move latency |
| `Move_WithDescendants` | `Move()` | 1 parent + 50 children | Descendant path updates |
| `MoveToRecycleBin_Published` | `MoveToRecycleBin()` | 1 published item | Unpublish + move orchestration |
| `MoveToRecycleBin_LargeTree` | `MoveToRecycleBin()` | 100 descendants | Lock duration (line 2600+) |
| `Copy_SingleItem` | `Copy()` | 1 | Single copy latency |
| `Copy_Recursive_50Items` | `Copy(..., recursive: true)` | 50 descendants | Recursive copy overhead |
| `Sort_100Children` | `Sort(IEnumerable<IContent>)` | 100 siblings | Sort operation efficiency |
| `EmptyRecycleBin_100Items` | `EmptyRecycleBin()` | 100 items in bin | Bulk delete performance |

### Version Operation Benchmarks (4 tests)

| Test Name | Operation | Data Size | What It Measures |
|-----------|-----------|-----------|------------------|
| `GetVersions_ItemWith50Versions` | `GetVersions()` | 50 versions | Version history retrieval |
| `GetVersionsSlim_Paged` | `GetVersionsSlim(skip, take)` | 50 versions, page of 10 | Paged version query |
| `Rollback_ToVersion` | `Rollback()` | 1 item with 10 versions | Rollback latency |
| `DeleteVersions_ByDate` | `DeleteVersions()` | 50 versions | Version cleanup performance |

### Baseline Comparison (1 test)

| Test Name | Purpose |
|-----------|---------|
| `CompareAgainstBaseline` | Compares current run against saved baseline, fails if >20% regression |

### Benchmark Summary

| Category | Test Count |
|----------|------------|
| CRUD Operations | 7 |
| Query Operations | 6 |
| Publish Operations | 7 |
| Move Operations | 8 |
| Version Operations | 4 |
| Comparison | 1 |
| **Total** | **33** |

### Benchmark Execution Commands

```bash
# Run all benchmarks and save as baseline (before refactoring)
dotnet test tests/Umbraco.Tests.Integration \
  --filter "Category=Benchmark&FullyQualifiedName~ContentServiceRefactoringBenchmarks" \
  -- TestRunParameters.Parameter(name="SaveAsBaseline", value="true")

# Run benchmarks and compare against baseline (after each phase)
dotnet test tests/Umbraco.Tests.Integration \
  --filter "Category=Benchmark&FullyQualifiedName~ContentServiceRefactoringBenchmarks"
```

### Sample Comparison Output

```
| Operation                    | Baseline | Current | Change  |
|------------------------------|----------|---------|---------|
| Save_BatchOf100              | 245ms    | 198ms   | -19.2%  |
| GetContentSchedulesByIds     | 892ms    | 156ms   | -82.5%  | ✓ N+1 fixed
| MoveToRecycleBin_LargeTree   | 1240ms   | 1180ms  | -4.8%   |
```
