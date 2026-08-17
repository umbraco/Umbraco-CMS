using Umbraco.Cms.Core.HostedServices;

namespace Umbraco.Cms.Tests.Integration.Testing.Search;

// Runs queued work items inline so search indexing is synchronous within a test.
internal class ImmediateBackgroundTaskQueue : IBackgroundTaskQueue
{
    public void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem)
        => workItem(CancellationToken.None).GetAwaiter().GetResult();

    public Task<Func<CancellationToken, Task>?> DequeueAsync(CancellationToken cancellationToken)
        => throw new NotImplementedException($"${nameof(ImmediateBackgroundTaskQueue)} should execute background jobs immediately, so {nameof(DequeueAsync)} is not implemented.");
}
