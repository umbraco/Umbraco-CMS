using System.Collections.Concurrent;
using NUnit.Framework;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

internal sealed partial class DocumentNavigationServiceTests
{
    [Test]
    public async Task Concurrent_Rebuild_And_Queries_Never_Transiently_Lose_Content()
    {
        // Arrange
        // Create a new DocumentNavigationService and establish baseline via RebuildAsync.
        var sut = new DocumentNavigationService(
            GetRequiredService<ICoreScopeProvider>(),
            GetRequiredService<INavigationRepository>(),
            GetRequiredService<IContentTypeService>());
        await sut.RebuildAsync();

        // Confirm baseline: root keys are present.
        Assert.IsTrue(sut.TryGetRootKeys(out IEnumerable<Guid> initialRootKeys), "Root keys should exist after initial rebuild");
        Assert.IsTrue(initialRootKeys.Any(), "There should be at least one root key");

        var emptyRootCount = 0;
        var exceptionCount = 0;
        var totalReads = 0;
        using var cts = new CancellationTokenSource();

        // Act - readers loop continuously while rebuilds run.
        // TryGetRootKeys reads from _roots (a non-thread-safe HashSet) which is
        // cleared inside HandleRebuildAsync, creating a window where it returns empty.
        var readerTasks = new List<Task>();
        for (var i = 0; i < 4; i++)
        {
            readerTasks.Add(RunWithSuppressedExecutionContext(() =>
            {
                while (cts.IsCancellationRequested is false)
                {
                    Interlocked.Increment(ref totalReads);
                    try
                    {
                        sut.TryGetRootKeys(out IEnumerable<Guid> rootKeys);
                        if (!rootKeys.Any())
                        {
                            Interlocked.Increment(ref emptyRootCount);
                        }
                    }
                    catch
                    {
                        // HashSet is not thread-safe; concurrent enumeration during
                        // modification can throw. Count these as failures too.
                        Interlocked.Increment(ref exceptionCount);
                    }

                    // Reduce CPU pressure in CI without yielding the thread or losing contention.
                    Thread.SpinWait(20);
                }

                return Task.CompletedTask;
            }));
        }

        // Run 50 sequential rebuilds. Readers run throughout, maximizing
        // the chance of observing the transient empty state after _roots.Clear().
        for (var i = 0; i < 50; i++)
        {
            await sut.RebuildAsync();
        }

        cts.Cancel();
        await Task.WhenAll(readerTasks);

        var failureCount = emptyRootCount + exceptionCount;

        // Assert - root keys should never be transiently empty during a rebuild.
        Assert.IsTrue(failureCount == 0, $"Expected all reads to see root keys, but {emptyRootCount} returned empty and {exceptionCount} threw exceptions (out of {totalReads} total reads)");
    }

    private static Task RunWithSuppressedExecutionContext(Func<Task> action)
    {
        using (ExecutionContext.SuppressFlow())
        {
            return Task.Run(action);
        }
    }
}
