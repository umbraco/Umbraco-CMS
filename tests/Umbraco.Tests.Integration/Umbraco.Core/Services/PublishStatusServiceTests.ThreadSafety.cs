using System.Collections.Concurrent;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

internal sealed partial class PublishStatusServiceTests
{
    [Test]
    public async Task Concurrent_Reads_And_Writes_Do_Not_Throw()
    {
        // Arrange
        var sut = CreatePublishedStatusService();
        await sut.InitializeAsync(CancellationToken.None);

        const int numberOfOperations = 1000;
        var documentKeys = Enumerable.Range(0, 100).Select(_ => Guid.NewGuid()).ToArray();
        var exceptions = new List<Exception>();
        var lockObj = new object();

        // Act - run concurrent reads and writes
        var tasks = new List<Task>();

        // Writers - AddOrUpdateStatusAsync
        for (var i = 0; i < numberOfOperations; i++)
        {
            var key = documentKeys[i % documentKeys.Length];
            tasks.Add(RunWithSuppressedExecutionContext(async () =>
            {
                try
                {
                    await sut.AddOrUpdateStatusAsync(key, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    lock (lockObj)
                    {
                        exceptions.Add(ex);
                    }
                }
            }));
        }

        // Readers - IsDocumentPublished
        for (var i = 0; i < numberOfOperations; i++)
        {
            var key = documentKeys[i % documentKeys.Length];
            tasks.Add(RunWithSuppressedExecutionContext(() =>
            {
                try
                {
                    _ = sut.IsDocumentPublished(key, DefaultCulture);
                }
                catch (Exception ex)
                {
                    lock (lockObj)
                    {
                        exceptions.Add(ex);
                    }
                }

                return Task.CompletedTask;
            }));
        }

        // Readers - IsDocumentPublishedInAnyCulture
        for (var i = 0; i < numberOfOperations; i++)
        {
            var key = documentKeys[i % documentKeys.Length];
            tasks.Add(RunWithSuppressedExecutionContext(() =>
            {
                try
                {
                    _ = sut.IsDocumentPublishedInAnyCulture(key);
                }
                catch (Exception ex)
                {
                    lock (lockObj)
                    {
                        exceptions.Add(ex);
                    }
                }

                return Task.CompletedTask;
            }));
        }

        // Removers
        for (var i = 0; i < numberOfOperations; i++)
        {
            var key = documentKeys[i % documentKeys.Length];
            tasks.Add(RunWithSuppressedExecutionContext(async () =>
            {
                try
                {
                    await sut.RemoveAsync(key, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    lock (lockObj)
                    {
                        exceptions.Add(ex);
                    }
                }
            }));
        }

        await Task.WhenAll(tasks);

        // Assert
        Assert.IsEmpty(exceptions, $"Expected no exceptions but got {exceptions.Count}: {string.Join(", ", exceptions.Select(e => e.Message))}");
    }

    [Test]
    public async Task Concurrent_Initialize_And_Queries_Do_Not_Throw()
    {
        // Arrange
        var sut = CreatePublishedStatusService();

        // Publish some content first so InitializeAsync has data to load
        ContentService.PublishBranch(Textpage, PublishBranchFilter.IncludeUnpublished, ["*"]);

        const int numberOfOperations = 100;
        var exceptions = new List<Exception>();
        var lockObj = new object();

        // Act - run concurrent initializations and queries
        var tasks = new List<Task>();

        // Multiple initializations (simulates cache rebuild)
        for (var i = 0; i < 10; i++)
        {
            tasks.Add(RunWithSuppressedExecutionContext(async () =>
            {
                try
                {
                    await sut.InitializeAsync(CancellationToken.None);
                }
                catch (Exception ex)
                {
                    lock (lockObj)
                    {
                        exceptions.Add(ex);
                    }
                }
            }));
        }

        // Concurrent reads during initialization
        for (var i = 0; i < numberOfOperations; i++)
        {
            tasks.Add(RunWithSuppressedExecutionContext(() =>
            {
                try
                {
                    _ = sut.IsDocumentPublished(Textpage.Key, DefaultCulture);
                    _ = sut.IsDocumentPublishedInAnyCulture(Subpage.Key);
                    _ = sut.HasPublishedAncestorPath(Subpage2.Key);
                }
                catch (Exception ex)
                {
                    lock (lockObj)
                    {
                        exceptions.Add(ex);
                    }
                }

                return Task.CompletedTask;
            }));
        }

        await Task.WhenAll(tasks);

        // Assert
        Assert.IsEmpty(exceptions, $"Expected no exceptions but got {exceptions.Count}: {string.Join(", ", exceptions.Select(e => e.Message))}");
    }

    [Test]
    public async Task Concurrent_Initialize_Never_Transiently_Loses_Published_Status()
    {
        // Arrange
        var sut = CreatePublishedStatusService();

        // Publish the Textpage branch so InitializeAsync has data to load.
        ContentService.PublishBranch(Textpage, PublishBranchFilter.IncludeUnpublished, ["*"]);

        // Initialize once and confirm the baseline.
        await sut.InitializeAsync(CancellationToken.None);
        Assert.IsTrue(sut.IsDocumentPublishedInAnyCulture(Textpage.Key), "Textpage should be published after initial load");

        var falseCount = 0;
        var totalReads = 0;
        using var cts = new CancellationTokenSource();

        // Act - readers loop continuously while initializers run, ensuring reads
        // overlap with the Clear()-then-rebuild window inside InitializeAsync.
        var readerTasks = new List<Task>();
        for (var i = 0; i < 4; i++)
        {
            readerTasks.Add(RunWithSuppressedExecutionContext(() =>
            {
                while (!cts.IsCancellationRequested)
                {
                    Interlocked.Increment(ref totalReads);
                    if (sut.IsDocumentPublishedInAnyCulture(Textpage.Key) is false)
                    {
                        Interlocked.Increment(ref falseCount);
                    }
                }

                return Task.CompletedTask;
            }));
        }

        // Run 50 sequential re-initializations (simulates repeated cache rebuilds
        // on a subscriber server). Readers run throughout, maximizing the chance
        // of observing the transient empty state.
        for (var i = 0; i < 50; i++)
        {
            await sut.InitializeAsync(CancellationToken.None);
        }

        cts.Cancel();
        await Task.WhenAll(readerTasks);

        // Assert - every single read should have returned true; the published status
        // must never be transiently lost during re-initialization.
        Assert.IsTrue(falseCount == 0, $"Expected all reads to return true, but {falseCount} out of {totalReads} returned false");
    }

    private static Task RunWithSuppressedExecutionContext(Func<Task> action)
    {
        using (ExecutionContext.SuppressFlow())
        {
            return Task.Run(action);
        }
    }
}
