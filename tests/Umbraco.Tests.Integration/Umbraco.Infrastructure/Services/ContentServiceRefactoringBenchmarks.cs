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
