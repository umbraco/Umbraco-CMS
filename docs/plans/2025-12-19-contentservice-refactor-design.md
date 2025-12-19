# ContentService Refactoring Design

**Date**: 2025-12-19
**Status**: Approved
**Branch**: refactor/ContentService

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

## Architecture

### New Public Service Interfaces (3)

| Interface | Responsibility | Est. Lines |
|-----------|---------------|------------|
| `IContentCrudService` | Create, Get, Save, Delete | ~400 |
| `IContentPublishingService` | Publish, Unpublish, Scheduling, Branch | ~800 |
| `IContentMoveService` | Move, RecycleBin, Copy, Sort | ~350 |

### Internal Helper Classes (4)

| Class | Responsibility | Est. Lines |
|-------|---------------|------------|
| `ContentVersioningHelper` | Versions, Rollback, DeleteVersions | ~150 |
| `ContentQueryHelper` | Count, Paged queries, Hierarchy | ~200 |
| `ContentPermissionHelper` | Get/Set permissions | ~50 |
| `ContentBlueprintHelper` | Blueprint CRUD | ~200 |

### ContentService Facade (~200 lines)

Thin wrapper delegating to services and helpers for backward compatibility.

## File Structure

```
src/Umbraco.Core/Services/
├── IContentCrudService.cs
├── IContentPublishingService.cs
├── IContentMoveService.cs
└── ContentService.cs (facade - existing file, refactored)

src/Umbraco.Infrastructure/Services/
├── ContentCrudService.cs
├── ContentPublishingService.cs
├── ContentMoveService.cs
├── ContentServiceBase.cs (shared infrastructure)
└── Helpers/
    ├── ContentVersioningHelper.cs
    ├── ContentQueryHelper.cs
    ├── ContentPermissionHelper.cs
    └── ContentBlueprintHelper.cs
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

### IContentPublishingService

```csharp
public interface IContentPublishingService
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

### Internal Helpers

```csharp
internal class ContentVersioningHelper
{
    IContent? GetVersion(int versionId);
    IEnumerable<IContent> GetVersions(int id);
    IEnumerable<IContent> GetVersionsSlim(int id, int skip, int take);
    IEnumerable<int> GetVersionIds(int id, int maxRows);
    OperationResult Rollback(int id, int versionId, string culture, int userId);
    void DeleteVersions(int id, DateTime versionDate, int userId);
    void DeleteVersion(int id, int versionId, bool deletePriorVersions, int userId);
}

internal class ContentQueryHelper
{
    int Count(string? contentTypeAlias = null);
    int CountPublished(string? contentTypeAlias = null);
    int CountChildren(int parentId, string? contentTypeAlias = null);
    int CountDescendants(int parentId, string? contentTypeAlias = null);
    IEnumerable<IContent> GetByLevel(int level);
    IEnumerable<IContent> GetAncestors(int id);
    IEnumerable<IContent> GetAncestors(IContent content);
    IEnumerable<IContent> GetPublishedChildren(int id);
    bool HasChildren(int id);
    IEnumerable<IContent> GetPagedChildren(...);
    IEnumerable<IContent> GetPagedDescendants(...);
    IEnumerable<IContent> GetPagedOfType(...);
    IEnumerable<IContent> GetPagedOfTypes(...);
}

internal class ContentPermissionHelper
{
    void SetPermissions(EntityPermissionSet permissionSet);
    void SetPermission(IContent entity, string permission, IEnumerable<int> groupIds);
    EntityPermissionCollection GetPermissions(IContent content);
}

internal class ContentBlueprintHelper
{
    IContent? GetBlueprintById(int id);
    IContent? GetBlueprintById(Guid id);
    void SaveBlueprint(IContent content, IContent? createdFromContent, int userId);
    void DeleteBlueprint(IContent content, int userId);
    IContent CreateBlueprintFromContent(IContent blueprint, string name, int userId);
    IEnumerable<IContent> GetBlueprintsForContentTypes(params int[] contentTypeId);
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
        builder.Services.AddScoped<IContentPublishingService, ContentPublishingService>();
        builder.Services.AddScoped<IContentMoveService, ContentMoveService>();

        // Internal helpers
        builder.Services.AddScoped<ContentVersioningHelper>();
        builder.Services.AddScoped<ContentQueryHelper>();
        builder.Services.AddScoped<ContentPermissionHelper>();
        builder.Services.AddScoped<ContentBlueprintHelper>();

        // Facade (backward compatible)
        builder.Services.AddScoped<IContentService, ContentService>();
    }
}
```

## Implementation Order

1. **Phase 1: CRUD Service** - Establish patterns
2. **Phase 2: Publishing Service** - Most complex, tackle early
3. **Phase 3: Move Service** - Depends on CRUD/Publishing
4. **Phase 4: Versioning Helper** - Depends on CRUD
5. **Phase 5: Query Helper** - Utility operations
6. **Phase 6: Permission Helper** - Small extraction
7. **Phase 7: Blueprint Helper** - Final cleanup
8. **Phase 8: Facade** - Wire everything together

## Risk Mitigation

| Risk | Mitigation |
|------|------------|
| Circular dependencies | Inject repository directly where needed |
| Transaction boundaries | Ensure scopes work across service calls |
| Notification consistency | Events fire from correct service |
| Breaking changes | Extensive test coverage before refactor |

## Testing Strategy

1. Ensure existing unit tests pass throughout refactor
2. Add new unit tests for each extracted service
3. Integration tests validate end-to-end behavior unchanged
4. Benchmark critical paths before/after

## Success Criteria

- [ ] All existing tests pass
- [ ] No public API breaking changes
- [ ] ContentService reduced to ~200 lines
- [ ] Each new service independently testable
- [ ] No performance regression
