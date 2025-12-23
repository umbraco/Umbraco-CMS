# ContentService Refactoring Phase 2: Query Service Implementation Plan

## Version History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2025-12-22 | Claude | Initial plan creation |
| 1.1 | 2025-12-22 | Claude | Applied critical review feedback: documented implementation location as tech debt, fixed test assertions to use precise values, added null check for contentTypeIds, removed unused logger field, corrected DI registration file reference, added lazy evaluation remarks to GetByLevel, added edge case tests, improved test method naming to behavior-focused, added XML doc clarifications for non-existent IDs |
| 1.2 | 2025-12-22 | Claude | Applied critical review 2 feedback: documented scope lifetime as follow-up task, added obsolete constructor support with lazy resolution, changed DI registration to AddUnique, added DI factory verification step, added trashed content behavior docs, added missing tests (CountDescendants, GetPagedOfType edge case, CountPublished) |
| 1.3 | 2025-12-22 | Claude | Applied critical review 3 feedback: added explicit ContentService factory DI registration update, added typed logger for consistency with Phase 1, completed Task 4 constructor code with defensive null handling, added default ordering constant, added performance notes for List conversion, clarified parameter order decision |

---

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Extract content query operations (Count, GetByLevel, GetPagedOfType/s) into a focused IContentQueryOperationService.

**Architecture:** Follows Phase 1 patterns - interface in Umbraco.Core, implementation inherits from ContentServiceBase, ContentService facade delegates to the new service. Read-only operations, low risk.

> **Note on Implementation Location:** Per architectural best practices, implementations should be in `Umbraco.Infrastructure`, not `Umbraco.Core`. However, Phase 1 placed `ContentCrudService` in `Umbraco.Core` for consistency with the existing `ContentService` location. This phase follows the same pattern for consistency. This is documented as technical debt to be addressed in a future cleanup when the full refactoring is complete.

**Tech Stack:** .NET 10, C# 13, NUnit 3, Umbraco scoping/repository patterns

---

## Pre-Implementation Notes

### Naming Decision: IContentQueryOperationService

An `IContentQueryService` already exists in `Umbraco.Cms.Core.Services.Querying` namespace (a higher-level async API service). To avoid collision, this interface is named `IContentQueryOperationService`, following the same pattern used for `IContentPublishOperationService`.

### Scope Clarification

**Methods for IContentQueryOperationService (not already in IContentCrudService):**
- `Count(string? contentTypeAlias = null)`
- `CountPublished(string? contentTypeAlias = null)`
- `CountChildren(int parentId, string? contentTypeAlias = null)`
- `CountDescendants(int parentId, string? contentTypeAlias = null)`
- `GetByLevel(int level)`
- `GetPagedOfType(int contentTypeId, ...)`
- `GetPagedOfTypes(int[] contentTypeIds, ...)`

**Already in IContentCrudService (Phase 1):** GetAncestors, GetPagedChildren, GetPagedDescendants, HasChildren, Exists

### Dependency Flow

```
ContentService (Facade)
    │
    ├──► IContentCrudService (Phase 1)
    │
    └──► IContentQueryOperationService (Phase 2) ◄── This phase
              │
              └──► DocumentRepository (via ContentServiceBase)
```

### Design Decisions (Resolved from Review Feedback)

**Constructor Parameter Order:** `IContentQueryOperationService` is placed **after** `IContentCrudService` in the primary constructor for logical grouping of extracted services.

**Logger Pattern:** Typed logger `ILogger<ContentQueryOperationService>` is included for consistency with Phase 1's `ContentCrudService` pattern, even if not immediately used. This provides infrastructure for future debugging, performance monitoring, or error tracking without requiring constructor changes.

**Default Ordering Constant:** A static readonly `DefaultSortOrdering` constant is used to avoid repeating `Ordering.By("sortOrder")` across multiple methods (DRY principle).

### Known Issue: Scope Lifetime in GetByLevel

> **Follow-up Task Required:** The `GetByLevel` method returns an `IEnumerable<IContent>` that may be lazily evaluated. The scope is disposed when the method returns, but if `DocumentRepository.Get(query)` returns a lazy enumerable, enumeration after scope disposal could cause errors. This matches the existing `ContentService.GetByLevel` behavior for behavioral parity, but should be investigated as a potential latent bug across all services. Create a follow-up task to verify whether `DocumentRepository.Get()` is lazy and, if so, whether this causes issues in practice.

### Files to Create/Modify

| Action | File | Est. Lines |
|--------|------|------------|
| Create | `src/Umbraco.Core/Services/IContentQueryOperationService.cs` | ~80 |
| Create | `src/Umbraco.Core/Services/ContentQueryOperationService.cs` | ~150 |
| Modify | `src/Umbraco.Core/Services/ContentService.cs` | ~15 (delegation) |
| Create | `tests/.../Services/ContentQueryOperationServiceTests.cs` | ~200 |

---

## Task 1: Create IContentQueryOperationService Interface

**Files:**
- Create: `src/Umbraco.Core/Services/IContentQueryOperationService.cs`

**Step 1: Write the failing test**

Create test file first to verify interface doesn't exist yet.

```csharp
// tests/Umbraco.Tests.UnitTests/Umbraco.Core/Services/ContentQueryOperationServiceInterfaceTests.cs
using NUnit.Framework;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services;

[TestFixture]
public class ContentQueryOperationServiceInterfaceTests
{
    [Test]
    public void IContentQueryOperationService_Interface_Exists()
    {
        // Arrange & Act
        var interfaceType = typeof(IContentQueryOperationService);

        // Assert
        Assert.That(interfaceType, Is.Not.Null);
        Assert.That(interfaceType.IsInterface, Is.True);
    }

    [Test]
    public void IContentQueryOperationService_Extends_IService()
    {
        // Arrange
        var interfaceType = typeof(IContentQueryOperationService);

        // Act & Assert
        Assert.That(typeof(IService).IsAssignableFrom(interfaceType), Is.True);
    }
}
```

**Step 2: Run test to verify it fails**

Run: `dotnet test tests/Umbraco.Tests.UnitTests --filter "FullyQualifiedName~ContentQueryOperationServiceInterfaceTests" -v n`
Expected: FAIL with "type or namespace 'IContentQueryOperationService' could not be found"

**Step 3: Create the interface**

```csharp
// src/Umbraco.Core/Services/IContentQueryOperationService.cs
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Service for content query operations (counting, filtering by type/level).
/// </summary>
/// <remarks>
/// <para>
/// <strong>Implementation Note:</strong> Do not implement this interface directly.
/// Instead, inherit from <see cref="ContentServiceBase"/> which provides required
/// infrastructure (scoping, repository access, auditing). Direct implementation
/// without this base class will result in missing functionality.
/// </para>
/// <para>
/// This interface is part of the ContentService refactoring initiative (Phase 2).
/// It extracts query operations into a focused, testable service.
/// </para>
/// <para>
/// <strong>Versioning Policy:</strong> This interface follows additive-only changes.
/// New methods may be added with default implementations. Existing methods will not
/// be removed or have signatures changed without a 2 major version deprecation period.
/// </para>
/// <para>
/// <strong>Version History:</strong>
/// <list type="bullet">
///   <item><description>v1.0 (Phase 2): Initial interface with Count, GetByLevel, GetPagedOfType operations</description></item>
/// </list>
/// </para>
/// </remarks>
/// <since>1.0</since>
public interface IContentQueryOperationService : IService
{
    #region Count Operations

    /// <summary>
    /// Counts content items, optionally filtered by content type.
    /// </summary>
    /// <param name="contentTypeAlias">Optional content type alias to filter by. If the alias doesn't exist, returns 0.</param>
    /// <returns>The count of matching content items (includes trashed items).</returns>
    int Count(string? contentTypeAlias = null);

    /// <summary>
    /// Counts published content items, optionally filtered by content type.
    /// </summary>
    /// <param name="contentTypeAlias">Optional content type alias to filter by. If the alias doesn't exist, returns 0.</param>
    /// <returns>The count of matching published content items.</returns>
    int CountPublished(string? contentTypeAlias = null);

    /// <summary>
    /// Counts children of a parent, optionally filtered by content type.
    /// </summary>
    /// <param name="parentId">The parent content id. If the parent doesn't exist, returns 0.</param>
    /// <param name="contentTypeAlias">Optional content type alias to filter by. If the alias doesn't exist, returns 0.</param>
    /// <returns>The count of matching child content items.</returns>
    int CountChildren(int parentId, string? contentTypeAlias = null);

    /// <summary>
    /// Counts descendants of an ancestor, optionally filtered by content type.
    /// </summary>
    /// <param name="parentId">The ancestor content id. If the ancestor doesn't exist, returns 0.</param>
    /// <param name="contentTypeAlias">Optional content type alias to filter by. If the alias doesn't exist, returns 0.</param>
    /// <returns>The count of matching descendant content items.</returns>
    int CountDescendants(int parentId, string? contentTypeAlias = null);

    #endregion

    #region Hierarchy Queries

    /// <summary>
    /// Gets content items at a specific tree level.
    /// </summary>
    /// <param name="level">The tree level (1 = root children, 2 = grandchildren, etc.).</param>
    /// <returns>Content items at the specified level, excluding trashed items.</returns>
    IEnumerable<IContent> GetByLevel(int level);

    #endregion

    #region Paged Type Queries

    /// <summary>
    /// Gets paged content items of a specific content type.
    /// </summary>
    /// <param name="contentTypeId">The content type id. If the content type doesn't exist, returns empty results with totalRecords = 0.</param>
    /// <param name="pageIndex">Zero-based page index.</param>
    /// <param name="pageSize">Page size.</param>
    /// <param name="totalRecords">Output: total number of matching records.</param>
    /// <param name="filter">Optional filter query.</param>
    /// <param name="ordering">Optional ordering (defaults to sortOrder).</param>
    /// <returns>Paged content items.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when pageIndex is negative or pageSize is less than or equal to zero.</exception>
    IEnumerable<IContent> GetPagedOfType(
        int contentTypeId,
        long pageIndex,
        int pageSize,
        out long totalRecords,
        IQuery<IContent>? filter = null,
        Ordering? ordering = null);

    /// <summary>
    /// Gets paged content items of multiple content types.
    /// </summary>
    /// <param name="contentTypeIds">The content type ids. If empty or containing non-existent IDs, returns empty results with totalRecords = 0.</param>
    /// <param name="pageIndex">Zero-based page index.</param>
    /// <param name="pageSize">Page size.</param>
    /// <param name="totalRecords">Output: total number of matching records.</param>
    /// <param name="filter">Optional filter query.</param>
    /// <param name="ordering">Optional ordering (defaults to sortOrder).</param>
    /// <returns>Paged content items.</returns>
    /// <exception cref="ArgumentNullException">Thrown when contentTypeIds is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when pageIndex is negative or pageSize is less than or equal to zero.</exception>
    IEnumerable<IContent> GetPagedOfTypes(
        int[] contentTypeIds,
        long pageIndex,
        int pageSize,
        out long totalRecords,
        IQuery<IContent>? filter = null,
        Ordering? ordering = null);

    #endregion
}
```

**Step 4: Run test to verify it passes**

Run: `dotnet test tests/Umbraco.Tests.UnitTests --filter "FullyQualifiedName~ContentQueryOperationServiceInterfaceTests" -v n`
Expected: PASS

**Step 5: Commit**

```bash
git add src/Umbraco.Core/Services/IContentQueryOperationService.cs tests/Umbraco.Tests.UnitTests/Umbraco.Core/Services/ContentQueryOperationServiceInterfaceTests.cs
git commit -m "$(cat <<'EOF'
feat(core): add IContentQueryOperationService interface for Phase 2

Extracts query operations (Count, GetByLevel, GetPagedOfType/s) into
focused interface following Phase 1 patterns.

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
EOF
)"
```

---

## Task 2: Create ContentQueryOperationService Implementation

**Files:**
- Create: `src/Umbraco.Core/Services/ContentQueryOperationService.cs`

**Step 1: Write the failing test**

```csharp
// tests/Umbraco.Tests.Integration/Umbraco.Infrastructure/Services/ContentQueryOperationServiceTests.cs
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

/// <summary>
/// Integration tests for ContentQueryOperationService.
/// </summary>
[TestFixture]
[UmbracoTest(
    Database = UmbracoTestOptions.Database.NewSchemaPerTest,
    WithApplication = true)]
public class ContentQueryOperationServiceTests : UmbracoIntegrationTestWithContent
{
    private IContentQueryOperationService QueryService => GetRequiredService<IContentQueryOperationService>();

    [Test]
    public void Count_WithNoFilter_ReturnsAllContentCount()
    {
        // Arrange - base class creates Textpage, Subpage, Subpage2, Subpage3, Trashed
        // Note: Count() may or may not include trashed items depending on repository implementation.
        // Verify with existing ContentService.Count() behavior first, then update assertion.

        // Act
        var count = QueryService.Count();

        // Assert - should return at least 4 (Textpage + 3 subpages), possibly 5 if trashed included
        // TODO: Verify DocumentRepository.Count() behavior with trashed items and update to exact value
        Assert.That(count, Is.EqualTo(5)); // All 5 items including Trashed (repository counts all non-deleted)
    }

    [Test]
    public void Count_WithNonExistentContentTypeAlias_ReturnsZero()
    {
        // Arrange
        var nonExistentAlias = "nonexistent-content-type-alias";

        // Act
        var count = QueryService.Count(nonExistentAlias);

        // Assert
        Assert.That(count, Is.EqualTo(0));
    }

    [Test]
    public void Count_WithContentTypeAlias_ReturnsFilteredCount()
    {
        // Arrange
        var alias = ContentType.Alias;

        // Act
        var count = QueryService.Count(alias);

        // Assert - all 5 content items use the same content type
        Assert.That(count, Is.EqualTo(5));
    }

    [Test]
    public void CountChildren_ReturnsChildCount()
    {
        // Arrange - Textpage has children: Subpage, Subpage2, Subpage3
        var parentId = Textpage.Id;

        // Act
        var count = QueryService.CountChildren(parentId);

        // Assert
        Assert.That(count, Is.EqualTo(3));
    }

    [Test]
    public void GetByLevel_ReturnsContentAtLevel()
    {
        // Arrange - level 1 is root content

        // Act
        var items = QueryService.GetByLevel(1);

        // Assert
        Assert.That(items, Is.Not.Null);
        Assert.That(items.All(x => x.Level == 1), Is.True);
    }

    [Test]
    public void GetPagedOfType_ReturnsPaginatedResults()
    {
        // Arrange
        var contentTypeId = ContentType.Id;

        // Act
        var items = QueryService.GetPagedOfType(contentTypeId, 0, 10, out var total);

        // Assert
        Assert.That(items, Is.Not.Null);
        Assert.That(total, Is.EqualTo(5)); // All 5 content items are of this type
    }

    [Test]
    public void GetPagedOfTypes_WithEmptyArray_ReturnsEmpty()
    {
        // Act
        var items = QueryService.GetPagedOfTypes(Array.Empty<int>(), 0, 10, out var total);

        // Assert
        Assert.That(items, Is.Empty);
        Assert.That(total, Is.EqualTo(0));
    }

    [Test]
    public void GetPagedOfTypes_WithNonExistentContentTypeIds_ReturnsEmpty()
    {
        // Arrange
        var nonExistentIds = new[] { 999999, 999998 };

        // Act
        var items = QueryService.GetPagedOfTypes(nonExistentIds, 0, 10, out var total);

        // Assert
        Assert.That(items, Is.Empty);
        Assert.That(total, Is.EqualTo(0));
    }

    [Test]
    public void CountChildren_WithNonExistentParentId_ReturnsZero()
    {
        // Arrange
        var nonExistentParentId = 999999;

        // Act
        var count = QueryService.CountChildren(nonExistentParentId);

        // Assert
        Assert.That(count, Is.EqualTo(0));
    }

    [Test]
    public void GetByLevel_WithLevelZero_ReturnsEmpty()
    {
        // Arrange - level 0 doesn't exist (content starts at level 1)

        // Act
        var items = QueryService.GetByLevel(0);

        // Assert
        Assert.That(items, Is.Empty);
    }

    [Test]
    public void GetByLevel_WithNegativeLevel_ReturnsEmpty()
    {
        // Arrange

        // Act
        var items = QueryService.GetByLevel(-1);

        // Assert
        Assert.That(items, Is.Empty);
    }

    [Test]
    public void GetPagedOfType_WithNonExistentContentTypeId_ReturnsEmpty()
    {
        // Arrange
        var nonExistentId = 999999;

        // Act
        var items = QueryService.GetPagedOfType(nonExistentId, 0, 10, out var total);

        // Assert
        Assert.That(items, Is.Empty);
        Assert.That(total, Is.EqualTo(0));
    }

    [Test]
    public void CountDescendants_ReturnsDescendantCount()
    {
        // Arrange - Textpage has descendants: Subpage, Subpage2, Subpage3
        var ancestorId = Textpage.Id;

        // Act
        var count = QueryService.CountDescendants(ancestorId);

        // Assert
        Assert.That(count, Is.EqualTo(3));
    }

    [Test]
    public void CountDescendants_WithNonExistentAncestorId_ReturnsZero()
    {
        // Arrange
        var nonExistentId = 999999;

        // Act
        var count = QueryService.CountDescendants(nonExistentId);

        // Assert
        Assert.That(count, Is.EqualTo(0));
    }

    [Test]
    public void CountPublished_WithNoPublishedContent_ReturnsZero()
    {
        // Arrange - base class creates content but doesn't publish

        // Act
        var count = QueryService.CountPublished();

        // Assert
        Assert.That(count, Is.EqualTo(0));
    }
}
```

**Step 2: Run test to verify it fails**

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentQueryOperationServiceTests" -v n`
Expected: FAIL - service not registered or not found

**Step 3: Create the implementation**

> **Note:** Use `#region` blocks matching the interface organization. Verify this matches the pattern established in `ContentCrudService.cs` for consistency across Phase 1 and Phase 2 services.

```csharp
// src/Umbraco.Core/Services/ContentQueryOperationService.cs
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
```

**Step 4: Run test to verify it still fails (not registered yet)**

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentQueryOperationServiceTests" -v n`
Expected: FAIL - service not registered in DI

**Step 5: Commit implementation (before DI registration)**

```bash
git add src/Umbraco.Core/Services/ContentQueryOperationService.cs tests/Umbraco.Tests.Integration/Umbraco.Infrastructure/Services/ContentQueryOperationServiceTests.cs
git commit -m "$(cat <<'EOF'
feat(core): add ContentQueryOperationService implementation

Implements IContentQueryOperationService with Count, GetByLevel, and
GetPagedOfType operations. Follows Phase 1 patterns.

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
EOF
)"
```

---

## Task 3: Register ContentQueryOperationService in DI

**Files:**
- Modify: `src/Umbraco.Core/DependencyInjection/UmbracoBuilder.cs`

**Step 1: Find the registration location and verify ContentService registration pattern**

Search for `IContentCrudService` registration to find where to add the new service:

Run: `grep -n "IContentCrudService" src/Umbraco.Core/DependencyInjection/UmbracoBuilder.cs`
Expected: Line ~301 shows `Services.AddUnique<IContentCrudService, ContentCrudService>();`

Also verify how `ContentService` itself is registered (standard registration vs factory pattern):

Run: `grep -n "IContentService" src/Umbraco.Core/DependencyInjection/UmbracoBuilder.cs`

> **Important:** `ContentService` uses a **factory pattern** for DI registration. The factory must be updated to resolve and inject `IContentQueryOperationService`.

**Step 2: Add the IContentQueryOperationService registration**

Add after `IContentCrudService` registration (around line 301). Use `AddUnique` for consistency with `IContentCrudService` registration pattern:

```csharp
Services.AddUnique<IContentQueryOperationService, ContentQueryOperationService>();
```

> **Note:** Using `AddUnique` (Umbraco extension) instead of `AddScoped` (standard .NET DI) for consistency with Phase 1. `AddUnique` ensures only one implementation is registered and can be replaced by consumers.

**Step 3: Update ContentService factory registration (CRITICAL)**

The `ContentService` is registered via a factory pattern that explicitly constructs the service. This factory **must** be updated to include the new `IContentQueryOperationService` dependency. Find the factory registration (around lines 302-321) and update it:

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
        sp.GetRequiredService<IContentQueryOperationService>()));  // NEW - Phase 2
```

> **Why this is critical:** Without updating this factory, the new `IContentQueryOperationService` parameter added to ContentService's primary constructor will cause a compilation error. The factory explicitly constructs ContentService and must include all constructor parameters.

**Step 4: Run test to verify it passes**

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentQueryOperationServiceTests" -v n`
Expected: PASS

**Step 5: Run all ContentService tests to verify no regression**

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentService" -v n`
Expected: All PASS

**Step 6: Commit**

```bash
git add src/Umbraco.Core/DependencyInjection/UmbracoBuilder.cs
git commit -m "$(cat <<'EOF'
feat(core): register IContentQueryOperationService in DI container

Adds unique registration for ContentQueryOperationService matching
the Phase 1 pattern for IContentCrudService.

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
EOF
)"
```

---

## Task 4: Add QueryService Property to ContentService

**Files:**
- Modify: `src/Umbraco.Core/Services/ContentService.cs`

**Step 1: Add the fields (at class level, near other service fields)**

Add after the `IContentCrudService` fields for logical grouping:

```csharp
// Fields (at class level)
private readonly IContentQueryOperationService? _queryOperationService;
private readonly Lazy<IContentQueryOperationService>? _queryOperationServiceLazy;
```

**Step 2: Add the property with defensive null handling**

```csharp
/// <summary>
/// Gets the query operation service.
/// </summary>
/// <exception cref="InvalidOperationException">Thrown if the service was not properly initialized.</exception>
private IContentQueryOperationService QueryOperationService =>
    _queryOperationService ?? _queryOperationServiceLazy?.Value
    ?? throw new InvalidOperationException("QueryOperationService not initialized. Ensure the service is properly injected via constructor.");
```

> **Note:** The defensive exception replaces the null-forgiving operator (`!`) to catch initialization failures explicitly rather than causing NullReferenceException.

**Step 3: Update primary constructor to inject the service**

Add `IContentQueryOperationService queryOperationService` parameter **after** `IContentCrudService crudService` for logical grouping of extracted services:

```csharp
// Primary constructor signature (add after crudService parameter)
public ContentService(
    // ... existing params ...
    IContentCrudService crudService,
    IContentQueryOperationService queryOperationService)  // NEW - after crudService
    : base(...)
{
    // ... existing assignments ...

    // Add null check and assignment
    ArgumentNullException.ThrowIfNull(queryOperationService);
    _queryOperationService = queryOperationService;
    _queryOperationServiceLazy = null;  // Not needed when directly injected
}
```

**Step 4: Update obsolete constructors to use lazy resolution**

Following the same pattern used for `IContentCrudService`, update all obsolete constructors to include lazy resolution:

```csharp
// In each obsolete constructor, after the _crudServiceLazy assignment:
_queryOperationServiceLazy = new Lazy<IContentQueryOperationService>(() =>
    StaticServiceProvider.Instance.GetRequiredService<IContentQueryOperationService>(),
    LazyThreadSafetyMode.ExecutionAndPublication);
```

> **Why:** Obsolete constructors don't receive the new dependency through DI. Without this, code using obsolete constructors would get `InvalidOperationException` (thrown by our defensive property) when calling methods that delegate to `QueryOperationService`.

**Step 5: Run build to verify compilation**

Run: `dotnet build src/Umbraco.Core`
Expected: Build succeeded

**Step 6: Commit**

```bash
git add src/Umbraco.Core/Services/ContentService.cs
git commit -m "$(cat <<'EOF'
refactor(core): add QueryOperationService to ContentService facade

Injects IContentQueryOperationService for future delegation.
Includes lazy resolution support for obsolete constructors.

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
EOF
)"
```

---

## Task 5: Delegate Count Methods to QueryOperationService

**Files:**
- Modify: `src/Umbraco.Core/Services/ContentService.cs`

**Step 1: Write test to verify current behavior (baseline)**

```csharp
// Add to ContentServiceRefactoringTests.cs
[Test]
public void Count_ViaFacade_ReturnsEquivalentResultToDirectService()
{
    // Arrange
    var queryService = GetRequiredService<IContentQueryOperationService>();

    // Act
    var facadeCount = ContentService.Count();
    var directCount = queryService.Count();

    // Assert
    Assert.That(facadeCount, Is.EqualTo(directCount));
}

[Test]
public void CountPublished_ViaFacade_ReturnsEquivalentResultToDirectService()
{
    // Arrange
    var queryService = GetRequiredService<IContentQueryOperationService>();
    ContentService.Publish(Textpage, new[] { "*" });

    // Act
    var facadeCount = ContentService.CountPublished();
    var directCount = queryService.CountPublished();

    // Assert
    Assert.That(facadeCount, Is.EqualTo(directCount));
}

[Test]
public void CountChildren_ViaFacade_ReturnsEquivalentResultToDirectService()
{
    // Arrange
    var queryService = GetRequiredService<IContentQueryOperationService>();
    var parentId = Textpage.Id;

    // Act
    var facadeCount = ContentService.CountChildren(parentId);
    var directCount = queryService.CountChildren(parentId);

    // Assert
    Assert.That(facadeCount, Is.EqualTo(directCount));
}

[Test]
public void CountDescendants_ViaFacade_ReturnsEquivalentResultToDirectService()
{
    // Arrange
    var queryService = GetRequiredService<IContentQueryOperationService>();
    var parentId = Textpage.Id;

    // Act
    var facadeCount = ContentService.CountDescendants(parentId);
    var directCount = queryService.CountDescendants(parentId);

    // Assert
    Assert.That(facadeCount, Is.EqualTo(directCount));
}
```

**Step 2: Run baseline test**

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentServiceRefactoringTests&Name~Count" -v n`
Expected: PASS (both implementations should return same results)

**Step 3: Update ContentService to delegate**

Replace the Count region methods:

```csharp
#region Count

public int CountPublished(string? contentTypeAlias = null)
    => QueryOperationService.CountPublished(contentTypeAlias);

public int Count(string? contentTypeAlias = null)
    => QueryOperationService.Count(contentTypeAlias);

public int CountChildren(int parentId, string? contentTypeAlias = null)
    => QueryOperationService.CountChildren(parentId, contentTypeAlias);

public int CountDescendants(int parentId, string? contentTypeAlias = null)
    => QueryOperationService.CountDescendants(parentId, contentTypeAlias);

#endregion
```

**Step 4: Run tests to verify delegation works**

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentServiceRefactoringTests&Name~Count" -v n`
Expected: PASS

**Step 5: Run all ContentService tests**

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentService" -v n`
Expected: All PASS

**Step 6: Commit**

```bash
git add src/Umbraco.Core/Services/ContentService.cs tests/Umbraco.Tests.Integration/Umbraco.Infrastructure/Services/ContentServiceRefactoringTests.cs
git commit -m "$(cat <<'EOF'
refactor(core): delegate Count methods to QueryOperationService

ContentService now delegates Count, CountPublished, CountChildren,
CountDescendants to the new QueryOperationService.

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
EOF
)"
```

---

## Task 6: Delegate GetByLevel to QueryOperationService

**Files:**
- Modify: `src/Umbraco.Core/Services/ContentService.cs`

**Step 1: Write baseline test**

```csharp
// Add to ContentServiceRefactoringTests.cs
[Test]
public void GetByLevel_ViaFacade_ReturnsEquivalentResultToDirectService()
{
    // Arrange
    var queryService = GetRequiredService<IContentQueryOperationService>();

    // Act
    var facadeItems = ContentService.GetByLevel(1).ToList();
    var directItems = queryService.GetByLevel(1).ToList();

    // Assert
    Assert.That(facadeItems.Count, Is.EqualTo(directItems.Count));
    Assert.That(facadeItems.Select(x => x.Id), Is.EquivalentTo(directItems.Select(x => x.Id)));
}
```

**Step 2: Run baseline test**

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentServiceRefactoringTests&Name~GetByLevel" -v n`
Expected: PASS

**Step 3: Update ContentService to delegate**

Replace GetByLevel method:

```csharp
public IEnumerable<IContent> GetByLevel(int level)
    => QueryOperationService.GetByLevel(level);
```

**Step 4: Run tests**

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentService" -v n`
Expected: All PASS

**Step 5: Commit**

```bash
git add src/Umbraco.Core/Services/ContentService.cs tests/Umbraco.Tests.Integration/Umbraco.Infrastructure/Services/ContentServiceRefactoringTests.cs
git commit -m "$(cat <<'EOF'
refactor(core): delegate GetByLevel to QueryOperationService

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
EOF
)"
```

---

## Task 7: Delegate GetPagedOfType/s to QueryOperationService

**Files:**
- Modify: `src/Umbraco.Core/Services/ContentService.cs`

**Step 1: Write baseline tests**

```csharp
// Add to ContentServiceRefactoringTests.cs
[Test]
public void GetPagedOfType_ViaFacade_ReturnsEquivalentResultToDirectService()
{
    // Arrange
    var queryService = GetRequiredService<IContentQueryOperationService>();
    var contentTypeId = ContentType.Id;

    // Act
    var facadeItems = ContentService.GetPagedOfType(contentTypeId, 0, 10, out var facadeTotal).ToList();
    var directItems = queryService.GetPagedOfType(contentTypeId, 0, 10, out var directTotal).ToList();

    // Assert
    Assert.That(facadeTotal, Is.EqualTo(directTotal));
    Assert.That(facadeItems.Select(x => x.Id), Is.EquivalentTo(directItems.Select(x => x.Id)));
}

[Test]
public void GetPagedOfTypes_ViaFacade_ReturnsEquivalentResultToDirectService()
{
    // Arrange
    var queryService = GetRequiredService<IContentQueryOperationService>();
    var contentTypeIds = new[] { ContentType.Id };

    // Act
    var facadeItems = ContentService.GetPagedOfTypes(contentTypeIds, 0, 10, out var facadeTotal, null).ToList();
    var directItems = queryService.GetPagedOfTypes(contentTypeIds, 0, 10, out var directTotal).ToList();

    // Assert
    Assert.That(facadeTotal, Is.EqualTo(directTotal));
    Assert.That(facadeItems.Select(x => x.Id), Is.EquivalentTo(directItems.Select(x => x.Id)));
}
```

**Step 2: Run baseline test**

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentServiceRefactoringTests&Name~GetPagedOf" -v n`
Expected: PASS

**Step 3: Update ContentService to delegate**

Replace GetPagedOfType and GetPagedOfTypes methods:

```csharp
/// <inheritdoc />
public IEnumerable<IContent> GetPagedOfType(
    int contentTypeId,
    long pageIndex,
    int pageSize,
    out long totalRecords,
    IQuery<IContent>? filter = null,
    Ordering? ordering = null)
    => QueryOperationService.GetPagedOfType(contentTypeId, pageIndex, pageSize, out totalRecords, filter, ordering);

/// <inheritdoc />
public IEnumerable<IContent> GetPagedOfTypes(int[] contentTypeIds, long pageIndex, int pageSize, out long totalRecords, IQuery<IContent>? filter, Ordering? ordering = null)
    => QueryOperationService.GetPagedOfTypes(contentTypeIds, pageIndex, pageSize, out totalRecords, filter, ordering);
```

**Step 4: Run tests**

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentService" -v n`
Expected: All PASS

**Step 5: Commit**

```bash
git add src/Umbraco.Core/Services/ContentService.cs tests/Umbraco.Tests.Integration/Umbraco.Infrastructure/Services/ContentServiceRefactoringTests.cs
git commit -m "$(cat <<'EOF'
refactor(core): delegate GetPagedOfType/s to QueryOperationService

ContentService now delegates all paged type queries to the new
QueryOperationService.

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
EOF
)"
```

---

## Task 8: Run Phase Gate Tests

**Files:**
- None (test execution only)

**Step 1: Run refactoring-specific tests**

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentServiceRefactoringTests" -v n`
Expected: All PASS

**Step 2: Run all ContentService tests**

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentService" -v n`
Expected: All PASS

**Step 3: Run ContentQueryOperationService tests**

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentQueryOperationService" -v n`
Expected: All PASS

**Step 4: Verify no compilation warnings**

Run: `dotnet build src/Umbraco.Core --warnaserror`
Expected: Build succeeded with no warnings

---

## Task 9: Update Design Document

**Files:**
- Modify: `docs/plans/2025-12-19-contentservice-refactor-design.md`

**Step 1: Update Phase 2 status**

Change Phase 2 from "Pending" to "✅ Complete" in the Implementation Order table.

**Step 2: Add revision note**

Add to revision history:
```
| 1.6 | Phase 2 complete - QueryOperationService extracted |
```

**Step 3: Commit**

```bash
git add docs/plans/2025-12-19-contentservice-refactor-design.md
git commit -m "$(cat <<'EOF'
docs: mark Phase 2 complete in design document

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
EOF
)"
```

---

## Task 10: Create Git Tag for Phase 2

**Files:**
- None (git operation only)

**Step 1: Create annotated tag**

```bash
git tag -a phase-2-query-extraction -m "Phase 2 complete: ContentQueryOperationService extracted from ContentService"
```

**Step 2: Verify tag**

```bash
git tag -l "phase-*"
```
Expected output includes: `phase-1-crud-extraction` and `phase-2-query-extraction`

---

## Summary

### Files Created
1. `src/Umbraco.Core/Services/IContentQueryOperationService.cs` - Interface (~90 lines)
2. `src/Umbraco.Core/Services/ContentQueryOperationService.cs` - Implementation (~140 lines)
3. `tests/Umbraco.Tests.UnitTests/Umbraco.Core/Services/ContentQueryOperationServiceInterfaceTests.cs` - Unit tests
4. `tests/Umbraco.Tests.Integration/Umbraco.Infrastructure/Services/ContentQueryOperationServiceTests.cs` - Integration tests (including edge cases for CountDescendants, GetPagedOfType, CountPublished)

### Files Modified
1. `src/Umbraco.Core/DependencyInjection/UmbracoBuilder.cs` - DI registration (using AddUnique)
2. `src/Umbraco.Core/Services/ContentService.cs` - Added delegation (~7 methods) with lazy resolution support for obsolete constructors
3. `tests/Umbraco.Tests.Integration/Umbraco.Infrastructure/Services/ContentServiceRefactoringTests.cs` - Delegation tests
4. `docs/plans/2025-12-19-contentservice-refactor-design.md` - Status update

### Methods Delegated (7)
- `Count(string?)`
- `CountPublished(string?)`
- `CountChildren(int, string?)`
- `CountDescendants(int, string?)`
- `GetByLevel(int)`
- `GetPagedOfType(...)`
- `GetPagedOfTypes(...)`

### Estimated ContentService Reduction
- Before Phase 2: ~3497 lines
- After Phase 2: ~3420 lines (reduced by ~77 lines)

### Commits (10)
1. Interface creation
2. Implementation creation
3. DI registration
4. Add QueryOperationService property
5. Delegate Count methods
6. Delegate GetByLevel
7. Delegate GetPagedOfType/s
8. (no commit - test execution only)
9. Design doc update
10. (no commit - tag creation)
