# Phase 7: ContentBlueprintManager Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-24 | Initial plan |
| 2.0 | 2025-12-24 | Applied critical review feedback: audit logging for DeleteBlueprint/DeleteBlueprintsOfTypes, scope fixes, early return patterns, debug logging, naming comments, additional test |
| 3.0 | 2025-12-24 | Applied v2 critical review: fix double enumeration bug, add read lock to GetBlueprintsForContentTypes, add empty array guard to DeleteBlueprintsOfTypes, remove dead code |

---

**Goal:** Extract blueprint operations (10 methods) from ContentService into a public ContentBlueprintManager class.

**Architecture:** The ContentBlueprintManager will be a public sealed class that encapsulates all blueprint-related operations. It will be registered as a scoped service in DI and injected into ContentService for delegation. Following the Phase 6 pattern, this is a "manager" class (not a service with interface) because blueprint operations are tightly coupled to content entities and don't require independent testability beyond ContentService tests.

**Tech Stack:** .NET 10.0, Umbraco.Core, NUnit for testing

---

## Pre-Implementation Checklist

Before starting, verify the baseline:

```bash
# Build to ensure current state compiles
dotnet build src/Umbraco.Core/Umbraco.Core.csproj --no-restore

# Run refactoring tests to establish baseline
dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentServiceRefactoringTests" --no-build

# Run all ContentService tests
dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentService" --no-build
```

**Expected:** All tests pass.

---

## Methods to Extract

Per the design document, the following 10 methods will be extracted:

| Method | Purpose | Notes |
|--------|---------|-------|
| `GetBlueprintById(int id)` | Get blueprint by int ID | Read-only |
| `GetBlueprintById(Guid id)` | Get blueprint by GUID | Read-only |
| `SaveBlueprint(IContent, int)` | Save blueprint (obsolete overload) | Delegates to 3-param version |
| `SaveBlueprint(IContent, IContent?, int)` | Save blueprint with source | Fires notifications |
| `DeleteBlueprint(IContent, int)` | Delete blueprint | Fires notifications, includes audit |
| `CreateBlueprintFromContent(IContent, string, int)` | Create content from blueprint | Uses language repository |
| `CreateContentFromBlueprint` | Obsolete alias | Delegates to CreateBlueprintFromContent |
| `GetBlueprintsForContentTypes(params int[])` | Get blueprints by type IDs | Query operation |
| `DeleteBlueprintsOfTypes(IEnumerable<int>, int)` | Delete blueprints by types | Bulk delete with notifications |
| `DeleteBlueprintsOfType(int, int)` | Delete blueprints by single type | Delegates to DeleteBlueprintsOfTypes |

---

## Task 1: Create ContentBlueprintManager Class

**Files:**
- Create: `src/Umbraco.Core/Services/ContentBlueprintManager.cs`

**Step 1: Verify target directory exists**

The file will be placed directly in `src/Umbraco.Core/Services/` to match the pattern from Phases 1-6:

```bash
ls src/Umbraco.Core/Services/ContentPermissionManager.cs  # Should exist from Phase 6
```

**Step 2: Create the ContentBlueprintManager class**

```csharp
// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Manager for content blueprint (template) operations.
/// </summary>
/// <remarks>
/// <para>
/// This class encapsulates blueprint operations extracted from ContentService
/// as part of the ContentService refactoring initiative (Phase 7).
/// </para>
/// <para>
/// <strong>Design Decision:</strong> This class is public for DI but not intended for direct external use:
/// <list type="bullet">
///   <item><description>Blueprint operations are tightly coupled to content entities</description></item>
///   <item><description>They don't require independent testability beyond ContentService tests</description></item>
///   <item><description>The public API remains through IContentService for backward compatibility</description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Notifications:</strong> Blueprint operations fire the following notifications:
/// <list type="bullet">
///   <item><description><see cref="ContentSavedBlueprintNotification"/> - after saving a blueprint</description></item>
///   <item><description><see cref="ContentDeletedBlueprintNotification"/> - after deleting blueprint(s)</description></item>
///   <item><description><see cref="ContentTreeChangeNotification"/> - after save/delete for cache invalidation</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed class ContentBlueprintManager
{
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly IDocumentBlueprintRepository _documentBlueprintRepository;
    private readonly ILanguageRepository _languageRepository;
    private readonly IContentTypeRepository _contentTypeRepository;
    private readonly IEventMessagesFactory _eventMessagesFactory;
    private readonly IAuditService _auditService;
    private readonly ILogger<ContentBlueprintManager> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentBlueprintManager"/> class.
    /// </summary>
    public ContentBlueprintManager(
        ICoreScopeProvider scopeProvider,
        IDocumentBlueprintRepository documentBlueprintRepository,
        ILanguageRepository languageRepository,
        IContentTypeRepository contentTypeRepository,
        IEventMessagesFactory eventMessagesFactory,
        IAuditService auditService,
        ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(scopeProvider);
        ArgumentNullException.ThrowIfNull(documentBlueprintRepository);
        ArgumentNullException.ThrowIfNull(languageRepository);
        ArgumentNullException.ThrowIfNull(contentTypeRepository);
        ArgumentNullException.ThrowIfNull(eventMessagesFactory);
        ArgumentNullException.ThrowIfNull(auditService);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        _scopeProvider = scopeProvider;
        _documentBlueprintRepository = documentBlueprintRepository;
        _languageRepository = languageRepository;
        _contentTypeRepository = contentTypeRepository;
        _eventMessagesFactory = eventMessagesFactory;
        _auditService = auditService;
        _logger = loggerFactory.CreateLogger<ContentBlueprintManager>();
    }

    private static readonly string?[] ArrayOfOneNullString = { null };

    /// <summary>
    /// Gets a blueprint by its integer ID.
    /// </summary>
    /// <param name="id">The blueprint ID.</param>
    /// <returns>The blueprint content, or null if not found.</returns>
    public IContent? GetBlueprintById(int id)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(Constants.Locks.ContentTree);

        IContent? blueprint = _documentBlueprintRepository.Get(id);
        if (blueprint is null)
        {
            return null;
        }

        blueprint.Blueprint = true;
        return blueprint;
    }

    /// <summary>
    /// Gets a blueprint by its GUID key.
    /// </summary>
    /// <param name="id">The blueprint GUID key.</param>
    /// <returns>The blueprint content, or null if not found.</returns>
    public IContent? GetBlueprintById(Guid id)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(Constants.Locks.ContentTree);

        IContent? blueprint = _documentBlueprintRepository.Get(id);
        if (blueprint is null)
        {
            return null;
        }

        blueprint.Blueprint = true;
        return blueprint;
    }

    /// <summary>
    /// Saves a blueprint.
    /// </summary>
    /// <param name="content">The blueprint content to save.</param>
    /// <param name="userId">The user ID performing the operation.</param>
    [Obsolete("Use SaveBlueprint(IContent, IContent?, int) instead. Scheduled for removal in V19.")]
    public void SaveBlueprint(IContent content, int userId = Constants.Security.SuperUserId)
        => SaveBlueprint(content, null, userId);

    /// <summary>
    /// Saves a blueprint with optional source content reference.
    /// </summary>
    /// <param name="content">The blueprint content to save.</param>
    /// <param name="createdFromContent">The source content the blueprint was created from, if any.</param>
    /// <param name="userId">The user ID performing the operation.</param>
    public void SaveBlueprint(IContent content, IContent? createdFromContent, int userId = Constants.Security.SuperUserId)
    {
        ArgumentNullException.ThrowIfNull(content);

        EventMessages evtMsgs = _eventMessagesFactory.Get();

        content.Blueprint = true;

        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        scope.WriteLock(Constants.Locks.ContentTree);

        if (content.HasIdentity == false)
        {
            content.CreatorId = userId;
        }

        content.WriterId = userId;

        _documentBlueprintRepository.Save(content);

        _auditService.Add(AuditType.Save, userId, content.Id, UmbracoObjectTypes.DocumentBlueprint.GetName(), $"Saved content template: {content.Name}");

        _logger.LogDebug("Saved blueprint {BlueprintId} ({BlueprintName})", content.Id, content.Name);

        scope.Notifications.Publish(new ContentSavedBlueprintNotification(content, createdFromContent, evtMsgs));
        scope.Notifications.Publish(new ContentTreeChangeNotification(content, TreeChangeTypes.RefreshNode, evtMsgs));

        scope.Complete();
    }

    /// <summary>
    /// Deletes a blueprint.
    /// </summary>
    /// <param name="content">The blueprint content to delete.</param>
    /// <param name="userId">The user ID performing the operation.</param>
    public void DeleteBlueprint(IContent content, int userId = Constants.Security.SuperUserId)
    {
        ArgumentNullException.ThrowIfNull(content);

        EventMessages evtMsgs = _eventMessagesFactory.Get();

        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        scope.WriteLock(Constants.Locks.ContentTree);
        _documentBlueprintRepository.Delete(content);

        // Audit deletion for security traceability (v2.0: added per critical review)
        _auditService.Add(AuditType.Delete, userId, content.Id, UmbracoObjectTypes.DocumentBlueprint.GetName(), $"Deleted content template: {content.Name}");

        _logger.LogDebug("Deleted blueprint {BlueprintId} ({BlueprintName})", content.Id, content.Name);

        scope.Notifications.Publish(new ContentDeletedBlueprintNotification(content, evtMsgs));
        scope.Notifications.Publish(new ContentTreeChangeNotification(content, TreeChangeTypes.Remove, evtMsgs));
        scope.Complete();
    }

    /// <summary>
    /// Creates a new content item from a blueprint template.
    /// </summary>
    /// <param name="blueprint">The blueprint to create content from.</param>
    /// <param name="name">The name for the new content.</param>
    /// <param name="userId">The user ID performing the operation.</param>
    /// <returns>A new unsaved content item populated from the blueprint.</returns>
    public IContent CreateContentFromBlueprint(
        IContent blueprint,
        string name,
        int userId = Constants.Security.SuperUserId)
    {
        ArgumentNullException.ThrowIfNull(blueprint);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        // v2.0: Use single scope for entire method (per critical review - avoids scope overhead)
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);

        IContentType contentType = GetContentTypeInternal(blueprint.ContentType.Alias);
        var content = new Content(name, -1, contentType);
        content.Path = string.Concat(content.ParentId.ToString(), ",", content.Id);

        content.CreatorId = userId;
        content.WriterId = userId;

        IEnumerable<string?> cultures = ArrayOfOneNullString;
        if (blueprint.CultureInfos?.Count > 0)
        {
            cultures = blueprint.CultureInfos.Values.Select(x => x.Culture);
            if (blueprint.CultureInfos.TryGetValue(_languageRepository.GetDefaultIsoCode(), out ContentCultureInfos defaultCulture))
            {
                defaultCulture.Name = name;
            }
        }

        DateTime now = DateTime.UtcNow;
        foreach (var culture in cultures)
        {
            foreach (IProperty property in blueprint.Properties)
            {
                var propertyCulture = property.PropertyType.VariesByCulture() ? culture : null;
                content.SetValue(property.Alias, property.GetValue(propertyCulture), propertyCulture);
            }

            if (!string.IsNullOrEmpty(culture))
            {
                content.SetCultureInfo(culture, blueprint.GetCultureName(culture), now);
            }
        }

        return content;
    }

    /// <summary>
    /// Gets all blueprints for the specified content type IDs.
    /// </summary>
    /// <param name="contentTypeId">The content type IDs to filter by (empty returns all).</param>
    /// <returns>Collection of blueprints.</returns>
    public IEnumerable<IContent> GetBlueprintsForContentTypes(params int[] contentTypeId)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        // v3.0: Added read lock to match GetBlueprintById pattern (per critical review)
        scope.ReadLock(Constants.Locks.ContentTree);

        IQuery<IContent> query = _scopeProvider.CreateQuery<IContent>();
        if (contentTypeId.Length > 0)
        {
            // Need to use a List here because the expression tree cannot convert the array when used in Contains.
            List<int> contentTypeIdsAsList = [.. contentTypeId];
            query.Where(x => contentTypeIdsAsList.Contains(x.ContentTypeId));
        }

        // v3.0: Materialize to array to avoid double enumeration bug (per critical review)
        // Calling .Count() on IEnumerable then returning it would cause double database query
        IContent[] blueprints = _documentBlueprintRepository.Get(query).Select(x =>
        {
            x.Blueprint = true;
            return x;
        }).ToArray();

        // v2.0: Added debug logging for consistency with other methods (per critical review)
        _logger.LogDebug("Retrieved {Count} blueprints for content types {ContentTypeIds}",
            blueprints.Length, contentTypeId.Length > 0 ? string.Join(", ", contentTypeId) : "(all)");

        return blueprints;
    }

    /// <summary>
    /// Deletes all blueprints of the specified content types.
    /// </summary>
    /// <param name="contentTypeIds">The content type IDs whose blueprints should be deleted.</param>
    /// <param name="userId">The user ID performing the operation.</param>
    /// <remarks>
    /// <para>
    /// <strong>Known Limitation:</strong> Blueprints are deleted one at a time in a loop.
    /// If there are many blueprints (e.g., 100+), this results in N separate delete operations.
    /// This matches the original ContentService behavior and is acceptable for Phase 7
    /// (behavior preservation). A bulk delete optimization could be added in a future phase
    /// if IDocumentBlueprintRepository is extended with a bulk delete method.
    /// </para>
    /// </remarks>
    public void DeleteBlueprintsOfTypes(IEnumerable<int> contentTypeIds, int userId = Constants.Security.SuperUserId)
    {
        ArgumentNullException.ThrowIfNull(contentTypeIds);

        // v3.0: Guard against accidental deletion of all blueprints (per critical review)
        // An empty array means "delete blueprints of no types" = do nothing (not "delete all")
        var contentTypeIdsAsList = contentTypeIds.ToList();
        if (contentTypeIdsAsList.Count == 0)
        {
            _logger.LogDebug("DeleteBlueprintsOfTypes called with empty contentTypeIds, no action taken");
            return;
        }

        EventMessages evtMsgs = _eventMessagesFactory.Get();

        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        scope.WriteLock(Constants.Locks.ContentTree);

        IQuery<IContent> query = _scopeProvider.CreateQuery<IContent>();
        query.Where(x => contentTypeIdsAsList.Contains(x.ContentTypeId));

        IContent[]? blueprints = _documentBlueprintRepository.Get(query)?.Select(x =>
        {
            x.Blueprint = true;
            return x;
        }).ToArray();

        // v2.0: Early return with scope.Complete() to ensure scope completes in all paths (per critical review)
        if (blueprints is null || blueprints.Length == 0)
        {
            scope.Complete();
            return;
        }

        foreach (IContent blueprint in blueprints)
        {
            _documentBlueprintRepository.Delete(blueprint);
        }

        // v2.0: Added audit logging for security traceability (per critical review)
        _auditService.Add(AuditType.Delete, userId, -1, UmbracoObjectTypes.DocumentBlueprint.GetName(),
            $"Deleted {blueprints.Length} content template(s) for content types: {string.Join(", ", contentTypeIdsAsList)}");

        _logger.LogDebug("Deleted {Count} blueprints for content types {ContentTypeIds}",
            blueprints.Length, string.Join(", ", contentTypeIdsAsList));

        scope.Notifications.Publish(new ContentDeletedBlueprintNotification(blueprints, evtMsgs));
        scope.Notifications.Publish(new ContentTreeChangeNotification(blueprints, TreeChangeTypes.Remove, evtMsgs));
        scope.Complete();
    }

    /// <summary>
    /// Deletes all blueprints of the specified content type.
    /// </summary>
    /// <param name="contentTypeId">The content type ID whose blueprints should be deleted.</param>
    /// <param name="userId">The user ID performing the operation.</param>
    public void DeleteBlueprintsOfType(int contentTypeId, int userId = Constants.Security.SuperUserId) =>
        DeleteBlueprintsOfTypes(new[] { contentTypeId }, userId);

    /// <summary>
    /// Gets the content type by alias, throwing if not found.
    /// </summary>
    /// <remarks>
    /// This is an internal helper that assumes a scope is already active.
    /// </remarks>
    private IContentType GetContentTypeInternal(string alias)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(alias);

        IContentType? contentType = _contentTypeRepository.Get(alias);

        if (contentType == null)
        {
            throw new InvalidOperationException($"Content type with alias '{alias}' not found.");
        }

        return contentType;
    }

    // v3.0: Removed unused GetContentType(string) method (per critical review - dead code)
}
```

**Step 3: Verify the file compiles**

```bash
dotnet build src/Umbraco.Core/Umbraco.Core.csproj --no-restore
```

**Expected:** Build succeeds.

**Step 4: Commit**

```bash
git add src/Umbraco.Core/Services/ContentBlueprintManager.cs
git commit -m "$(cat <<'EOF'
feat(core): add ContentBlueprintManager for Phase 7 extraction

Phase 7: Blueprint manager with 10 methods:
- GetBlueprintById (int/Guid overloads)
- SaveBlueprint (with obsolete overload)
- DeleteBlueprint (with audit logging)
- CreateContentFromBlueprint
- GetBlueprintsForContentTypes (with debug logging)
- DeleteBlueprintsOfTypes/DeleteBlueprintsOfType (with audit logging)

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
EOF
)"
```

---

## Task 2: Register ContentBlueprintManager in DI

**Files:**
- Modify: `src/Umbraco.Core/DependencyInjection/UmbracoBuilder.cs`

**Step 1: Find the correct location**

Find the service registrations section (around line 307, after `Services.AddScoped<ContentPermissionManager>()` from Phase 6) and add:

```csharp
// Phase 7: Internal blueprint manager (AddScoped, not AddUnique, because it's internal without interface)
Services.AddScoped<ContentBlueprintManager>();
```

> **Design Note:** We use `AddScoped` (matching Phase 6 pattern) because:
> - ContentBlueprintManager is a concrete class without an interface (per design document)
> - Scoped lifetime matches ContentService's request-scoped usage patterns

**Step 2: Verify the registration compiles**

```bash
dotnet build src/Umbraco.Core/Umbraco.Core.csproj --no-restore
```

**Expected:** Build succeeds.

**Step 3: Commit**

```bash
git add src/Umbraco.Core/DependencyInjection/UmbracoBuilder.cs
git commit -m "$(cat <<'EOF'
chore(di): register ContentBlueprintManager as scoped service

Phase 7: Internal blueprint manager with scoped lifetime.

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
EOF
)"
```

---

## Task 3: Add ContentBlueprintManager to ContentService Constructor

**Files:**
- Modify: `src/Umbraco.Core/Services/ContentService.cs`

**Step 1: Add private fields for ContentBlueprintManager**

In the field declarations section (after the `_permissionManagerLazy` field), add:

```csharp
// Blueprint manager field (for Phase 7 extracted blueprint operations)
private readonly ContentBlueprintManager? _blueprintManager;
private readonly Lazy<ContentBlueprintManager>? _blueprintManagerLazy;
```

**Step 2: Add property accessor**

After the PermissionManager property, add:

```csharp
/// <summary>
/// Gets the blueprint manager.
/// </summary>
/// <exception cref="InvalidOperationException">Thrown if the manager was not properly initialized.</exception>
private ContentBlueprintManager BlueprintManager =>
    _blueprintManager ?? _blueprintManagerLazy?.Value
    ?? throw new InvalidOperationException("BlueprintManager not initialized. Ensure the manager is properly injected via constructor.");
```

**Step 3: Update the primary constructor (ActivatorUtilitiesConstructor)**

Add the new parameter to the primary constructor as the last parameter (after `permissionManager`):

```csharp
ContentBlueprintManager blueprintManager)  // NEW PARAMETER - Phase 7 blueprint operations
```

And in the constructor body, add:

```csharp
// Phase 7: Blueprint manager (direct injection)
ArgumentNullException.ThrowIfNull(blueprintManager);
_blueprintManager = blueprintManager;
_blueprintManagerLazy = null;  // Not needed when directly injected
```

**Step 4: Update the obsolete constructors**

For each obsolete constructor, add lazy resolution:

```csharp
// Phase 7: Lazy resolution of ContentBlueprintManager
_blueprintManagerLazy = new Lazy<ContentBlueprintManager>(() =>
    StaticServiceProvider.Instance.GetRequiredService<ContentBlueprintManager>(),
    LazyThreadSafetyMode.ExecutionAndPublication);
```

**Step 5: Verify the file compiles**

```bash
dotnet build src/Umbraco.Core/Umbraco.Core.csproj --no-restore
```

**Expected:** Build succeeds.

**Step 6: Commit**

```bash
git add src/Umbraco.Core/Services/ContentService.cs
git commit -m "$(cat <<'EOF'
refactor(core): inject ContentBlueprintManager into ContentService

Phase 7: Add constructor parameter and lazy fallback for ContentBlueprintManager.

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
EOF
)"
```

---

## Task 4: Update DI Registration to Pass ContentBlueprintManager

**Files:**
- Modify: `src/Umbraco.Core/DependencyInjection/UmbracoBuilder.cs`

**Step 1: Update the ContentService factory registration**

Find the ContentService factory registration and add the new parameter as the last parameter:

```csharp
sp.GetRequiredService<ContentBlueprintManager>()));   // NEW Phase 7
```

The full factory should now look like (abridged):

```csharp
Services.AddUnique<IContentService>(sp =>
    new ContentService(
        // ... parameters 1-22 ...
        sp.GetRequiredService<ContentPermissionManager>(),   // Phase 6
        sp.GetRequiredService<ContentBlueprintManager>()));  // Phase 7 - NEW
```

**Step 2: Verify the registration compiles**

```bash
dotnet build src/Umbraco.Core/Umbraco.Core.csproj --no-restore
```

**Expected:** Build succeeds.

**Step 3: Commit**

```bash
git add src/Umbraco.Core/DependencyInjection/UmbracoBuilder.cs
git commit -m "$(cat <<'EOF'
chore(di): pass ContentBlueprintManager to ContentService factory

Phase 7: Add final constructor parameter for blueprint operations.

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
EOF
)"
```

---

## Task 5: Delegate Blueprint Methods to ContentBlueprintManager

**Files:**
- Modify: `src/Umbraco.Core/Services/ContentService.cs`

**Step 1: Replace GetBlueprintById(int) with delegation**

**Before:**
```csharp
public IContent? GetBlueprintById(int id)
{
    using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
    {
        scope.ReadLock(Constants.Locks.ContentTree);
        IContent? blueprint = _documentBlueprintRepository.Get(id);
        if (blueprint != null)
        {
            blueprint.Blueprint = true;
        }

        return blueprint;
    }
}
```

**After:**
```csharp
public IContent? GetBlueprintById(int id)
    => BlueprintManager.GetBlueprintById(id);
```

**Step 2: Replace GetBlueprintById(Guid) with delegation**

**After:**
```csharp
public IContent? GetBlueprintById(Guid id)
    => BlueprintManager.GetBlueprintById(id);
```

**Step 3: Replace SaveBlueprint methods with delegation**

**After:**
```csharp
public void SaveBlueprint(IContent content, int userId = Constants.Security.SuperUserId)
    => BlueprintManager.SaveBlueprint(content, userId);

public void SaveBlueprint(IContent content, IContent? createdFromContent, int userId = Constants.Security.SuperUserId)
    => BlueprintManager.SaveBlueprint(content, createdFromContent, userId);
```

**Step 4: Replace DeleteBlueprint with delegation**

**After:**
```csharp
public void DeleteBlueprint(IContent content, int userId = Constants.Security.SuperUserId)
    => BlueprintManager.DeleteBlueprint(content, userId);
```

**Step 5: Replace CreateBlueprintFromContent with delegation**

> **Note (v2.0):** The naming here is intentionally confusing due to historical API evolution.
> `CreateBlueprintFromContent` in ContentService delegates to `CreateContentFromBlueprint` in the manager.
> The manager method name reflects what it actually does: creates content FROM a blueprint.
> The ContentService method name is preserved for backward compatibility.

**After:**
```csharp
/// <remarks>
/// Note: This method name is historically confusing. It creates content FROM a blueprint,
/// not a blueprint from content. The manager method is named correctly (CreateContentFromBlueprint).
/// This method name is preserved for backward compatibility.
/// </remarks>
public IContent CreateBlueprintFromContent(
    IContent blueprint,
    string name,
    int userId = Constants.Security.SuperUserId)
    => BlueprintManager.CreateContentFromBlueprint(blueprint, name, userId);
```

**Step 6: Keep CreateContentFromBlueprint obsolete method as-is**

This method already delegates to CreateBlueprintFromContent, so it will now delegate through the chain:

```csharp
[Obsolete("Use IContentBlueprintEditingService.GetScaffoldedAsync() instead. Scheduled for removal in V18.")]
public IContent CreateContentFromBlueprint(IContent blueprint, string name, int userId = Constants.Security.SuperUserId)
    => CreateBlueprintFromContent(blueprint, name, userId);
```

**Step 7: Replace GetBlueprintsForContentTypes with delegation**

**After:**
```csharp
public IEnumerable<IContent> GetBlueprintsForContentTypes(params int[] contentTypeId)
    => BlueprintManager.GetBlueprintsForContentTypes(contentTypeId);
```

**Step 8: Replace DeleteBlueprintsOfTypes with delegation**

**After:**
```csharp
public void DeleteBlueprintsOfTypes(IEnumerable<int> contentTypeIds, int userId = Constants.Security.SuperUserId)
    => BlueprintManager.DeleteBlueprintsOfTypes(contentTypeIds, userId);
```

**Step 9: Replace DeleteBlueprintsOfType with delegation**

**After:**
```csharp
public void DeleteBlueprintsOfType(int contentTypeId, int userId = Constants.Security.SuperUserId)
    => BlueprintManager.DeleteBlueprintsOfType(contentTypeId, userId);
```

**Step 10: Verify ArrayOfOneNullString has no other usages**

Before removing this field, verify no other code in ContentService depends on it:

```bash
# v3.0: Made this an explicit verification step (per critical review)
grep -n "ArrayOfOneNullString" src/Umbraco.Core/Services/ContentService.cs
```

**Expected:** Only ONE match - the field declaration itself. If there are other usages, investigate before removing.

**Step 11: Remove the static ArrayOfOneNullString field**

After verifying no other usages, remove the field from ContentService:

```csharp
// REMOVE THIS LINE from ContentService:
private static readonly string?[] ArrayOfOneNullString = { null };
```

**Step 12: Verify the file compiles**

```bash
dotnet build src/Umbraco.Core/Umbraco.Core.csproj --no-restore
```

**Expected:** Build succeeds.

**Step 13: Commit**

```bash
git add src/Umbraco.Core/Services/ContentService.cs
git commit -m "$(cat <<'EOF'
refactor(core): delegate blueprint methods to ContentBlueprintManager

Phase 7: All 10 blueprint methods now delegate to ContentBlueprintManager:
- GetBlueprintById (int/Guid)
- SaveBlueprint (2 overloads)
- DeleteBlueprint
- CreateBlueprintFromContent
- GetBlueprintsForContentTypes
- DeleteBlueprintsOfTypes/DeleteBlueprintsOfType

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
EOF
)"
```

---

## Task 6: Add Phase 7 DI Test

**Files:**
- Modify: `tests/Umbraco.Tests.Integration/Umbraco.Infrastructure/Services/ContentServiceRefactoringTests.cs`

**Step 1: Add Phase 7 test region**

After the `#region Phase 6 - Permission Manager Tests` region, add:

```csharp
#region Phase 7 - Blueprint Manager Tests

/// <summary>
/// Phase 7 Test: Verifies ContentBlueprintManager is registered and resolvable from DI.
/// </summary>
[Test]
public void ContentBlueprintManager_CanBeResolvedFromDI()
{
    // Act
    var blueprintManager = GetRequiredService<ContentBlueprintManager>();

    // Assert
    Assert.That(blueprintManager, Is.Not.Null);
    Assert.That(blueprintManager, Is.InstanceOf<ContentBlueprintManager>());
}

/// <summary>
/// Phase 7 Test: Verifies ContentBlueprintManager can be used directly without going through ContentService.
/// This validates the manager is independently functional (v2.0: added per critical review).
/// </summary>
[Test]
public void ContentBlueprintManager_CanBeUsedDirectly()
{
    // Arrange
    var blueprintManager = GetRequiredService<ContentBlueprintManager>();
    var blueprint = ContentBuilder.CreateSimpleContent(ContentType, "DirectManagerBlueprint", -1);

    // Act - Use manager directly, not through ContentService
    blueprintManager.SaveBlueprint(blueprint, null, Constants.Security.SuperUserId);

    // Assert
    Assert.That(blueprint.Blueprint, Is.True, "Content should be marked as blueprint");
    Assert.That(blueprint.HasIdentity, Is.True, "Blueprint should have been saved");

    // Retrieve directly through manager
    var retrieved = blueprintManager.GetBlueprintById(blueprint.Id);
    Assert.That(retrieved, Is.Not.Null, "Blueprint should be retrievable via manager");
    Assert.That(retrieved!.Name, Is.EqualTo("DirectManagerBlueprint"));
}

/// <summary>
/// Phase 7 Test: Verifies blueprint operations work via ContentService after delegation.
/// </summary>
[Test]
public void SaveBlueprint_ViaContentService_DelegatesToBlueprintManager()
{
    // Arrange
    var blueprint = ContentBuilder.CreateSimpleContent(ContentType, "TestBlueprint", -1);

    // Act - This should delegate to ContentBlueprintManager
    ContentService.SaveBlueprint(blueprint);

    // Assert - Verify it was saved as a blueprint
    Assert.That(blueprint.Blueprint, Is.True, "Content should be marked as blueprint");
    Assert.That(blueprint.HasIdentity, Is.True, "Blueprint should have been saved");

    // Retrieve and verify
    var retrieved = ContentService.GetBlueprintById(blueprint.Id);
    Assert.That(retrieved, Is.Not.Null, "Blueprint should be retrievable");
    Assert.That(retrieved!.Blueprint, Is.True, "Retrieved content should be marked as blueprint");
    Assert.That(retrieved.Name, Is.EqualTo("TestBlueprint"));
}

/// <summary>
/// Phase 7 Test: Verifies DeleteBlueprint works via ContentService.
/// </summary>
[Test]
public void DeleteBlueprint_ViaContentService_DelegatesToBlueprintManager()
{
    // Arrange
    var blueprint = ContentBuilder.CreateSimpleContent(ContentType, "BlueprintToDelete", -1);
    ContentService.SaveBlueprint(blueprint);
    var blueprintId = blueprint.Id;

    Assert.That(ContentService.GetBlueprintById(blueprintId), Is.Not.Null, "Blueprint should exist before delete");

    // Act
    ContentService.DeleteBlueprint(blueprint);

    // Assert
    Assert.That(ContentService.GetBlueprintById(blueprintId), Is.Null, "Blueprint should be deleted");
}

/// <summary>
/// Phase 7 Test: Verifies GetBlueprintsForContentTypes works via ContentService.
/// </summary>
[Test]
public void GetBlueprintsForContentTypes_ViaContentService_DelegatesToBlueprintManager()
{
    // Arrange
    var blueprint1 = ContentBuilder.CreateSimpleContent(ContentType, "Blueprint1", -1);
    var blueprint2 = ContentBuilder.CreateSimpleContent(ContentType, "Blueprint2", -1);
    ContentService.SaveBlueprint(blueprint1);
    ContentService.SaveBlueprint(blueprint2);

    // Act
    var blueprints = ContentService.GetBlueprintsForContentTypes(ContentType.Id).ToList();

    // Assert
    Assert.That(blueprints.Count, Is.GreaterThanOrEqualTo(2), "Should find at least 2 blueprints");
    Assert.That(blueprints.All(b => b.Blueprint), Is.True, "All returned items should be blueprints");
}

#endregion
```

**Step 2: Verify the test compiles**

```bash
dotnet build tests/Umbraco.Tests.Integration/Umbraco.Tests.Integration.csproj --no-restore
```

**Expected:** Build succeeds.

**Step 3: Commit**

```bash
git add tests/Umbraco.Tests.Integration/Umbraco.Infrastructure/Services/ContentServiceRefactoringTests.cs
git commit -m "$(cat <<'EOF'
test(integration): add Phase 7 ContentBlueprintManager DI tests

Phase 7: 5 integration tests for blueprint manager:
- DI resolution
- Direct manager usage (without ContentService)
- SaveBlueprint delegation
- DeleteBlueprint delegation
- GetBlueprintsForContentTypes delegation

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
EOF
)"
```

---

## Task 7: Run Phase Gate Tests

**Step 1: Run the refactoring-specific tests**

```bash
dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentServiceRefactoringTests" --no-build
```

**Expected:** All tests pass, including the 5 new Phase 7 tests.

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

**Step 2: Create git tag for Phase 7**

```bash
git tag -a phase-7-blueprint-extraction -m "Phase 7 complete: ContentBlueprintManager extracted"
```

**Step 3: Update design document**

Update `/home/yv01p/Umbraco-CMS/docs/plans/2025-12-19-contentservice-refactor-design.md`:

Change Phase 7 status from `Pending` to `✅ Complete`:

```markdown
| 7 | Blueprint Manager | All ContentService*Tests | All pass | ✅ Complete |
```

Add to Phase Details section:

```markdown
7. **Phase 7: Blueprint Manager** ✅ - Complete! Created:
   - `ContentBlueprintManager.cs` - Public sealed class (~280 lines)
   - 10 methods: GetBlueprintById (2), SaveBlueprint (2), DeleteBlueprint, CreateContentFromBlueprint, GetBlueprintsForContentTypes, DeleteBlueprintsOfTypes, DeleteBlueprintsOfType
   - Includes audit logging for delete operations (v2.0 enhancement)
   - Updated `ContentService.cs` to delegate blueprint operations
   - Git tag: `phase-7-blueprint-extraction`
```

**Step 4: Commit the documentation update**

```bash
git add docs/plans/2025-12-19-contentservice-refactor-design.md
git commit -m "$(cat <<'EOF'
docs: mark Phase 7 complete in design document

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
EOF
)"
```

---

## Summary

Phase 7 extracts blueprint operations into a public `ContentBlueprintManager` class:

| Task | Files Changed | Purpose |
|------|---------------|---------|
| 1 | Create `src/Umbraco.Core/Services/ContentBlueprintManager.cs` | New manager class (~280 lines) |
| 2 | Modify `src/Umbraco.Core/DependencyInjection/UmbracoBuilder.cs` | Register manager in DI (AddScoped) |
| 3 | Modify `src/Umbraco.Core/Services/ContentService.cs` | Add constructor parameter |
| 4 | Modify `src/Umbraco.Core/DependencyInjection/UmbracoBuilder.cs` | Update ContentService factory |
| 5 | Modify `src/Umbraco.Core/Services/ContentService.cs` | Delegate 10 methods to manager |
| 6 | Modify `tests/.../ContentServiceRefactoringTests.cs` | Add 5 DI verification tests |
| 7 | Run tests | Verify no regressions |
| 8 | Tag and document | Complete phase |

**Expected Line Count Reduction:** ~190 lines removed from ContentService (replaced with 10 one-liner delegations).

**Risk Level:** Low - Blueprint operations are isolated, don't participate in transactions with other operations, and have straightforward notification patterns.

---

## v2.0 Changes Summary

The following improvements were applied based on critical review feedback:

| Issue | Fix Applied |
|-------|-------------|
| Missing audit for DeleteBlueprint | Added `_auditService.Add(AuditType.Delete, ...)` |
| Missing audit for DeleteBlueprintsOfTypes | Added `_auditService.Add(AuditType.Delete, ...)` with userId |
| Scope not completing in DeleteBlueprintsOfTypes when blueprints is null | Added early return with `scope.Complete()` |
| Unnecessary inner scope in CreateContentFromBlueprint | Refactored to single scope for entire method |
| GetBlueprintById nesting | Refactored to early return pattern |
| Missing logging in GetBlueprintsForContentTypes | Added debug logging |
| Confusing CreateBlueprintFromContent naming | Added explanatory remarks comment |
| No test for direct manager usage | Added `ContentBlueprintManager_CanBeUsedDirectly` test |
| N+1 delete in DeleteBlueprintsOfTypes | Documented as known limitation in remarks |

---

## v3.0 Changes Summary

The following improvements were applied based on v2 critical review feedback:

| Issue | Fix Applied |
|-------|-------------|
| Double enumeration bug in GetBlueprintsForContentTypes | Materialize to `IContent[]` before logging `.Length` |
| Missing read lock in GetBlueprintsForContentTypes | Added `scope.ReadLock(Constants.Locks.ContentTree)` to match GetBlueprintById |
| Empty contentTypeIds deletes ALL blueprints | Added early return guard when `contentTypeIdsAsList.Count == 0` |
| Unused GetContentType method (dead code) | Removed the unused method, kept only GetContentTypeInternal |
| ArrayOfOneNullString verification was only a note | Made it an explicit Step 10 with expected output |

---

## Execution Options

**1. Subagent-Driven (this session)** - I dispatch fresh subagent per task, review between tasks, fast iteration

**2. Parallel Session (separate)** - Open new session with executing-plans, batch execution with checkpoints

**Which approach?**
