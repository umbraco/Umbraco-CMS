# ContentService Phase 8: Facade Finalization Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Version:** 4.0 (Updated based on Critical Review 3)
**Last Updated:** 2025-12-24
**Change Summary:** Fixed Task 4 Step 6 (IContentCrudService already exists); corrected line count target (~990 lines); added _contentSettings verification step; changed PerformMoveLocked to return collection (Option A); added unit tests for new interface methods; added API project checks; use method signatures instead of line numbers; resolved _queryNotTrashed disposition.

---

**Goal:** Finalize the ContentService refactoring by cleaning up the facade to ~990 lines (from 1330), removing dead code, simplifying constructor dependencies, and running full validation.

**Architecture:** ContentService becomes a thin facade delegating to extracted services (ContentCrudService, ContentQueryOperationService, ContentVersionOperationService, ContentMoveOperationService, ContentPublishOperationService) and managers (ContentPermissionManager, ContentBlueprintManager). Remaining orchestration methods (MoveToRecycleBin, DeleteOfTypes) stay in the facade.

**Tech Stack:** C# 12, .NET 10.0, Umbraco CMS patterns, xUnit integration tests

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-24 | Initial plan |
| 2.0 | 2025-12-24 | Updated based on Critical Review 1 - see change summary |
| 3.0 | 2025-12-24 | Updated based on Critical Review 2 - reordered tasks, unified DeleteLocked, explicit constructor steps |
| 4.0 | 2025-12-24 | Updated based on Critical Review 3 - fixed Task 4 Step 6, corrected line count, PerformMoveLocked returns collection |

---

## Current State Analysis

### ContentService Metrics
- **Current lines**: 1330
- **Target lines**: ~990 (based on calculated removal of ~340 lines)
- **Lines to remove**: ~340 (obsolete constructors ~160, Lazy fields ~40, duplicate methods ~100, field declarations ~10, internal methods ~30)

### Key Discovery from Critical Review

**Methods already exist in target services (as private):**
- `ContentMoveOperationService.PerformMoveLocked` (line 140) - private
- `ContentMoveOperationService.PerformMoveContentLocked` (line 186) - private
- `ContentMoveOperationService.GetPagedDescendantQuery` (line 591) - private
- `ContentMoveOperationService.GetPagedLocked` (line 602) - private
- `ContentCrudService.DeleteLocked` (line 637) - private

**Duplicate methods still in ContentService:**
- `ContentService.PerformMoveLocked` (line 950) - duplicate
- `ContentService.PerformMoveContentLocked` (line 1002) - duplicate
- `ContentService.DeleteLocked` (line 825) - duplicate
- `ContentService.GetPagedDescendantQuery` (line 671) - duplicate
- `ContentService.GetPagedLocked` (line 682) - duplicate

**Action:** Expose existing service methods on interfaces, then remove ContentService duplicates.

### Methods Already Delegating (Keep as-is)
All CRUD, Query, Version, Move, Publish, Permission, and Blueprint methods are already one-liners delegating to extracted services.

### Methods With Implementation Logic (Analyze for cleanup/extraction)
| Method | Lines | Status | Action |
|--------|-------|--------|--------|
| `MoveToRecycleBin` | 44 | Orchestration | Keep (unpublish+move coordination) |
| `PerformMoveLocked` | 50 | Duplicate | **Remove** - use MoveOperationService version |
| `PerformMoveContentLocked` | 10 | Duplicate | **Remove** - use MoveOperationService version |
| `DeleteLocked` | 24 | Duplicate | **Remove** - use CrudService version |
| `DeleteOfTypes/DeleteOfType` | 80 | Orchestration | Keep (descendant handling) |
| `GetPagedLocked` | 18 | Duplicate | **Remove** - use MoveOperationService version |
| `GetPagedDescendantQuery` | 10 | Duplicate | **Remove** - use MoveOperationService version |
| `CheckDataIntegrity` | 20 | Feature | Delegate to new method in ContentCrudService |
| `GetContentType` | 40 | Helper | Already used internally, keep minimal |
| `GetAllPublished` | 7 | Internal | Remove or inline |
| `Audit/AuditAsync` | 15 | Helper | Already used by services, can remove |
| Obsolete constructors | 160 | Legacy | Remove (per design) |

### Fields Analysis (CORRECTED from v1.0)
| Field | Still Used | Reason | Action |
|-------|------------|--------|--------|
| `_documentBlueprintRepository` | No | Delegated to BlueprintManager | Remove |
| `_propertyValidationService` | No | Delegated to extracted services | Remove |
| `_cultureImpactFactory` | No | Delegated to extracted services | Remove |
| `_propertyEditorCollection` | No | Delegated to extracted services | Remove |
| `_contentSettings` | **Yes** | Used via `optionsMonitor.OnChange` (lines 168-172) | **Keep OR remove with callback** |
| `_relationService` | No | Delegated to ContentMoveOperationService | Remove |
| `_queryNotTrashed` | Yes | Used in `GetAllPublished` | **Remove with GetAllPublished** (if GetAllPublished is removed in Task 6) |
| `_documentRepository` | Yes | Used in DeleteOfTypes, GetContentType | Keep |
| `_entityRepository` | No | Unused | Remove |
| `_contentTypeRepository` | Yes | Used in GetContentType, DeleteOfTypes | Keep |
| `_languageRepository` | No | Delegated to extracted services | Remove |
| `_shortStringHelper` | Yes | Used in CheckDataIntegrity | Keep until CheckDataIntegrity extracted |

**v2.0 Correction:** `_contentSettings` is used by the `optionsMonitor.OnChange` callback. If no remaining facade methods use `_contentSettings`, the callback AND the field must be removed together.

---

## Execution Order (v3.0 Update)

**Previous order:** 1 → 2 → 3 → 4 → 5 → 6 → 7 → 8 → 9

**New order:** 5 → 4 → 1 → 2 → 3 → 6 → 7 → 8 → 9

**Rationale:** Removing obsolete constructors first (old Task 5) eliminates the OnChange callbacks in those constructors, so Task 4 only needs to handle one callback location (line 168-172) instead of three. This reduces redundant work and potential merge conflicts.

**Renumbered Tasks:**
| New # | Old # | Description |
|-------|-------|-------------|
| 1 | 5 | Remove Obsolete Constructors |
| 2 | 4 | Remove Unused Fields and Simplify Constructor |
| 3 | 1 | Expose PerformMoveLocked on IContentMoveOperationService |
| 4 | 2 | Expose DeleteLocked on IContentCrudService + Unify Implementations |
| 5 | 3 | Extract CheckDataIntegrity to ContentCrudService |
| 6 | 6 | Clean Up Remaining Internal Methods |
| 7 | 7 | Verify Line Count and Final Cleanup |
| 8 | 8 | Run Full Test Suite (Phase Gate) |
| 9 | 9 | Update Design Document |

---

## Task 1: Remove Obsolete Constructors (was Task 5)

**Goal:** Remove backward-compatibility constructors and simplify service accessor properties. This must be done first to eliminate duplicate OnChange callbacks.

**Files:**
- Modify: `src/Umbraco.Core/Services/ContentService.cs`

### Step 1: Verify breaking change is acceptable

The obsolete constructors are marked "Scheduled removal in v19". Verify this is acceptable:

1. Check if current version is v19 or if early removal is approved
2. Review the obsolete message text

If removal is NOT approved for current version, **skip this task** and keep obsolete constructors.

### Step 2: Remove obsolete constructors

Delete both constructors marked with `[Obsolete("Use the non-obsolete constructor instead. Scheduled removal in v19.")]`:

**Finding the constructors:**
```bash
grep -n 'Obsolete.*Scheduled removal in v19' src/Umbraco.Core/Services/ContentService.cs
```

**Identification:**
- First obsolete constructor: The one with `IAuditRepository auditRepository` parameter (legacy signature)
- Second obsolete constructor: The one without the Phase 2-7 service parameters (intermediate signature)

Delete both constructors entirely, including their `[Obsolete]` attributes and method bodies.

### Step 3: Remove Lazy field declarations (v3.0 explicit list)

Remove these Lazy fields that are no longer needed:
- `_queryOperationServiceLazy`
- `_versionOperationServiceLazy`
- `_moveOperationServiceLazy`
- `_publishOperationServiceLazy`
- `_permissionManagerLazy`
- `_blueprintManagerLazy`

**Note:** Keep `_crudServiceLazy` - it is used by the main constructor.

### Step 4: Simplify service accessor properties

Update the service accessor properties to remove null checks for lazy fields:

```csharp
private IContentQueryOperationService QueryOperationService =>
    _queryOperationService ?? throw new InvalidOperationException("QueryOperationService not initialized.");

private IContentVersionOperationService VersionOperationService =>
    _versionOperationService ?? throw new InvalidOperationException("VersionOperationService not initialized.");

private IContentMoveOperationService MoveOperationService =>
    _moveOperationService ?? throw new InvalidOperationException("MoveOperationService not initialized.");

private IContentPublishOperationService PublishOperationService =>
    _publishOperationService ?? throw new InvalidOperationException("PublishOperationService not initialized.");

private ContentPermissionManager PermissionManager =>
    _permissionManager ?? throw new InvalidOperationException("PermissionManager not initialized.");

private ContentBlueprintManager BlueprintManager =>
    _blueprintManager ?? throw new InvalidOperationException("BlueprintManager not initialized.");
```

### Step 5: Run build to verify compilation

Run: `dotnet build src/Umbraco.Core/Umbraco.Core.csproj`
Expected: Build succeeds

### Step 6: Run tests to verify functionality

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentService"`
Expected: All tests pass

### Step 7: Commit

```bash
git add src/Umbraco.Core/Services/ContentService.cs
git commit -m "$(cat <<'EOF'
refactor(core): remove obsolete constructors from ContentService

Remove backward-compatibility constructors that used StaticServiceProvider
for lazy resolution. All dependencies are now injected directly through
the main constructor.

Remove Lazy fields no longer needed:
- _queryOperationServiceLazy
- _versionOperationServiceLazy
- _moveOperationServiceLazy
- _publishOperationServiceLazy
- _permissionManagerLazy
- _blueprintManagerLazy

BREAKING CHANGE: External code using obsolete constructors must update
to use dependency injection.

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
EOF
)"
```

---

## Task 2: Remove Unused Fields and Simplify Constructor (was Task 4)

**Goal:** Remove fields that are no longer used after service extraction and simplify the constructor. Only one OnChange callback location (line 168-172) needs removal now that obsolete constructors are gone.

**Files:**
- Modify: `src/Umbraco.Core/Services/ContentService.cs`
- Modify: `src/Umbraco.Core/DependencyInjection/UmbracoBuilder.cs`

### Step 1: Verify _contentSettings usage (v3.0 simplified)

After Task 1, only one OnChange callback remains (line 168-172). Check if any remaining ContentService methods use `_contentSettings`:

Run: `grep -n "_contentSettings" src/Umbraco.Core/Services/ContentService.cs`

Expected findings:
- Field declaration
- optionsMonitor.OnChange callback (line 168-172 only)
- No actual usage in remaining methods

**Decision:** Remove BOTH the field AND the optionsMonitor.OnChange callback together.

### Step 1a: Verify _contentSettings is not shared with extracted services (v4.0 addition)

Check that no extracted services receive _contentSettings or depend on its live updates:

```bash
grep -rn "_contentSettings" src/Umbraco.Core/Services/Content*.cs | grep -v ContentService.cs
```

Expected: No matches in ContentCrudService, ContentQueryOperationService, ContentVersionOperationService, ContentMoveOperationService, ContentPublishOperationService, ContentPermissionManager, or ContentBlueprintManager.

If any matches found, those services must either:
- Inject `IOptionsMonitor<ContentSettings>` directly
- Or the callback must be preserved in ContentService

### Step 2: Verify _relationService is not referenced (v3.0 addition)

Run: `grep -n "_relationService" src/Umbraco.Core/Services/ContentService.cs`

Expected: Only field declaration (to be removed) - no method body references. If any method body references exist, investigate before removal.

### Step 3: Identify fields to remove

Remove these fields that are no longer used directly by ContentService:
- `_documentBlueprintRepository` (delegated to BlueprintManager)
- `_propertyValidationService` (delegated to extracted services)
- `_cultureImpactFactory` (delegated to extracted services)
- `_propertyEditorCollection` (delegated to extracted services)
- `_contentSettings` AND `optionsMonitor.OnChange` callback
- `_relationService` (delegated to ContentMoveOperationService)
- `_entityRepository` (not used)
- `_languageRepository` (delegated to extracted services)
- `_shortStringHelper` (will be moved to ContentCrudService in Task 5)

Keep:
- `_documentRepository` (still used in DeleteOfTypes)
- `_contentTypeRepository` (still used in GetContentType, DeleteOfTypes)
- `_auditService` (still used in DeleteOfTypes, MoveToRecycleBin)
- `_idKeyMap` (still used in TryGetParentKey)
- `_userIdKeyResolver` (still used in AuditAsync)
- `_logger` (always useful)

### Step 4: Remove unused fields from ContentService

Edit ContentService.cs to remove the field declarations:

```csharp
// Remove these lines:
private readonly IDocumentBlueprintRepository _documentBlueprintRepository;
private readonly Lazy<IPropertyValidationService> _propertyValidationService;
private readonly ICultureImpactFactory _cultureImpactFactory;
private readonly PropertyEditorCollection _propertyEditorCollection;
private ContentSettings _contentSettings;  // Remove WITH OnChange callback
private readonly IRelationService _relationService;
private readonly IEntityRepository _entityRepository;
private readonly ILanguageRepository _languageRepository;
private readonly IShortStringHelper _shortStringHelper;
```

Also remove the `optionsMonitor.OnChange` callback block (line 168-172 only - others already removed in Task 1).

### Step 5: Update main constructor to remove unused parameters

Update the `[ActivatorUtilitiesConstructor]` constructor to remove unused parameters:

```csharp
[Microsoft.Extensions.DependencyInjection.ActivatorUtilitiesConstructor]
public ContentService(
    ICoreScopeProvider provider,
    ILoggerFactory loggerFactory,
    IEventMessagesFactory eventMessagesFactory,
    IDocumentRepository documentRepository,
    IAuditService auditService,
    IContentTypeRepository contentTypeRepository,
    IUserIdKeyResolver userIdKeyResolver,
    IIdKeyMap idKeyMap,
    IContentCrudService crudService,
    IContentQueryOperationService queryOperationService,
    IContentVersionOperationService versionOperationService,
    IContentMoveOperationService moveOperationService,
    IContentPublishOperationService publishOperationService,
    ContentPermissionManager permissionManager,
    ContentBlueprintManager blueprintManager)
    : base(provider, loggerFactory, eventMessagesFactory)
{
    _documentRepository = documentRepository;
    _auditService = auditService;
    _contentTypeRepository = contentTypeRepository;
    _userIdKeyResolver = userIdKeyResolver;
    _idKeyMap = idKeyMap;
    _logger = loggerFactory.CreateLogger<ContentService>();

    ArgumentNullException.ThrowIfNull(crudService);
    _crudServiceLazy = new Lazy<IContentCrudService>(() => crudService);

    ArgumentNullException.ThrowIfNull(queryOperationService);
    _queryOperationService = queryOperationService;

    ArgumentNullException.ThrowIfNull(versionOperationService);
    _versionOperationService = versionOperationService;

    ArgumentNullException.ThrowIfNull(moveOperationService);
    _moveOperationService = moveOperationService;

    ArgumentNullException.ThrowIfNull(publishOperationService);
    _publishOperationService = publishOperationService;

    ArgumentNullException.ThrowIfNull(permissionManager);
    _permissionManager = permissionManager;

    ArgumentNullException.ThrowIfNull(blueprintManager);
    _blueprintManager = blueprintManager;
}
```

### Step 6: Update UmbracoBuilder.cs DI registration

Update the ContentService factory in UmbracoBuilder.cs to match new constructor:

```csharp
Services.AddUnique<IContentService>(sp =>
    new ContentService(
        sp.GetRequiredService<ICoreScopeProvider>(),
        sp.GetRequiredService<ILoggerFactory>(),
        sp.GetRequiredService<IEventMessagesFactory>(),
        sp.GetRequiredService<IDocumentRepository>(),
        sp.GetRequiredService<IAuditService>(),
        sp.GetRequiredService<IContentTypeRepository>(),
        sp.GetRequiredService<IUserIdKeyResolver>(),
        sp.GetRequiredService<IIdKeyMap>(),
        sp.GetRequiredService<IContentCrudService>(),
        sp.GetRequiredService<IContentQueryOperationService>(),
        sp.GetRequiredService<IContentVersionOperationService>(),
        sp.GetRequiredService<IContentMoveOperationService>(),
        sp.GetRequiredService<IContentPublishOperationService>(),
        sp.GetRequiredService<ContentPermissionManager>(),
        sp.GetRequiredService<ContentBlueprintManager>()));
```

### Step 7: Run build to verify compilation

Run: `dotnet build src/Umbraco.Core/Umbraco.Core.csproj`
Expected: Build succeeds

### Step 8: Run tests to verify functionality

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentService"`
Expected: All tests pass

### Step 9: Commit

```bash
git add src/Umbraco.Core/Services/ContentService.cs \
        src/Umbraco.Core/DependencyInjection/UmbracoBuilder.cs
git commit -m "$(cat <<'EOF'
refactor(core): remove unused fields from ContentService

Remove fields that are now handled by extracted services:
- IDocumentBlueprintRepository (BlueprintManager)
- IPropertyValidationService (extracted services)
- ICultureImpactFactory (extracted services)
- PropertyEditorCollection (extracted services)
- ContentSettings + optionsMonitor.OnChange callback
- IRelationService (ContentMoveOperationService)
- IEntityRepository (unused)
- ILanguageRepository (extracted services)
- IShortStringHelper (ContentCrudService)

Simplify constructor to only inject dependencies still used directly.

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
EOF
)"
```

---

## Task 3: Expose PerformMoveLocked on IContentMoveOperationService (was Task 1)

**Goal:** Make existing private `PerformMoveLocked` in `ContentMoveOperationService` public and accessible via interface, then remove duplicate from `ContentService`.

**Files:**
- Modify: `src/Umbraco.Core/Services/IContentMoveOperationService.cs`
- Modify: `src/Umbraco.Core/Services/ContentMoveOperationService.cs`
- Modify: `src/Umbraco.Core/Services/ContentService.cs`

### Step 1: Add method signature to IContentMoveOperationService (v4.0 - returns collection)

Add interface method for the internal move logic. **v4.0 change:** Returns the moves collection instead of mutating a parameter (cleaner API):

```csharp
// At end of IContentMoveOperationService.cs, add:

/// <summary>
/// Performs the locked move operation for a content item and its descendants.
/// Used internally by MoveToRecycleBin orchestration.
/// </summary>
/// <param name="content">The content to move.</param>
/// <param name="parentId">The target parent id.</param>
/// <param name="parent">The target parent content (can be null for root/recycle bin).</param>
/// <param name="userId">The user performing the operation.</param>
/// <param name="trash">Whether to mark as trashed (true), un-trashed (false), or unchanged (null).</param>
/// <returns>Collection of moved items with their original paths.</returns>
IReadOnlyCollection<(IContent Content, string OriginalPath)> PerformMoveLocked(
    IContent content, int parentId, IContent? parent, int userId, bool? trash);
```

### Step 2: Update existing method in ContentMoveOperationService (v4.0 - new signature)

The existing private method has signature:
```csharp
private void PerformMoveLocked(IContent content, int parentId, IContent? parent, int userId, ICollection<(IContent, string)> moves, bool? trash)
```

Create a new public method with the cleaner signature that wraps the existing implementation:

```csharp
/// <inheritdoc />
public IReadOnlyCollection<(IContent Content, string OriginalPath)> PerformMoveLocked(
    IContent content, int parentId, IContent? parent, int userId, bool? trash)
{
    var moves = new List<(IContent, string)>();
    PerformMoveLockedInternal(content, parentId, parent, userId, moves, trash);
    return moves.AsReadOnly();
}

// Rename existing private method to:
private void PerformMoveLockedInternal(IContent content, int parentId, IContent? parent, int userId, ICollection<(IContent, string)> moves, bool? trash)
```

This keeps the internal recursive implementation unchanged while providing a clean public interface.

### Step 3: Verify build compiles

Run: `dotnet build src/Umbraco.Core/Umbraco.Core.csproj`
Expected: Build succeeds

### Step 4: Update ContentService.MoveToRecycleBin to use service (v4.0 - new return value)

Replace the `PerformMoveLocked` call in ContentService with delegation. The new signature returns the collection:

```csharp
// In MoveToRecycleBin, replace:
// var moves = new List<(IContent, string)>();
// PerformMoveLocked(content, Constants.System.RecycleBinContent, null, userId, moves, true);
// With:
var moves = MoveOperationService.PerformMoveLocked(content, Constants.System.RecycleBinContent, null, userId, true);
```

The caller no longer needs to create the collection - it's returned by the service.

### Step 5: Remove duplicate methods from ContentService

Delete these methods from ContentService.cs:
- `PerformMoveLocked` (lines ~950-1000)
- `PerformMoveContentLocked` (lines ~1002-1011)
- `GetPagedDescendantQuery` (lines ~671-680)
- `GetPagedLocked` (lines ~682-700)

### Step 6: Run tests to verify move operations work

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentService"`
Expected: All tests pass

### Step 7: Commit

```bash
git add src/Umbraco.Core/Services/IContentMoveOperationService.cs \
        src/Umbraco.Core/Services/ContentMoveOperationService.cs \
        src/Umbraco.Core/Services/ContentService.cs
git commit -m "$(cat <<'EOF'
refactor(core): expose PerformMoveLocked on IContentMoveOperationService

Make existing private PerformMoveLocked method public and add to interface.
Update ContentService.MoveToRecycleBin to delegate to the service.
Remove duplicate helper methods from ContentService:
- PerformMoveLocked
- PerformMoveContentLocked
- GetPagedDescendantQuery
- GetPagedLocked

This reduces ContentService complexity while maintaining MoveToRecycleBin
orchestration in the facade.

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
EOF
)"
```

---

## Task 4: Expose DeleteLocked on IContentCrudService + Unify Implementations (was Task 2)

**Goal:** Make existing private `DeleteLocked` in `ContentCrudService` public and accessible via interface. Also unify with `ContentMoveOperationService.DeleteLocked` by having EmptyRecycleBin call the CrudService version. This eliminates duplicate implementations.

**Files:**
- Modify: `src/Umbraco.Core/Services/IContentCrudService.cs`
- Modify: `src/Umbraco.Core/Services/ContentCrudService.cs`
- Modify: `src/Umbraco.Core/Services/ContentService.cs`
- Modify: `src/Umbraco.Core/Services/ContentMoveOperationService.cs` (v3.0 addition for unification)

### Step 1: Add DeleteLocked to IContentCrudService

```csharp
// Add to IContentCrudService.cs:

/// <summary>
/// Performs the locked delete operation including descendants.
/// Used internally by DeleteOfTypes orchestration and EmptyRecycleBin.
/// </summary>
void DeleteLocked(ICoreScope scope, IContent content, EventMessages evtMsgs);
```

### Step 2: Change existing method visibility in ContentCrudService

Change the existing private method to public:

```csharp
// In ContentCrudService.cs, change line 637:
// FROM:
private void DeleteLocked(ICoreScope scope, IContent content, EventMessages evtMsgs)
// TO:
public void DeleteLocked(ICoreScope scope, IContent content, EventMessages evtMsgs)
```

**Important:** The existing implementation in ContentCrudService already has:
- Iteration bounds (maxIterations = 10000)
- Proper logging for edge cases
- Empty batch detection

Do NOT replace this with the simplified version from v1.0 of the plan.

### Step 3: Run build to verify interface compiles

Run: `dotnet build src/Umbraco.Core/Umbraco.Core.csproj`
Expected: Build succeeds

### Step 4: Update ContentService.DeleteOfTypes to use service

Replace the `DeleteLocked` call:

```csharp
// Replace:
// DeleteLocked(scope, content, eventMessages);
// With:
CrudService.DeleteLocked(scope, content, eventMessages);
```

### Step 5: Remove DeleteLocked from ContentService

Delete the `DeleteLocked` method (lines ~825-848).

### Step 6: Unify ContentMoveOperationService.EmptyRecycleBin (v4.0 corrected)

ContentMoveOperationService **already has** `IContentCrudService` as a constructor parameter (assigned to `_crudService` field). Update `EmptyRecycleBin` to call `IContentCrudService.DeleteLocked` instead of its own local `DeleteLocked` method:

1. In `EmptyRecycleBin`, replace:
   ```csharp
   // FROM:
   DeleteLocked(scope, content, eventMessages);
   // TO:
   _crudService.DeleteLocked(scope, content, eventMessages);
   ```
2. Remove the local `DeleteLocked` method from `ContentMoveOperationService` (search for `private void DeleteLocked`)

This eliminates duplicate implementations and ensures bug fixes only need to be applied once.

### Step 7: Run tests to verify delete operations work

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentService"`
Expected: All tests pass

### Step 8: Commit

```bash
git add src/Umbraco.Core/Services/IContentCrudService.cs \
        src/Umbraco.Core/Services/ContentCrudService.cs \
        src/Umbraco.Core/Services/ContentService.cs \
        src/Umbraco.Core/Services/ContentMoveOperationService.cs
git commit -m "$(cat <<'EOF'
refactor(core): expose DeleteLocked on IContentCrudService and unify implementations

Make existing private DeleteLocked method public and add to interface.
Update ContentService.DeleteOfTypes to delegate to the service.
Remove duplicate DeleteLocked from ContentService.

Unify implementations by having ContentMoveOperationService.EmptyRecycleBin
call IContentCrudService.DeleteLocked instead of its own local method.
This eliminates the duplicate implementation and reduces maintenance burden.

The ContentCrudService implementation includes iteration bounds
and proper logging for edge cases.

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
EOF
)"
```

---

## Task 5: Extract CheckDataIntegrity to ContentCrudService (was Task 3)

**Goal:** Move `CheckDataIntegrity` from ContentService to ContentCrudService.

**Files:**
- Modify: `src/Umbraco.Core/Services/IContentCrudService.cs`
- Modify: `src/Umbraco.Core/Services/ContentCrudService.cs`
- Modify: `src/Umbraco.Core/Services/ContentService.cs`

### Step 1: Add CheckDataIntegrity to IContentCrudService

```csharp
// Add to IContentCrudService.cs:

/// <summary>
/// Checks content data integrity and optionally fixes issues.
/// </summary>
ContentDataIntegrityReport CheckDataIntegrity(ContentDataIntegrityReportOptions options);
```

### Step 2: Run build to verify interface compiles

Run: `dotnet build src/Umbraco.Core/Umbraco.Core.csproj`
Expected: Build succeeds

### Step 3: Add IShortStringHelper to ContentCrudService constructor (v3.0 explicit steps)

1. Add `IShortStringHelper shortStringHelper` parameter to ContentCrudService constructor
2. Add private field `private readonly IShortStringHelper _shortStringHelper;`
3. Assign in constructor: `_shortStringHelper = shortStringHelper;`

```csharp
// In ContentCrudService constructor, add parameter:
public ContentCrudService(
    // ... existing parameters ...
    IShortStringHelper shortStringHelper)
{
    // ... existing assignments ...
    _shortStringHelper = shortStringHelper;
}
```

### Step 4: Verify IShortStringHelper DI registration

Verify that ContentCrudService DI registration in UmbracoBuilder.cs will resolve IShortStringHelper. Since it uses `AddUnique<IContentCrudService, ContentCrudService>()`, DI should auto-resolve the new dependency.

Run build after this step to confirm: `dotnet build src/Umbraco.Core/Umbraco.Core.csproj`

If build fails with DI resolution error, check that `IShortStringHelper` is registered (search for `AddShortString` or `IShortStringHelper` in UmbracoBuilder.cs).

### Step 5: Move CheckDataIntegrity implementation to ContentCrudService

Add to ContentCrudService.cs:

```csharp
/// <inheritdoc />
public ContentDataIntegrityReport CheckDataIntegrity(ContentDataIntegrityReportOptions options)
{
    using (ICoreScope scope = ScopeProvider.CreateCoreScope())
    {
        scope.WriteLock(Constants.Locks.ContentTree);

        ContentDataIntegrityReport report = _documentRepository.CheckDataIntegrity(options);

        if (report.FixedIssues.Count > 0)
        {
            var root = new Content("root", -1, new ContentType(_shortStringHelper, -1)) { Id = -1, Key = Guid.Empty };
            scope.Notifications.Publish(new ContentTreeChangeNotification(root, TreeChangeTypes.RefreshAll, EventMessagesFactory.Get()));
        }

        scope.Complete();

        return report;
    }
}
```

### Step 6: Update ContentService.CheckDataIntegrity to delegate

```csharp
public ContentDataIntegrityReport CheckDataIntegrity(ContentDataIntegrityReportOptions options)
    => CrudService.CheckDataIntegrity(options);
```

### Step 7: Run tests to verify data integrity operations work

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentService"`
Expected: All tests pass

### Step 8: Commit

```bash
git add src/Umbraco.Core/Services/IContentCrudService.cs \
        src/Umbraco.Core/Services/ContentCrudService.cs \
        src/Umbraco.Core/Services/ContentService.cs
git commit -m "$(cat <<'EOF'
refactor(core): extract CheckDataIntegrity to ContentCrudService

Move CheckDataIntegrity from ContentService to ContentCrudService.
Add IShortStringHelper dependency to ContentCrudService.

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
EOF
)"
```

---

## Task 6: Clean Up Remaining Internal Methods

**Files:**
- Modify: `src/Umbraco.Core/Services/ContentService.cs`

### Step 1: Check GetAllPublished usage (v4.0 expanded)

This method is `internal` (not public). Check if it's called externally:

Run: `grep -rn "GetAllPublished" src/ --include="*.cs" | grep -v ContentService.cs`

**Note (v2.0):** Internal methods can be used by test projects via `InternalsVisibleTo`. Check test projects too:

Run: `grep -rn "GetAllPublished" tests/ --include="*.cs"`

**Note (v3.0):** Also check web projects that may have access via `InternalsVisibleTo`:

Run: `grep -rn "GetAllPublished" src/Umbraco.Infrastructure/ src/Umbraco.Web.Common/ --include="*.cs"`

**Note (v4.0):** Also check API projects which may have InternalsVisibleTo access:

Run: `grep -rn "GetAllPublished" src/Umbraco.Cms.Api.Management/ src/Umbraco.Cms.Api.Delivery/ --include="*.cs"`

If not used externally, in tests, or in API projects, remove it along with `_queryNotTrashed` field. If used, keep both.

### Step 2: Remove HasUnsavedChanges if unused

Check usage:
Run: `grep -rn "HasUnsavedChanges" src/ tests/ --include="*.cs" | grep -v ContentService.cs`

If not used, remove the static method.

### Step 3: Remove TryGetParentKey if unused

Check usage:
Run: `grep -rn "TryGetParentKey" src/ tests/ --include="*.cs" | grep -v ContentService.cs`

If not used externally and not used in remaining ContentService methods, remove it.

### Step 4: Simplify Audit methods

If AuditAsync is only called by the sync Audit method, and Audit is only used in MoveToRecycleBin and DeleteOfTypes, keep them but simplify.

### Step 5: Run build to verify compilation

Run: `dotnet build src/Umbraco.Core/Umbraco.Core.csproj`
Expected: Build succeeds

### Step 6: Run tests to verify functionality

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentService"`
Expected: All tests pass

### Step 7: Commit

```bash
git add src/Umbraco.Core/Services/ContentService.cs
git commit -m "$(cat <<'EOF'
refactor(core): clean up remaining internal methods in ContentService

Remove or simplify internal helper methods that are no longer needed
after service extraction.

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
EOF
)"
```

---

## Task 7: Verify Line Count and Final Cleanup

**Files:**
- Review: `src/Umbraco.Core/Services/ContentService.cs`

### Step 1: Count lines (v4.0 corrected)

Run: `wc -l src/Umbraco.Core/Services/ContentService.cs`

**Expected: ~990 lines** (calculated from 1330 - ~340 lines of removal)

Line removal breakdown:
- Obsolete constructors: ~160 lines
- Lazy fields and duplicate properties: ~40 lines
- Duplicate methods (PerformMoveLocked, DeleteLocked, etc.): ~100 lines
- Field declarations removal: ~10 lines
- Internal method cleanup: ~30 lines
- **Total removal:** ~340 lines

### Step 2: Review remaining structure

The file should contain:
1. Field declarations (~10-15 lines)
2. Constructor (~30-40 lines)
3. Service accessor properties (~30 lines)
4. One-liner delegation methods (~100-150 lines)
5. Orchestration methods: MoveToRecycleBin, DeleteOfTypes/DeleteOfType (~80 lines)
6. Helper methods: GetContentType, Audit (~30 lines)

### Step 3: Format code

Run: `dotnet format src/Umbraco.Core/Umbraco.Core.csproj`
Expected: No formatting changes or only minor whitespace

### Step 4: Commit if formatting changed

```bash
git add src/Umbraco.Core/Services/ContentService.cs
git commit -m "$(cat <<'EOF'
style(core): format ContentService.cs

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
EOF
)"
```

---

## Task 8: Run Full Test Suite (Phase Gate)

**Files:**
- None (verification only)

### Step 1: Run refactoring-specific tests

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentServiceRefactoringTests"`
Expected: All 15+ tests pass

### Step 2: Run all ContentService tests

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentService"`
Expected: All tests pass

### Step 2a: Add unit tests for newly exposed interface methods (v4.0 addition)

Create or update unit tests to cover the newly public interface methods:

**For IContentMoveOperationService.PerformMoveLocked:**
- Test that it returns a non-null collection
- Test that moved items are included in the returned collection
- Test edge cases: single item, nested hierarchy

**For IContentCrudService.DeleteLocked:**
- Test that it handles empty tree (no descendants)
- Test that it handles large tree (iteration bounds)
- Test that null content throws appropriate exception

Add tests to: `tests/Umbraco.Tests.Integration/Umbraco.Core/Services/ContentServiceRefactoringTests.cs`

### Step 3: Run full integration test suite (v3.0 duration note)

**Note:** The full integration test suite can take 10+ minutes. Consider quick verification first:

```bash
# Quick verification (2-3 min) - run critical paths first
dotnet test tests/Umbraco.Tests.Integration --filter "Category=Quick"

# Full suite (10+ min)
dotnet test tests/Umbraco.Tests.Integration
```

Run: `dotnet test tests/Umbraco.Tests.Integration`
Expected: All tests pass (or only pre-existing failures)

### Step 4: Document any test failures

If tests fail, document them and investigate whether they're:
1. Pre-existing failures (acceptable)
2. Regressions from this phase (must fix)

### Step 5: Create Phase 8 git tag

```bash
git tag -a phase-8-facade-finalization -m "Phase 8: ContentService facade finalization complete"
```

---

## Task 9: Update Design Document

**Files:**
- Modify: `docs/plans/2025-12-19-contentservice-refactor-design.md`

### Step 1: Mark Phase 8 complete

Update the implementation order table:

```markdown
| 8 | Facade | **Full test suite** | All pass | ✅ Complete |
```

### Step 2: Update success criteria

Check off completed items:
- [x] All existing tests pass
- [x] No public API breaking changes
- [x] ContentService reduced to ~990 lines (from 1330)
- [x] Each new service independently testable
- [x] Notification ordering matches current behavior
- [x] All 80+ IContentService methods mapped to new services

### Step 3: Add Phase 8 details

Add to the phase details section:

```markdown
10. **Phase 8: Facade Finalization** ✅ - Complete! Changes:
    - Exposed PerformMoveLocked on IContentMoveOperationService (returns collection - clean API)
    - Exposed DeleteLocked on IContentCrudService
    - Unified DeleteLocked implementations (ContentMoveOperationService now calls CrudService)
    - Extracted CheckDataIntegrity to ContentCrudService
    - Removed 9 unused fields from ContentService
    - Removed optionsMonitor.OnChange callback (no longer needed)
    - Removed 2 obsolete constructors (~160 lines)
    - Simplified constructor to 15 parameters (services only)
    - ContentService reduced from 1330 lines to ~990 lines
    - Git tag: `phase-8-facade-finalization`
```

### Step 4: Commit

```bash
git add docs/plans/2025-12-19-contentservice-refactor-design.md
git commit -m "$(cat <<'EOF'
docs: mark Phase 8 complete in design document

Update ContentService refactoring design document to reflect
Phase 8 facade finalization completion.

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
EOF
)"
```

---

## Summary

### Tasks Overview (v4.0 - Updated)

| Task | Description | Est. Steps | v4.0 Changes |
|------|-------------|------------|--------------|
| 1 | Remove Obsolete Constructors | 7 | Use method signatures instead of line numbers |
| 2 | Remove Unused Fields and Simplify Constructor | 10 | **Added** Step 1a: _contentSettings verification |
| 3 | Expose PerformMoveLocked on interface | 7 | **Changed** to return collection (Option A - clean API) |
| 4 | Expose DeleteLocked + Unify Implementations | 8 | **Fixed** Step 6: IContentCrudService already exists |
| 5 | Extract CheckDataIntegrity to ContentCrudService | 8 | No change |
| 6 | Clean Up Remaining Internal Methods | 7 | **Added** API project checks |
| 7 | Verify Line Count and Final Cleanup | 4 | **Fixed** target: ~990 lines |
| 8 | Run Full Test Suite (Phase Gate) | 6 | **Added** Step 2a: unit tests for new interface methods |
| 9 | Update Design Document | 4 | Updated line count references |
| **Total** | | **61** | |

### Expected Outcomes

1. **ContentService reduced to ~990 lines** (from 1330) - v4.0 corrected calculation
2. **Constructor simplified** to only essential dependencies
3. **No obsolete constructors** remaining (if version approved)
4. **All duplicate methods** removed (not re-extracted)
5. **DeleteLocked unified** - Single implementation in ContentCrudService (v3.0)
6. **PerformMoveLocked returns collection** - Clean interface without mutable parameter (v4.0)
7. **All tests passing** (full integration suite)
8. **Design document updated** with Phase 8 completion

### Risk Mitigation

| Risk | Mitigation |
|------|------------|
| Breaking existing callers | Only remove internal/private methods, keep public API |
| Test failures | Run tests after each extraction, fix before proceeding |
| Missing dependencies | Verify each exposed method has access to required repositories |
| DI registration issues | Update UmbracoBuilder.cs in sync with constructor changes |
| Breaking change version | Verify obsolete constructor removal is approved for current version |

### Rollback Plan

If issues are discovered:
1. Revert to the commit before the problematic change
2. Investigate the root cause
3. Fix and retry the specific task
4. Do not batch multiple risky changes

---

## Review Feedback Incorporated (v2.0)

This version addresses all findings from Critical Review 1:

| Review Finding | Resolution |
|----------------|------------|
| Task 1-2: Methods already exist | Rewritten to expose existing methods, not re-extract |
| Task 3: Missing DI registration step | Added Step 4 for UmbracoBuilder.cs verification |
| Task 4: _contentSettings still used | Added removal of OnChange callback with field |
| Task 5: Breaking change timing | Added version verification step |
| Task 6: Internal method grep incorrect | Fixed grep to include tests folder |
| Task 7: Unrealistic line target | Changed from ~200 to ~250-300 |
| Constructor parameter count | Updated to reflect actual 15 parameters |

---

## Review Feedback Incorporated (v3.0)

This version addresses all findings from Critical Review 2:

| Review Finding | Section | Resolution |
|----------------|---------|------------|
| Duplicate DeleteLocked in two services | 2.1 | **Unify implementations** - ContentMoveOperationService calls IContentCrudService.DeleteLocked |
| Task execution order creates redundant work | 2.2 | **Reordered tasks** - Task 5 (obsolete constructors) now first, then Task 4 (fields) |
| Missing IShortStringHelper constructor steps | 2.3 | Added explicit steps: add parameter, add field, verify DI |
| Interface signature exposes mutable collection | 2.4 | Added XML documentation warning about collection mutation |
| Potential null reference after field removal | 2.5 | Added verification step to check _relationService references |
| Lazy field removal not explicit | 3.2 | Listed all 6 Lazy fields to remove explicitly |
| Internal method check should include web projects | 3.3 | Added grep for Umbraco.Infrastructure and Umbraco.Web.Common |
| Full integration test duration | 3.5 | Added note about 10+ minute duration and quick verification option |

---

## Review Feedback Incorporated (v4.0)

This version addresses all findings from Critical Review 3:

| Review Finding | Section | Resolution |
|----------------|---------|------------|
| Task 4 Step 6: IContentCrudService already exists | 2.1 | **Fixed** - Removed incorrect instruction to add dependency; clarified it already exists as `_crudService` |
| Task 7 line count target incorrect (~250-300 vs ~990) | 2.5 | **Fixed** - Recalculated to ~990 lines based on ~340 lines of removal |
| Missing _contentSettings verification before removal | 2.3 | **Added** Step 1a to Task 2 - verify no extracted services depend on _contentSettings |
| PerformMoveLocked exposes mutable collection parameter | 2.2 | **Fixed** - Changed to Option A: returns `IReadOnlyCollection<(IContent, string)>` instead of mutating parameter |
| No unit tests for newly exposed interface methods | 3.3 | **Added** Step 2a to Task 8 - add tests for PerformMoveLocked and DeleteLocked |
| Missing API project checks in Task 6 | 3.2 | **Added** grep for Umbraco.Cms.Api.Management and Umbraco.Cms.Api.Delivery |
| Line numbers for obsolete constructors may be stale | 3.4 | **Fixed** - Use method signatures and grep patterns instead of line numbers |
| _queryNotTrashed disposition unclear | Q2 | **Resolved** - Remove with GetAllPublished if GetAllPublished is removed |

---

**End of Phase 8 Implementation Plan v4.0**
