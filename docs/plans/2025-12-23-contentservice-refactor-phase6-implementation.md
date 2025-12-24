# Phase 6: ContentPermissionManager Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-23 | Initial plan |
| 1.1 | 2025-12-23 | Applied critical review feedback: file location changed to Infrastructure, added input validation, fixed ArgumentNullException pattern, added logging |
| 1.2 | 2025-12-23 | Applied critical review 2 feedback: **REVERTED** file location to Core (Infrastructure placement was blocking architectural violation), added permission character validation warning |
| 1.3 | 2025-12-23 | Applied critical review 3 feedback: **FIXED** DI registration file path (now Core/UmbracoBuilder.cs not Infrastructure), confirmed directory path has no `/Content/` subdirectory, documented constructor parameter position and AddScoped choice |

---

**Goal:** Extract permission operations (SetPermissions, SetPermission, GetPermissions) from ContentService into an internal ContentPermissionManager class.

**Architecture:** The ContentPermissionManager will be an internal class that encapsulates all permission-related operations. It will be registered as a scoped service in DI and injected into ContentService for delegation. Unlike the public operation services (IContentCrudService, etc.), this is an internal helper class per the design document specification.

**Tech Stack:** .NET 10.0, Umbraco.Core, NUnit for testing

---

## Pre-Implementation Checklist

Before starting, verify the baseline:

```bash
# Run permission tests to establish baseline
dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentServiceRefactoringTests" --no-build

# Run all ContentService tests
dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentService" --no-build
```

**Expected:** All tests pass (including the 4 permission tests: 9, 10, 11, 12).

---

## Task 1: Create ContentPermissionManager Class

**Files:**
- Create: `src/Umbraco.Core/Services/ContentPermissionManager.cs`

> **v1.2 Change:** File location **REVERTED** to `Umbraco.Core`. The v1.1 change to Infrastructure was a blocking architectural violation - Core cannot reference Infrastructure (wrong dependency direction). All Phases 1-5 extracted services are in Core, following the established pattern.

**Step 1: Verify target directory exists**

The file will be placed directly in `src/Umbraco.Core/Services/` to match the pattern from Phases 1-5:

```bash
ls src/Umbraco.Core/Services/ContentCrudService.cs  # Should exist from Phase 1
```

**Step 2: Create the ContentPermissionManager class**

```csharp
// src/Umbraco.Core/Services/ContentPermissionManager.cs
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Internal manager for content permission operations.
/// </summary>
/// <remarks>
/// <para>
/// This is an internal class that encapsulates permission operations extracted from ContentService
/// as part of the ContentService refactoring initiative (Phase 6).
/// </para>
/// <para>
/// <strong>Design Decision:</strong> This class is internal (not public interface) because:
/// <list type="bullet">
///   <item><description>Permission operations are tightly coupled to content entities</description></item>
///   <item><description>They don't require independent testability beyond ContentService tests</description></item>
///   <item><description>The public API remains through IContentService for backward compatibility</description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Note:</strong> GetPermissionsForEntity returns EntityPermissionCollection which is a
/// materialized collection (not deferred), so scope disposal before enumeration is safe.
/// </para>
/// </remarks>
internal sealed class ContentPermissionManager
{
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly IDocumentRepository _documentRepository;
    private readonly ILogger<ContentPermissionManager> _logger;

    public ContentPermissionManager(
        ICoreScopeProvider scopeProvider,
        IDocumentRepository documentRepository,
        ILoggerFactory loggerFactory)
    {
        // v1.1: Use ArgumentNullException.ThrowIfNull for consistency with codebase patterns
        ArgumentNullException.ThrowIfNull(scopeProvider);
        ArgumentNullException.ThrowIfNull(documentRepository);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        _scopeProvider = scopeProvider;
        _documentRepository = documentRepository;
        _logger = loggerFactory.CreateLogger<ContentPermissionManager>();
    }

    /// <summary>
    /// Used to bulk update the permissions set for a content item. This will replace all permissions
    /// assigned to an entity with a list of user id &amp; permission pairs.
    /// </summary>
    /// <param name="permissionSet">The permission set to assign.</param>
    public void SetPermissions(EntityPermissionSet permissionSet)
    {
        // v1.1: Add input validation
        ArgumentNullException.ThrowIfNull(permissionSet);

        // v1.1: Add logging for security-relevant operations
        _logger.LogDebug("Replacing all permissions for entity {EntityId}", permissionSet.EntityId);

        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        scope.WriteLock(Constants.Locks.ContentTree);
        _documentRepository.ReplaceContentPermissions(permissionSet);
        scope.Complete();
    }

    /// <summary>
    /// Assigns a single permission to the current content item for the specified group ids.
    /// </summary>
    /// <param name="entity">The content entity.</param>
    /// <param name="permission">The permission character (e.g., "F" for Browse, "U" for Update).</param>
    /// <param name="groupIds">The user group IDs to assign the permission to.</param>
    public void SetPermission(IContent entity, string permission, IEnumerable<int> groupIds)
    {
        // v1.1: Add input validation
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentException.ThrowIfNullOrWhiteSpace(permission);
        ArgumentNullException.ThrowIfNull(groupIds);

        // v1.2: Add warning for non-standard permission codes (Umbraco uses single characters)
        if (permission.Length != 1)
        {
            _logger.LogWarning(
                "Permission code {Permission} has length {Length}; expected single character for entity {EntityId}",
                permission, permission.Length, entity.Id);
        }

        // v1.1: Add logging for security-relevant operations
        _logger.LogDebug("Assigning permission {Permission} to groups for entity {EntityId}",
            permission, entity.Id);

        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        scope.WriteLock(Constants.Locks.ContentTree);
        _documentRepository.AssignEntityPermission(entity, permission, groupIds);
        scope.Complete();
    }

    /// <summary>
    /// Returns implicit/inherited permissions assigned to the content item for all user groups.
    /// </summary>
    /// <param name="content">The content item to get permissions for.</param>
    /// <returns>Collection of entity permissions (materialized, not deferred).</returns>
    public EntityPermissionCollection GetPermissions(IContent content)
    {
        // v1.1: Add input validation
        ArgumentNullException.ThrowIfNull(content);

        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(Constants.Locks.ContentTree);
        return _documentRepository.GetPermissionsForEntity(content.Id);
    }
}
```

**Step 3: Verify the file compiles**

```bash
dotnet build src/Umbraco.Core/Umbraco.Core.csproj --no-restore
```

**Expected:** Build succeeds.

**Step 4: Commit**

```bash
git add src/Umbraco.Core/Services/ContentPermissionManager.cs
git commit -m "feat(core): add ContentPermissionManager for Phase 6 extraction

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>"
```

---

## Task 2: Register ContentPermissionManager in DI

**Files:**
- Modify: `src/Umbraco.Core/DependencyInjection/UmbracoBuilder.cs`

> **v1.3 Change:** DI registration is in `Umbraco.Core/DependencyInjection/UmbracoBuilder.cs` (not Infrastructure), matching Phases 1-5 pattern. The ContentService factory and all extracted services are registered here (lines ~301-329).

**Step 1: Register the ContentPermissionManager**

Find the service registrations section in `UmbracoBuilder.cs` (around line 305, after `AddUnique<IContentPublishOperationService>`) and add:

```csharp
// Phase 6: Internal permission manager (AddScoped, not AddUnique, because it's internal without interface)
Services.AddScoped<ContentPermissionManager>();
```

> **Design Note (v1.3):** We use `AddScoped` instead of `AddUnique` because:
> - `AddUnique` is for interface-based registrations (prevents duplicate implementations)
> - `ContentPermissionManager` is `internal` without an interface (per design document)
> - Scoped lifetime matches ContentService's request-scoped usage patterns

Add this line after the ContentPublishOperationService registration (around line 305) and before the IContentService factory registration.

**Step 2: Verify the registration compiles**

```bash
dotnet build src/Umbraco.Core/Umbraco.Core.csproj --no-restore
```

**Expected:** Build succeeds.

**Step 3: Commit**

```bash
git add src/Umbraco.Core/DependencyInjection/UmbracoBuilder.cs
git commit -m "chore(di): register ContentPermissionManager as scoped service

Phase 6: Internal permission manager with scoped lifetime.

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>"
```

---

## Task 3: Add ContentPermissionManager to ContentService Constructor

**Files:**
- Modify: `src/Umbraco.Core/Services/ContentService.cs`

**Step 1: Verify the using directive**

The `Umbraco.Cms.Core.Services` namespace should already be available (same namespace). No additional using directive needed.

> **v1.2 Change:** No using directive needed - ContentPermissionManager is in the same namespace as ContentService (`Umbraco.Cms.Core.Services`).

**Step 2: Add private field for ContentPermissionManager**

In the field declarations section (around line 48), add:

```csharp
// Permission manager field (for Phase 6 extracted permission operations)
private readonly ContentPermissionManager? _permissionManager;
private readonly Lazy<ContentPermissionManager>? _permissionManagerLazy;
```

**Step 3: Add property accessor**

After the PublishOperationService property (around line 100), add:

```csharp
/// <summary>
/// Gets the permission manager.
/// </summary>
/// <exception cref="InvalidOperationException">Thrown if the manager was not properly initialized.</exception>
private ContentPermissionManager PermissionManager =>
    _permissionManager ?? _permissionManagerLazy?.Value
    ?? throw new InvalidOperationException("PermissionManager not initialized. Ensure the manager is properly injected via constructor.");
```

**Step 4: Update the primary constructor (ActivatorUtilitiesConstructor)**

Add the new parameter to the primary constructor (the one with `[ActivatorUtilitiesConstructor]`).

> **v1.3 Note:** The ContentService constructor currently has 21 parameters (lines 105-127). ContentPermissionManager will be **parameter 22**, added AFTER `publishOperationService`. The exact order matters for the DI factory in Task 4.

```csharp
[Microsoft.Extensions.DependencyInjection.ActivatorUtilitiesConstructor]
public ContentService(
    ICoreScopeProvider provider,                        // 1
    ILoggerFactory loggerFactory,                       // 2
    IEventMessagesFactory eventMessagesFactory,         // 3
    IDocumentRepository documentRepository,             // 4
    IEntityRepository entityRepository,                 // 5
    IAuditService auditService,                         // 6
    IContentTypeRepository contentTypeRepository,       // 7
    IDocumentBlueprintRepository documentBlueprintRepository, // 8
    ILanguageRepository languageRepository,             // 9
    Lazy<IPropertyValidationService> propertyValidationService, // 10
    IShortStringHelper shortStringHelper,               // 11
    ICultureImpactFactory cultureImpactFactory,         // 12
    IUserIdKeyResolver userIdKeyResolver,               // 13
    PropertyEditorCollection propertyEditorCollection,  // 14
    IIdKeyMap idKeyMap,                                 // 15
    IOptionsMonitor<ContentSettings> optionsMonitor,    // 16
    IRelationService relationService,                   // 17
    IContentCrudService crudService,                    // 18
    IContentQueryOperationService queryOperationService, // 19
    IContentVersionOperationService versionOperationService, // 20
    IContentMoveOperationService moveOperationService,  // 21
    IContentPublishOperationService publishOperationService, // 22
    ContentPermissionManager permissionManager)         // 23 - NEW Phase 6 permission operations
```

And in the constructor body, add:

```csharp
// Phase 6: Permission manager (direct injection)
ArgumentNullException.ThrowIfNull(permissionManager);
_permissionManager = permissionManager;
_permissionManagerLazy = null;  // Not needed when directly injected
```

**Step 5: Update the obsolete constructors**

For each obsolete constructor, add lazy resolution:

```csharp
// Phase 6: Lazy resolution of ContentPermissionManager
_permissionManagerLazy = new Lazy<ContentPermissionManager>(() =>
    StaticServiceProvider.Instance.GetRequiredService<ContentPermissionManager>(),
    LazyThreadSafetyMode.ExecutionAndPublication);
```

**Step 6: Verify the file compiles**

```bash
dotnet build src/Umbraco.Core/Umbraco.Core.csproj --no-restore
```

**Expected:** Build succeeds.

**Step 7: Commit**

```bash
git add src/Umbraco.Core/Services/ContentService.cs
git commit -m "refactor(core): inject ContentPermissionManager into ContentService

Phase 6: Add constructor parameter and lazy fallback for ContentPermissionManager.

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>"
```

---

## Task 4: Update DI Registration to Pass ContentPermissionManager

**Files:**
- Modify: `src/Umbraco.Core/DependencyInjection/UmbracoBuilder.cs`

> **v1.3 Change:** Factory is in `Umbraco.Core/DependencyInjection/UmbracoBuilder.cs` (lines ~306-329), not Infrastructure. This matches where we registered ContentPermissionManager in Task 2.

**Step 1: Update the ContentService factory registration**

Find the ContentService factory registration in `UmbracoBuilder.cs` (around lines 306-329) and add the new parameter as the **23rd parameter** (matching the constructor order from Task 3):

```csharp
Services.AddUnique<IContentService>(sp =>
    new ContentService(
        sp.GetRequiredService<ICoreScopeProvider>(),           // 1
        sp.GetRequiredService<ILoggerFactory>(),               // 2
        sp.GetRequiredService<IEventMessagesFactory>(),        // 3
        sp.GetRequiredService<IDocumentRepository>(),          // 4
        sp.GetRequiredService<IEntityRepository>(),            // 5
        sp.GetRequiredService<IAuditService>(),                // 6
        sp.GetRequiredService<IContentTypeRepository>(),       // 7
        sp.GetRequiredService<IDocumentBlueprintRepository>(), // 8
        sp.GetRequiredService<ILanguageRepository>(),          // 9
        new Lazy<IPropertyValidationService>(() => sp.GetRequiredService<IPropertyValidationService>()), // 10
        sp.GetRequiredService<IShortStringHelper>(),           // 11
        sp.GetRequiredService<ICultureImpactFactory>(),        // 12
        sp.GetRequiredService<IUserIdKeyResolver>(),           // 13
        sp.GetRequiredService<PropertyEditorCollection>(),     // 14
        sp.GetRequiredService<IIdKeyMap>(),                    // 15
        sp.GetRequiredService<IOptionsMonitor<ContentSettings>>(), // 16
        sp.GetRequiredService<IRelationService>(),             // 17
        sp.GetRequiredService<IContentCrudService>(),          // 18
        sp.GetRequiredService<IContentQueryOperationService>(), // 19
        sp.GetRequiredService<IContentVersionOperationService>(), // 20
        sp.GetRequiredService<IContentMoveOperationService>(), // 21
        sp.GetRequiredService<IContentPublishOperationService>(), // 22
        sp.GetRequiredService<ContentPermissionManager>()));   // 23 - NEW Phase 6
```

**Step 2: Verify the registration compiles**

```bash
dotnet build src/Umbraco.Core/Umbraco.Core.csproj --no-restore
```

**Expected:** Build succeeds.

**Step 3: Commit**

```bash
git add src/Umbraco.Core/DependencyInjection/UmbracoBuilder.cs
git commit -m "chore(di): pass ContentPermissionManager to ContentService factory

Phase 6: Add 23rd constructor parameter for permission operations.

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>"
```

---

## Task 5: Delegate Permission Methods to ContentPermissionManager

**Files:**
- Modify: `src/Umbraco.Core/Services/ContentService.cs`

**Step 1: Replace SetPermissions implementation with delegation**

Find the `#region Permissions` section and replace the method implementations:

**Before:**
```csharp
public void SetPermissions(EntityPermissionSet permissionSet)
{
    using (ICoreScope scope = ScopeProvider.CreateCoreScope())
    {
        scope.WriteLock(Constants.Locks.ContentTree);
        _documentRepository.ReplaceContentPermissions(permissionSet);
        scope.Complete();
    }
}
```

**After:**
```csharp
public void SetPermissions(EntityPermissionSet permissionSet)
    => PermissionManager.SetPermissions(permissionSet);
```

**Step 2: Replace SetPermission implementation with delegation**

**Before:**
```csharp
public void SetPermission(IContent entity, string permission, IEnumerable<int> groupIds)
{
    using (ICoreScope scope = ScopeProvider.CreateCoreScope())
    {
        scope.WriteLock(Constants.Locks.ContentTree);
        _documentRepository.AssignEntityPermission(entity, permission, groupIds);
        scope.Complete();
    }
}
```

**After:**
```csharp
public void SetPermission(IContent entity, string permission, IEnumerable<int> groupIds)
    => PermissionManager.SetPermission(entity, permission, groupIds);
```

**Step 3: Replace GetPermissions implementation with delegation**

**Before:**
```csharp
public EntityPermissionCollection GetPermissions(IContent content)
{
    using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
    {
        scope.ReadLock(Constants.Locks.ContentTree);
        return _documentRepository.GetPermissionsForEntity(content.Id);
    }
}
```

**After:**
```csharp
public EntityPermissionCollection GetPermissions(IContent content)
    => PermissionManager.GetPermissions(content);
```

**Step 4: Verify the file compiles**

```bash
dotnet build src/Umbraco.Core/Umbraco.Core.csproj --no-restore
```

**Expected:** Build succeeds.

**Step 5: Commit**

```bash
git add src/Umbraco.Core/Services/ContentService.cs
git commit -m "refactor(core): delegate permission methods to ContentPermissionManager

Phase 6: SetPermissions, SetPermission, GetPermissions now delegate to internal manager.

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>"
```

---

## Task 6: Add Phase 6 DI Test

**Files:**
- Modify: `tests/Umbraco.Tests.Integration/Umbraco.Infrastructure/Services/ContentServiceRefactoringTests.cs`

**Step 1: Verify using directive**

The file should already have:

```csharp
using Umbraco.Cms.Core.Services;  // ContentPermissionManager is here
```

> **v1.2 Change:** ContentPermissionManager is in `Umbraco.Cms.Core.Services` namespace (same as other content services).

**Step 2: Add Phase 6 test region**

After the `#region Phase 5 - Publish Operation Tests` region, add:

```csharp
#region Phase 6 - Permission Manager Tests

/// <summary>
/// Phase 6 Test: Verifies ContentPermissionManager is registered and resolvable from DI.
/// </summary>
[Test]
public void ContentPermissionManager_CanBeResolvedFromDI()
{
    // Act
    var permissionManager = GetRequiredService<ContentPermissionManager>();

    // Assert
    Assert.That(permissionManager, Is.Not.Null);
    Assert.That(permissionManager, Is.InstanceOf<ContentPermissionManager>());
}

/// <summary>
/// Phase 6 Test: Verifies permission operations work via ContentService after delegation.
/// </summary>
[Test]
public async Task SetPermission_ViaContentService_DelegatesToPermissionManager()
{
    // Arrange
    var content = ContentBuilder.CreateSimpleContent(ContentType, "PermissionDelegationTest", -1);
    ContentService.Save(content);

    var adminGroup = await UserGroupService.GetAsync(Constants.Security.AdminGroupAlias);
    Assert.That(adminGroup, Is.Not.Null, "Admin group should exist");

    // Act - This should delegate to ContentPermissionManager
    ContentService.SetPermission(content, "F", new[] { adminGroup!.Id });

    // Assert - Verify it worked (via GetPermissions which also delegates)
    var permissions = ContentService.GetPermissions(content);
    var adminPermissions = permissions.FirstOrDefault(p => p.UserGroupId == adminGroup.Id);

    Assert.That(adminPermissions, Is.Not.Null, "Should have permissions for admin group");
    Assert.That(adminPermissions!.AssignedPermissions, Does.Contain("F"),
        "Admin group should have Browse permission");
}

#endregion
```

**Step 3: Verify the test compiles**

```bash
dotnet build tests/Umbraco.Tests.Integration/Umbraco.Tests.Integration.csproj --no-restore
```

**Expected:** Build succeeds.

**Step 4: Commit**

```bash
git add tests/Umbraco.Tests.Integration/Umbraco.Infrastructure/Services/ContentServiceRefactoringTests.cs
git commit -m "test(integration): add Phase 6 ContentPermissionManager DI tests

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>"
```

---

## Task 7: Run Phase Gate Tests

**Step 1: Run the refactoring-specific tests**

```bash
dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentServiceRefactoringTests" --no-build
```

**Expected:** All tests pass, including:
- The 4 existing permission tests (Tests 9-12)
- The 2 new Phase 6 tests

**Step 2: Run all ContentService tests**

```bash
dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentService" --no-build
```

**Expected:** All tests pass (no regressions).

**Step 3: Document results**

If any tests fail, follow the Regression Protocol from the design document:
1. STOP - Do not proceed
2. DIAGNOSE - Identify which behavior changed
3. FIX - Restore expected behavior
4. VERIFY - Re-run all tests
5. CONTINUE - Only after all tests pass

---

## Task 8: Final Commit and Tag

**Step 1: Create final commit if not already done**

```bash
git status
# If there are uncommitted changes, commit them
```

**Step 2: Create git tag for Phase 6**

```bash
git tag -a phase-6-permission-extraction -m "Phase 6 complete: ContentPermissionManager extracted"
```

**Step 3: Update design document**

Update `/home/yv01p/Umbraco-CMS/docs/plans/2025-12-19-contentservice-refactor-design.md`:

Change Phase 6 status from `Pending` to `✅ Complete`:

```markdown
| 6 | Permission Manager | All ContentService*Tests + Permission tests | All pass | ✅ Complete |
```

Add to Phase Details section:

```markdown
6. **Phase 6: Permission Manager** ✅ - Complete! Created:
   - `ContentPermissionManager.cs` - Internal manager class (~50 lines)
   - Updated `ContentService.cs` to delegate permission operations
   - Git tag: `phase-6-permission-extraction`
```

**Step 4: Commit the documentation update**

```bash
git add docs/plans/2025-12-19-contentservice-refactor-design.md
git commit -m "docs: mark Phase 6 complete in design document

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>"
```

---

## Summary

Phase 6 extracts permission operations into an internal `ContentPermissionManager` class:

| Task | Files Changed | Purpose |
|------|---------------|---------|
| 1 | Create `src/Umbraco.Core/Services/ContentPermissionManager.cs` | New internal manager class |
| 2 | Modify `src/Umbraco.Core/DependencyInjection/UmbracoBuilder.cs` | Register manager in DI (AddScoped) |
| 3 | Modify `src/Umbraco.Core/Services/ContentService.cs` | Add constructor parameter (position 23) |
| 4 | Modify `src/Umbraco.Core/DependencyInjection/UmbracoBuilder.cs` | Update ContentService factory |
| 5 | Modify `src/Umbraco.Core/Services/ContentService.cs` | Delegate methods to manager |
| 6 | Modify `tests/.../ContentServiceRefactoringTests.cs` | Add DI verification tests |
| 7 | Run tests | Verify no regressions |
| 8 | Tag and document | Complete phase |

> **v1.3 Note:** All file paths are in `Umbraco.Core`, not `Umbraco.Infrastructure`. This matches Phases 1-5.

**Expected Line Count Reduction:** ~15 lines removed from ContentService (replaced with 3 one-liner delegations).

**Risk Level:** Low - Permission operations are simple, isolated, and have comprehensive test coverage.

---

## v1.1 Changes Summary

Applied based on critical review 1 feedback:

| Issue | Resolution |
|-------|------------|
| **File location** | ~~Moved from `Umbraco.Core` to `Umbraco.Infrastructure`~~ (REVERTED in v1.2) |
| **ArgumentNullException pattern** | Changed to `ThrowIfNull()` for consistency with codebase patterns |
| **Input validation** | Added null/empty checks for all method parameters |
| **Logger usage** | Added `LogDebug` calls for security-relevant permission operations |
| **Return type materialization** | Added documentation note that `EntityPermissionCollection` is materialized |
| **Audit logging** | Deferred to optional future enhancement - LogDebug provides basic auditability |

---

## v1.2 Changes Summary

Applied based on critical review 2 feedback:

| Issue | Resolution |
|-------|------------|
| **BLOCKING: File location** | **REVERTED** to `Umbraco.Core/Services/` - the v1.1 Infrastructure placement was an architectural violation (Core cannot reference Infrastructure). All Phases 1-5 services are in Core. |
| **DI registration using** | Updated to use `Umbraco.Cms.Core.Services` namespace |
| **ContentService using** | Removed - same namespace, no using directive needed |
| **Test using** | Uses existing `Umbraco.Cms.Core.Services` namespace |
| **Permission validation** | Added `LogWarning` for non-single-character permission codes |
| **Summary table** | Updated to reflect Core location |

---

## v1.3 Changes Summary

Applied based on critical review 3 feedback:

| Issue | Resolution |
|-------|------------|
| **BLOCKING: Wrong DI file** | Tasks 2 & 4: Changed from `Infrastructure/DependencyInjection/UmbracoBuilder.CoreServices.cs` to `Core/DependencyInjection/UmbracoBuilder.cs` |
| **BLOCKING: Wrong directory** | Confirmed file path is `Services/ContentPermissionManager.cs` (no `/Content/` subdirectory) |
| **High: Constructor order** | Added explicit numbering - ContentPermissionManager is parameter 23 (after publishOperationService) |
| **High: Namespace** | Confirmed `Umbraco.Cms.Core.Services` throughout |
| **Medium: AddScoped vs AddUnique** | Added design note explaining why AddScoped is appropriate for internal class |
| **Minor: Commit messages** | Added Phase context to commit messages |
| **Minor: Summary table** | Updated with full file paths |

---

## Execution Options

**1. Subagent-Driven (this session)** - I dispatch fresh subagent per task, review between tasks, fast iteration

**2. Parallel Session (separate)** - Open new session with executing-plans, batch execution with checkpoints

**Which approach?**
