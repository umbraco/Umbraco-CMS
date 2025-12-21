# ContentService CRUD Extraction - Phase 1 Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Extract CRUD operations (Create, Get, Save, Delete) from ContentService into a dedicated `IContentCrudService` interface and `ContentCrudService` implementation.

**Architecture:** Create a shared `ContentServiceBase` abstract class containing common dependencies and infrastructure. `ContentCrudService` implements `IContentCrudService` and inherits from `ContentServiceBase`. The existing `ContentService` facade will delegate to `ContentCrudService` for CRUD operations.

**Tech Stack:** .NET 10.0, NUnit, Microsoft.Extensions.DependencyInjection

---

## Plan Version History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2025-12-20 | Claude | Initial plan creation |
| 1.1 | 2025-12-20 | Claude | Applied critical review feedback (see "Critical Review Changes Applied" section) |
| 1.2 | 2025-12-20 | Claude | Added versioning strategy and plan version history |
| 1.3 | 2025-12-20 | Claude | Applied second critical review feedback (see "Critical Review 2 Changes Applied" section) |
| 1.4 | 2025-12-20 | Claude | Applied third critical review feedback (see "Critical Review 3 Changes Applied" section) |
| 1.5 | 2025-12-20 | Claude | Applied fourth critical review feedback (see "Critical Review 4 Changes Applied" section) |
| 1.6 | 2025-12-20 | Claude | Applied fifth critical review feedback (see "From Critical Review 5" checklist section) |

---

## Versioning Strategy

### Interface Versioning (`IContentCrudService`)

**Policy:** The `IContentCrudService` interface follows **additive-only changes** until a major version bump.

| Change Type | Allowed? | Action Required |
|-------------|----------|-----------------|
| Add new method | ✅ Yes | Add with default implementation in abstract base class |
| Remove method | ❌ No | Mark `[Obsolete]` for 2 major versions, then remove |
| Change method signature | ❌ No | Add new overload, deprecate old |
| Change return type | ❌ No | Add new method with different name |
| Add optional parameter | ✅ Yes | Use default value |

**Breaking Change Process:**
1. Mark existing member with `[Obsolete("Use X instead. Will be removed in v{N+2}.")]`
2. Add replacement member
3. Update all internal usages to new member
4. Remove in version N+2 (minimum 2 major version grace period)

### Interface Extensibility Model

`IContentCrudService` is designed for **composition, not direct implementation**.

**Supported usage:**
- Inject `IContentCrudService` as a dependency ✅
- Extend `ContentCrudService` via inheritance ✅
- Replace registration in DI with custom implementation inheriting `ContentServiceBase` ✅

**Unsupported usage:**
- Implement `IContentCrudService` directly without inheriting `ContentServiceBase` ❌

**Rationale:** All CRUD operations require shared infrastructure (scoping, repositories,
auditing). `ContentServiceBase` provides this infrastructure. Direct interface implementation
would require re-implementing this infrastructure correctly, which is error-prone and
creates maintenance burden.

**Adding New Methods (Umbraco internal process):**
1. Add method signature to `IContentCrudService` interface
2. Add virtual implementation to `ContentServiceBase` (if shareable) or `ContentCrudService`
3. Existing subclasses automatically inherit the new implementation
4. Mark with `[Since("X.Y")]` attribute if adding after initial release

### Obsolete Constructor Support Duration

Obsolete constructors on `ContentService` that use the `Lazy<IContentCrudService>` pattern:
- **Support duration:** 2 major versions (e.g., introduced in v14, removed in v16)
- **Warning level:** Compile-time warning via `[Obsolete]` attribute
- **Migration path:** Update to primary constructor with direct `IContentCrudService` injection

### Semantic Versioning Alignment

This refactoring aligns with Umbraco's semantic versioning:
- **Major (X.0.0):** Breaking changes allowed (interface method removal, signature changes)
- **Minor (X.Y.0):** New features, additive interface changes only
- **Patch (X.Y.Z):** Bug fixes, no interface changes

---

## Phase 1 Overview

Phase 1 establishes the foundational patterns for the entire refactoring:

1. **ContentServiceBase** - Abstract base class with shared dependencies
2. **IContentCrudService** - Interface for CRUD operations
3. **ContentCrudService** - Implementation of CRUD operations
4. **DI Registration** - Register new services alongside existing ContentService
5. **ContentService Delegation** - Update facade to delegate to new service

The existing `ContentService` (3823 lines) will shrink as we extract methods to `ContentCrudService`.

---

## Task 1: Create ContentServiceBase Abstract Class

**Files:**
- Create: `src/Umbraco.Core/Services/ContentServiceBase.cs`

**Step 1: Write the failing test**

The tracking test already exists in `ContentServiceBaseTests.cs`. It will pass once we create the class.

**Step 2: Create the abstract base class**

```csharp
// src/Umbraco.Core/Services/ContentServiceBase.cs
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Abstract base class for content-related services providing shared infrastructure.
/// </summary>
public abstract class ContentServiceBase : RepositoryService
{
    protected readonly IDocumentRepository DocumentRepository;
    protected readonly IAuditService AuditService;
    protected readonly IUserIdKeyResolver UserIdKeyResolver;

    protected ContentServiceBase(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IDocumentRepository documentRepository,
        IAuditService auditService,
        IUserIdKeyResolver userIdKeyResolver)
        : base(provider, loggerFactory, eventMessagesFactory)
    {
        DocumentRepository = documentRepository ?? throw new ArgumentNullException(nameof(documentRepository));
        AuditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        UserIdKeyResolver = userIdKeyResolver ?? throw new ArgumentNullException(nameof(userIdKeyResolver));
    }

    /// <summary>
    /// Records an audit entry for a content operation (synchronous).
    /// </summary>
    /// <remarks>
    /// Uses ConfigureAwait(false) to avoid capturing synchronization context and prevent deadlocks.
    /// TODO: Replace with sync overloads when IAuditService.Add and IUserIdKeyResolver.Get are available.
    /// </remarks>
    protected void Audit(AuditType type, int userId, int objectId, string? message = null, string? parameters = null)
    {
        // Use ConfigureAwait(false) to avoid context capture and potential deadlocks
        Guid userKey = UserIdKeyResolver.GetAsync(userId).ConfigureAwait(false).GetAwaiter().GetResult();

        AuditService.AddAsync(
            type,
            userKey,
            objectId,
            UmbracoObjectTypes.Document.GetName(),
            message,
            parameters).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Records an audit entry for a content operation asynchronously.
    /// </summary>
    protected async Task AuditAsync(AuditType type, int userId, int objectId, string? message = null, string? parameters = null)
    {
        Guid userKey = await UserIdKeyResolver.GetAsync(userId).ConfigureAwait(false);

        await AuditService.AddAsync(
            type,
            userKey,
            objectId,
            UmbracoObjectTypes.Document.GetName(),
            message,
            parameters).ConfigureAwait(false);
    }
}
```

**Step 3: Add ContentServiceConstants class**

```csharp
// src/Umbraco.Core/Services/ContentServiceConstants.cs
namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Constants used by content-related services.
/// </summary>
public static class ContentServiceConstants
{
    /// <summary>
    /// Default page size for batch operations (e.g., cascade delete).
    /// </summary>
    public const int DefaultBatchPageSize = 500;
}
```

**Step 4: Verify the project builds**

Run: `dotnet build src/Umbraco.Core/Umbraco.Core.csproj`
Expected: Build succeeded.

**Step 5: Run the tracking test**

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentServiceBaseTests"`
Expected: Test passes (class now exists)

**Step 6: Commit**

```bash
git add src/Umbraco.Core/Services/ContentServiceBase.cs src/Umbraco.Core/Services/ContentServiceConstants.cs
git commit -m "$(cat <<'EOF'
feat(core): add ContentServiceBase abstract class for Phase 1

Establishes shared infrastructure for content services:
- Common dependencies (DocumentRepository, AuditService, UserIdKeyResolver)
- Audit helper methods
- Inherits from RepositoryService for scope/query support
- Adds ContentServiceConstants for shared constants (batch page size)

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
EOF
)"
```

---

## Task 2: Create IContentCrudService Interface

**Files:**
- Create: `src/Umbraco.Core/Services/IContentCrudService.cs`

**Step 1: Create the interface**

```csharp
// src/Umbraco.Core/Services/IContentCrudService.cs
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Service for content CRUD (Create, Read, Update, Delete) operations.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Implementation Note:</strong> Do not implement this interface directly.
/// Instead, inherit from <see cref="ContentServiceBase"/> which provides required
/// infrastructure (scoping, repository access, auditing). Direct implementation
/// without this base class will result in missing functionality.
/// </para>
/// <para>
/// This interface is part of the ContentService refactoring initiative.
/// It extracts core CRUD operations into a focused, testable service.
/// </para>
/// <para>
/// <strong>Versioning Policy:</strong> This interface follows additive-only changes.
/// New methods may be added with default implementations. Existing methods will not
/// be removed or have signatures changed without a 2 major version deprecation period.
/// </para>
/// <para>
/// <strong>Version History:</strong>
/// <list type="bullet">
///   <item><description>v1.0 (Phase 1): Initial interface with Create, Read, Save, Delete operations</description></item>
/// </list>
/// </para>
/// </remarks>
/// <since>1.0</since>
public interface IContentCrudService : IService
{
    #region Create

    /// <summary>
    /// Creates a document without persisting it.
    /// </summary>
    /// <param name="name">Name of the document.</param>
    /// <param name="parentId">Id of the parent, or -1 for root.</param>
    /// <param name="contentTypeAlias">Alias of the content type.</param>
    /// <param name="userId">Optional id of the user creating the content.</param>
    /// <returns>The new document.</returns>
    IContent Create(string name, int parentId, string contentTypeAlias, int userId = Constants.Security.SuperUserId);

    /// <summary>
    /// Creates a document without persisting it.
    /// </summary>
    /// <param name="name">Name of the document.</param>
    /// <param name="parentId">Guid key of the parent.</param>
    /// <param name="contentTypeAlias">Alias of the content type.</param>
    /// <param name="userId">Optional id of the user creating the content.</param>
    /// <returns>The new document.</returns>
    IContent Create(string name, Guid parentId, string contentTypeAlias, int userId = Constants.Security.SuperUserId);

    /// <summary>
    /// Creates a document without persisting it.
    /// </summary>
    /// <param name="name">Name of the document.</param>
    /// <param name="parentId">Id of the parent, or -1 for root.</param>
    /// <param name="contentType">The content type.</param>
    /// <param name="userId">Optional id of the user creating the content.</param>
    /// <returns>The new document.</returns>
    IContent Create(string name, int parentId, IContentType contentType, int userId = Constants.Security.SuperUserId);

    /// <summary>
    /// Creates a document without persisting it.
    /// </summary>
    /// <param name="name">Name of the document.</param>
    /// <param name="parent">The parent document.</param>
    /// <param name="contentTypeAlias">Alias of the content type.</param>
    /// <param name="userId">Optional id of the user creating the content.</param>
    /// <returns>The new document.</returns>
    IContent Create(string name, IContent? parent, string contentTypeAlias, int userId = Constants.Security.SuperUserId);

    /// <summary>
    /// Creates and persists a document.
    /// </summary>
    /// <param name="name">Name of the document.</param>
    /// <param name="parentId">Id of the parent, or -1 for root.</param>
    /// <param name="contentTypeAlias">Alias of the content type.</param>
    /// <param name="userId">Optional id of the user creating the content.</param>
    /// <returns>The persisted document.</returns>
    IContent CreateAndSave(string name, int parentId, string contentTypeAlias, int userId = Constants.Security.SuperUserId);

    /// <summary>
    /// Creates and persists a document.
    /// </summary>
    /// <param name="name">Name of the document.</param>
    /// <param name="parent">The parent document.</param>
    /// <param name="contentTypeAlias">Alias of the content type.</param>
    /// <param name="userId">Optional id of the user creating the content.</param>
    /// <returns>The persisted document.</returns>
    IContent CreateAndSave(string name, IContent parent, string contentTypeAlias, int userId = Constants.Security.SuperUserId);

    #endregion

    #region Read

    /// <summary>
    /// Gets a document by id.
    /// </summary>
    /// <param name="id">The document id.</param>
    /// <returns>The document, or null if not found.</returns>
    IContent? GetById(int id);

    /// <summary>
    /// Gets a document by key.
    /// </summary>
    /// <param name="key">The document key.</param>
    /// <returns>The document, or null if not found.</returns>
    IContent? GetById(Guid key);

    /// <summary>
    /// Gets documents by ids.
    /// </summary>
    /// <param name="ids">The document ids.</param>
    /// <returns>The documents.</returns>
    IEnumerable<IContent> GetByIds(IEnumerable<int> ids);

    /// <summary>
    /// Gets documents by keys.
    /// </summary>
    /// <param name="ids">The document keys.</param>
    /// <returns>The documents.</returns>
    IEnumerable<IContent> GetByIds(IEnumerable<Guid> ids);

    /// <summary>
    /// Gets root-level documents.
    /// </summary>
    /// <returns>The root documents.</returns>
    IEnumerable<IContent> GetRootContent();

    /// <summary>
    /// Gets the parent of a document.
    /// </summary>
    /// <param name="id">Id of the document.</param>
    /// <returns>The parent document, or null if at root.</returns>
    IContent? GetParent(int id);

    /// <summary>
    /// Gets the parent of a document.
    /// </summary>
    /// <param name="content">The document.</param>
    /// <returns>The parent document, or null if at root.</returns>
    IContent? GetParent(IContent? content);

    #endregion

    #region Read (Tree Traversal)

    /// <summary>
    /// Gets ancestors of a document.
    /// </summary>
    /// <param name="id">Id of the document.</param>
    /// <returns>The ancestor documents, from root to parent (closest to root first).</returns>
    IEnumerable<IContent> GetAncestors(int id);

    /// <summary>
    /// Gets ancestors of a document.
    /// </summary>
    /// <param name="content">The document.</param>
    /// <returns>The ancestor documents, from root to parent (closest to root first).</returns>
    IEnumerable<IContent> GetAncestors(IContent content);

    /// <summary>
    /// Gets paged children of a document.
    /// </summary>
    /// <param name="id">Id of the parent document.</param>
    /// <param name="pageIndex">Zero-based page index.</param>
    /// <param name="pageSize">Page size.</param>
    /// <param name="totalChildren">Total number of children.</param>
    /// <param name="filter">Optional filter query.</param>
    /// <param name="ordering">Optional ordering.</param>
    /// <returns>The child documents.</returns>
    IEnumerable<IContent> GetPagedChildren(int id, long pageIndex, int pageSize, out long totalChildren, IQuery<IContent>? filter = null, Ordering? ordering = null);

    /// <summary>
    /// Gets paged descendants of a document.
    /// </summary>
    /// <param name="id">Id of the ancestor document.</param>
    /// <param name="pageIndex">Zero-based page index.</param>
    /// <param name="pageSize">Page size.</param>
    /// <param name="totalChildren">Total number of descendants.</param>
    /// <param name="filter">Optional filter query.</param>
    /// <param name="ordering">Optional ordering.</param>
    /// <returns>The descendant documents.</returns>
    IEnumerable<IContent> GetPagedDescendants(int id, long pageIndex, int pageSize, out long totalChildren, IQuery<IContent>? filter = null, Ordering? ordering = null);

    /// <summary>
    /// Checks whether a document has children.
    /// </summary>
    /// <param name="id">The document id.</param>
    /// <returns>True if the document has children; otherwise false.</returns>
    bool HasChildren(int id);

    /// <summary>
    /// Checks whether a document with the specified id exists.
    /// </summary>
    /// <param name="id">The document id.</param>
    /// <returns>True if the document exists; otherwise false.</returns>
    bool Exists(int id);

    /// <summary>
    /// Checks whether a document with the specified key exists.
    /// </summary>
    /// <param name="key">The document key.</param>
    /// <returns>True if the document exists; otherwise false.</returns>
    bool Exists(Guid key);

    #endregion

    #region Save

    /// <summary>
    /// Saves a document.
    /// </summary>
    /// <param name="content">The document to save.</param>
    /// <param name="userId">Optional id of the user saving the content.</param>
    /// <param name="contentSchedule">Optional content schedule.</param>
    /// <returns>The operation result.</returns>
    OperationResult Save(IContent content, int? userId = null, ContentScheduleCollection? contentSchedule = null);

    /// <summary>
    /// Saves multiple documents.
    /// </summary>
    /// <param name="contents">The documents to save.</param>
    /// <param name="userId">Optional id of the user saving the content.</param>
    /// <returns>The operation result.</returns>
    /// <remarks>
    /// This method does not support content schedules. To save content with schedules,
    /// use the single-item <see cref="Save(IContent, int?, ContentScheduleCollection?)"/> overload.
    /// </remarks>
    OperationResult Save(IEnumerable<IContent> contents, int userId = Constants.Security.SuperUserId);

    #endregion

    #region Delete

    /// <summary>
    /// Permanently deletes a document and all its descendants.
    /// </summary>
    /// <param name="content">The document to delete.</param>
    /// <param name="userId">Optional id of the user deleting the content.</param>
    /// <returns>The operation result.</returns>
    OperationResult Delete(IContent content, int userId = Constants.Security.SuperUserId);

    #endregion
}
```

**Step 2: Verify the project builds**

Run: `dotnet build src/Umbraco.Core/Umbraco.Core.csproj`
Expected: Build succeeded.

**Step 3: Commit**

```bash
git add src/Umbraco.Core/Services/IContentCrudService.cs
git commit -m "$(cat <<'EOF'
feat(core): add IContentCrudService interface for Phase 1

Defines the contract for content CRUD operations:
- Create: 6 overloads for creating documents
- Read: GetById, GetByIds, GetRootContent, GetParent
- Read (Tree Traversal): GetAncestors, GetPagedChildren, GetPagedDescendants
- Save: Single and batch save operations
- Delete: Permanent deletion with cascade

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
EOF
)"
```

---

## Task 3: Create ContentCrudService Implementation

**Files:**
- Create: `src/Umbraco.Core/Services/ContentCrudService.cs`

**Step 1: Write a failing unit test for ContentCrudService**

Create a simple test to verify the service can be constructed:

```csharp
// tests/Umbraco.Tests.UnitTests/Umbraco.Core/Services/ContentCrudServiceTests.cs
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using IsolationLevel = System.Data.IsolationLevel;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services;

[TestFixture]
[Category("UnitTest")]
public class ContentCrudServiceTests
{
    private Mock<ICoreScopeProvider> _scopeProvider = null!;
    private Mock<ILoggerFactory> _loggerFactory = null!;
    private Mock<IEventMessagesFactory> _eventMessagesFactory = null!;
    private Mock<IDocumentRepository> _documentRepository = null!;
    private Mock<IEntityRepository> _entityRepository = null!;
    private Mock<IContentTypeRepository> _contentTypeRepository = null!;
    private Mock<IAuditService> _auditService = null!;
    private Mock<IUserIdKeyResolver> _userIdKeyResolver = null!;
    private Mock<ILanguageRepository> _languageRepository = null!;
    private ContentCrudService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _scopeProvider = new Mock<ICoreScopeProvider>();
        _loggerFactory = new Mock<ILoggerFactory>();
        _eventMessagesFactory = new Mock<IEventMessagesFactory>();
        _documentRepository = new Mock<IDocumentRepository>();
        _entityRepository = new Mock<IEntityRepository>();
        _contentTypeRepository = new Mock<IContentTypeRepository>();
        _auditService = new Mock<IAuditService>();
        _userIdKeyResolver = new Mock<IUserIdKeyResolver>();
        _languageRepository = new Mock<ILanguageRepository>();

        // Setup logger factory to return a mock logger
        _loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>()))
            .Returns(Mock.Of<ILogger>());

        // Setup event messages factory
        _eventMessagesFactory.Setup(x => x.Get())
            .Returns(new EventMessages());

        _sut = new ContentCrudService(
            _scopeProvider.Object,
            _loggerFactory.Object,
            _eventMessagesFactory.Object,
            _documentRepository.Object,
            _entityRepository.Object,
            _contentTypeRepository.Object,
            _auditService.Object,
            _userIdKeyResolver.Object,
            _languageRepository.Object);
    }

    #region Mock Setup Helpers

    /// <summary>
    /// Creates a mock scope configured for read operations.
    /// </summary>
    private ICoreScope CreateMockScopeWithReadLock()
    {
        var scope = new Mock<ICoreScope>();
        scope.Setup(x => x.ReadLock(It.IsAny<int[]>()));
        _scopeProvider.Setup(x => x.CreateCoreScope(
            It.IsAny<IsolationLevel>(),
            It.IsAny<RepositoryCacheMode>(),
            It.IsAny<IEventDispatcher?>(),
            It.IsAny<IScopedNotificationPublisher?>(),
            It.IsAny<bool?>(),
            It.IsAny<bool>(),
            It.IsAny<bool>()))
            .Returns(scope.Object);
        return scope.Object;
    }

    /// <summary>
    /// Creates a mock scope configured for write operations.
    /// </summary>
    private ICoreScope CreateMockScopeWithWriteLock()
    {
        var scope = CreateMockScopeWithReadLock();
        Mock.Get(scope).Setup(x => x.WriteLock(It.IsAny<int[]>()));
        Mock.Get(scope).Setup(x => x.Notifications).Returns(Mock.Of<IScopedNotificationPublisher>());
        return scope;
    }

    #endregion

    [Test]
    public void Constructor_WithValidDependencies_CreatesInstance()
    {
        // Assert
        Assert.That(_sut, Is.Not.Null);
        Assert.That(_sut, Is.InstanceOf<IContentCrudService>());
    }

    [Test]
    public void Create_WithInvalidParentId_ThrowsArgumentException()
    {
        // Arrange
        CreateMockScopeWithReadLock();
        var contentType = Mock.Of<IContentType>(x => x.Alias == "testType");
        _contentTypeRepository.Setup(x => x.Get("testType")).Returns(contentType);
        _documentRepository.Setup(x => x.Get(999)).Returns((IContent?)null);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            _sut.Create("Test", 999, "testType"));
    }

    [Test]
    public void Create_WithNullContentType_ThrowsArgumentException()
    {
        // Arrange
        CreateMockScopeWithReadLock();
        _contentTypeRepository.Setup(x => x.Get("nonExistentType")).Returns((IContentType?)null);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            _sut.Create("Test", Constants.System.Root, "nonExistentType"));
    }

    [Test]
    public void GetById_WithNonExistentId_ReturnsNull()
    {
        // Arrange
        CreateMockScopeWithReadLock();
        _documentRepository.Setup(x => x.Get(999)).Returns((IContent?)null);

        // Act
        var result = _sut.GetById(999);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void Save_WithEmptyCollection_ReturnsSuccessWithoutCallingRepository()
    {
        // Arrange
        CreateMockScopeWithWriteLock();

        // Act
        var result = _sut.Save(Enumerable.Empty<IContent>());

        // Assert
        Assert.That(result.Success, Is.True);
        _documentRepository.Verify(x => x.Save(It.IsAny<IContent>()), Times.Never);
    }

    [Test]
    public void Exists_WithExistingId_ReturnsTrue()
    {
        // Arrange
        CreateMockScopeWithReadLock();
        _documentRepository.Setup(x => x.Exists(123)).Returns(true);

        // Act
        var result = _sut.Exists(123);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void HasChildren_WithNoChildren_ReturnsFalse()
    {
        // Arrange
        CreateMockScopeWithReadLock();
        _documentRepository.Setup(x => x.Count(It.IsAny<IQuery<IContent>>())).Returns(0);

        // Act
        var result = _sut.HasChildren(123);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void Save_WithVariantContent_CallsLanguageRepository()
    {
        // Arrange
        var scope = CreateMockScopeWithWriteLock();

        var contentType = new Mock<IContentType>();
        contentType.Setup(x => x.VariesByCulture()).Returns(true);

        var cultureInfo = new Mock<ContentCultureInfos>();
        var cultureInfoDict = new Mock<ContentCultureInfosCollection>();

        var content = new Mock<IContent>();
        content.Setup(x => x.ContentType).Returns(contentType.Object);
        content.Setup(x => x.CultureInfos).Returns(cultureInfoDict.Object);
        content.Setup(x => x.HasIdentity).Returns(true);
        content.Setup(x => x.PublishedState).Returns(PublishedState.Unpublished);
        content.Setup(x => x.Name).Returns("Test");

        // Setup culture infos to return dirty cultures
        var dirtyCultureInfo = Mock.Of<IContentCultureInfo>(x =>
            x.IsDirty() == true && x.Culture == "en-US");
        cultureInfoDict.Setup(x => x.Values).Returns(new[] { dirtyCultureInfo });

        _languageRepository.Setup(x => x.GetMany()).Returns(new List<ILanguage>());

        // Act
        var result = _sut.Save(content.Object);

        // Assert
        Assert.That(result.Success, Is.True);
        _languageRepository.Verify(x => x.GetMany(), Times.Once,
            "Language repository should be called for variant content");
    }
}
```

**Step 2: Run test to verify it fails**

Run: `dotnet test tests/Umbraco.Tests.UnitTests --filter "FullyQualifiedName~ContentCrudServiceTests"`
Expected: FAIL - ContentCrudService class not found

**Step 3: Write the ContentCrudService implementation**

```csharp
// src/Umbraco.Core/Services/ContentCrudService.cs
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Querying;  // Required for TextColumnType, Direction, Ordering
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Implementation of content CRUD operations.
/// </summary>
public class ContentCrudService : ContentServiceBase, IContentCrudService
{
    private readonly IContentTypeRepository _contentTypeRepository;
    private readonly IEntityRepository _entityRepository;
    private readonly ILanguageRepository _languageRepository;
    private readonly ILogger<ContentCrudService> _logger;

    public ContentCrudService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IDocumentRepository documentRepository,
        IEntityRepository entityRepository,
        IContentTypeRepository contentTypeRepository,
        IAuditService auditService,
        IUserIdKeyResolver userIdKeyResolver,
        ILanguageRepository languageRepository)
        : base(provider, loggerFactory, eventMessagesFactory, documentRepository, auditService, userIdKeyResolver)
    {
        _entityRepository = entityRepository ?? throw new ArgumentNullException(nameof(entityRepository));
        _contentTypeRepository = contentTypeRepository ?? throw new ArgumentNullException(nameof(contentTypeRepository));
        _languageRepository = languageRepository ?? throw new ArgumentNullException(nameof(languageRepository));
        _logger = loggerFactory.CreateLogger<ContentCrudService>();
    }

    #region Create

    /// <inheritdoc />
    public IContent Create(string name, Guid parentId, string contentTypeAlias, int userId = Constants.Security.SuperUserId)
    {
        IContent? parent = GetById(parentId);
        if (parent is null)
        {
            throw new ArgumentException($"No content with key '{parentId}' exists.", nameof(parentId));
        }
        return Create(name, parent, contentTypeAlias, userId);
    }

    /// <inheritdoc />
    public IContent Create(string name, int parentId, string contentTypeAlias, int userId = Constants.Security.SuperUserId)
    {
        IContentType contentType = GetContentType(contentTypeAlias);
        return Create(name, parentId, contentType, userId);
    }

    /// <inheritdoc />
    public IContent Create(string name, int parentId, IContentType contentType, int userId = Constants.Security.SuperUserId)
    {
        if (contentType is null)
        {
            throw new ArgumentException("Content type must be specified", nameof(contentType));
        }

        IContent? parent = parentId > 0 ? GetById(parentId) : null;
        if (parentId > 0 && parent is null)
        {
            throw new ArgumentException("No content with that id.", nameof(parentId));
        }

        var content = new Content(name, parentId, contentType, userId);
        return content;
    }

    /// <inheritdoc />
    public IContent Create(string name, IContent? parent, string contentTypeAlias, int userId = Constants.Security.SuperUserId)
    {
        if (parent is null)
        {
            throw new ArgumentNullException(nameof(parent));
        }

        IContentType contentType = GetContentType(contentTypeAlias)
            ?? throw new ArgumentException("No content type with that alias.", nameof(contentTypeAlias));

        var content = new Content(name, parent, contentType, userId);
        return content;
    }

    /// <inheritdoc />
    public IContent CreateAndSave(string name, int parentId, string contentTypeAlias, int userId = Constants.Security.SuperUserId)
    {
        IContent? parent = parentId > 0 ? GetById(parentId) : null;
        if (parentId > 0 && parent is null)
        {
            throw new ArgumentException("No content with that id.", nameof(parentId));
        }

        return CreateAndSaveInternal(name, parent, parentId, contentTypeAlias, userId);
    }

    /// <inheritdoc />
    public IContent CreateAndSave(string name, IContent parent, string contentTypeAlias, int userId = Constants.Security.SuperUserId)
    {
        if (parent is null)
        {
            throw new ArgumentNullException(nameof(parent));
        }

        return CreateAndSaveInternal(name, parent, parent.Id, contentTypeAlias, userId);
    }

    /// <summary>
    /// Internal helper for CreateAndSave operations. Handles validation, content creation, and persistence.
    /// </summary>
    /// <param name="name">Name of the document.</param>
    /// <param name="parent">Parent document, or null for root.</param>
    /// <param name="parentId">Parent ID (-1 for root).</param>
    /// <param name="contentTypeAlias">Alias of the content type.</param>
    /// <param name="userId">Id of the user creating the content.</param>
    /// <returns>The persisted content.</returns>
    /// <remarks>
    /// <para><strong>Lock Ordering:</strong></para>
    /// <para>This method acquires ContentTree (write), ContentTypes (read), and Languages (read) locks at the start.
    /// This ordering is consistent with the documented lock hierarchy to prevent deadlocks.</para>
    /// </remarks>
    private IContent CreateAndSaveInternal(string name, IContent? parent, int parentId, string contentTypeAlias, int userId)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            // Acquire ALL locks upfront to avoid nested scope creation
            scope.WriteLock(Constants.Locks.ContentTree);
            scope.ReadLock(Constants.Locks.ContentTypes);
            scope.ReadLock(Constants.Locks.Languages);  // Required for GetLanguageDetailsForAuditEntryLocked in SaveLocked

            // Prevent creating content under trashed parents
            if (parent?.Trashed == true)
            {
                throw new InvalidOperationException(
                    $"Cannot create content under trashed parent '{parent.Name}' (id={parent.Id}).");
            }

            // Use locked variant - no nested scope
            IContentType contentType = GetContentTypeLocked(contentTypeAlias);

            // Parent is guaranteed non-null here if parentId > 0 because caller validated
            Content content = parent is not null
                ? new Content(name, parent, contentType, userId)
                : new Content(name, parentId, contentType, userId);

            // Use SaveLocked - no nested scope
            SaveLocked(scope, content, userId, null, EventMessagesFactory.Get());

            scope.Complete();
            return content;
        }
    }

    #endregion

    #region Read

    /// <inheritdoc />
    public IContent? GetById(int id)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(Constants.Locks.ContentTree);
            return DocumentRepository.Get(id);
        }
    }

    /// <inheritdoc />
    public IContent? GetById(Guid key)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(Constants.Locks.ContentTree);
            return DocumentRepository.Get(key);
        }
    }

    /// <inheritdoc />
    public IEnumerable<IContent> GetByIds(IEnumerable<int> ids)
    {
        // Avoid allocation if input is already an array
        var idsA = ids as int[] ?? ids.ToArray();
        if (idsA.Length == 0)
        {
            return Enumerable.Empty<IContent>();
        }

        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(Constants.Locks.ContentTree);
            IEnumerable<IContent>? items = DocumentRepository.GetMany(idsA);

            if (items is not null)
            {
                // Use GroupBy to handle potential duplicate keys from repository
                var index = items.GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.First());
                return idsA.Select(x => index.GetValueOrDefault(x)).WhereNotNull();
            }

            return Enumerable.Empty<IContent>();
        }
    }

    /// <inheritdoc />
    public IEnumerable<IContent> GetByIds(IEnumerable<Guid> ids)
    {
        // Avoid allocation if input is already an array
        Guid[] idsA = ids as Guid[] ?? ids.ToArray();
        if (idsA.Length == 0)
        {
            return Enumerable.Empty<IContent>();
        }

        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(Constants.Locks.ContentTree);
            IEnumerable<IContent>? items = DocumentRepository.GetMany(idsA);

            if (items is not null)
            {
                // Use GroupBy to handle potential duplicate keys from repository
                var index = items.GroupBy(x => x.Key).ToDictionary(g => g.Key, g => g.First());
                return idsA.Select(x => index.GetValueOrDefault(x)).WhereNotNull();
            }

            return Enumerable.Empty<IContent>();
        }
    }

    /// <inheritdoc />
    public IEnumerable<IContent> GetRootContent()
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(Constants.Locks.ContentTree);
            IQuery<IContent> query = Query<IContent>().Where(x => x.ParentId == Constants.System.Root);
            return DocumentRepository.Get(query);
        }
    }

    /// <inheritdoc />
    public IContent? GetParent(int id)
    {
        IContent? content = GetById(id);
        return GetParent(content);
    }

    /// <inheritdoc />
    public IContent? GetParent(IContent? content)
    {
        if (content?.ParentId == Constants.System.Root || content?.ParentId == Constants.System.RecycleBinContent ||
            content is null)
        {
            return null;
        }

        return GetById(content.ParentId);
    }

    #endregion

    #region Read (Tree Traversal)

    /// <inheritdoc />
    public IEnumerable<IContent> GetAncestors(int id)
    {
        IContent? content = GetById(id);
        return content is null ? Enumerable.Empty<IContent>() : GetAncestors(content);
    }

    /// <inheritdoc />
    public IEnumerable<IContent> GetAncestors(IContent content)
    {
        if (content?.Path == null || content.Level <= 1)
        {
            return Enumerable.Empty<IContent>();
        }

        // Parse path to get ancestor IDs: "-1,123,456,789" -> [123, 456]
        // Skip root (-1) and exclude self
        // Use TryParse for resilience against malformed path data
        var ancestorIds = content.Path
            .Split(',')
            .Skip(1)  // Skip root (-1)
            .Select(s => int.TryParse(s, out var id) ? id : (int?)null)
            .Where(id => id.HasValue && id.Value != content.Id)  // Exclude nulls and self
            .Select(id => id!.Value)
            .ToArray();

        // Log warning if path appears malformed (expected ancestors but found none)
        if (ancestorIds.Length == 0 && content.Level > 1)
        {
            _logger.LogWarning(
                "Malformed path '{Path}' for content {ContentId} at level {Level} - expected {ExpectedCount} ancestors but parsed {ActualCount}",
                content.Path, content.Id, content.Level, content.Level - 1, ancestorIds.Length);
        }

        return GetByIds(ancestorIds);  // Single batch query instead of N+1
    }

    /// <inheritdoc />
    public IEnumerable<IContent> GetPagedChildren(int id, long pageIndex, int pageSize, out long totalChildren, IQuery<IContent>? filter = null, Ordering? ordering = null)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(Constants.Locks.ContentTree);

            // Create base query for parent constraint
            IQuery<IContent> query = Query<IContent>().Where(x => x.ParentId == id);

            // Pass both query and filter to repository - repository handles combination
            // The filter parameter is applied as an additional WHERE clause by the repository
            return DocumentRepository.GetPage(query, pageIndex, pageSize, out totalChildren, filter, ordering);
        }
    }

    /// <inheritdoc />
    public IEnumerable<IContent> GetPagedDescendants(int id, long pageIndex, int pageSize, out long totalChildren, IQuery<IContent>? filter = null, Ordering? ordering = null)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(Constants.Locks.ContentTree);

            if (id != Constants.System.Root)
            {
                var contentPath = _entityRepository.GetAllPaths(Constants.ObjectTypes.Document, id).FirstOrDefault();
                if (contentPath is null)
                {
                    totalChildren = 0;
                    return Enumerable.Empty<IContent>();
                }

                IQuery<IContent>? query = Query<IContent>();
                query?.Where(x => x.Path.SqlStartsWith($"{contentPath.Path},", TextColumnType.NVarchar));
                return DocumentRepository.GetPage(query, pageIndex, pageSize, out totalChildren, filter, ordering ?? Ordering.By("Path", Direction.Descending));
            }

            return DocumentRepository.GetPage(null, pageIndex, pageSize, out totalChildren, filter, ordering ?? Ordering.By("Path", Direction.Descending));
        }
    }

    /// <inheritdoc />
    public bool HasChildren(int id)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(Constants.Locks.ContentTree);
            IQuery<IContent> query = Query<IContent>().Where(x => x.ParentId == id);
            return DocumentRepository.Count(query) > 0;
        }
    }

    /// <inheritdoc />
    public bool Exists(int id)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(Constants.Locks.ContentTree);
            return DocumentRepository.Exists(id);
        }
    }

    /// <inheritdoc />
    public bool Exists(Guid key)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(Constants.Locks.ContentTree);
            return DocumentRepository.Exists(key);
        }
    }

    #endregion

    #region Save

    /// <inheritdoc />
    public OperationResult Save(IContent content, int? userId = null, ContentScheduleCollection? contentSchedule = null)
    {
        EventMessages eventMessages = EventMessagesFactory.Get();

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            scope.WriteLock(Constants.Locks.ContentTree);
            scope.ReadLock(Constants.Locks.Languages);  // Required for GetLanguageDetailsForAuditEntryLocked

            var result = SaveLocked(scope, content, userId ?? Constants.Security.SuperUserId, contentSchedule, eventMessages);

            scope.Complete();
            return result;
        }
    }

    /// <summary>
    /// Internal save implementation. Caller MUST hold scope with ContentTree write lock and Languages read lock.
    /// </summary>
    /// <param name="scope">The active scope (caller must hold write lock on ContentTree and read lock on Languages).</param>
    /// <param name="content">The content to save.</param>
    /// <param name="userId">The user ID performing the save.</param>
    /// <param name="contentSchedule">Optional content schedule.</param>
    /// <param name="eventMessages">Event messages for notifications.</param>
    /// <returns>The operation result.</returns>
    /// <remarks>
    /// <para><strong>Preconditions:</strong></para>
    /// <list type="bullet">
    ///   <item><description>Caller MUST hold an active scope with write lock on <c>Constants.Locks.ContentTree</c>.</description></item>
    ///   <item><description>Caller MUST hold an active scope with read lock on <c>Constants.Locks.Languages</c> (required for variant content audit entries).</description></item>
    ///   <item><description>This method does NOT create its own scope - it operates within the caller's scope.</description></item>
    /// </list>
    /// </remarks>
    private OperationResult SaveLocked(ICoreScope scope, IContent content, int userId,
        ContentScheduleCollection? contentSchedule, EventMessages eventMessages)
    {
        // Validate - content state is stable under lock
        PublishedState publishedState = content.PublishedState;
        if (publishedState != PublishedState.Published && publishedState != PublishedState.Unpublished)
        {
            throw new InvalidOperationException(
                $"Cannot save (un)publishing content with name: {content.Name} - and state: {content.PublishedState}, use the dedicated SavePublished method.");
        }

        if (content.Name != null && content.Name.Length > 255)
        {
            throw new InvalidOperationException(
                $"Content with the name {content.Name} cannot be more than 255 characters in length.");
        }

        var savingNotification = new ContentSavingNotification(content, eventMessages);
        if (scope.Notifications.PublishCancelable(savingNotification))
        {
            _logger.LogInformation("Save operation cancelled for content {ContentId} ({ContentName}) by notification handler",
                content.Id, content.Name);
            return OperationResult.Cancel(eventMessages);
        }

        if (content.HasIdentity == false)
        {
            content.CreatorId = userId;
        }

        content.WriterId = userId;

        // track the cultures that have changed
        List<string>? culturesChanging = content.ContentType.VariesByCulture()
            ? content.CultureInfos?.Values.Where(x => x.IsDirty()).Select(x => x.Culture).ToList()
            : null;

        DocumentRepository.Save(content);

        if (contentSchedule != null)
        {
            DocumentRepository.PersistContentSchedule(content, contentSchedule);
        }

        scope.Notifications.Publish(
            new ContentSavedNotification(content, eventMessages).WithStateFrom(savingNotification));

        scope.Notifications.Publish(
            new ContentTreeChangeNotification(content, TreeChangeTypes.RefreshNode, eventMessages));

        if (culturesChanging != null)
        {
            // Use locked variant - no nested scope
            var langs = GetLanguageDetailsForAuditEntryLocked(culturesChanging);
            Audit(AuditType.SaveVariant, userId, content.Id, $"Saved languages: {langs}", langs);
        }
        else
        {
            Audit(AuditType.Save, userId, content.Id);
        }

        return OperationResult.Succeed(eventMessages);
    }

    /// <inheritdoc />
    /// <remarks>
    /// <para><strong>Validation Parity Note:</strong></para>
    /// <para>This method intentionally does NOT validate <c>PublishedState</c> or name length,
    /// matching the original <c>ContentService</c> batch save behavior. Use single-item
    /// <see cref="Save(IContent, int?, ContentScheduleCollection?)"/> for full validation.</para>
    /// <para><strong>Behavioral Change (v1.5):</strong></para>
    /// <para>Lock is now acquired BEFORE sending notification, consistent with single-item Save.
    /// This ensures notification handlers see stable content state.</para>
    /// </remarks>
    public OperationResult Save(IEnumerable<IContent> contents, int userId = Constants.Security.SuperUserId)
    {
        EventMessages eventMessages = EventMessagesFactory.Get();
        IContent[] contentsA = contents.ToArray();

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            // Acquire lock FIRST for consistency with single-item Save
            // This ensures notification handlers see stable content state
            scope.WriteLock(Constants.Locks.ContentTree);

            var savingNotification = new ContentSavingNotification(contentsA, eventMessages);
            if (scope.Notifications.PublishCancelable(savingNotification))
            {
                _logger.LogInformation("Batch save operation cancelled for {ContentCount} content items by notification handler",
                    contentsA.Length);
                return OperationResult.Cancel(eventMessages);  // No scope.Complete() - consistent with single-item Save
            }

            foreach (IContent content in contentsA)
            {
                if (content.HasIdentity == false)
                {
                    content.CreatorId = userId;
                }

                content.WriterId = userId;

                DocumentRepository.Save(content);
            }

            scope.Notifications.Publish(
                new ContentSavedNotification(contentsA, eventMessages).WithStateFrom(savingNotification));

            scope.Notifications.Publish(
                new ContentTreeChangeNotification(contentsA, TreeChangeTypes.RefreshNode, eventMessages));

            // Note: Using contentsA.Length (item count), NOT contentIds.Length (string length)
            // This fixes a bug from the original ContentService that reported character count instead of item count
            Audit(AuditType.Save, userId, Constants.System.Root, $"Saved {contentsA.Length} content items");

            scope.Complete();
        }

        return OperationResult.Succeed(eventMessages);
    }

    #endregion

    #region Delete

    /// <inheritdoc />
    public OperationResult Delete(IContent content, int userId = Constants.Security.SuperUserId)
    {
        EventMessages eventMessages = EventMessagesFactory.Get();

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            if (scope.Notifications.PublishCancelable(new ContentDeletingNotification(content, eventMessages)))
            {
                _logger.LogInformation("Delete operation cancelled for content {ContentId} ({ContentName}) by notification handler",
                    content.Id, content.Name);
                scope.Complete();
                return OperationResult.Cancel(eventMessages);
            }

            scope.WriteLock(Constants.Locks.ContentTree);

            // if it's not trashed yet, and published, we should unpublish
            // but... Unpublishing event makes no sense (not going to cancel?) and no need to save
            // just raise the event
            if (content.Trashed == false && content.Published)
            {
                scope.Notifications.Publish(new ContentUnpublishedNotification(content, eventMessages));
            }

            DeleteLocked(scope, content, eventMessages);

            scope.Notifications.Publish(
                new ContentTreeChangeNotification(content, TreeChangeTypes.Remove, eventMessages));
            Audit(AuditType.Delete, userId, content.Id);

            scope.Complete();
        }

        return OperationResult.Succeed(eventMessages);
    }

    /// <summary>
    /// Deletes a content item and all its descendants. Internal locked operation.
    /// </summary>
    /// <param name="scope">The active scope (caller must hold write lock on ContentTree).</param>
    /// <param name="content">The content to delete.</param>
    /// <param name="evtMsgs">Event messages for notifications.</param>
    /// <remarks>
    /// <para><strong>Preconditions:</strong></para>
    /// <list type="bullet">
    ///   <item><description>Caller MUST hold an active scope with write lock on <c>Constants.Locks.ContentTree</c>.</description></item>
    ///   <item><description>This method does NOT create its own scope - it operates within the caller's scope.</description></item>
    /// </list>
    /// <para><strong>Safety Features:</strong></para>
    /// <list type="bullet">
    ///   <item><description>Maximum iteration limit (10,000) prevents infinite loops.</description></item>
    ///   <item><description>Empty batch check protects against race conditions.</description></item>
    /// </list>
    /// </remarks>
    private void DeleteLocked(ICoreScope scope, IContent content, EventMessages evtMsgs)
    {
        void DoDelete(IContent c)
        {
            DocumentRepository.Delete(c);
            scope.Notifications.Publish(new ContentDeletedNotification(c, evtMsgs));
        }

        const int pageSize = ContentServiceConstants.DefaultBatchPageSize;
        const int maxIterations = 10000; // Safety limit to prevent infinite loop
        var total = long.MaxValue;
        var iterations = 0;

        while (total > 0 && iterations++ < maxIterations)
        {
            // get descendants - ordered from deepest to shallowest
            // Uses GetPagedDescendantsLocked (no nested scope) since we already hold write lock
            IEnumerable<IContent> descendants = GetPagedDescendantsLocked(content.Id, 0, pageSize, out total);
            var batch = descendants.ToList();  // Materialize once

            // Exit if no results even though total > 0 (race condition protection)
            if (batch.Count == 0)
            {
                break;
            }

            foreach (IContent c in batch)
            {
                DoDelete(c);
            }
        }

        if (iterations >= maxIterations)
        {
            _logger.LogWarning("Delete operation for content {ContentId} reached max iterations ({MaxIterations})",
                content.Id, maxIterations);
        }

        DoDelete(content);
    }

    /// <summary>
    /// Gets paged descendants for internal use. MUST be called within an existing scope.
    /// </summary>
    /// <param name="id">The parent content ID.</param>
    /// <param name="pageIndex">Zero-based page index.</param>
    /// <param name="pageSize">Page size.</param>
    /// <param name="totalRecords">Total number of descendants (output).</param>
    /// <returns>The descendant documents, ordered from deepest to shallowest.</returns>
    /// <remarks>
    /// <para><strong>Preconditions:</strong></para>
    /// <list type="bullet">
    ///   <item><description>Caller MUST hold an active scope with appropriate locks (read or write).</description></item>
    ///   <item><description>This method does NOT create its own scope - it operates within the caller's scope.</description></item>
    /// </list>
    /// <para><strong>Usage:</strong></para>
    /// <para>Used internally by <see cref="DeleteLocked"/> which holds a write lock on ContentTree.
    /// Results are ordered by Path descending to ensure deepest descendants are processed first,
    /// allowing safe deletion from leaf to parent.</para>
    /// </remarks>
    private IEnumerable<IContent> GetPagedDescendantsLocked(int id, long pageIndex, int pageSize, out long totalRecords)
    {
        // No scope creation - assumes caller holds scope with proper locks
        if (id != Constants.System.Root)
        {
            var contentPath = _entityRepository.GetAllPaths(Constants.ObjectTypes.Document, id).FirstOrDefault();
            if (contentPath == null)
            {
                totalRecords = 0;
                return Enumerable.Empty<IContent>();
            }

            IQuery<IContent>? query = Query<IContent>();
            query?.Where(x => x.Path.SqlStartsWith($"{contentPath.Path},", TextColumnType.NVarchar));
            return DocumentRepository.GetPage(query, pageIndex, pageSize, out totalRecords, null, Ordering.By("Path", Direction.Descending));
        }

        return DocumentRepository.GetPage(null, pageIndex, pageSize, out totalRecords, null, Ordering.By("Path", Direction.Descending));
    }

    #endregion

    #region Private Helpers

    /// <summary>
    /// Gets a content type by alias. Creates its own scope.
    /// </summary>
    /// <param name="alias">The content type alias.</param>
    /// <returns>The content type.</returns>
    /// <exception cref="ArgumentException">Thrown when no content type with the alias exists.</exception>
    private IContentType GetContentType(string alias)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(Constants.Locks.ContentTypes);
            return GetContentTypeLocked(alias);
        }
    }

    /// <summary>
    /// Gets a content type by alias. Assumes caller holds scope with ContentTypes lock.
    /// </summary>
    /// <param name="alias">The content type alias.</param>
    /// <returns>The content type.</returns>
    /// <exception cref="ArgumentException">Thrown when no content type with the alias exists.</exception>
    /// <remarks>
    /// <para><strong>Preconditions:</strong></para>
    /// <list type="bullet">
    ///   <item><description>Caller MUST hold an active scope with read/write lock on <c>Constants.Locks.ContentTypes</c>.</description></item>
    ///   <item><description>This method does NOT create its own scope - it operates within the caller's scope.</description></item>
    /// </list>
    /// </remarks>
    private IContentType GetContentTypeLocked(string alias)
    {
        return _contentTypeRepository.Get(alias)
               ?? throw new ArgumentException($"No content type with alias '{alias}' exists.", nameof(alias));
    }

    /// <summary>
    /// Gets language ISO codes for audit entry. Creates its own scope.
    /// </summary>
    /// <param name="affectedCultures">The affected cultures.</param>
    /// <returns>Comma-separated language ISO codes.</returns>
    private string GetLanguageDetailsForAuditEntry(IEnumerable<string> affectedCultures)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(Constants.Locks.Languages);
            return GetLanguageDetailsForAuditEntryLocked(affectedCultures);
        }
    }

    /// <summary>
    /// Gets language ISO codes for audit entry. Assumes caller holds scope with Languages lock.
    /// </summary>
    /// <param name="affectedCultures">The affected cultures.</param>
    /// <returns>Comma-separated language ISO codes.</returns>
    /// <remarks>
    /// <para><strong>Preconditions:</strong></para>
    /// <list type="bullet">
    ///   <item><description>Caller MUST hold an active scope with read/write lock on <c>Constants.Locks.Languages</c>.</description></item>
    ///   <item><description>This method does NOT create its own scope - it operates within the caller's scope.</description></item>
    /// </list>
    /// </remarks>
    private string GetLanguageDetailsForAuditEntryLocked(IEnumerable<string> affectedCultures)
    {
        var languages = _languageRepository.GetMany();
        IEnumerable<string> languageIsoCodes = languages
            .Where(x => affectedCultures.InvariantContains(x.IsoCode))
            .Select(x => x.IsoCode);
        return string.Join(", ", languageIsoCodes);
    }

    #endregion
}
```

**Step 4: Verify the project builds**

Run: `dotnet build src/Umbraco.Core/Umbraco.Core.csproj`
Expected: Build succeeded.

**Step 5: Run the unit test**

Run: `dotnet test tests/Umbraco.Tests.UnitTests --filter "FullyQualifiedName~ContentCrudServiceTests"`
Expected: PASS

**Step 6: Commit**

```bash
git add src/Umbraco.Core/Services/ContentCrudService.cs tests/Umbraco.Tests.UnitTests/Umbraco.Core/Services/ContentCrudServiceTests.cs
git commit -m "$(cat <<'EOF'
feat(core): add ContentCrudService implementation for Phase 1

Implements IContentCrudService with full CRUD operations:
- Create: 6 overloads matching IContentService
- Read: GetById, GetByIds, GetRootContent, GetParent
- Save: Single and batch with notifications
- Delete: Cascade deletion with notifications

All methods maintain exact behavioral parity with ContentService.

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
EOF
)"
```

---

## Task 4: Register ContentCrudService in DI

**Files:**
- Modify: `src/Umbraco.Core/DependencyInjection/UmbracoBuilder.cs`

**Step 1: Find the IContentService registration**

The registration is at line 300:
```csharp
Services.AddUnique<IContentService, ContentService>();
```

**Step 2: Add IContentCrudService registration**

Add the following line immediately before the IContentService registration (around line 300):

```csharp
Services.AddUnique<IContentCrudService, ContentCrudService>();
```

**Step 3: Verify the project builds**

Run: `dotnet build src/Umbraco.Core/Umbraco.Core.csproj`
Expected: Build succeeded.

**Step 4: Write integration test to verify DI resolution**

```csharp
// Add to tests/Umbraco.Tests.Integration/Umbraco.Infrastructure/Services/ContentServiceRefactoringTests.cs

/// <summary>
/// Phase 1 Test: Verifies IContentCrudService is registered and resolvable from DI.
/// </summary>
[Test]
public void IContentCrudService_CanBeResolvedFromDI()
{
    // Act
    var crudService = GetRequiredService<IContentCrudService>();

    // Assert
    Assert.That(crudService, Is.Not.Null);
    Assert.That(crudService, Is.InstanceOf<ContentCrudService>());
}
```

**Step 5: Run the integration test**

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentServiceRefactoringTests.IContentCrudService_CanBeResolvedFromDI"`
Expected: PASS

**Step 6: Commit**

```bash
git add src/Umbraco.Core/DependencyInjection/UmbracoBuilder.cs tests/Umbraco.Tests.Integration/Umbraco.Infrastructure/Services/ContentServiceRefactoringTests.cs
git commit -m "$(cat <<'EOF'
feat(core): register IContentCrudService in DI container

Adds IContentCrudService registration to UmbracoBuilder alongside
IContentService. Both services are now resolvable from DI.

Includes integration test verifying successful resolution.

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
EOF
)"
```

---

## Task 5: Update ContentService to Delegate CRUD Operations

**Files:**
- Modify: `src/Umbraco.Core/Services/ContentService.cs`

**Step 1: Add IContentCrudService dependency to ContentService constructor**

Add the new dependency field to the class at line 50:

```csharp
private readonly Lazy<IContentCrudService> _crudServiceLazy;

// Property for convenient access (deferred resolution for both paths)
private IContentCrudService CrudService => _crudServiceLazy.Value;
```

Update the **primary constructor** to accept and wrap the dependency in Lazy:

```csharp
public ContentService(
    ICoreScopeProvider provider,
    ILoggerFactory loggerFactory,
    IEventMessagesFactory eventMessagesFactory,
    IDocumentRepository documentRepository,
    IEntityRepository entityRepository,
    IAuditService auditService,
    IContentTypeRepository contentTypeRepository,
    IDocumentBlueprintRepository documentBlueprintRepository,
    ILanguageRepository languageRepository,
    Lazy<IPropertyValidationService> propertyValidationService,
    IShortStringHelper shortStringHelper,
    ICultureImpactFactory cultureImpactFactory,
    IUserIdKeyResolver userIdKeyResolver,
    PropertyEditorCollection propertyEditorCollection,
    IIdKeyMap idKeyMap,
    IOptionsMonitor<ContentSettings> optionsMonitor,
    IRelationService relationService,
    IContentCrudService crudService)  // NEW PARAMETER - direct injection
    : base(provider, loggerFactory, eventMessagesFactory)
{
    // ... existing assignments ...
    ArgumentNullException.ThrowIfNull(crudService);
    // Wrap in Lazy for consistent access pattern (already resolved, so returns immediately)
    _crudServiceLazy = new Lazy<IContentCrudService>(() => crudService);
}
```

**Rationale:** Using a single `Lazy<IContentCrudService>` field simplifies the code and eliminates
the risk of NullReferenceException from the dual-field pattern (`_crudService ?? _crudServiceLazy.Value`).

**Step 2: Update CRUD methods to delegate**

Replace the following methods to delegate to `CrudService` (property that handles both direct and lazy resolution):

```csharp
// Create methods - delegate to CrudService
public IContent Create(string name, Guid parentId, string contentTypeAlias, int userId = Constants.Security.SuperUserId)
    => CrudService.Create(name, parentId, contentTypeAlias, userId);

public IContent Create(string name, int parentId, string contentTypeAlias, int userId = Constants.Security.SuperUserId)
    => CrudService.Create(name, parentId, contentTypeAlias, userId);

public IContent Create(string name, int parentId, IContentType contentType, int userId = Constants.Security.SuperUserId)
    => CrudService.Create(name, parentId, contentType, userId);

public IContent Create(string name, IContent? parent, string contentTypeAlias, int userId = Constants.Security.SuperUserId)
    => CrudService.Create(name, parent, contentTypeAlias, userId);

public IContent CreateAndSave(string name, int parentId, string contentTypeAlias, int userId = Constants.Security.SuperUserId)
    => CrudService.CreateAndSave(name, parentId, contentTypeAlias, userId);

public IContent CreateAndSave(string name, IContent parent, string contentTypeAlias, int userId = Constants.Security.SuperUserId)
    => CrudService.CreateAndSave(name, parent, contentTypeAlias, userId);

// Read methods - delegate to CrudService
public IContent? GetById(int id)
    => CrudService.GetById(id);

public IContent? GetById(Guid key)
    => CrudService.GetById(key);

public IEnumerable<IContent> GetByIds(IEnumerable<int> ids)
    => CrudService.GetByIds(ids);

public IEnumerable<IContent> GetByIds(IEnumerable<Guid> ids)
    => CrudService.GetByIds(ids);

public IEnumerable<IContent> GetRootContent()
    => CrudService.GetRootContent();

public IContent? GetParent(int id)
    => CrudService.GetParent(id);

public IContent? GetParent(IContent? content)
    => CrudService.GetParent(content);

// Tree traversal methods - delegate to CrudService
public IEnumerable<IContent> GetAncestors(int id)
    => CrudService.GetAncestors(id);

public IEnumerable<IContent> GetAncestors(IContent content)
    => CrudService.GetAncestors(content);

public IEnumerable<IContent> GetPagedChildren(int id, long pageIndex, int pageSize, out long totalChildren, IQuery<IContent>? filter = null, Ordering? ordering = null)
    => CrudService.GetPagedChildren(id, pageIndex, pageSize, out totalChildren, filter, ordering);

public IEnumerable<IContent> GetPagedDescendants(int id, long pageIndex, int pageSize, out long totalChildren, IQuery<IContent>? filter = null, Ordering? ordering = null)
    => CrudService.GetPagedDescendants(id, pageIndex, pageSize, out totalChildren, filter, ordering);

public bool HasChildren(int id)
    => CrudService.HasChildren(id);

public bool Exists(int id)
    => CrudService.Exists(id);

public bool Exists(Guid key)
    => CrudService.Exists(key);

// Save methods - delegate to CrudService
public OperationResult Save(IContent content, int? userId = null, ContentScheduleCollection? contentSchedule = null)
    => CrudService.Save(content, userId, contentSchedule);

public OperationResult Save(IEnumerable<IContent> contents, int userId = Constants.Security.SuperUserId)
    => CrudService.Save(contents, userId);

// Delete method - delegate to CrudService
public OperationResult Delete(IContent content, int userId = Constants.Security.SuperUserId)
    => CrudService.Delete(content, userId);
```

**Step 3: Update obsolete constructors to use Lazy<T>**

The obsolete constructors need to defer resolution of IContentCrudService to avoid circular dependencies during startup. These constructors should NOT chain to the new primary constructor. Instead, they have their own complete body with lazy resolution.

**Why Full Body (Not Chaining)?**
- The primary constructor now requires `IContentCrudService` which obsolete callers don't have
- Chaining would require passing null or a placeholder, which is error-prone
- Full body gives explicit control over initialization

**Complete Obsolete Constructor Specification:**

```csharp
[Obsolete("Use the non-obsolete constructor instead. Scheduled removal in v19.")]
public ContentService(
    ICoreScopeProvider provider,
    ILoggerFactory loggerFactory,
    IEventMessagesFactory eventMessagesFactory,
    IDocumentRepository documentRepository,
    IEntityRepository entityRepository,
    IAuditRepository auditRepository,  // Old parameter (not used in new pattern, but kept for signature compatibility)
    IContentTypeRepository contentTypeRepository,
    IDocumentBlueprintRepository documentBlueprintRepository,
    ILanguageRepository languageRepository,
    Lazy<IPropertyValidationService> propertyValidationService,
    IShortStringHelper shortStringHelper,
    ICultureImpactFactory cultureImpactFactory,
    IUserIdKeyResolver userIdKeyResolver,
    PropertyEditorCollection propertyEditorCollection,
    IIdKeyMap idKeyMap,
    IOptionsMonitor<ContentSettings> optionsMonitor,
    IRelationService relationService)
    : base(provider, loggerFactory, eventMessagesFactory)
{
    // All existing field assignments from current ContentService constructor...
    _documentRepository = documentRepository ?? throw new ArgumentNullException(nameof(documentRepository));
    _entityRepository = entityRepository ?? throw new ArgumentNullException(nameof(entityRepository));
    _contentTypeRepository = contentTypeRepository ?? throw new ArgumentNullException(nameof(contentTypeRepository));
    _documentBlueprintRepository = documentBlueprintRepository ?? throw new ArgumentNullException(nameof(documentBlueprintRepository));
    _languageRepository = languageRepository ?? throw new ArgumentNullException(nameof(languageRepository));
    _propertyValidationService = propertyValidationService ?? throw new ArgumentNullException(nameof(propertyValidationService));
    _shortStringHelper = shortStringHelper ?? throw new ArgumentNullException(nameof(shortStringHelper));
    _cultureImpactFactory = cultureImpactFactory ?? throw new ArgumentNullException(nameof(cultureImpactFactory));
    _userIdKeyResolver = userIdKeyResolver ?? throw new ArgumentNullException(nameof(userIdKeyResolver));
    _propertyEditorCollection = propertyEditorCollection ?? throw new ArgumentNullException(nameof(propertyEditorCollection));
    _idKeyMap = idKeyMap ?? throw new ArgumentNullException(nameof(idKeyMap));
    _contentSettings = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
    _relationService = relationService ?? throw new ArgumentNullException(nameof(relationService));
    _logger = loggerFactory.CreateLogger<ContentService>();

    // NEW: Lazy resolution of IContentCrudService
    _crudServiceLazy = new Lazy<IContentCrudService>(() =>
        StaticServiceProvider.Instance.GetRequiredService<IContentCrudService>(),
        LazyThreadSafetyMode.ExecutionAndPublication);
}
```

**Note:** Both obsolete constructors (if there are multiple) should follow this same pattern - complete body with all field assignments and lazy `_crudServiceLazy` initialization.

**Why Lazy<T>?**
- `StaticServiceProvider.Instance` may not be fully initialized during constructor execution
- Circular dependency risk: ContentService depends on IContentCrudService, and both share repositories
- Lazy defers resolution until first actual usage, by which time DI is fully initialized
- `LazyThreadSafetyMode.ExecutionAndPublication` ensures thread-safe initialization

**Step 4: Verify the project builds**

Run: `dotnet build src/Umbraco.Core/Umbraco.Core.csproj`
Expected: Build succeeded.

**Step 5: Run all ContentService tests**

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentService"`
Expected: All tests PASS (behavioral parity maintained)

**Step 6: Run refactoring-specific tests**

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentServiceRefactoringTests"`
Expected: All 15+ tests PASS

**Step 7: Commit**

```bash
git add src/Umbraco.Core/Services/ContentService.cs
git commit -m "$(cat <<'EOF'
refactor(core): delegate ContentService CRUD to ContentCrudService

ContentService facade now delegates CRUD operations:
- Create (6 overloads) -> IContentCrudService
- GetById, GetByIds, GetRootContent, GetParent -> IContentCrudService
- Save (2 overloads) -> IContentCrudService
- Delete -> IContentCrudService

All existing behavior maintained through delegation.
ContentService line count reduced by ~400 lines.

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
EOF
)"
```

---

## Task 6: Add Benchmark Regression Enforcement

**Files:**
- Modify: `tests/Umbraco.Tests.Integration/Testing/ContentServiceBenchmarkBase.cs`
- Modify: `tests/Umbraco.Tests.Integration/Umbraco.Infrastructure/Services/ContentServiceRefactoringBenchmarks.cs`

This task implements automated performance regression detection by asserting benchmarks don't exceed 20% of baseline values.

**Step 1: Add AssertNoRegression method to ContentServiceBenchmarkBase**

Add the following fields and methods to `ContentServiceBenchmarkBase.cs`:

```csharp
// Add to class fields
private const double DefaultRegressionThreshold = 20.0;

// Allow CI override via environment variable
private static readonly double RegressionThreshold =
    double.TryParse(Environment.GetEnvironmentVariable("BENCHMARK_REGRESSION_THRESHOLD"), out var t)
        ? t
        : DefaultRegressionThreshold;

// Optional strict mode: fail if baseline is missing (useful for CI)
private static readonly bool RequireBaseline =
    bool.TryParse(Environment.GetEnvironmentVariable("BENCHMARK_REQUIRE_BASELINE"), out var b) && b;

// Thread-safe lazy initialization of repository root
private static readonly Lazy<string> _repositoryRoot = new(FindRepositoryRoot);

// Thread-safe lazy initialization of baseline data
private static readonly Lazy<Dictionary<string, BenchmarkResult>> _baselineLoader =
    new(() => LoadBaselineInternal(), LazyThreadSafetyMode.ExecutionAndPublication);

private static Dictionary<string, BenchmarkResult> Baseline => _baselineLoader.Value;

private static string BaselinePath => Path.Combine(_repositoryRoot.Value, "docs", "plans", "baseline-phase0.json");

/// <summary>
/// Finds the repository root by searching for umbraco.sln.
/// </summary>
private static string FindRepositoryRoot()
{
    var dir = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
    while (dir != null)
    {
        if (File.Exists(Path.Combine(dir.FullName, "umbraco.sln")))
        {
            return dir.FullName;
        }
        dir = dir.Parent;
    }
    throw new InvalidOperationException(
        $"Cannot find repository root (umbraco.sln) starting from {TestContext.CurrentContext.TestDirectory}");
}

/// <summary>
/// Records a benchmark and asserts no regression beyond the threshold.
/// </summary>
/// <param name="name">Benchmark name (must match baseline JSON key).</param>
/// <param name="elapsedMs">Measured elapsed time in milliseconds.</param>
/// <param name="itemCount">Number of items processed.</param>
/// <param name="thresholdPercent">Maximum allowed regression percentage (default: 20%, configurable via BENCHMARK_REGRESSION_THRESHOLD env var).</param>
protected void AssertNoRegression(string name, long elapsedMs, int itemCount, double thresholdPercent = -1)
{
    RecordBenchmark(name, elapsedMs, itemCount);

    // Use environment-configurable threshold if not explicitly specified
    var effectiveThreshold = thresholdPercent < 0 ? RegressionThreshold : thresholdPercent;

    if (Baseline.TryGetValue(name, out var baselineResult))
    {
        var maxAllowed = baselineResult.ElapsedMs * (1 + effectiveThreshold / 100);

        if (elapsedMs > maxAllowed)
        {
            var regressionPct = ((double)(elapsedMs - baselineResult.ElapsedMs) / baselineResult.ElapsedMs) * 100;
            Assert.Fail(
                $"Performance regression detected for '{name}': " +
                $"{elapsedMs}ms exceeds threshold of {maxAllowed:F0}ms " +
                $"(baseline: {baselineResult.ElapsedMs}ms, regression: +{regressionPct:F1}%, threshold: {effectiveThreshold}%)");
        }

        TestContext.WriteLine($"[REGRESSION_CHECK] {name}: PASS ({elapsedMs}ms <= {maxAllowed:F0}ms, baseline: {baselineResult.ElapsedMs}ms, threshold: {effectiveThreshold}%)");
    }
    else if (RequireBaseline)
    {
        Assert.Fail($"No baseline entry found for '{name}' and BENCHMARK_REQUIRE_BASELINE=true");
    }
    else
    {
        TestContext.WriteLine($"[REGRESSION_CHECK] {name}: SKIPPED (no baseline entry)");
    }
}

/// <summary>
/// Measures, records, and asserts no regression for the given action.
/// </summary>
protected long MeasureAndAssertNoRegression(string name, int itemCount, Action action, bool skipWarmup = false, double thresholdPercent = -1)
{
    // Warmup iteration (skip for destructive operations)
    if (!skipWarmup)
    {
        try { action(); }
        catch (Exception ex)
        {
            TestContext.WriteLine($"[WARMUP] {name} warmup failed: {ex.Message}");
        }
    }

    var sw = Stopwatch.StartNew();
    action();
    sw.Stop();

    AssertNoRegression(name, sw.ElapsedMilliseconds, itemCount, thresholdPercent);
    return sw.ElapsedMilliseconds;
}

private static Dictionary<string, BenchmarkResult> LoadBaselineInternal()
{
    if (!File.Exists(BaselinePath))
    {
        TestContext.WriteLine($"[BASELINE] File not found: {BaselinePath}");
        return new Dictionary<string, BenchmarkResult>();
    }

    try
    {
        var json = File.ReadAllText(BaselinePath);
        var results = JsonSerializer.Deserialize<List<BenchmarkResult>>(json) ?? new List<BenchmarkResult>();
        TestContext.WriteLine($"[BASELINE] Loaded {results.Count} baseline entries from {BaselinePath}");
        return results.ToDictionary(r => r.Name, r => r);
    }
    catch (Exception ex)
    {
        TestContext.WriteLine($"[BASELINE] Failed to load baseline: {ex.Message}");
        return new Dictionary<string, BenchmarkResult>();
    }
}
```

**Step 2: Replace RecordBenchmark with AssertNoRegression in Phase 1 CRUD benchmarks**

**Replace** `RecordBenchmark(...)` calls with `AssertNoRegression(...)` calls in the following 10 Phase 1 CRUD benchmarks. The `AssertNoRegression` method internally records the benchmark AND asserts no regression, so you should NOT call both - only call `AssertNoRegression`:

Update the following benchmarks in `ContentServiceRefactoringBenchmarks.cs`:

| Benchmark | Baseline (ms) | Max Allowed (ms) at 20% |
|-----------|---------------|-------------------------|
| `Save_SingleItem` | 7 | 8.4 |
| `Save_BatchOf100` | 676 | 811.2 |
| `Save_BatchOf1000` | 7649 | 9178.8 |
| `GetById_Single` | 8 | 9.6 |
| `GetByIds_BatchOf100` | 14 | 16.8 |
| `Delete_SingleItem` | 35 | 42 |
| `Delete_WithDescendants` | 243 | 291.6 |
| `GetAncestors_DeepHierarchy` | 31 | 37.2 |
| `GetPagedChildren_100Items` | 16 | 19.2 |
| `GetPagedDescendants_DeepTree` | 25 | 30 |

Example updated benchmark:

```csharp
/// <summary>
/// Benchmark 1: Single content save latency.
/// </summary>
[Test]
[LongRunning]
public void Benchmark_Save_SingleItem()
{
    // Warmup with throwaway content
    var warmupContent = ContentBuilder.CreateSimpleContent(ContentType, "Warmup_SingleItem", -1);
    ContentService.Save(warmupContent);

    // Measured run with fresh content
    var content = ContentBuilder.CreateSimpleContent(ContentType, "BenchmarkSingle", -1);
    var sw = Stopwatch.StartNew();
    ContentService.Save(content);
    sw.Stop();

    // Gate: Fail if >20% regression from baseline
    AssertNoRegression("Save_SingleItem", sw.ElapsedMilliseconds, 1);

    Assert.That(content.Id, Is.GreaterThan(0));
}
```

**Step 3: Add required using statements**

Add to `ContentServiceBenchmarkBase.cs`:
```csharp
using System.Text.Json;
```

**Step 4: Verify benchmarks build and pass**

Run: `dotnet build tests/Umbraco.Tests.Integration`
Expected: Build succeeded.

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "Category=Benchmark&FullyQualifiedName~ContentServiceRefactoringBenchmarks" --no-build`
Expected: All 10 CRUD benchmarks pass regression check.

**Step 5: Commit**

```bash
git add tests/Umbraco.Tests.Integration/Testing/ContentServiceBenchmarkBase.cs tests/Umbraco.Tests.Integration/Umbraco.Infrastructure/Services/ContentServiceRefactoringBenchmarks.cs
git commit -m "$(cat <<'EOF'
test: add benchmark regression enforcement for Phase 1

Adds AssertNoRegression method to ContentServiceBenchmarkBase:
- Loads baseline from baseline-phase0.json
- Fails test if benchmark exceeds 20% of baseline
- Logs regression check status for visibility

Updates 10 Phase 1 CRUD benchmarks to use regression assertions:
- Save_SingleItem, Save_BatchOf100, Save_BatchOf1000
- GetById_Single, GetByIds_BatchOf100
- Delete_SingleItem, Delete_WithDescendants
- GetAncestors_DeepHierarchy, GetPagedChildren_100Items, GetPagedDescendants_DeepTree

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
EOF
)"
```

---

## Task 7: Run Phase 1 Gate Tests

**Files:**
- None (verification only)

**Step 1: Run all ContentService tests**

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentService"`
Expected: All tests PASS

**Step 2: Run benchmarks to verify no regression**

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "Category=Benchmark&FullyQualifiedName~ContentServiceRefactoringBenchmarks"`
Expected: All benchmarks pass with regression assertions (no >20% regression)

**Step 3: Verify line count reduction**

Run: `wc -l src/Umbraco.Core/Services/ContentService.cs`
Expected: ~3400 lines (reduced from 3823)

**Step 4: Create Phase 1 completion tag**

```bash
git tag phase-1-crud-extraction -m "Phase 1 complete: CRUD operations extracted to ContentCrudService"
```

---

## Task 8: Update Phase Tracking Documentation

**Files:**
- Modify: `docs/plans/2025-12-19-contentservice-refactor-design.md`

**Step 1: Update the Implementation Order table**

Mark Phase 1 as complete in the design document.

**Step 2: Commit**

```bash
git add docs/plans/2025-12-19-contentservice-refactor-design.md
git commit -m "$(cat <<'EOF'
docs: mark Phase 1 complete in design document

Phase 1 (CRUD Service) successfully implemented:
- ContentServiceBase abstract class created
- IContentCrudService interface defined
- ContentCrudService implementation complete
- ContentService facade updated to delegate
- All tests passing

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
EOF
)"
```

---

## Summary

Phase 1 delivers:

| Deliverable | Description |
|-------------|-------------|
| `ContentServiceBase.cs` | Abstract base class with shared infrastructure |
| `ContentServiceConstants.cs` | Shared constants (DefaultBatchPageSize) |
| `IContentCrudService.cs` | Interface for CRUD operations (21 methods) |
| `ContentCrudService.cs` | Implementation of CRUD operations |
| `ContentCrudServiceTests.cs` | Unit test for service construction |
| DI Registration | `IContentCrudService` registered in `UmbracoBuilder` |
| ContentService Delegation | Facade delegates CRUD to new service |
| ~500 lines removed | From ContentService (reduced complexity) |

**Total new files:** 5
**Total modified files:** 3
**Estimated ContentService line reduction:** ~500 lines (from 3823 to ~3300)

### Interface Methods (23 public)
- **Create (6):** 4 Create overloads + 2 CreateAndSave overloads
- **Read (7):** GetById (2), GetByIds (2), GetRootContent, GetParent (2)
- **Read Tree (5):** GetAncestors (2), GetPagedChildren, GetPagedDescendants, HasChildren
- **Exists (2):** Exists by int, Exists by Guid
- **Save (2):** Single and batch save
- **Delete (1):** Cascade delete

*Note: `GetPagedDescendantsLocked` and `SaveLocked` are internal helper methods, not part of the public interface.*

---

## Critical Review Changes Applied

The following changes were incorporated from `2025-12-20-contentservice-refactor-phase1-implementation-critical-review-1.md`:

### High Priority (Required before implementation)

| Issue | Fix Applied |
|-------|-------------|
| **2.1 Nested scope in DeleteLocked** | Renamed `GetPagedDescendants` to `GetPagedDescendantsLocked`, removed scope creation. Method now assumes caller holds scope with proper locks. |
| **2.2 Circular DI dependency** | Added `Lazy<IContentCrudService>` pattern for obsolete constructors. Primary constructor takes direct injection, obsolete constructors use lazy resolution. Added `CrudService` property for unified access. |
| **2.3 Batch Save audit bug** | Changed `contentIds.Length` (string length) to `contentsA.Length` (item count). Fixes bug from original ContentService. |

### Medium Priority (Behavioral correctness)

| Issue | Fix Applied |
|-------|-------------|
| **2.4 Misleading null parent error** | Added explicit null check with descriptive error in `Create(Guid parentId, ...)`: "No content with key '{parentId}' exists." |
| **2.5 Missing trashed parent check** | Added `parent.Trashed` validation in both `CreateAndSave` overloads. Prevents creating content under trashed parents. |
| **2.6 Incomplete interface** | Added `GetAncestors(int)`, `GetAncestors(IContent)`, `GetPagedChildren`, `GetPagedDescendants` to interface and implementation. |

### Low Priority (Code quality)

| Issue | Fix Applied |
|-------|-------------|
| **3.1 Inconsistent null checks** | Standardized on `is null` pattern throughout |
| **3.2 Magic number** | Added `ContentServiceConstants.DefaultBatchPageSize = 500` |
| **3.3 Array allocation** | Added `ids as int[] ??` pattern to avoid allocation if input is already an array |

---

## Critical Review 2 Changes Applied

The following changes were incorporated from `2025-12-20-contentservice-refactor-phase1-implementation-critical-review-2.md`:

### P0 - Critical (Must fix before implementation)

| Issue | Fix Applied |
|-------|-------------|
| **2.1 GetAncestors N+1 Query** | Replaced iterative `GetParent` loop with single batch query using `Path` parsing. Now calls `GetByIds(ancestorIds)` instead of N separate database queries. |
| **2.2 Incomplete Interface Parity** | Added `HasChildren(int)`, `Exists(int)`, and `Exists(Guid)` to `IContentCrudService` interface with corresponding implementations. |
| **2.3 Filter Silently Ignored** | Removed misleading comment in `GetPagedChildren`. Clarified that repository handles `query` and `filter` combination internally. |
| **Section 5 Benchmark Enforcement** | Added new Task 6 with `AssertNoRegression` implementation in `ContentServiceBenchmarkBase`. 10 Phase 1 CRUD benchmarks now fail if >20% slower than baseline. |

### P1 - High (Should fix)

| Issue | Fix Applied |
|-------|-------------|
| **2.4 Delete Loop Race Condition** | Added `maxIterations = 10000` safety limit and empty-batch break condition. Logs warning if max iterations reached. |
| **2.5 Missing Operation Logging** | Added `_logger.LogInformation` for Save and Delete cancellations by notification handlers. |

### P2 - Medium (Recommended)

| Issue | Fix Applied |
|-------|-------------|
| **3.2 CreateAndSave Duplication** | Extracted shared logic to `CreateAndSaveInternal` private helper method. Both public overloads now delegate to this helper. |
| **3.4 Test Coverage Gap** | Added 6 behavioral unit tests: constructor, invalid parent, null content type, non-existent ID, empty batch save, Exists, HasChildren. |
| **3.3 Internal Method Documentation** | Added comprehensive `<remarks>` sections documenting preconditions for `DeleteLocked` and `GetPagedDescendantsLocked`. |

---

## Critical Review 3 Changes Applied

The following changes were incorporated from `2025-12-20-contentservice-refactor-phase1-implementation-critical-review-3.md`:

### P0 - Critical (Must fix before implementation)

| Issue | Fix Applied |
|-------|-------------|
| **2.1 Synchronous Async Wrapper Deadlock** | Updated `Audit()` and `AuditAsync()` to use `ConfigureAwait(false)` to avoid capturing synchronization context and prevent deadlocks. Added TODO comment for future sync overloads. |
| **2.2 Nested Scope Creation** | Added `GetContentTypeLocked()` and `GetLanguageDetailsForAuditEntryLocked()` private methods that assume caller holds scope. `Save()` now uses `GetLanguageDetailsForAuditEntryLocked()` to avoid nested scope creation. |
| **2.3 Interface Versioning Policy** | Replaced "Default Interface Implementations" section with "Interface Extensibility Model" documenting that `ContentServiceBase` inheritance is required. Added implementation warning to `IContentCrudService` XML documentation. |
| **2.4 Validation Before Lock Race Condition** | Moved `Save()` validation (PublishedState and name length checks) inside the locked section, after acquiring `WriteLock`. |

### P1 - High (Should fix)

| Issue | Fix Applied |
|-------|-------------|
| **3.1 Thread-Unsafe Baseline Loading** | Replaced mutable `_baseline` field with `static readonly Lazy<Dictionary<string, BenchmarkResult>>` for thread-safe initialization using `LazyThreadSafetyMode.ExecutionAndPublication`. |
| **3.2 Brittle Baseline Path** | Replaced 5-level relative path navigation with `FindRepositoryRoot()` method that searches for `umbraco.sln`. Uses `Lazy<string>` for thread-safe caching. |
| **3.3 GetByIds Dictionary Key Collision** | Changed `ToDictionary()` to `GroupBy().ToDictionary()` pattern to handle potential duplicate keys from repository gracefully. |
| **3.4 Missing ContentSchedule Batch Save Documentation** | Added XML `<remarks>` to batch `Save()` method documenting that content schedules are not supported. |

### P2 - Medium (Recommended)

| Issue | Fix Applied |
|-------|-------------|
| **4.1 Unit Test Mock Setup Repetition** | Extracted `CreateMockScopeWithReadLock()` and `CreateMockScopeWithWriteLock()` helper methods. All 6 tests updated to use helpers. |
| **4.2 Benchmark Threshold Configurability** | Added `RegressionThreshold` static field that reads from `BENCHMARK_REGRESSION_THRESHOLD` environment variable, defaulting to 20%. |
| **4.3 Missing Test Category** | Added `[Category("UnitTest")]` attribute to `ContentCrudServiceTests` test fixture. |
| **4.4 Summary Table File Count** | Updated summary to "5 new files" (was "4 new files"). |
| **4.5 GetAncestors Return Order Documentation** | Updated documentation to "from root to parent (closest to root first)" to match actual implementation behavior. |

---

## Critical Review 4 Changes Applied

The following changes were incorporated from `2025-12-20-contentservice-refactor-phase1-implementation-critical-review-4.md`:

### P0 - Critical (Must fix before implementation)

| Issue | Fix Applied |
|-------|-------------|
| **2.1 CreateAndSaveInternal Creates Nested Scopes via Save()** | Extracted `SaveLocked()` private method that operates within caller's scope. `CreateAndSaveInternal` now calls `SaveLocked()` instead of `Save()`. |
| **2.2 GetContentType Creates Nested Scope When Called From CreateAndSaveInternal** | `CreateAndSaveInternal` now acquires both `ContentTree` (write) and `ContentTypes` (read) locks upfront. Uses `GetContentTypeLocked()` instead of `GetContentType()`. |
| **2.3 GetAncestors Path Parsing Can Throw Unhandled FormatException** | Changed `int.Parse` to `int.TryParse` with logging. Returns empty for malformed segments instead of throwing. Logs warning when path appears malformed (expected ancestors but found none). |

### P1 - High (Should fix)

| Issue | Fix Applied |
|-------|-------------|
| **3.1 Lock Acquisition Timing Inconsistency Between Single and Batch Save** | Batch Save now acquires `WriteLock` BEFORE sending notification, consistent with single-item Save. Added `<remarks>` documenting this as a v1.5 behavioral change. |
| **3.2 Unit Test Missing IsolationLevel Import** | Added `using IsolationLevel = System.Data.IsolationLevel;` to test file imports. |
| **3.3 Batch Save Missing Validation That Exists in Single Save** | Added `<remarks>` documenting that batch Save intentionally does NOT validate `PublishedState` or name length, matching original `ContentService` behavior for parity. |

### P2 - Medium (Recommended)

| Issue | Fix Applied |
|-------|-------------|
| **4.1 GetByIds Nullable Handling Inconsistency** | Added `if (items is not null)` check to int overload, matching the Guid overload pattern. |
| **4.2 Interface Method Count Discrepancy in Summary** | Fixed method count from "24 total" to "23 public" with breakdown by category. Clarified that `SaveLocked` and `GetPagedDescendantsLocked` are internal helpers. |
| **4.3 Warmup Exception Silently Swallowed** | Added `TestContext.WriteLine($"[WARMUP] {name} warmup failed: {ex.Message}")` to log warmup failures at Debug level. |
| **4.5 Task 5 Lazy Pattern Could Be Simplified** | Simplified to single `Lazy<IContentCrudService>` field. Primary constructor wraps injected service in Lazy for consistent access pattern. Eliminates NullReferenceException risk from dual-field pattern. |

### Questions Resolved

| Question | Resolution |
|----------|------------|
| **Nested Scope Intent** | Not intentional. Fixed by extracting `SaveLocked()`. |
| **Validation Timing Change** | Documented as intentional behavioral change for safety. Lock-first ensures stable content state during validation. |
| **ContentSchedule Parity** | Documented that batch Save doesn't support schedules (matches original ContentService). |
| **Lock Ordering Strategy** | Documented in `CreateAndSaveInternal` that `ContentTree` (write) is acquired before `ContentTypes` (read), consistent with lock hierarchy. |

---

## Critical Review 5 Changes Applied

The following changes were incorporated from `2025-12-20-contentservice-refactor-phase1-implementation-critical-review-5.md`:

### P0 - Critical (Must fix before implementation)

| Issue | Fix Applied |
|-------|-------------|
| **2.1 Missing Languages Lock When Saving Variant Content** | `Save()` now acquires `scope.ReadLock(Constants.Locks.Languages)` after `ContentTree` write lock. `CreateAndSaveInternal()` also acquires Languages read lock. `SaveLocked` precondition documentation updated to require Languages lock. |
| **2.2 Batch Save Inconsistent scope.Complete() on Cancellation** | Removed `scope.Complete()` from batch Save cancellation path. Now consistent with single-item Save (no scope.Complete() on cancel). |

### P1 - High (Should fix)

| Issue | Fix Applied |
|-------|-------------|
| **3.1 Task 5 Obsolete Constructor Modification Underspecified** | Added complete obsolete constructor body specification showing full field assignments and lazy IContentCrudService resolution. Documented that obsolete constructors should NOT chain to primary constructor. |
| **3.2 Unit Tests Missing Variant Culture Save Path Coverage** | Added `Save_WithVariantContent_CallsLanguageRepository` unit test that mocks variant content and verifies language repository is called for variant saves. |

### P2 - Medium (Recommended)

| Issue | Fix Applied |
|-------|-------------|
| **4.1 Missing Using Statements in Code Samples** | Added `using Umbraco.Cms.Core.Persistence.Querying;` to ContentCrudService.cs for `TextColumnType`, `Direction`, `Ordering`. |
| **4.2 RecordBenchmark vs AssertNoRegression Clarification** | Updated Task 6 Step 2 text to explicitly state "Replace" instead of "Update", clarifying that `AssertNoRegression` internally calls `RecordBenchmark`. |
| **4.3 GetAncestors Warning Log Could Be Noisy** | Updated log message to include expected vs actual ancestor count: `"expected {ExpectedCount} ancestors but parsed {ActualCount}"`. |
| **4.4 Benchmark CI Failure Mode Consideration** | Added `RequireBaseline` static field that reads `BENCHMARK_REQUIRE_BASELINE` environment variable. When true, missing baseline entries cause test failure instead of skip. |

---

## Implementation Checklist

Before proceeding to implementation, verify:

### From Critical Review 1 & 2
- [x] `GetAncestors` uses `Path` parsing with batch `GetByIds`
- [x] `IContentCrudService` includes `HasChildren(int)`, `Exists(int)`, `Exists(Guid)`
- [x] `GetPagedChildren`/`GetPagedDescendants` pass both `query` and `filter` to repository (no misleading comments)
- [x] Task 6 added for `ContentServiceBenchmarkBase.AssertNoRegression` implementation
- [x] 10 Phase 1 CRUD benchmarks use `AssertNoRegression` instead of `RecordBenchmark`
- [x] `DeleteLocked` has iteration bound (10,000) and empty-batch exit condition
- [x] Save/Delete operations log cancellations
- [x] `CreateAndSaveInternal` helper extracts common logic
- [x] Unit tests cover behavioral edge cases (6 tests)
- [x] Internal methods have precondition documentation

### From Critical Review 3
- [x] `Audit()` uses `ConfigureAwait(false)` to avoid deadlocks
- [x] `GetContentTypeLocked()` and `GetLanguageDetailsForAuditEntryLocked()` variants added
- [x] Versioning Strategy documents `ContentServiceBase` inheritance requirement
- [x] `IContentCrudService` XML docs include implementation warning
- [x] `Save()` validation moved inside locked section
- [x] Baseline loading thread-safe with `Lazy<T>`
- [x] Repository root detection for baseline path
- [x] `GetByIds` uses `GroupBy` to handle duplicate keys
- [x] Batch `Save` documents schedule limitation
- [x] Unit test mock helpers extracted
- [x] `[Category("UnitTest")]` added to test fixture
- [x] Summary table shows "5 new files"
- [x] `GetAncestors` documentation fixed ("from root to parent")
- [x] Regression threshold configurable via environment variable

### From Critical Review 4
- [x] `SaveLocked()` extracted to eliminate nested scope in `CreateAndSaveInternal`
- [x] `CreateAndSaveInternal` acquires both locks upfront (`ContentTree` + `ContentTypes`)
- [x] `CreateAndSaveInternal` uses `GetContentTypeLocked()` instead of `GetContentType()`
- [x] `GetAncestors` uses `TryParse` with logging for malformed paths
- [x] Batch Save lock ordering aligned with single Save (lock before notification)
- [x] Batch Save validation parity documented in `<remarks>`
- [x] `IsolationLevel` import added to unit test file
- [x] `GetByIds` int overload has consistent null handling
- [x] Method count fixed to "23 public" with internal helpers clarified
- [x] Warmup exceptions logged instead of silently swallowed
- [x] Lazy pattern simplified to single field (eliminates dual-field NullReferenceException risk)
- [x] Obsolete constructor uses `LazyThreadSafetyMode.ExecutionAndPublication`

### From Critical Review 5
- [x] `Save()` acquires `Constants.Locks.Languages` read lock
- [x] `CreateAndSaveInternal()` acquires `Constants.Locks.Languages` read lock
- [x] Batch `Save()` does NOT call `scope.Complete()` on cancellation
- [x] Obsolete constructors have complete body specification (not chained)
- [x] Unit test added for variant culture save path
- [x] `using Umbraco.Cms.Core.Persistence.Querying;` added to ContentCrudService.cs
- [x] Task 6 Step 2 clarifies "replace" not "add" AssertNoRegression
- [x] GetAncestors warning log includes expected vs actual count
- [x] `BENCHMARK_REQUIRE_BASELINE` environment variable support for strict CI mode

---

**Plan complete and saved to `docs/plans/2025-12-20-contentservice-refactor-phase1-implementation.md`. Two execution options:**

**1. Subagent-Driven (this session)** - I dispatch fresh subagent per task, review between tasks, fast iteration

**2. Parallel Session (separate)** - Open new session with executing-plans, batch execution with checkpoints

**Which approach?**
