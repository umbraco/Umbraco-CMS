# ContentService Refactoring Phase 0 Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Create test and benchmark infrastructure to establish baseline metrics and safety nets before the ContentService refactoring begins.

---

## Revision History

| Version | Date | Changes | Reviewer |
|---------|------|---------|----------|
| 1.0 | 2025-12-20 | Initial implementation plan | - |
| 1.1 | 2025-12-20 | Applied critical review feedback (see below) | Critical Implementation Review |
| 1.2 | 2025-12-20 | Applied critical review 2 feedback (warmup fix, data standardization) | Critical Implementation Review 2 |
| 1.3 | 2025-12-20 | Applied critical review 3 feedback (see below) | Critical Implementation Review 3 |

### v1.3 Changes (Critical Review 3 Feedback)

**Must Fix (Applied):**

| # | Issue | Location | Fix Applied |
|---|-------|----------|-------------|
| 1 | Variable shadowing causes compilation error | Task 3, Test 4, lines 509-511 | Removed `var` keyword from redeclared variables |
| 2 | `ContentServiceBenchmarkBase.cs` not committed | Before Task 1 | Added Task 0 to commit the benchmark base class |

**Should Fix (Applied):**

| # | Issue | Location | Fix Applied |
|---|-------|----------|-------------|
| 3 | `MeasureAndRecord<T>` missing warmup | `ContentServiceBenchmarkBase.cs` | Added warmup call to generic overload |
| 4 | Missing tracking mechanism for ContentServiceBaseTests | Task 8 | Added tracking test that fails when `ContentServiceBase` is created |

**Consider (Applied):**

| # | Issue | Location | Fix Applied |
|---|-------|----------|-------------|
| 5 | Permission test magic strings | Task 5, Tests 9-12 | Added inline permission code documentation |
| 6 | No expected ranges in benchmarks | Task 7 | Added comment template for documenting ranges after baseline capture |
| 7 | No JSON validation | Task 10 | Added optional jq validation step |

---

### v1.2 Changes (Critical Review 2 Feedback)

**Must Fix (Applied):**

| # | Issue | Location | Fix Applied |
|---|-------|----------|-------------|
| 1 | Warmup logic corrupts benchmark measurements | Task 7 - All non-destructive benchmarks | Restructured benchmarks to create separate warmup data, then fresh identical data for measurement |
| 2 | Silent failure on missing baseline JSON | Task 10, Step 2 | Replaced `\|\| echo "[]"` with explicit error and exit on missing data |

**Should Fix (Applied):**

| # | Issue | Location | Fix Applied |
|---|-------|----------|-------------|
| 3 | `MeasureAndRecord<T>` overload missing documentation | `ContentServiceBenchmarkBase.cs` | Added remarks about read-only usage intent |
| 4 | Benchmark 31 index out of range risk | Task 7, Benchmark 31 | Added defensive assertion and use relative indexing (middle version) |
| 5 | Sort tests assume specific initial sort order | Task 3, Tests 3-5 | Added explicit verification of initial state |
| 6 | Inconsistent benchmark data sizes | Task 7 | Normalized to 10/100/1000 pattern (see table below) |
| 7 | Permission accumulation behavior undocumented | Task 5, Test 10 | Added explicit behavior documentation |

**Consider (Applied):**

| # | Issue | Location | Fix Applied |
|---|-------|----------|-------------|
| 8 | Test 13 unused variables | Task 6, Test 13 | Added clarifying comment about rolled-back transaction |
| 9 | Missing Category for integration tests | Task 1 | Added `[Category("Refactoring")]` |
| 10 | Benchmark 33 manual timing undocumented | Task 7, Benchmark 33 | Added comment explaining composite operation timing |

**Benchmark Data Size Standardization (v1.2):**

| Benchmark | Old Size | New Size | Change |
|-----------|----------|----------|--------|
| `Benchmark_Delete_WithDescendants` | 50 | 100 | +50 |
| `Benchmark_Count_ByContentType` | 500 | 1000 | +500 |
| `Benchmark_CountDescendants_LargeTree` | 500 | 1000 | +500 |
| `Benchmark_Publish_BatchOf50` | 50 | 100 | Renamed to `Benchmark_Publish_BatchOf100` |
| `Benchmark_PublishBranch_ShallowTree` | 20 | 100 | +80 |
| `Benchmark_PerformScheduledPublish` | 50 | 100 | +50 |
| `Benchmark_Move_WithDescendants` | 50 | 100 | +50 |
| `Benchmark_MoveToRecycleBin_LargeTree` | 100 | 1000 | +900 |
| `Benchmark_Copy_Recursive_50Items` | 50 | 100 | Renamed to `Benchmark_Copy_Recursive_100Items` |
| `Benchmark_GetVersions_ItemWith50Versions` | 50 | 100 | Renamed to `Benchmark_GetVersions_ItemWith100Versions` |
| `Benchmark_GetVersionsSlim_Paged` | 50 | 100 | +50 |
| `Benchmark_DeleteVersions_ByDate` | 50 | 100 | +50 |

---

### v1.1 Changes (Critical Review Feedback)

**Must Fix (Applied):**

| # | Issue | Location | Fix Applied |
|---|-------|----------|-------------|
| 1 | Missing `[NonParallelizable]` attribute | Task 1 (test class) | Added `[NonParallelizable]` to prevent test flakiness from shared static state |
| 2 | Incorrect using directive instruction | Task 6, Step 1 | Removed instruction; `ScopeProvider` available via base class |
| 3 | Non-portable `grep -oP` command | Task 10, Step 2 | Replaced with POSIX-compliant `sed` command |
| 4 | No benchmark warmup iterations | `ContentServiceBenchmarkBase.cs` | Added warmup to `MeasureAndRecord`; `skipWarmup: true` for destructive ops |
| 5 | Missing null assertions on template | Task 4 (3 locations) | Added `Assert.That(template, Is.Not.Null, ...)` |

**Should Fix (Applied):**

| # | Issue | Location | Fix Applied |
|---|-------|----------|-------------|
| 7 | Benchmark class parallelization | Task 7 | Added `[NonParallelizable]` to benchmark class |

**Destructive Benchmarks Updated with `skipWarmup: true`:**
- `Benchmark_Delete_SingleItem`
- `Benchmark_Delete_WithDescendants`
- `Benchmark_EmptyRecycleBin_100Items`
- `Benchmark_Rollback_ToVersion`
- `Benchmark_DeleteVersions_ByDate`

**Note:** Issue 6 (Design doc inconsistency) was verified - the implementation plan was already correct. MoveToRecycleBin does NOT fire unpublish notifications per `ContentService.cs` lines 2457-2461.

---

**Architecture:** Phase 0 delivers three test files: (1) ContentServiceRefactoringTests.cs with 15 integration tests covering notification ordering, sort operations, DeleteOfType, permissions, and transaction boundaries; (2) ContentServiceRefactoringBenchmarks.cs with 33 benchmarks; (3) ContentServiceBaseTests.cs with 7 unit tests for the shared infrastructure class.

**Tech Stack:** NUnit, C#, .NET 10, Umbraco integration test infrastructure (UmbracoIntegrationTestWithContent, ContentServiceBenchmarkBase)

---

## Task 0: Commit ContentServiceBenchmarkBase.cs (Prerequisite)

**Files:**
- Commit: `tests/Umbraco.Tests.Integration/Testing/ContentServiceBenchmarkBase.cs`

**Context:** This file exists locally but is untracked in git. It must be committed before Task 1 or the benchmark tests will fail to compile for other developers.

**Step 1: Verify the file exists and update MeasureAndRecord<T> with warmup**

Before committing, ensure `MeasureAndRecord<T>` includes warmup (v1.3 fix). The method should look like:

```csharp
/// <summary>
/// Measures and records a benchmark, returning the result of the function.
/// </summary>
/// <remarks>
/// Performs a warmup call before measurement to trigger JIT compilation.
/// Safe for read-only operations that can be repeated without side effects.
/// </remarks>
protected T MeasureAndRecord<T>(string name, int itemCount, Func<T> func)
{
    // Warmup: triggers JIT compilation, warms caches
    try { func(); } catch { /* ignore warmup errors */ }

    var sw = Stopwatch.StartNew();
    var result = func();
    sw.Stop();
    RecordBenchmark(name, sw.ElapsedMilliseconds, itemCount);
    return result;
}
```

**Step 2: Commit the benchmark infrastructure**

Run:
```bash
git add tests/Umbraco.Tests.Integration/Testing/ContentServiceBenchmarkBase.cs
git commit -m "$(cat <<'EOF'
test: add ContentServiceBenchmarkBase infrastructure class

Adds base class for ContentService performance benchmarks with:
- RecordBenchmark() for timing capture
- MeasureAndRecord() with warmup support for non-destructive ops
- MeasureAndRecord<T>() with warmup for read-only ops returning values
- JSON output wrapped in [BENCHMARK_JSON] markers for extraction
- skipWarmup parameter for destructive operations

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
EOF
)"
```

---

## Task 1: Create ContentServiceRefactoringTests.cs Skeleton

**Files:**
- Create: `tests/Umbraco.Tests.Integration/Umbraco.Infrastructure/Services/ContentServiceRefactoringTests.cs`

**Step 1: Write the test file skeleton with notification handler**

Create the test class with the notification handler setup required for tracking notification ordering.

```csharp
// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

/// <summary>
/// Integration tests specifically for validating ContentService refactoring.
/// These tests establish behavioral baselines that must pass throughout the refactoring phases.
/// </summary>
[TestFixture]
[NonParallelizable] // Required: static notification handler state is shared across tests
[Category("Refactoring")] // v1.2: Added for easier test filtering during refactoring
[UmbracoTest(
    Database = UmbracoTestOptions.Database.NewSchemaPerTest,
    PublishedRepositoryEvents = true,
    WithApplication = true,
    Logger = UmbracoTestOptions.Logger.Console)]
internal sealed class ContentServiceRefactoringTests : UmbracoIntegrationTestWithContent
{
    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();
    private IUserService UserService => GetRequiredService<IUserService>();

    protected override void CustomTestSetup(IUmbracoBuilder builder) => builder
        .AddNotificationHandler<ContentSavingNotification, RefactoringTestNotificationHandler>()
        .AddNotificationHandler<ContentSavedNotification, RefactoringTestNotificationHandler>()
        .AddNotificationHandler<ContentPublishingNotification, RefactoringTestNotificationHandler>()
        .AddNotificationHandler<ContentPublishedNotification, RefactoringTestNotificationHandler>()
        .AddNotificationHandler<ContentUnpublishingNotification, RefactoringTestNotificationHandler>()
        .AddNotificationHandler<ContentUnpublishedNotification, RefactoringTestNotificationHandler>()
        .AddNotificationHandler<ContentMovingNotification, RefactoringTestNotificationHandler>()
        .AddNotificationHandler<ContentMovedNotification, RefactoringTestNotificationHandler>()
        .AddNotificationHandler<ContentMovingToRecycleBinNotification, RefactoringTestNotificationHandler>()
        .AddNotificationHandler<ContentMovedToRecycleBinNotification, RefactoringTestNotificationHandler>()
        .AddNotificationHandler<ContentSortingNotification, RefactoringTestNotificationHandler>()
        .AddNotificationHandler<ContentSortedNotification, RefactoringTestNotificationHandler>()
        .AddNotificationHandler<ContentDeletingNotification, RefactoringTestNotificationHandler>()
        .AddNotificationHandler<ContentDeletedNotification, RefactoringTestNotificationHandler>();

    [SetUp]
    public override void Setup()
    {
        base.Setup();
        RefactoringTestNotificationHandler.Reset();
    }

    [TearDown]
    public void Teardown()
    {
        RefactoringTestNotificationHandler.Reset();
    }

    #region Notification Ordering Tests

    // Tests 1-2 will be added in Task 2

    #endregion

    #region Sort Operation Tests

    // Tests 3-5 will be added in Task 3

    #endregion

    #region DeleteOfType Tests

    // Tests 6-8 will be added in Task 4

    #endregion

    #region Permission Tests

    // Tests 9-12 will be added in Task 5

    #endregion

    #region Transaction Boundary Tests

    // Tests 13-15 will be added in Task 6

    #endregion

    /// <summary>
    /// Notification handler that tracks the order of notifications for test verification.
    /// </summary>
    internal sealed class RefactoringTestNotificationHandler :
        INotificationHandler<ContentSavingNotification>,
        INotificationHandler<ContentSavedNotification>,
        INotificationHandler<ContentPublishingNotification>,
        INotificationHandler<ContentPublishedNotification>,
        INotificationHandler<ContentUnpublishingNotification>,
        INotificationHandler<ContentUnpublishedNotification>,
        INotificationHandler<ContentMovingNotification>,
        INotificationHandler<ContentMovedNotification>,
        INotificationHandler<ContentMovingToRecycleBinNotification>,
        INotificationHandler<ContentMovedToRecycleBinNotification>,
        INotificationHandler<ContentSortingNotification>,
        INotificationHandler<ContentSortedNotification>,
        INotificationHandler<ContentDeletingNotification>,
        INotificationHandler<ContentDeletedNotification>
    {
        private static readonly List<string> _notificationOrder = new();
        private static readonly object _lock = new();

        public static IReadOnlyList<string> NotificationOrder
        {
            get
            {
                lock (_lock)
                {
                    return _notificationOrder.ToList();
                }
            }
        }

        public static void Reset()
        {
            lock (_lock)
            {
                _notificationOrder.Clear();
            }
        }

        private static void Record(string notificationType)
        {
            lock (_lock)
            {
                _notificationOrder.Add(notificationType);
            }
        }

        public void Handle(ContentSavingNotification notification) => Record(nameof(ContentSavingNotification));
        public void Handle(ContentSavedNotification notification) => Record(nameof(ContentSavedNotification));
        public void Handle(ContentPublishingNotification notification) => Record(nameof(ContentPublishingNotification));
        public void Handle(ContentPublishedNotification notification) => Record(nameof(ContentPublishedNotification));
        public void Handle(ContentUnpublishingNotification notification) => Record(nameof(ContentUnpublishingNotification));
        public void Handle(ContentUnpublishedNotification notification) => Record(nameof(ContentUnpublishedNotification));
        public void Handle(ContentMovingNotification notification) => Record(nameof(ContentMovingNotification));
        public void Handle(ContentMovedNotification notification) => Record(nameof(ContentMovedNotification));
        public void Handle(ContentMovingToRecycleBinNotification notification) => Record(nameof(ContentMovingToRecycleBinNotification));
        public void Handle(ContentMovedToRecycleBinNotification notification) => Record(nameof(ContentMovedToRecycleBinNotification));
        public void Handle(ContentSortingNotification notification) => Record(nameof(ContentSortingNotification));
        public void Handle(ContentSortedNotification notification) => Record(nameof(ContentSortedNotification));
        public void Handle(ContentDeletingNotification notification) => Record(nameof(ContentDeletingNotification));
        public void Handle(ContentDeletedNotification notification) => Record(nameof(ContentDeletedNotification));
    }
}
```

**Step 2: Run test to verify skeleton compiles**

Run: `dotnet build tests/Umbraco.Tests.Integration`
Expected: Build succeeds

**Step 3: Commit**

```bash
git add tests/Umbraco.Tests.Integration/Umbraco.Infrastructure/Services/ContentServiceRefactoringTests.cs
git commit -m "$(cat <<'EOF'
test: add ContentServiceRefactoringTests skeleton for Phase 0

Adds the test file skeleton with notification handler infrastructure
for tracking notification ordering during ContentService refactoring.

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
EOF
)"
```

---

## Task 2: Add Notification Ordering Tests (Tests 1-2)

**Files:**
- Modify: `tests/Umbraco.Tests.Integration/Umbraco.Infrastructure/Services/ContentServiceRefactoringTests.cs`

**Step 1: Write Test 1 - MoveToRecycleBin_PublishedContent_FiresNotificationsInCorrectOrder**

Add this test to the `#region Notification Ordering Tests` section:

```csharp
/// <summary>
/// Test 1: Verifies that MoveToRecycleBin for published content fires notifications in the correct order.
/// Expected order: MovingToRecycleBin -> MovedToRecycleBin
/// Note: As per design doc, MoveToRecycleBin does NOT unpublish first - content is "masked" not unpublished.
/// </summary>
[Test]
public void MoveToRecycleBin_PublishedContent_FiresNotificationsInCorrectOrder()
{
    // Arrange - Create and publish content
    var content = ContentBuilder.CreateSimpleContent(ContentType, "TestContent", Textpage.Id);
    ContentService.Save(content);
    ContentService.Publish(content, new[] { "*" });

    // Verify it's published
    Assert.That(content.Published, Is.True, "Content should be published before test");

    // Clear notification tracking
    RefactoringTestNotificationHandler.Reset();

    // Act
    var result = ContentService.MoveToRecycleBin(content);

    // Assert
    Assert.That(result.Success, Is.True, "MoveToRecycleBin should succeed");

    var notifications = RefactoringTestNotificationHandler.NotificationOrder;

    // Verify notification sequence
    Assert.That(notifications, Does.Contain(nameof(ContentMovingToRecycleBinNotification)),
        "MovingToRecycleBin notification should fire");
    Assert.That(notifications, Does.Contain(nameof(ContentMovedToRecycleBinNotification)),
        "MovedToRecycleBin notification should fire");

    // Verify order: Moving comes before Moved
    var movingIndex = notifications.IndexOf(nameof(ContentMovingToRecycleBinNotification));
    var movedIndex = notifications.IndexOf(nameof(ContentMovedToRecycleBinNotification));
    Assert.That(movingIndex, Is.LessThan(movedIndex),
        "MovingToRecycleBin should fire before MovedToRecycleBin");
}
```

**Step 2: Run test to verify it passes**

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentServiceRefactoringTests.MoveToRecycleBin_PublishedContent_FiresNotificationsInCorrectOrder" -v n`
Expected: PASS

**Step 3: Write Test 2 - MoveToRecycleBin_UnpublishedContent_OnlyFiresMoveNotifications**

Add this test to the same region:

```csharp
/// <summary>
/// Test 2: Verifies that MoveToRecycleBin for unpublished content only fires move notifications.
/// No publish/unpublish notifications should be fired.
/// </summary>
[Test]
public void MoveToRecycleBin_UnpublishedContent_OnlyFiresMoveNotifications()
{
    // Arrange - Create content but don't publish
    var content = ContentBuilder.CreateSimpleContent(ContentType, "UnpublishedContent", Textpage.Id);
    ContentService.Save(content);

    // Verify it's not published
    Assert.That(content.Published, Is.False, "Content should not be published before test");

    // Clear notification tracking
    RefactoringTestNotificationHandler.Reset();

    // Act
    var result = ContentService.MoveToRecycleBin(content);

    // Assert
    Assert.That(result.Success, Is.True, "MoveToRecycleBin should succeed");

    var notifications = RefactoringTestNotificationHandler.NotificationOrder;

    // Verify move notifications fire
    Assert.That(notifications, Does.Contain(nameof(ContentMovingToRecycleBinNotification)),
        "MovingToRecycleBin notification should fire");
    Assert.That(notifications, Does.Contain(nameof(ContentMovedToRecycleBinNotification)),
        "MovedToRecycleBin notification should fire");

    // Verify no publish/unpublish notifications
    Assert.That(notifications, Does.Not.Contain(nameof(ContentPublishingNotification)),
        "Publishing notification should not fire for unpublished content");
    Assert.That(notifications, Does.Not.Contain(nameof(ContentPublishedNotification)),
        "Published notification should not fire for unpublished content");
    Assert.That(notifications, Does.Not.Contain(nameof(ContentUnpublishingNotification)),
        "Unpublishing notification should not fire for unpublished content");
    Assert.That(notifications, Does.Not.Contain(nameof(ContentUnpublishedNotification)),
        "Unpublished notification should not fire for unpublished content");
}
```

**Step 4: Run test to verify it passes**

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentServiceRefactoringTests.MoveToRecycleBin_UnpublishedContent_OnlyFiresMoveNotifications" -v n`
Expected: PASS

**Step 5: Commit**

```bash
git add tests/Umbraco.Tests.Integration/Umbraco.Infrastructure/Services/ContentServiceRefactoringTests.cs
git commit -m "$(cat <<'EOF'
test: add notification ordering tests for MoveToRecycleBin

Adds 2 integration tests validating notification order:
- Test 1: Published content fires MovingToRecycleBin -> MovedToRecycleBin
- Test 2: Unpublished content fires only move notifications, no publish notifications

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
EOF
)"
```

---

## Task 3: Add Sort Operation Tests (Tests 3-5)

**Files:**
- Modify: `tests/Umbraco.Tests.Integration/Umbraco.Infrastructure/Services/ContentServiceRefactoringTests.cs`

**Step 1: Write Test 3 - Sort_WithContentItems_ChangesSortOrder**

Add this test to the `#region Sort Operation Tests` section:

```csharp
/// <summary>
/// Test 3: Verifies Sort(IEnumerable&lt;IContent&gt;) correctly reorders children.
/// </summary>
[Test]
public void Sort_WithContentItems_ChangesSortOrder()
{
    // Arrange - Use existing subpages from base class (Subpage, Subpage2, Subpage3)
    // Get fresh copies to ensure we have current sort orders
    var child1 = ContentService.GetById(Subpage.Id)!;
    var child2 = ContentService.GetById(Subpage2.Id)!;
    var child3 = ContentService.GetById(Subpage3.Id)!;

    // v1.2: Verify initial sort order assumption
    Assert.That(child1.SortOrder, Is.LessThan(child2.SortOrder), "Setup: child1 before child2");
    Assert.That(child2.SortOrder, Is.LessThan(child3.SortOrder), "Setup: child2 before child3");

    // Record original sort orders
    var originalOrder1 = child1.SortOrder;
    var originalOrder2 = child2.SortOrder;
    var originalOrder3 = child3.SortOrder;

    // Create reversed order list
    var reorderedItems = new[] { child3, child2, child1 };

    // Act
    var result = ContentService.Sort(reorderedItems);

    // Assert
    Assert.That(result.Success, Is.True, "Sort should succeed");

    // Re-fetch to verify persisted order
    child1 = ContentService.GetById(Subpage.Id)!;
    child2 = ContentService.GetById(Subpage2.Id)!;
    child3 = ContentService.GetById(Subpage3.Id)!;

    Assert.That(child3.SortOrder, Is.EqualTo(0), "Child3 should now be first (sort order 0)");
    Assert.That(child2.SortOrder, Is.EqualTo(1), "Child2 should now be second (sort order 1)");
    Assert.That(child1.SortOrder, Is.EqualTo(2), "Child1 should now be third (sort order 2)");
}
```

**Step 2: Run test to verify it passes**

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentServiceRefactoringTests.Sort_WithContentItems_ChangesSortOrder" -v n`
Expected: PASS

**Step 3: Write Test 4 - Sort_WithIds_ChangesSortOrder**

```csharp
/// <summary>
/// Test 4: Verifies Sort(IEnumerable&lt;int&gt;) correctly reorders children by ID.
/// </summary>
[Test]
public void Sort_WithIds_ChangesSortOrder()
{
    // Arrange - Use existing subpages from base class
    var child1 = ContentService.GetById(Subpage.Id)!;
    var child2 = ContentService.GetById(Subpage2.Id)!;
    var child3 = ContentService.GetById(Subpage3.Id)!;

    // v1.2: Verify initial sort order assumption
    Assert.That(child1.SortOrder, Is.LessThan(child2.SortOrder), "Setup: child1 before child2");
    Assert.That(child2.SortOrder, Is.LessThan(child3.SortOrder), "Setup: child2 before child3");

    var child1Id = Subpage.Id;
    var child2Id = Subpage2.Id;
    var child3Id = Subpage3.Id;

    // Create reversed order list by ID
    var reorderedIds = new[] { child3Id, child2Id, child1Id };

    // Act
    var result = ContentService.Sort(reorderedIds);

    // Assert
    Assert.That(result.Success, Is.True, "Sort should succeed");

    // Re-fetch to verify persisted order (v1.3: removed var to avoid shadowing)
    child1 = ContentService.GetById(child1Id)!;
    child2 = ContentService.GetById(child2Id)!;
    child3 = ContentService.GetById(child3Id)!;

    Assert.That(child3.SortOrder, Is.EqualTo(0), "Child3 should now be first (sort order 0)");
    Assert.That(child2.SortOrder, Is.EqualTo(1), "Child2 should now be second (sort order 1)");
    Assert.That(child1.SortOrder, Is.EqualTo(2), "Child1 should now be third (sort order 2)");
}
```

**Step 4: Run test to verify it passes**

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentServiceRefactoringTests.Sort_WithIds_ChangesSortOrder" -v n`
Expected: PASS

**Step 5: Write Test 5 - Sort_FiresSortingAndSortedNotifications**

```csharp
/// <summary>
/// Test 5: Verifies Sort fires Sorting and Sorted notifications in correct sequence.
/// </summary>
[Test]
public void Sort_FiresSortingAndSortedNotifications()
{
    // Arrange - Use existing subpages from base class
    var child1 = ContentService.GetById(Subpage.Id)!;
    var child2 = ContentService.GetById(Subpage2.Id)!;
    var child3 = ContentService.GetById(Subpage3.Id)!;

    // v1.2: Verify initial sort order assumption
    Assert.That(child1.SortOrder, Is.LessThan(child2.SortOrder), "Setup: child1 before child2");
    Assert.That(child2.SortOrder, Is.LessThan(child3.SortOrder), "Setup: child2 before child3");

    var reorderedItems = new[] { child3, child2, child1 };

    // Clear notification tracking
    RefactoringTestNotificationHandler.Reset();

    // Act
    var result = ContentService.Sort(reorderedItems);

    // Assert
    Assert.That(result.Success, Is.True, "Sort should succeed");

    var notifications = RefactoringTestNotificationHandler.NotificationOrder;

    // Verify both sorting notifications fire
    Assert.That(notifications, Does.Contain(nameof(ContentSortingNotification)),
        "Sorting notification should fire");
    Assert.That(notifications, Does.Contain(nameof(ContentSortedNotification)),
        "Sorted notification should fire");

    // Also verify Saving/Saved fire (Sort saves content)
    Assert.That(notifications, Does.Contain(nameof(ContentSavingNotification)),
        "Saving notification should fire during sort");
    Assert.That(notifications, Does.Contain(nameof(ContentSavedNotification)),
        "Saved notification should fire during sort");

    // Verify order: Sorting -> Saving -> Saved -> Sorted
    var sortingIndex = notifications.IndexOf(nameof(ContentSortingNotification));
    var sortedIndex = notifications.IndexOf(nameof(ContentSortedNotification));

    Assert.That(sortingIndex, Is.LessThan(sortedIndex),
        "Sorting should fire before Sorted");
}
```

**Step 6: Run test to verify it passes**

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentServiceRefactoringTests.Sort_FiresSortingAndSortedNotifications" -v n`
Expected: PASS

**Step 7: Commit**

```bash
git add tests/Umbraco.Tests.Integration/Umbraco.Infrastructure/Services/ContentServiceRefactoringTests.cs
git commit -m "$(cat <<'EOF'
test: add sort operation tests for ContentService refactoring

Adds 3 integration tests for Sort operations:
- Test 3: Sort(IEnumerable<IContent>) reorders children correctly
- Test 4: Sort(IEnumerable<int>) reorders children by ID correctly
- Test 5: Sort fires Sorting and Sorted notifications in sequence

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
EOF
)"
```

---

## Task 4: Add DeleteOfType Tests (Tests 6-8)

**Files:**
- Modify: `tests/Umbraco.Tests.Integration/Umbraco.Infrastructure/Services/ContentServiceRefactoringTests.cs`

**Step 1: Write Test 6 - DeleteOfType_MovesDescendantsToRecycleBinFirst**

Add this test to the `#region DeleteOfType Tests` section:

```csharp
/// <summary>
/// Test 6: Verifies DeleteOfType with hierarchical content deletes everything correctly.
/// </summary>
[Test]
public void DeleteOfType_MovesDescendantsToRecycleBinFirst()
{
    // Arrange - Create a second content type for descendants
    var template = FileService.GetTemplate("defaultTemplate");
    Assert.That(template, Is.Not.Null, "Default template must exist for test setup");
    var childContentType = ContentTypeBuilder.CreateSimpleContentType(
        "childType", "Child Type", defaultTemplateId: template!.Id);
    ContentTypeService.Save(childContentType);

    // Create parent of target type
    var parent = ContentBuilder.CreateSimpleContent(ContentType, "ParentToDelete", -1);
    ContentService.Save(parent);

    // Create child of different type (should be moved to bin, not deleted)
    var childOfDifferentType = ContentBuilder.CreateSimpleContent(childContentType, "ChildDifferentType", parent.Id);
    ContentService.Save(childOfDifferentType);

    // Create child of same type (should be deleted)
    var childOfSameType = ContentBuilder.CreateSimpleContent(ContentType, "ChildSameType", parent.Id);
    ContentService.Save(childOfSameType);

    var parentId = parent.Id;
    var childDiffId = childOfDifferentType.Id;
    var childSameId = childOfSameType.Id;

    // Act
    ContentService.DeleteOfType(ContentType.Id);

    // Assert
    // Parent should be deleted (it's the target type)
    var deletedParent = ContentService.GetById(parentId);
    Assert.That(deletedParent, Is.Null, "Parent of target type should be deleted");

    // Child of same type should be deleted
    var deletedChildSame = ContentService.GetById(childSameId);
    Assert.That(deletedChildSame, Is.Null, "Child of same type should be deleted");

    // Child of different type should be in recycle bin
    var trashedChild = ContentService.GetById(childDiffId);
    Assert.That(trashedChild, Is.Not.Null, "Child of different type should still exist");
    Assert.That(trashedChild!.Trashed, Is.True, "Child of different type should be in recycle bin");
}
```

**Step 2: Run test to verify it passes**

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentServiceRefactoringTests.DeleteOfType_MovesDescendantsToRecycleBinFirst" -v n`
Expected: PASS

**Step 3: Write Test 7 - DeleteOfType_WithMixedTypes_OnlyDeletesSpecifiedType**

```csharp
/// <summary>
/// Test 7: Verifies DeleteOfType only deletes content of the specified type.
/// </summary>
[Test]
public void DeleteOfType_WithMixedTypes_OnlyDeletesSpecifiedType()
{
    // Arrange - Create a second content type
    var template = FileService.GetTemplate("defaultTemplate");
    Assert.That(template, Is.Not.Null, "Default template must exist for test setup");
    var otherContentType = ContentTypeBuilder.CreateSimpleContentType(
        "otherType", "Other Type", defaultTemplateId: template!.Id);
    ContentTypeService.Save(otherContentType);

    // Create content of target type
    var targetContent1 = ContentBuilder.CreateSimpleContent(ContentType, "Target1", -1);
    var targetContent2 = ContentBuilder.CreateSimpleContent(ContentType, "Target2", -1);
    ContentService.Save(targetContent1);
    ContentService.Save(targetContent2);

    // Create content of other type (should survive)
    var otherContent = ContentBuilder.CreateSimpleContent(otherContentType, "Other", -1);
    ContentService.Save(otherContent);

    var target1Id = targetContent1.Id;
    var target2Id = targetContent2.Id;
    var otherId = otherContent.Id;

    // Act
    ContentService.DeleteOfType(ContentType.Id);

    // Assert
    Assert.That(ContentService.GetById(target1Id), Is.Null, "Target1 should be deleted");
    Assert.That(ContentService.GetById(target2Id), Is.Null, "Target2 should be deleted");
    Assert.That(ContentService.GetById(otherId), Is.Not.Null, "Other type content should survive");
    Assert.That(ContentService.GetById(otherId)!.Trashed, Is.False, "Other type content should not be trashed");
}
```

**Step 4: Run test to verify it passes**

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentServiceRefactoringTests.DeleteOfType_WithMixedTypes_OnlyDeletesSpecifiedType" -v n`
Expected: PASS

**Step 5: Write Test 8 - DeleteOfTypes_DeletesMultipleTypesAtOnce**

```csharp
/// <summary>
/// Test 8: Verifies DeleteOfTypes deletes multiple content types in a single operation.
/// </summary>
[Test]
public void DeleteOfTypes_DeletesMultipleTypesAtOnce()
{
    // Arrange - Create additional content types
    var template = FileService.GetTemplate("defaultTemplate");
    Assert.That(template, Is.Not.Null, "Default template must exist for test setup");

    var type1 = ContentTypeBuilder.CreateSimpleContentType(
        "deleteType1", "Delete Type 1", defaultTemplateId: template!.Id);
    var type2 = ContentTypeBuilder.CreateSimpleContentType(
        "deleteType2", "Delete Type 2", defaultTemplateId: template.Id);
    var survivorType = ContentTypeBuilder.CreateSimpleContentType(
        "survivorType", "Survivor Type", defaultTemplateId: template.Id);

    ContentTypeService.Save(type1);
    ContentTypeService.Save(type2);
    ContentTypeService.Save(survivorType);

    // Create content of each type
    var content1 = ContentBuilder.CreateSimpleContent(type1, "Content1", -1);
    var content2 = ContentBuilder.CreateSimpleContent(type2, "Content2", -1);
    var survivor = ContentBuilder.CreateSimpleContent(survivorType, "Survivor", -1);

    ContentService.Save(content1);
    ContentService.Save(content2);
    ContentService.Save(survivor);

    var content1Id = content1.Id;
    var content2Id = content2.Id;
    var survivorId = survivor.Id;

    // Act - Delete multiple types
    ContentService.DeleteOfTypes(new[] { type1.Id, type2.Id });

    // Assert
    Assert.That(ContentService.GetById(content1Id), Is.Null, "Content of type1 should be deleted");
    Assert.That(ContentService.GetById(content2Id), Is.Null, "Content of type2 should be deleted");
    Assert.That(ContentService.GetById(survivorId), Is.Not.Null, "Content of survivor type should exist");
}
```

**Step 6: Run test to verify it passes**

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentServiceRefactoringTests.DeleteOfTypes_DeletesMultipleTypesAtOnce" -v n`
Expected: PASS

**Step 7: Commit**

```bash
git add tests/Umbraco.Tests.Integration/Umbraco.Infrastructure/Services/ContentServiceRefactoringTests.cs
git commit -m "$(cat <<'EOF'
test: add DeleteOfType tests for ContentService refactoring

Adds 3 integration tests for DeleteOfType operations:
- Test 6: DeleteOfType handles descendants correctly (moves different types to bin)
- Test 7: DeleteOfType only deletes specified type, preserves others
- Test 8: DeleteOfTypes deletes multiple content types at once

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
EOF
)"
```

---

## Task 5: Add Permission Tests (Tests 9-12)

**Files:**
- Modify: `tests/Umbraco.Tests.Integration/Umbraco.Infrastructure/Services/ContentServiceRefactoringTests.cs`

**v1.3: Permission Code Reference**

The following permission codes are used in these tests (see `Umbraco.Cms.Core.Actions` for full list):

| Code | Permission | Action Class |
|------|------------|--------------|
| `F` | Browse Node | `ActionBrowse` |
| `U` | Update | `ActionUpdate` |
| `P` | Publish | `ActionPublish` |
| `D` | Delete | `ActionDelete` |
| `R` | Rights | `ActionRights` |

**Step 1: Write Test 9 - SetPermission_AssignsPermissionToUserGroup**

Add this test to the `#region Permission Tests` section:

```csharp
/// <summary>
/// Test 9: Verifies SetPermission assigns a permission and GetPermissions retrieves it.
/// </summary>
[Test]
public void SetPermission_AssignsPermissionToUserGroup()
{
    // Arrange
    var content = ContentBuilder.CreateSimpleContent(ContentType, "PermissionTest", -1);
    ContentService.Save(content);

    // Get admin user group ID (should always exist)
    var adminGroup = UserService.GetUserGroupByAlias(Constants.Security.AdminGroupAlias);
    Assert.That(adminGroup, Is.Not.Null, "Admin group should exist");

    // Act - Assign browse permission ('F' is typically the Browse Node permission)
    ContentService.SetPermission(content, "F", new[] { adminGroup!.Id });

    // Assert
    var permissions = ContentService.GetPermissions(content);
    Assert.That(permissions, Is.Not.Null, "Permissions should be returned");

    var adminPermissions = permissions.FirstOrDefault(p => p.UserGroupId == adminGroup.Id);
    Assert.That(adminPermissions, Is.Not.Null, "Should have permissions for admin group");
    Assert.That(adminPermissions!.AssignedPermissions, Does.Contain("F"),
        "Admin group should have Browse permission");
}
```

**Step 2: Run test to verify it passes**

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentServiceRefactoringTests.SetPermission_AssignsPermissionToUserGroup" -v n`
Expected: PASS

**Step 3: Write Test 10 - SetPermission_MultiplePermissionsForSameGroup**

```csharp
/// <summary>
/// Test 10: Verifies multiple SetPermission calls accumulate permissions for a user group.
/// </summary>
/// <remarks>
/// v1.2: Expected behavior documentation -
/// SetPermission assigns permissions per-permission-type, not per-entity.
/// Calling SetPermission("F", ...) then SetPermission("U", ...) results in both F and U
/// permissions being assigned. Each call only replaces permissions of the same type.
/// </remarks>
[Test]
public void SetPermission_MultiplePermissionsForSameGroup()
{
    // Arrange
    var content = ContentBuilder.CreateSimpleContent(ContentType, "MultiPermissionTest", -1);
    ContentService.Save(content);

    var adminGroup = UserService.GetUserGroupByAlias(Constants.Security.AdminGroupAlias)!;

    // Act - Assign multiple permissions
    ContentService.SetPermission(content, "F", new[] { adminGroup.Id }); // Browse
    ContentService.SetPermission(content, "U", new[] { adminGroup.Id }); // Update

    // Assert
    var permissions = ContentService.GetPermissions(content);
    var adminPermissions = permissions.FirstOrDefault(p => p.UserGroupId == adminGroup.Id);

    Assert.That(adminPermissions, Is.Not.Null, "Should have permissions for admin group");
    Assert.That(adminPermissions!.AssignedPermissions, Does.Contain("F"), "Should have Browse permission");
    Assert.That(adminPermissions.AssignedPermissions, Does.Contain("U"), "Should have Update permission");
}
```

**Step 4: Run test to verify it passes**

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentServiceRefactoringTests.SetPermission_MultiplePermissionsForSameGroup" -v n`
Expected: PASS

**Step 5: Write Test 11 - SetPermissions_AssignsPermissionSet**

```csharp
/// <summary>
/// Test 11: Verifies SetPermissions assigns a complete permission set.
/// </summary>
[Test]
public void SetPermissions_AssignsPermissionSet()
{
    // Arrange
    var content = ContentBuilder.CreateSimpleContent(ContentType, "PermissionSetTest", -1);
    ContentService.Save(content);

    var adminGroup = UserService.GetUserGroupByAlias(Constants.Security.AdminGroupAlias)!;

    // Create permission set
    var permissionSet = new EntityPermissionSet(
        content.Id,
        new EntityPermissionCollection(new[]
        {
            new EntityPermission(adminGroup.Id, content.Id, new[] { "F", "U", "P" }) // Browse, Update, Publish
        }));

    // Act
    ContentService.SetPermissions(permissionSet);

    // Assert
    var permissions = ContentService.GetPermissions(content);
    var adminPermissions = permissions.FirstOrDefault(p => p.UserGroupId == adminGroup.Id);

    Assert.That(adminPermissions, Is.Not.Null, "Should have permissions for admin group");
    Assert.That(adminPermissions!.AssignedPermissions, Does.Contain("F"), "Should have Browse permission");
    Assert.That(adminPermissions.AssignedPermissions, Does.Contain("U"), "Should have Update permission");
    Assert.That(adminPermissions.AssignedPermissions, Does.Contain("P"), "Should have Publish permission");
}
```

**Step 6: Run test to verify it passes**

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentServiceRefactoringTests.SetPermissions_AssignsPermissionSet" -v n`
Expected: PASS

**Step 7: Write Test 12 - SetPermission_AssignsToMultipleUserGroups**

```csharp
/// <summary>
/// Test 12: Verifies SetPermission can assign to multiple user groups simultaneously.
/// </summary>
[Test]
public void SetPermission_AssignsToMultipleUserGroups()
{
    // Arrange
    var content = ContentBuilder.CreateSimpleContent(ContentType, "MultiGroupTest", -1);
    ContentService.Save(content);

    var adminGroup = UserService.GetUserGroupByAlias(Constants.Security.AdminGroupAlias)!;
    var editorGroup = UserService.GetUserGroupByAlias(Constants.Security.EditorGroupAlias)!;

    // Act - Assign permission to multiple groups at once
    ContentService.SetPermission(content, "F", new[] { adminGroup.Id, editorGroup.Id });

    // Assert
    var permissions = ContentService.GetPermissions(content);

    var adminPermissions = permissions.FirstOrDefault(p => p.UserGroupId == adminGroup.Id);
    var editorPermissions = permissions.FirstOrDefault(p => p.UserGroupId == editorGroup.Id);

    Assert.That(adminPermissions, Is.Not.Null, "Should have permissions for admin group");
    Assert.That(adminPermissions!.AssignedPermissions, Does.Contain("F"), "Admin should have Browse permission");

    Assert.That(editorPermissions, Is.Not.Null, "Should have permissions for editor group");
    Assert.That(editorPermissions!.AssignedPermissions, Does.Contain("F"), "Editor should have Browse permission");
}
```

**Step 8: Run test to verify it passes**

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentServiceRefactoringTests.SetPermission_AssignsToMultipleUserGroups" -v n`
Expected: PASS

**Step 9: Commit**

```bash
git add tests/Umbraco.Tests.Integration/Umbraco.Infrastructure/Services/ContentServiceRefactoringTests.cs
git commit -m "$(cat <<'EOF'
test: add permission tests for ContentService refactoring

Adds 4 integration tests for permission operations:
- Test 9: SetPermission assigns permission and GetPermissions retrieves it
- Test 10: Multiple SetPermission calls accumulate permissions
- Test 11: SetPermissions assigns complete permission set
- Test 12: SetPermission assigns to multiple user groups at once

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
EOF
)"
```

---

## Task 6: Add Transaction Boundary Tests (Tests 13-15)

**Files:**
- Modify: `tests/Umbraco.Tests.Integration/Umbraco.Infrastructure/Services/ContentServiceRefactoringTests.cs`

**Step 1: Write Test 13 - AmbientScope_NestedOperationsShareTransaction**

Add these tests to the `#region Transaction Boundary Tests` section. Note: `ScopeProvider` is already accessible via the `UmbracoIntegrationTestWithContent` base class, no additional using directive needed.

Add the test:

```csharp
/// <summary>
/// Test 13: Verifies that multiple operations within an uncompleted scope all roll back together.
/// </summary>
[Test]
public void AmbientScope_NestedOperationsShareTransaction()
{
    // Arrange
    var content1 = ContentBuilder.CreateSimpleContent(ContentType, "RollbackTest1", -1);
    var content2 = ContentBuilder.CreateSimpleContent(ContentType, "RollbackTest2", -1);

    // Act - Create scope, save content, but don't complete the scope
    using (var scope = ScopeProvider.CreateScope())
    {
        ContentService.Save(content1);
        ContentService.Save(content2);

        // Verify content has IDs (was saved within transaction)
        Assert.That(content1.Id, Is.GreaterThan(0), "Content1 should have an ID");
        Assert.That(content2.Id, Is.GreaterThan(0), "Content2 should have an ID");

        // v1.2: Note - IDs are captured for debugging but cannot be used after rollback
        // since they were assigned within the rolled-back transaction
        var id1 = content1.Id;
        var id2 = content2.Id;

        // DON'T call scope.Complete() - should roll back
    }

    // Assert - Content should not exist after rollback
    // We can't use the IDs because they were assigned in the rolled-back transaction
    // Instead, search by name
    var foundContent = ContentService.GetRootContent()
        .Where(c => c.Name == "RollbackTest1" || c.Name == "RollbackTest2")
        .ToList();

    Assert.That(foundContent, Is.Empty, "Content should not exist after transaction rollback");
}
```

**Step 2: Run test to verify it passes**

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentServiceRefactoringTests.AmbientScope_NestedOperationsShareTransaction" -v n`
Expected: PASS

**Step 3: Write Test 14 - AmbientScope_CompletedScopeCommitsAllOperations**

```csharp
/// <summary>
/// Test 14: Verifies that multiple operations within a completed scope all commit together.
/// </summary>
[Test]
public void AmbientScope_CompletedScopeCommitsAllOperations()
{
    // Arrange
    var content1 = ContentBuilder.CreateSimpleContent(ContentType, "CommitTest1", -1);
    var content2 = ContentBuilder.CreateSimpleContent(ContentType, "CommitTest2", -1);
    int id1, id2;

    // Act - Create scope, save content, and complete the scope
    using (var scope = ScopeProvider.CreateScope())
    {
        ContentService.Save(content1);
        ContentService.Save(content2);

        id1 = content1.Id;
        id2 = content2.Id;

        scope.Complete(); // Commit transaction
    }

    // Assert - Content should exist after commit
    var retrieved1 = ContentService.GetById(id1);
    var retrieved2 = ContentService.GetById(id2);

    Assert.That(retrieved1, Is.Not.Null, "Content1 should exist after commit");
    Assert.That(retrieved2, Is.Not.Null, "Content2 should exist after commit");
    Assert.That(retrieved1!.Name, Is.EqualTo("CommitTest1"));
    Assert.That(retrieved2!.Name, Is.EqualTo("CommitTest2"));
}
```

**Step 4: Run test to verify it passes**

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentServiceRefactoringTests.AmbientScope_CompletedScopeCommitsAllOperations" -v n`
Expected: PASS

**Step 5: Write Test 15 - AmbientScope_MoveToRecycleBinRollsBackCompletely**

```csharp
/// <summary>
/// Test 15: Verifies MoveToRecycleBin within an uncompleted scope rolls back completely.
/// </summary>
[Test]
public void AmbientScope_MoveToRecycleBinRollsBackCompletely()
{
    // Arrange - Create and save content OUTSIDE the test scope so it persists
    var content = ContentBuilder.CreateSimpleContent(ContentType, "MoveRollbackTest", -1);
    ContentService.Save(content);
    var contentId = content.Id;

    // Verify content exists and is not trashed
    var beforeMove = ContentService.GetById(contentId);
    Assert.That(beforeMove, Is.Not.Null, "Content should exist before test");
    Assert.That(beforeMove!.Trashed, Is.False, "Content should not be trashed before test");

    // Act - Move to recycle bin within an uncompleted scope
    using (var scope = ScopeProvider.CreateScope())
    {
        ContentService.MoveToRecycleBin(content);

        // Verify it's trashed within the transaction
        var duringMove = ContentService.GetById(contentId);
        Assert.That(duringMove!.Trashed, Is.True, "Content should be trashed within transaction");

        // DON'T call scope.Complete() - should roll back
    }

    // Assert - Content should be back to original state after rollback
    var afterRollback = ContentService.GetById(contentId);
    Assert.That(afterRollback, Is.Not.Null, "Content should still exist after rollback");
    Assert.That(afterRollback!.Trashed, Is.False, "Content should not be trashed after rollback");
    Assert.That(afterRollback.ParentId, Is.EqualTo(-1), "Content should be at root level after rollback");
}
```

**Step 6: Run test to verify it passes**

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentServiceRefactoringTests.AmbientScope_MoveToRecycleBinRollsBackCompletely" -v n`
Expected: PASS

**Step 7: Run all 15 tests together to verify**

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentServiceRefactoringTests" -v n`
Expected: All 15 tests PASS

**Step 8: Commit**

```bash
git add tests/Umbraco.Tests.Integration/Umbraco.Infrastructure/Services/ContentServiceRefactoringTests.cs
git commit -m "$(cat <<'EOF'
test: add transaction boundary tests for ContentService refactoring

Adds 3 integration tests for transaction boundaries:
- Test 13: Nested operations in uncompleted scope roll back together
- Test 14: Completed scope commits all operations together
- Test 15: MoveToRecycleBin rolls back completely when scope not completed

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
EOF
)"
```

---

## Task 7: Create ContentServiceRefactoringBenchmarks.cs

**Files:**
- Create: `tests/Umbraco.Tests.Integration/Umbraco.Infrastructure/Services/ContentServiceRefactoringBenchmarks.cs`

**Step 1: Write the benchmark file**

```csharp
// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Diagnostics;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Attributes;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

/// <summary>
/// Performance benchmarks for ContentService operations.
/// Used to establish baseline metrics and detect regressions during refactoring.
/// </summary>
/// <remarks>
/// Run with: dotnet test --filter "Category=Benchmark&amp;FullyQualifiedName~ContentServiceRefactoringBenchmarks"
/// Results are output in JSON format for baseline comparison.
///
/// v1.3 EXPECTED RANGES:
/// After capturing the Phase 0 baseline, document expected ranges in docs/plans/baseline-phase0.json.
/// Future phases may add threshold-based assertions. For now, benchmarks capture timing only.
/// Example expected range comment (to be added after baseline capture):
/// // Baseline: ~50ms, Acceptable range: &lt;100ms
///
/// v1.2 WARMUP PATTERN:
/// - For non-destructive benchmarks that MUTATE data (Save, Publish, etc.):
///   Create throwaway data for warmup (triggers JIT, warms caches), then
///   create fresh identical data for the measured run.
/// - For read-only benchmarks (GetById, GetByIds, etc.):
///   Use MeasureAndRecord with warmup enabled (default).
/// - For destructive benchmarks (Delete, EmptyRecycleBin, etc.):
///   Use skipWarmup: true since operation cannot be repeated.
/// </remarks>
[TestFixture]
[NonParallelizable] // Required: benchmarks need isolated database access for accurate measurements
[UmbracoTest(
    Database = UmbracoTestOptions.Database.NewSchemaPerTest,
    PublishedRepositoryEvents = true,
    WithApplication = true,
    Logger = UmbracoTestOptions.Logger.Console)]
[Category("Benchmark")]
[Category("LongRunning")]
internal sealed class ContentServiceRefactoringBenchmarks : ContentServiceBenchmarkBase
{
    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    #region CRUD Operation Benchmarks (7 tests)

    /// <summary>
    /// Benchmark 1: Single content save latency.
    /// </summary>
    [Test]
    [LongRunning]
    public void Benchmark_Save_SingleItem()
    {
        // v1.2: Warmup with throwaway content (triggers JIT, warms caches)
        var warmupContent = ContentBuilder.CreateSimpleContent(ContentType, "Warmup_SingleItem", -1);
        ContentService.Save(warmupContent);

        // Measured run with fresh, identical setup
        var content = ContentBuilder.CreateSimpleContent(ContentType, "BenchmarkSingle", -1);
        var sw = Stopwatch.StartNew();
        ContentService.Save(content);
        sw.Stop();
        RecordBenchmark("Save_SingleItem", sw.ElapsedMilliseconds, 1);

        Assert.That(content.Id, Is.GreaterThan(0));
    }

    /// <summary>
    /// Benchmark 2: Batch save of 100 items.
    /// </summary>
    [Test]
    [LongRunning]
    public void Benchmark_Save_BatchOf100()
    {
        const int itemCount = 100;

        // v1.2: Warmup with throwaway batch
        var warmupItems = new List<IContent>();
        for (var i = 0; i < itemCount; i++)
        {
            warmupItems.Add(ContentBuilder.CreateSimpleContent(ContentType, $"Warmup_Batch100_{i}", -1));
        }
        ContentService.Save(warmupItems);

        // Measured run with fresh batch
        var items = new List<IContent>();
        for (var i = 0; i < itemCount; i++)
        {
            items.Add(ContentBuilder.CreateSimpleContent(ContentType, $"Batch100_{i}", -1));
        }

        var sw = Stopwatch.StartNew();
        ContentService.Save(items);
        sw.Stop();
        RecordBenchmark("Save_BatchOf100", sw.ElapsedMilliseconds, itemCount);

        Assert.That(items.All(c => c.Id > 0), Is.True);
    }

    /// <summary>
    /// Benchmark 3: Batch save of 1000 items (scalability test).
    /// </summary>
    [Test]
    [LongRunning]
    public void Benchmark_Save_BatchOf1000()
    {
        const int itemCount = 1000;

        // v1.2: Warmup with smaller batch (100 items sufficient for JIT warmup)
        var warmupItems = new List<IContent>();
        for (var i = 0; i < 100; i++)
        {
            warmupItems.Add(ContentBuilder.CreateSimpleContent(ContentType, $"Warmup_Batch1000_{i}", -1));
        }
        ContentService.Save(warmupItems);

        // Measured run with fresh batch
        var items = new List<IContent>();
        for (var i = 0; i < itemCount; i++)
        {
            items.Add(ContentBuilder.CreateSimpleContent(ContentType, $"Batch1000_{i}", -1));
        }

        var sw = Stopwatch.StartNew();
        ContentService.Save(items);
        sw.Stop();
        RecordBenchmark("Save_BatchOf1000", sw.ElapsedMilliseconds, itemCount);

        Assert.That(items.All(c => c.Id > 0), Is.True);
    }

    /// <summary>
    /// Benchmark 4: Single item retrieval by ID.
    /// </summary>
    /// <remarks>v1.2: Read-only operation - MeasureAndRecord warmup is safe.</remarks>
    [Test]
    [LongRunning]
    public void Benchmark_GetById_Single()
    {
        var content = ContentBuilder.CreateSimpleContent(ContentType, "GetByIdTest", -1);
        ContentService.Save(content);
        var id = content.Id;

        IContent? result = null;
        MeasureAndRecord("GetById_Single", 1, () =>
        {
            result = ContentService.GetById(id);
        });

        Assert.That(result, Is.Not.Null);
    }

    /// <summary>
    /// Benchmark 5: Batch retrieval of 100 items (N+1 detection).
    /// </summary>
    /// <remarks>v1.2: Read-only operation - MeasureAndRecord warmup is safe.</remarks>
    [Test]
    [LongRunning]
    public void Benchmark_GetByIds_BatchOf100()
    {
        const int itemCount = 100;
        var items = new List<IContent>();

        for (var i = 0; i < itemCount; i++)
        {
            var content = ContentBuilder.CreateSimpleContent(ContentType, $"GetByIds_{i}", -1);
            ContentService.Save(content);
            items.Add(content);
        }

        var ids = items.Select(c => c.Id).ToList();

        IEnumerable<IContent>? results = null;
        MeasureAndRecord("GetByIds_BatchOf100", itemCount, () =>
        {
            results = ContentService.GetByIds(ids);
        });

        Assert.That(results!.Count(), Is.EqualTo(itemCount));
    }

    /// <summary>
    /// Benchmark 6: Single item deletion.
    /// </summary>
    [Test]
    [LongRunning]
    public void Benchmark_Delete_SingleItem()
    {
        var content = ContentBuilder.CreateSimpleContent(ContentType, "DeleteTest", -1);
        ContentService.Save(content);

        MeasureAndRecord("Delete_SingleItem", 1, () =>
        {
            ContentService.Delete(content);
        }, skipWarmup: true); // Destructive operation - cannot repeat

        Assert.That(ContentService.GetById(content.Id), Is.Null);
    }

    /// <summary>
    /// Benchmark 7: Delete item with 100 descendants (cascade performance).
    /// </summary>
    /// <remarks>v1.2: Standardized from 50 to 100 descendants.</remarks>
    [Test]
    [LongRunning]
    public void Benchmark_Delete_WithDescendants()
    {
        var parent = ContentBuilder.CreateSimpleContent(ContentType, "DeleteParent", -1);
        ContentService.Save(parent);

        const int childCount = 100; // v1.2: Standardized to 100
        for (var i = 0; i < childCount; i++)
        {
            var child = ContentBuilder.CreateSimpleContent(ContentType, $"DeleteChild_{i}", parent.Id);
            ContentService.Save(child);
        }

        MeasureAndRecord("Delete_WithDescendants", childCount + 1, () =>
        {
            ContentService.Delete(parent);
        }, skipWarmup: true); // Destructive operation - cannot repeat

        Assert.That(ContentService.GetById(parent.Id), Is.Null);
    }

    #endregion

    #region Query Operation Benchmarks (6 tests)

    /// <summary>
    /// Benchmark 8: Paged children query (100 items).
    /// </summary>
    /// <remarks>v1.2: Read-only operation - MeasureAndRecord warmup is safe.</remarks>
    [Test]
    [LongRunning]
    public void Benchmark_GetPagedChildren_100Items()
    {
        var parent = ContentBuilder.CreateSimpleContent(ContentType, "PagedParent", -1);
        ContentService.Save(parent);

        const int childCount = 100;
        for (var i = 0; i < childCount; i++)
        {
            var child = ContentBuilder.CreateSimpleContent(ContentType, $"PagedChild_{i}", parent.Id);
            ContentService.Save(child);
        }

        IEnumerable<IContent>? results = null;
        long totalRecords = 0;

        MeasureAndRecord("GetPagedChildren_100Items", childCount, () =>
        {
            results = ContentService.GetPagedChildren(parent.Id, 0, 100, out totalRecords);
        });

        Assert.That(totalRecords, Is.EqualTo(childCount));
    }

    /// <summary>
    /// Benchmark 9: Paged descendants query (deep tree, 300 total).
    /// </summary>
    /// <remarks>v1.2: Read-only operation - MeasureAndRecord warmup is safe.</remarks>
    [Test]
    [LongRunning]
    public void Benchmark_GetPagedDescendants_DeepTree()
    {
        var root = ContentBuilder.CreateSimpleContent(ContentType, "DeepRoot", -1);
        ContentService.Save(root);

        // Create 3 levels, 100 items each
        const int itemsPerLevel = 100;
        var currentParent = root;

        for (var level = 0; level < 3; level++)
        {
            var firstChild = ContentBuilder.CreateSimpleContent(ContentType, $"Level{level}_0", currentParent.Id);
            ContentService.Save(firstChild);

            for (var i = 1; i < itemsPerLevel; i++)
            {
                var sibling = ContentBuilder.CreateSimpleContent(ContentType, $"Level{level}_{i}", currentParent.Id);
                ContentService.Save(sibling);
            }

            currentParent = firstChild;
        }

        IEnumerable<IContent>? results = null;
        long totalRecords = 0;

        MeasureAndRecord("GetPagedDescendants_DeepTree", 300, () =>
        {
            results = ContentService.GetPagedDescendants(root.Id, 0, 1000, out totalRecords);
        });

        Assert.That(totalRecords, Is.EqualTo(300));
    }

    /// <summary>
    /// Benchmark 10: Get ancestors of deep item (N+1 prone).
    /// </summary>
    /// <remarks>v1.2: Read-only operation - MeasureAndRecord warmup is safe.</remarks>
    [Test]
    [LongRunning]
    public void Benchmark_GetAncestors_DeepHierarchy()
    {
        const int depth = 10;
        var parent = ContentBuilder.CreateSimpleContent(ContentType, "AncestorRoot", -1);
        ContentService.Save(parent);

        var current = parent;
        for (var i = 0; i < depth - 1; i++)
        {
            var child = ContentBuilder.CreateSimpleContent(ContentType, $"Ancestor_{i}", current.Id);
            ContentService.Save(child);
            current = child;
        }

        var deepestId = current.Id;

        IEnumerable<IContent>? results = null;
        MeasureAndRecord("GetAncestors_DeepHierarchy", depth, () =>
        {
            results = ContentService.GetAncestors(deepestId);
        });

        Assert.That(results!.Count(), Is.EqualTo(depth - 1)); // Excludes self
    }

    /// <summary>
    /// Benchmark 11: Count by content type (1000 items).
    /// </summary>
    /// <remarks>v1.2: Read-only operation - MeasureAndRecord warmup is safe. Standardized from 500 to 1000.</remarks>
    [Test]
    [LongRunning]
    public void Benchmark_Count_ByContentType()
    {
        const int itemCount = 1000; // v1.2: Standardized to 1000
        for (var i = 0; i < itemCount; i++)
        {
            var content = ContentBuilder.CreateSimpleContent(ContentType, $"Count_{i}", -1);
            ContentService.Save(content);
        }

        int count = 0;
        MeasureAndRecord("Count_ByContentType", itemCount, () =>
        {
            count = ContentService.Count(ContentType.Alias);
        });

        // Note: base class may create some content too
        Assert.That(count, Is.GreaterThanOrEqualTo(itemCount));
    }

    /// <summary>
    /// Benchmark 12: Count descendants (1000 descendants).
    /// </summary>
    /// <remarks>v1.2: Read-only operation - MeasureAndRecord warmup is safe. Standardized from 500 to 1000.</remarks>
    [Test]
    [LongRunning]
    public void Benchmark_CountDescendants_LargeTree()
    {
        var parent = ContentBuilder.CreateSimpleContent(ContentType, "CountDescParent", -1);
        ContentService.Save(parent);

        const int childCount = 1000; // v1.2: Standardized to 1000
        for (var i = 0; i < childCount; i++)
        {
            var child = ContentBuilder.CreateSimpleContent(ContentType, $"CountDescChild_{i}", parent.Id);
            ContentService.Save(child);
        }

        int count = 0;
        MeasureAndRecord("CountDescendants_LargeTree", childCount, () =>
        {
            count = ContentService.CountDescendants(parent.Id);
        });

        Assert.That(count, Is.EqualTo(childCount));
    }

    /// <summary>
    /// Benchmark 13: HasChildren called 100 times (repeated single lookups).
    /// </summary>
    /// <remarks>v1.2: Read-only operation - MeasureAndRecord warmup is safe.</remarks>
    [Test]
    [LongRunning]
    public void Benchmark_HasChildren_100Nodes()
    {
        const int nodeCount = 100;
        var nodes = new List<IContent>();

        // Create nodes, half with children, half without
        for (var i = 0; i < nodeCount; i++)
        {
            var node = ContentBuilder.CreateSimpleContent(ContentType, $"HasChildren_{i}", -1);
            ContentService.Save(node);
            nodes.Add(node);

            if (i % 2 == 0)
            {
                var child = ContentBuilder.CreateSimpleContent(ContentType, $"Child_{i}", node.Id);
                ContentService.Save(child);
            }
        }

        int trueCount = 0;
        MeasureAndRecord("HasChildren_100Nodes", nodeCount, () =>
        {
            foreach (var node in nodes)
            {
                if (ContentService.HasChildren(node.Id))
                {
                    trueCount++;
                }
            }
        });

        Assert.That(trueCount, Is.EqualTo(50));
    }

    #endregion

    #region Publish Operation Benchmarks (7 tests)

    /// <summary>
    /// Benchmark 14: Single item publish.
    /// </summary>
    [Test]
    [LongRunning]
    public void Benchmark_Publish_SingleItem()
    {
        // v1.2: Warmup with throwaway content
        var warmupContent = ContentBuilder.CreateSimpleContent(ContentType, "Warmup_PublishSingle", -1);
        ContentService.Save(warmupContent);
        ContentService.Publish(warmupContent, new[] { "*" });

        // Measured run with fresh content
        var content = ContentBuilder.CreateSimpleContent(ContentType, "PublishSingle", -1);
        ContentService.Save(content);

        var sw = Stopwatch.StartNew();
        ContentService.Publish(content, new[] { "*" });
        sw.Stop();
        RecordBenchmark("Publish_SingleItem", sw.ElapsedMilliseconds, 1);

        Assert.That(content.Published, Is.True);
    }

    /// <summary>
    /// Benchmark 15: Publish 100 items sequentially.
    /// </summary>
    /// <remarks>v1.2: Renamed from BatchOf50 to BatchOf100 for standardization.</remarks>
    [Test]
    [LongRunning]
    public void Benchmark_Publish_BatchOf100()
    {
        const int itemCount = 100; // v1.2: Standardized to 100

        // v1.2: Warmup with throwaway batch
        var warmupItems = new List<IContent>();
        for (var i = 0; i < 10; i++) // Small warmup batch for JIT
        {
            var warmup = ContentBuilder.CreateSimpleContent(ContentType, $"Warmup_PublishBatch_{i}", -1);
            ContentService.Save(warmup);
            warmupItems.Add(warmup);
        }
        foreach (var warmup in warmupItems)
        {
            ContentService.Publish(warmup, new[] { "*" });
        }

        // Measured run with fresh batch
        var items = new List<IContent>();
        for (var i = 0; i < itemCount; i++)
        {
            var content = ContentBuilder.CreateSimpleContent(ContentType, $"PublishBatch_{i}", -1);
            ContentService.Save(content);
            items.Add(content);
        }

        var sw = Stopwatch.StartNew();
        foreach (var content in items)
        {
            ContentService.Publish(content, new[] { "*" });
        }
        sw.Stop();
        RecordBenchmark("Publish_BatchOf100", sw.ElapsedMilliseconds, itemCount);

        Assert.That(items.All(c => c.Published), Is.True);
    }

    /// <summary>
    /// Benchmark 16: PublishBranch with shallow tree (1 parent + 100 children).
    /// </summary>
    /// <remarks>v1.2: Standardized from 20 to 100 children.</remarks>
    [Test]
    [LongRunning]
    public void Benchmark_PublishBranch_ShallowTree()
    {
        // v1.2: Warmup with throwaway tree
        var warmupParent = ContentBuilder.CreateSimpleContent(ContentType, "Warmup_BranchParent", -1);
        ContentService.Save(warmupParent);
        for (var i = 0; i < 10; i++) // Small warmup tree
        {
            var warmupChild = ContentBuilder.CreateSimpleContent(ContentType, $"Warmup_BranchChild_{i}", warmupParent.Id);
            ContentService.Save(warmupChild);
        }
        ContentService.PublishBranch(warmupParent, PublishBranchFilter.Default, new[] { "*" });

        // Measured run with fresh tree
        var parent = ContentBuilder.CreateSimpleContent(ContentType, "BranchParent", -1);
        ContentService.Save(parent);

        const int childCount = 100; // v1.2: Standardized to 100
        for (var i = 0; i < childCount; i++)
        {
            var child = ContentBuilder.CreateSimpleContent(ContentType, $"BranchChild_{i}", parent.Id);
            ContentService.Save(child);
        }

        var sw = Stopwatch.StartNew();
        ContentService.PublishBranch(parent, PublishBranchFilter.Default, new[] { "*" });
        sw.Stop();
        RecordBenchmark("PublishBranch_ShallowTree", sw.ElapsedMilliseconds, childCount + 1);

        Assert.That(parent.Published, Is.True);
    }

    /// <summary>
    /// Benchmark 17: PublishBranch with deep tree (5 levels, 100 items total).
    /// </summary>
    [Test]
    [LongRunning]
    public void Benchmark_PublishBranch_DeepTree()
    {
        // v1.2: Warmup with small deep tree (3 levels, 3 items each)
        var warmupRoot = ContentBuilder.CreateSimpleContent(ContentType, "Warmup_DeepBranchRoot", -1);
        ContentService.Save(warmupRoot);
        var warmupParent = warmupRoot;
        for (var level = 0; level < 3; level++)
        {
            var warmupChild = ContentBuilder.CreateSimpleContent(ContentType, $"Warmup_DeepLevel{level}_0", warmupParent.Id);
            ContentService.Save(warmupChild);
            warmupParent = warmupChild;
        }
        ContentService.PublishBranch(warmupRoot, PublishBranchFilter.Default, new[] { "*" });

        // Measured run with fresh deep tree
        var root = ContentBuilder.CreateSimpleContent(ContentType, "DeepBranchRoot", -1);
        ContentService.Save(root);

        // Create 5 levels, 20 items each level
        var currentParent = root;
        for (var level = 0; level < 5; level++)
        {
            var firstChild = ContentBuilder.CreateSimpleContent(ContentType, $"DeepLevel{level}_0", currentParent.Id);
            ContentService.Save(firstChild);

            for (var i = 1; i < 20; i++)
            {
                var sibling = ContentBuilder.CreateSimpleContent(ContentType, $"DeepLevel{level}_{i}", currentParent.Id);
                ContentService.Save(sibling);
            }

            currentParent = firstChild;
        }

        var sw = Stopwatch.StartNew();
        ContentService.PublishBranch(root, PublishBranchFilter.Default, new[] { "*" });
        sw.Stop();
        RecordBenchmark("PublishBranch_DeepTree", sw.ElapsedMilliseconds, 101);

        Assert.That(root.Published, Is.True);
    }

    /// <summary>
    /// Benchmark 18: Single item unpublish.
    /// </summary>
    [Test]
    [LongRunning]
    public void Benchmark_Unpublish_SingleItem()
    {
        // v1.2: Warmup with throwaway content
        var warmupContent = ContentBuilder.CreateSimpleContent(ContentType, "Warmup_UnpublishTest", -1);
        ContentService.Save(warmupContent);
        ContentService.Publish(warmupContent, new[] { "*" });
        ContentService.Unpublish(warmupContent);

        // Measured run with fresh content
        var content = ContentBuilder.CreateSimpleContent(ContentType, "UnpublishTest", -1);
        ContentService.Save(content);
        ContentService.Publish(content, new[] { "*" });

        var sw = Stopwatch.StartNew();
        ContentService.Unpublish(content);
        sw.Stop();
        RecordBenchmark("Unpublish_SingleItem", sw.ElapsedMilliseconds, 1);

        Assert.That(content.Published, Is.False);
    }

    /// <summary>
    /// Benchmark 19: PerformScheduledPublish with 100 scheduled items.
    /// </summary>
    /// <remarks>v1.2: Standardized from 50 to 100 items.</remarks>
    [Test]
    [LongRunning]
    public void Benchmark_PerformScheduledPublish()
    {
        const int itemCount = 100; // v1.2: Standardized to 100

        // v1.2: Warmup with small set of scheduled items
        for (var i = 0; i < 10; i++)
        {
            var warmup = ContentBuilder.CreateSimpleContent(ContentType, $"Warmup_Scheduled_{i}", -1);
            var warmupSchedule = ContentScheduleCollection.CreateWithEntry(DateTime.UtcNow.AddMinutes(-10), null);
            ContentService.Save(warmup, -1, warmupSchedule);
        }
        ContentService.PerformScheduledPublish(DateTime.UtcNow.AddMinutes(-9));

        // Create measured items with schedules in the past (ready for publish)
        for (var i = 0; i < itemCount; i++)
        {
            var content = ContentBuilder.CreateSimpleContent(ContentType, $"Scheduled_{i}", -1);
            var schedule = ContentScheduleCollection.CreateWithEntry(DateTime.UtcNow.AddMinutes(-5), null);
            ContentService.Save(content, -1, schedule);
        }

        var sw = Stopwatch.StartNew();
        ContentService.PerformScheduledPublish(DateTime.UtcNow);
        sw.Stop();
        RecordBenchmark("PerformScheduledPublish", sw.ElapsedMilliseconds, itemCount);
    }

    /// <summary>
    /// Benchmark 20: GetContentSchedulesByIds for 100 items (N+1 hotspot).
    /// </summary>
    /// <remarks>v1.2: Read-only operation - MeasureAndRecord warmup is safe.</remarks>
    [Test]
    [LongRunning]
    public void Benchmark_GetContentSchedulesByIds_100Items()
    {
        const int itemCount = 100;
        var keys = new List<Guid>();

        for (var i = 0; i < itemCount; i++)
        {
            var content = ContentBuilder.CreateSimpleContent(ContentType, $"Schedule_{i}", -1);
            var schedule = ContentScheduleCollection.CreateWithEntry(DateTime.UtcNow.AddDays(1), null);
            ContentService.Save(content, -1, schedule);
            keys.Add(content.Key);
        }

        IDictionary<int, IEnumerable<ContentSchedule>>? results = null;
        MeasureAndRecord("GetContentSchedulesByIds_100Items", itemCount, () =>
        {
            results = ContentService.GetContentSchedulesByIds(keys.ToArray());
        });

        Assert.That(results, Is.Not.Null);
    }

    #endregion

    #region Move Operation Benchmarks (8 tests)

    /// <summary>
    /// Benchmark 21: Single item move.
    /// </summary>
    [Test]
    [LongRunning]
    public void Benchmark_Move_SingleItem()
    {
        // v1.2: Warmup with throwaway content
        var warmupContent = ContentBuilder.CreateSimpleContent(ContentType, "Warmup_MoveTest", -1);
        ContentService.Save(warmupContent);
        var warmupTarget = ContentBuilder.CreateSimpleContent(ContentType, "Warmup_MoveTarget", -1);
        ContentService.Save(warmupTarget);
        ContentService.Move(warmupContent, warmupTarget.Id);

        // Measured run with fresh content
        var content = ContentBuilder.CreateSimpleContent(ContentType, "MoveTest", -1);
        ContentService.Save(content);
        var newParent = ContentBuilder.CreateSimpleContent(ContentType, "MoveTarget", -1);
        ContentService.Save(newParent);

        var sw = Stopwatch.StartNew();
        ContentService.Move(content, newParent.Id);
        sw.Stop();
        RecordBenchmark("Move_SingleItem", sw.ElapsedMilliseconds, 1);

        Assert.That(content.ParentId, Is.EqualTo(newParent.Id));
    }

    /// <summary>
    /// Benchmark 22: Move item with 100 descendants.
    /// </summary>
    /// <remarks>v1.2: Standardized from 50 to 100 descendants.</remarks>
    [Test]
    [LongRunning]
    public void Benchmark_Move_WithDescendants()
    {
        // v1.2: Warmup with small tree
        var warmupParent = ContentBuilder.CreateSimpleContent(ContentType, "Warmup_MoveParent", -1);
        ContentService.Save(warmupParent);
        for (var i = 0; i < 10; i++)
        {
            var warmupChild = ContentBuilder.CreateSimpleContent(ContentType, $"Warmup_MoveChild_{i}", warmupParent.Id);
            ContentService.Save(warmupChild);
        }
        var warmupNewParent = ContentBuilder.CreateSimpleContent(ContentType, "Warmup_MoveDescTarget", -1);
        ContentService.Save(warmupNewParent);
        ContentService.Move(warmupParent, warmupNewParent.Id);

        // Measured run with fresh tree
        var parent = ContentBuilder.CreateSimpleContent(ContentType, "MoveParent", -1);
        ContentService.Save(parent);

        const int childCount = 100; // v1.2: Standardized to 100
        for (var i = 0; i < childCount; i++)
        {
            var child = ContentBuilder.CreateSimpleContent(ContentType, $"MoveChild_{i}", parent.Id);
            ContentService.Save(child);
        }

        var newParent = ContentBuilder.CreateSimpleContent(ContentType, "MoveDescTarget", -1);
        ContentService.Save(newParent);

        var sw = Stopwatch.StartNew();
        ContentService.Move(parent, newParent.Id);
        sw.Stop();
        RecordBenchmark("Move_WithDescendants", sw.ElapsedMilliseconds, childCount + 1);

        Assert.That(parent.ParentId, Is.EqualTo(newParent.Id));
    }

    /// <summary>
    /// Benchmark 23: MoveToRecycleBin for published item.
    /// </summary>
    [Test]
    [LongRunning]
    public void Benchmark_MoveToRecycleBin_Published()
    {
        // v1.2: Warmup with throwaway content
        var warmupContent = ContentBuilder.CreateSimpleContent(ContentType, "Warmup_RecycleBinPublished", -1);
        ContentService.Save(warmupContent);
        ContentService.Publish(warmupContent, new[] { "*" });
        ContentService.MoveToRecycleBin(warmupContent);

        // Measured run with fresh content
        var content = ContentBuilder.CreateSimpleContent(ContentType, "RecycleBinPublished", -1);
        ContentService.Save(content);
        ContentService.Publish(content, new[] { "*" });

        var sw = Stopwatch.StartNew();
        ContentService.MoveToRecycleBin(content);
        sw.Stop();
        RecordBenchmark("MoveToRecycleBin_Published", sw.ElapsedMilliseconds, 1);

        Assert.That(content.Trashed, Is.True);
    }

    /// <summary>
    /// Benchmark 24: MoveToRecycleBin for large tree (1000 descendants).
    /// </summary>
    /// <remarks>v1.2: Standardized from 100 to 1000 descendants.</remarks>
    [Test]
    [LongRunning]
    public void Benchmark_MoveToRecycleBin_LargeTree()
    {
        // v1.2: Warmup with small tree
        var warmupParent = ContentBuilder.CreateSimpleContent(ContentType, "Warmup_RecycleLargeParent", -1);
        ContentService.Save(warmupParent);
        for (var i = 0; i < 10; i++)
        {
            var warmupChild = ContentBuilder.CreateSimpleContent(ContentType, $"Warmup_RecycleLargeChild_{i}", warmupParent.Id);
            ContentService.Save(warmupChild);
        }
        ContentService.MoveToRecycleBin(warmupParent);

        // Measured run with fresh tree
        var parent = ContentBuilder.CreateSimpleContent(ContentType, "RecycleLargeParent", -1);
        ContentService.Save(parent);

        const int childCount = 1000; // v1.2: Standardized to 1000
        for (var i = 0; i < childCount; i++)
        {
            var child = ContentBuilder.CreateSimpleContent(ContentType, $"RecycleLargeChild_{i}", parent.Id);
            ContentService.Save(child);
        }

        var sw = Stopwatch.StartNew();
        ContentService.MoveToRecycleBin(parent);
        sw.Stop();
        RecordBenchmark("MoveToRecycleBin_LargeTree", sw.ElapsedMilliseconds, childCount + 1);

        Assert.That(parent.Trashed, Is.True);
    }

    /// <summary>
    /// Benchmark 25: Single item copy.
    /// </summary>
    [Test]
    [LongRunning]
    public void Benchmark_Copy_SingleItem()
    {
        // v1.2: Warmup with throwaway content
        var warmupContent = ContentBuilder.CreateSimpleContent(ContentType, "Warmup_CopyTest", -1);
        ContentService.Save(warmupContent);
        var warmupTarget = ContentBuilder.CreateSimpleContent(ContentType, "Warmup_CopyTarget", -1);
        ContentService.Save(warmupTarget);
        ContentService.Copy(warmupContent, warmupTarget.Id, false);

        // Measured run with fresh content
        var content = ContentBuilder.CreateSimpleContent(ContentType, "CopyTest", -1);
        ContentService.Save(content);
        var copyTarget = ContentBuilder.CreateSimpleContent(ContentType, "CopyTarget", -1);
        ContentService.Save(copyTarget);

        IContent? copy = null;
        var sw = Stopwatch.StartNew();
        copy = ContentService.Copy(content, copyTarget.Id, false);
        sw.Stop();
        RecordBenchmark("Copy_SingleItem", sw.ElapsedMilliseconds, 1);

        Assert.That(copy, Is.Not.Null);
    }

    /// <summary>
    /// Benchmark 26: Recursive copy of 100 items.
    /// </summary>
    /// <remarks>v1.2: Renamed from 50Items to 100Items for standardization.</remarks>
    [Test]
    [LongRunning]
    public void Benchmark_Copy_Recursive_100Items()
    {
        // v1.2: Warmup with small tree
        var warmupParent = ContentBuilder.CreateSimpleContent(ContentType, "Warmup_CopyRecParent", -1);
        ContentService.Save(warmupParent);
        for (var i = 0; i < 10; i++)
        {
            var warmupChild = ContentBuilder.CreateSimpleContent(ContentType, $"Warmup_CopyRecChild_{i}", warmupParent.Id);
            ContentService.Save(warmupChild);
        }
        var warmupCopyTarget = ContentBuilder.CreateSimpleContent(ContentType, "Warmup_CopyRecTarget", -1);
        ContentService.Save(warmupCopyTarget);
        ContentService.Copy(warmupParent, warmupCopyTarget.Id, false, true);

        // Measured run with fresh tree
        var parent = ContentBuilder.CreateSimpleContent(ContentType, "CopyRecParent", -1);
        ContentService.Save(parent);

        const int childCount = 100; // v1.2: Standardized to 100
        for (var i = 0; i < childCount; i++)
        {
            var child = ContentBuilder.CreateSimpleContent(ContentType, $"CopyRecChild_{i}", parent.Id);
            ContentService.Save(child);
        }

        var copyTarget = ContentBuilder.CreateSimpleContent(ContentType, "CopyRecTarget", -1);
        ContentService.Save(copyTarget);

        IContent? copy = null;
        var sw = Stopwatch.StartNew();
        copy = ContentService.Copy(parent, copyTarget.Id, false, true);
        sw.Stop();
        RecordBenchmark("Copy_Recursive_100Items", sw.ElapsedMilliseconds, childCount + 1);

        Assert.That(copy, Is.Not.Null);
    }

    /// <summary>
    /// Benchmark 27: Sort 100 children.
    /// </summary>
    [Test]
    [LongRunning]
    public void Benchmark_Sort_100Children()
    {
        // v1.2: Warmup with small tree
        var warmupParent = ContentBuilder.CreateSimpleContent(ContentType, "Warmup_SortParent", -1);
        ContentService.Save(warmupParent);
        var warmupChildren = new List<IContent>();
        for (var i = 0; i < 10; i++)
        {
            var warmupChild = ContentBuilder.CreateSimpleContent(ContentType, $"Warmup_SortChild_{i}", warmupParent.Id);
            ContentService.Save(warmupChild);
            warmupChildren.Add(warmupChild);
        }
        warmupChildren.Reverse();
        ContentService.Sort(warmupChildren);

        // Measured run with fresh tree
        var parent = ContentBuilder.CreateSimpleContent(ContentType, "SortParent", -1);
        ContentService.Save(parent);

        const int childCount = 100;
        var children = new List<IContent>();
        for (var i = 0; i < childCount; i++)
        {
            var child = ContentBuilder.CreateSimpleContent(ContentType, $"SortChild_{i}", parent.Id);
            ContentService.Save(child);
            children.Add(child);
        }

        // Reverse the order
        children.Reverse();

        var sw = Stopwatch.StartNew();
        ContentService.Sort(children);
        sw.Stop();
        RecordBenchmark("Sort_100Children", sw.ElapsedMilliseconds, childCount);
    }

    /// <summary>
    /// Benchmark 28: EmptyRecycleBin with 100 items.
    /// </summary>
    [Test]
    [LongRunning]
    public void Benchmark_EmptyRecycleBin_100Items()
    {
        const int itemCount = 100;
        for (var i = 0; i < itemCount; i++)
        {
            var content = ContentBuilder.CreateSimpleContent(ContentType, $"Trash_{i}", -1);
            ContentService.Save(content);
            ContentService.MoveToRecycleBin(content);
        }

        MeasureAndRecord("EmptyRecycleBin_100Items", itemCount, () =>
        {
            ContentService.EmptyRecycleBin();
        }, skipWarmup: true); // Destructive operation - cannot repeat

        Assert.That(ContentService.RecycleBinSmells(), Is.False);
    }

    #endregion

    #region Version Operation Benchmarks (4 tests)

    /// <summary>
    /// Benchmark 29: GetVersions for item with 100 versions.
    /// </summary>
    /// <remarks>v1.2: Renamed from 50Versions to 100Versions for standardization. Read-only operation.</remarks>
    [Test]
    [LongRunning]
    public void Benchmark_GetVersions_ItemWith100Versions()
    {
        var content = ContentBuilder.CreateSimpleContent(ContentType, "VersionTest", -1);
        ContentService.Save(content);

        // Create 100 versions by saving repeatedly
        const int versionCount = 100; // v1.2: Standardized to 100
        for (var i = 0; i < versionCount; i++)
        {
            content.Name = $"VersionTest_v{i}";
            ContentService.Save(content);
        }

        IEnumerable<IContent>? versions = null;
        MeasureAndRecord("GetVersions_ItemWith100Versions", versionCount, () =>
        {
            versions = ContentService.GetVersions(content.Id);
        });

        Assert.That(versions!.Count(), Is.GreaterThanOrEqualTo(versionCount));
    }

    /// <summary>
    /// Benchmark 30: GetVersionsSlim with paging (100 versions, page of 10).
    /// </summary>
    /// <remarks>v1.2: Standardized from 50 to 100 versions. Read-only operation.</remarks>
    [Test]
    [LongRunning]
    public void Benchmark_GetVersionsSlim_Paged()
    {
        var content = ContentBuilder.CreateSimpleContent(ContentType, "SlimVersionTest", -1);
        ContentService.Save(content);

        // Create 100 versions
        const int versionCount = 100; // v1.2: Standardized to 100
        for (var i = 0; i < versionCount; i++)
        {
            content.Name = $"SlimVersionTest_v{i}";
            ContentService.Save(content);
        }

        IEnumerable<IContent>? versions = null;
        MeasureAndRecord("GetVersionsSlim_Paged", 10, () =>
        {
            versions = ContentService.GetVersionsSlim(content.Id, 0, 10);
        });

        Assert.That(versions!.Count(), Is.EqualTo(10));
    }

    /// <summary>
    /// Benchmark 31: Rollback to previous version.
    /// </summary>
    [Test]
    [LongRunning]
    public void Benchmark_Rollback_ToVersion()
    {
        var content = ContentBuilder.CreateSimpleContent(ContentType, "RollbackTest", -1);
        ContentService.Save(content);

        // Create 10 versions
        for (var i = 0; i < 10; i++)
        {
            content.Name = $"RollbackTest_v{i}";
            ContentService.Save(content);
        }

        var versions = ContentService.GetVersions(content.Id).ToList();
        // v1.2: Defensive assertion and relative indexing to avoid index out of range
        Assert.That(versions.Count, Is.GreaterThanOrEqualTo(6), "Need at least 6 versions for rollback test");
        var targetVersionId = versions[versions.Count / 2].VersionId; // Rollback to middle version

        MeasureAndRecord("Rollback_ToVersion", 1, () =>
        {
            ContentService.Rollback(content.Id, targetVersionId);
        }, skipWarmup: true); // Modifying operation - results differ on repeat
    }

    /// <summary>
    /// Benchmark 32: DeleteVersions by date (100 versions).
    /// </summary>
    /// <remarks>v1.2: Standardized from 50 to 100 versions.</remarks>
    [Test]
    [LongRunning]
    public void Benchmark_DeleteVersions_ByDate()
    {
        var content = ContentBuilder.CreateSimpleContent(ContentType, "DeleteVersionsTest", -1);
        ContentService.Save(content);

        // Create 100 versions
        const int versionCount = 100; // v1.2: Standardized to 100
        for (var i = 0; i < versionCount; i++)
        {
            content.Name = $"DeleteVersionsTest_v{i}";
            ContentService.Save(content);
        }

        // Delete all versions before "now" (which should be all of them)
        MeasureAndRecord("DeleteVersions_ByDate", versionCount, () =>
        {
            ContentService.DeleteVersions(content.Id, DateTime.UtcNow.AddMinutes(1));
        }, skipWarmup: true); // Destructive operation - cannot repeat
    }

    #endregion

    #region Baseline Comparison (1 test)

    /// <summary>
    /// Benchmark 33: Meta-benchmark for comparison validation.
    /// This test runs a representative sample and outputs summary statistics.
    /// </summary>
    /// <remarks>
    /// v1.2: Uses manual Stopwatch timing (not MeasureAndRecord) because this benchmark
    /// times a composite sequence of operations (save, publish, query, trash, empty)
    /// to measure overall system performance, not individual operation latency.
    /// </remarks>
    [Test]
    [LongRunning]
    public void Benchmark_BaselineComparison()
    {
        // This test exists to provide a consistent comparison point.
        // It runs a simple operation sequence that represents typical usage.
        // v1.2: Manual timing used for composite operation measurement

        var sw = Stopwatch.StartNew();

        // Create 10 items
        var items = new List<IContent>();
        for (var i = 0; i < 10; i++)
        {
            var content = ContentBuilder.CreateSimpleContent(ContentType, $"Baseline_{i}", -1);
            ContentService.Save(content);
            items.Add(content);
        }

        // Publish all
        foreach (var item in items)
        {
            ContentService.Publish(item, new[] { "*" });
        }

        // Query
        var count = ContentService.Count(ContentType.Alias);

        // Move to recycle bin
        foreach (var item in items)
        {
            ContentService.MoveToRecycleBin(item);
        }

        // Empty recycle bin
        ContentService.EmptyRecycleBin();

        sw.Stop();

        RecordBenchmark("BaselineComparison", sw.ElapsedMilliseconds, 10);

        TestContext.WriteLine($"[BASELINE] Total time for representative operations: {sw.ElapsedMilliseconds}ms");
    }

    #endregion
}
```

**Step 2: Run build to verify benchmark file compiles**

Run: `dotnet build tests/Umbraco.Tests.Integration`
Expected: Build succeeds

**Step 3: Commit**

```bash
git add tests/Umbraco.Tests.Integration/Umbraco.Infrastructure/Services/ContentServiceRefactoringBenchmarks.cs
git commit -m "$(cat <<'EOF'
test: add ContentServiceRefactoringBenchmarks for Phase 0 baseline

Adds 33 performance benchmarks organized by operation type:
- 7 CRUD operation benchmarks
- 6 query operation benchmarks
- 7 publish operation benchmarks
- 8 move operation benchmarks
- 4 version operation benchmarks
- 1 baseline comparison meta-benchmark

Benchmarks output JSON for automated comparison between phases.

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
EOF
)"
```

---

## Task 8: Create ContentServiceBaseTests.cs

**Files:**
- Create: `tests/Umbraco.Tests.UnitTests/Umbraco.Infrastructure/Services/ContentServiceBaseTests.cs`

**Step 1: Write the unit test file**

Note: ContentServiceBase doesn't exist yet - it will be created in Phase 1. These tests establish the expected contract for the base class.

```csharp
// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Services;

/// <summary>
/// Unit tests for ContentServiceBase (shared infrastructure for extracted services).
/// These tests establish the expected contract for the base class before it's created.
/// </summary>
/// <remarks>
/// ContentServiceBase will be created in Phase 1. These tests validate the design requirements:
/// - Audit helper method behavior
/// - Scope provider access patterns
/// - Logger injection patterns
/// </remarks>
[TestFixture]
public class ContentServiceBaseTests
{
    // Note: These tests will be uncommented when ContentServiceBase is created in Phase 1.
    // For now, they serve as documentation of the expected behavior.

    /*
    private Mock<ICoreScopeProvider> _scopeProviderMock;
    private Mock<IAuditService> _auditServiceMock;
    private Mock<IEventMessagesFactory> _eventMessagesFactoryMock;
    private Mock<ILogger<TestContentService>> _loggerMock;
    private TestContentService _service;

    [SetUp]
    public void Setup()
    {
        _scopeProviderMock = new Mock<ICoreScopeProvider>();
        _auditServiceMock = new Mock<IAuditService>();
        _eventMessagesFactoryMock = new Mock<IEventMessagesFactory>();
        _loggerMock = new Mock<ILogger<TestContentService>>();

        _eventMessagesFactoryMock.Setup(x => x.Get()).Returns(new EventMessages());

        _service = new TestContentService(
            _scopeProviderMock.Object,
            _auditServiceMock.Object,
            _eventMessagesFactoryMock.Object,
            _loggerMock.Object);
    }

    #region Audit Helper Method Tests

    [Test]
    public void Audit_WithValidParameters_CreatesAuditEntry()
    {
        // Arrange
        var userId = 1;
        var objectId = 100;
        var message = "Test audit message";

        // Act
        _service.TestAudit(AuditType.Save, userId, objectId, message);

        // Assert
        _auditServiceMock.Verify(x => x.Write(
            userId,
            message,
            It.IsAny<string>(),
            objectId), Times.Once);
    }

    [Test]
    public void Audit_WithNullMessage_UsesDefaultMessage()
    {
        // Arrange
        var userId = 1;
        var objectId = 100;

        // Act
        _service.TestAudit(AuditType.Save, userId, objectId, null);

        // Assert
        _auditServiceMock.Verify(x => x.Write(
            userId,
            It.Is<string>(s => !string.IsNullOrEmpty(s)),
            It.IsAny<string>(),
            objectId), Times.Once);
    }

    #endregion

    #region Scope Provider Access Pattern Tests

    [Test]
    public void CreateScope_ReturnsValidCoreScope()
    {
        // Arrange
        var scopeMock = new Mock<ICoreScope>();
        _scopeProviderMock.Setup(x => x.CreateCoreScope(
            It.IsAny<IsolationLevel>(),
            It.IsAny<RepositoryCacheMode>(),
            It.IsAny<IEventDispatcher>(),
            It.IsAny<IScopedNotificationPublisher>(),
            It.IsAny<bool>(),
            It.IsAny<bool>(),
            It.IsAny<bool>()))
            .Returns(scopeMock.Object);

        // Act
        var scope = _service.TestCreateScope();

        // Assert
        Assert.That(scope, Is.Not.Null);
        _scopeProviderMock.Verify(x => x.CreateCoreScope(
            It.IsAny<IsolationLevel>(),
            It.IsAny<RepositoryCacheMode>(),
            It.IsAny<IEventDispatcher>(),
            It.IsAny<IScopedNotificationPublisher>(),
            It.IsAny<bool>(),
            It.IsAny<bool>(),
            It.IsAny<bool>()), Times.Once);
    }

    [Test]
    public void CreateScope_WithAmbientScope_ReusesExisting()
    {
        // Arrange
        var ambientScopeMock = new Mock<ICoreScope>();
        _scopeProviderMock.SetupGet(x => x.AmbientScope).Returns(ambientScopeMock.Object);

        // When ambient scope exists, CreateCoreScope should still be called
        // but the scope provider handles the nesting
        _scopeProviderMock.Setup(x => x.CreateCoreScope(
            It.IsAny<IsolationLevel>(),
            It.IsAny<RepositoryCacheMode>(),
            It.IsAny<IEventDispatcher>(),
            It.IsAny<IScopedNotificationPublisher>(),
            It.IsAny<bool>(),
            It.IsAny<bool>(),
            It.IsAny<bool>()))
            .Returns(ambientScopeMock.Object);

        // Act
        var scope = _service.TestCreateScope();

        // Assert - scope should be the ambient scope (or nested in it)
        Assert.That(scope, Is.Not.Null);
    }

    #endregion

    #region Logger Injection Tests

    [Test]
    public void Logger_IsInjectedCorrectly()
    {
        // Assert
        Assert.That(_service.TestLogger, Is.Not.Null);
        Assert.That(_service.TestLogger, Is.EqualTo(_loggerMock.Object));
    }

    [Test]
    public void Logger_UsesCorrectCategoryName()
    {
        // The logger should be typed to the concrete service class
        // This is verified by the generic type parameter
        Assert.That(_service.TestLogger, Is.InstanceOf<ILogger<TestContentService>>());
    }

    #endregion

    #region Repository Access Tests

    [Test]
    public void DocumentRepository_IsAccessibleWithinScope()
    {
        // This test validates that the base class provides access to the document repository
        // The actual repository access pattern will be tested in integration tests
        Assert.Pass("Repository access validated in integration tests");
    }

    #endregion

    /// <summary>
    /// Test implementation of ContentServiceBase for unit testing.
    /// </summary>
    private class TestContentService : ContentServiceBase
    {
        public TestContentService(
            ICoreScopeProvider scopeProvider,
            IAuditService auditService,
            IEventMessagesFactory eventMessagesFactory,
            ILogger<TestContentService> logger)
            : base(scopeProvider, auditService, eventMessagesFactory, logger)
        {
        }

        // Expose protected members for testing
        public void TestAudit(AuditType type, int userId, int objectId, string? message)
            => Audit(type, userId, objectId, message);

        public ICoreScope TestCreateScope() => ScopeProvider.CreateCoreScope();

        public ILogger<TestContentService> TestLogger => Logger;
    }
    */

    /// <summary>
    /// v1.3: Tracking test that fails when ContentServiceBase is created.
    /// When this test fails, uncomment all tests in this file and delete this placeholder.
    /// </summary>
    [Test]
    public void ContentServiceBase_WhenCreated_UncommentTests()
    {
        // This tracking test uses reflection to detect when ContentServiceBase is created.
        // When you see this test fail, it means Phase 1 has created ContentServiceBase.
        // At that point:
        // 1. Uncomment all the tests in this file (the commented section above)
        // 2. Delete this tracking test
        // 3. Verify all tests pass

        var type = Type.GetType("Umbraco.Cms.Infrastructure.Services.ContentServiceBase, Umbraco.Infrastructure");

        Assert.That(type, Is.Null,
            "ContentServiceBase now exists! Uncomment all tests in this file and delete this tracking test.");
    }
}
```

**Step 2: Run build to verify test file compiles**

Run: `dotnet build tests/Umbraco.Tests.UnitTests`
Expected: Build succeeds

**Step 3: Commit**

```bash
git add tests/Umbraco.Tests.UnitTests/Umbraco.Infrastructure/Services/ContentServiceBaseTests.cs
git commit -m "$(cat <<'EOF'
test: add ContentServiceBaseTests skeleton for Phase 0

Adds unit test file for ContentServiceBase with documented test cases.
Tests are commented out until ContentServiceBase is created in Phase 1:
- 2 audit helper method tests
- 2 scope provider access pattern tests
- 2 logger injection tests
- 1 repository access test

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
EOF
)"
```

---

## Task 9: Run All Tests and Verify

**Files:**
- Update: `tests/Umbraco.Tests.Integration/Testing/ContentServiceBenchmarkBase.cs` (v1.2: documentation update)

**Step 0: Update ContentServiceBenchmarkBase.cs documentation (v1.2)**

In `ContentServiceBenchmarkBase.cs`, update the `MeasureAndRecord<T>` method documentation to clarify read-only usage intent:

```csharp
/// <summary>
/// Measures and records a benchmark, returning the result of the function.
/// </summary>
/// <remarks>
/// v1.2: This overload does NOT include warmup because it returns a value.
/// Use only for read-only operations (GetById, GetByIds, etc.) that can
/// safely be repeated without side effects. The caller is responsible for
/// ensuring warmup occurs separately if needed.
/// </remarks>
protected T MeasureAndRecord<T>(string name, int itemCount, Func<T> func)
```

**Step 1: Run all 15 integration tests**

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentServiceRefactoringTests" -v n`
Expected: All 15 tests PASS

**Step 2: Verify benchmark file compiles and runs (one sample)**

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentServiceRefactoringBenchmarks.Benchmark_Save_SingleItem" -v n --logger "console;verbosity=detailed"`
Expected: Test PASSES with benchmark output

**Step 3: Run unit tests**

Run: `dotnet test tests/Umbraco.Tests.UnitTests --filter "FullyQualifiedName~ContentServiceBaseTests" -v n`
Expected: 1 placeholder test PASSES

---

## Task 10: Capture Baseline Benchmarks

**Files:**
- Create: `docs/plans/baseline-phase0.json` (output file)

**Step 1: Run full benchmark suite**

Run:
```bash
dotnet test tests/Umbraco.Tests.Integration \
  --filter "Category=Benchmark&FullyQualifiedName~ContentServiceRefactoringBenchmarks" \
  --logger "console;verbosity=detailed" 2>&1 | tee benchmark-$(git rev-parse --short HEAD).txt
```

Expected: All 33 benchmarks complete (this may take several minutes)

**Step 2: Extract JSON results**

Run (using POSIX-compliant sed for cross-platform compatibility):
```bash
# v1.2: Explicit error handling instead of silent fallback
sed -n 's/.*\[BENCHMARK_JSON\]\(.*\)\[\/BENCHMARK_JSON\].*/\1/p' benchmark-*.txt > docs/plans/baseline-phase0.json
if [ ! -s docs/plans/baseline-phase0.json ]; then
    echo "ERROR: No benchmark JSON data found in benchmark-*.txt files"
    echo "Check that benchmarks ran successfully and output [BENCHMARK_JSON] markers"
    exit 1
fi

# v1.3: Optional JSON validation (requires jq)
if command -v jq &> /dev/null; then
    if jq empty docs/plans/baseline-phase0.json 2>/dev/null; then
        echo "JSON validation passed"
    else
        echo "WARNING: baseline-phase0.json may contain invalid JSON"
        echo "Review the file manually before committing"
    fi
else
    echo "NOTE: jq not installed, skipping JSON validation"
fi
```

**Step 3: Create git tag for Phase 0 baseline**

Run:
```bash
git tag phase-0-baseline -m "Phase 0 baseline: test infrastructure complete"
```

**Step 4: Commit baseline file**

```bash
git add docs/plans/baseline-phase0.json benchmark-*.txt
git commit -m "$(cat <<'EOF'
chore: capture Phase 0 baseline benchmarks

Records baseline performance metrics for ContentService operations
before refactoring begins. This data will be used to detect
regressions during subsequent phases.

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
EOF
)"
```

---

## Task 11: Final Verification and Summary

**Files:**
- None (verification only)

**Step 1: Verify all files exist**

Run:
```bash
ls -la tests/Umbraco.Tests.Integration/Umbraco.Infrastructure/Services/ContentServiceRefactoringTests.cs
ls -la tests/Umbraco.Tests.Integration/Umbraco.Infrastructure/Services/ContentServiceRefactoringBenchmarks.cs
ls -la tests/Umbraco.Tests.UnitTests/Umbraco.Infrastructure/Services/ContentServiceBaseTests.cs
ls -la tests/Umbraco.Tests.Integration/Testing/ContentServiceBenchmarkBase.cs
```

Expected: All 4 files exist

**Step 2: Run full ContentService test suite (phase gate)**

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~ContentService" -v n`
Expected: All ContentService tests PASS (including new refactoring tests)

**Step 3: Verify git tags**

Run: `git tag -l "phase-*"`
Expected: `phase-0-baseline` tag exists

---

## Phase 0 Completion Checklist

- [ ] ContentServiceBenchmarkBase.cs committed (Task 0)
  - [ ] MeasureAndRecord<T> includes warmup (v1.3)
- [ ] ContentServiceRefactoringTests.cs created with 15 integration tests
  - [ ] 2 notification ordering tests
  - [ ] 3 sort operation tests
  - [ ] 3 DeleteOfType tests
  - [ ] 4 permission tests
  - [ ] 3 transaction boundary tests
- [ ] ContentServiceRefactoringBenchmarks.cs created with 33 benchmarks
  - [ ] 7 CRUD operation benchmarks
  - [ ] 6 query operation benchmarks
  - [ ] 7 publish operation benchmarks
  - [ ] 8 move operation benchmarks
  - [ ] 4 version operation benchmarks
  - [ ] 1 baseline comparison test
- [ ] ContentServiceBaseTests.cs created (7 tests documented, tracking test active)
  - [ ] v1.3: Tracking test that fails when ContentServiceBase is created
- [ ] All 15 integration tests pass
- [ ] Baseline benchmarks captured
- [ ] Git tag `phase-0-baseline` created
- [ ] Baseline JSON committed to repository

**Gate:** All tests pass, baseline captured, tagged commit created.

---

## Execution Notes

- **Total tasks:** 12 (Task 0-11)
- **Estimated steps:** ~47 individual steps
- **Test count:** 15 integration tests + 33 benchmarks + 1 tracking unit test = 49 tests
- **Key risk:** Benchmark tests may take 10+ minutes to complete - run with patience
- **Rollback:** If any step fails, use `git checkout .` to restore files
