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

    private static Task RunWithSuppressedExecutionContext(Func<Task> action)
    {
        using (ExecutionContext.SuppressFlow())
        {
            return Task.Run(action);
        }
    }
}
