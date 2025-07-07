using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Infrastructure.BackgroundJobs.Jobs;

/// <summary>
/// Cleans up long-running operations that have exceeded a specified age.
/// </summary>
public class LongRunningOperationsCleanupJob : IRecurringBackgroundJob
{
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly ILongRunningOperationRepository _longRunningOperationRepository;

    private readonly TimeSpan _deleteTime = TimeSpan.FromDays(1);

    /// <summary>
    /// Initializes a new instance of the <see cref="LongRunningOperationsCleanupJob"/> class.
    /// </summary>
    /// <param name="scopeProvider">The scope provider for managing database transactions.</param>
    /// <param name="longRunningOperationRepository">The repository for managing long-running operations.</param>
    public LongRunningOperationsCleanupJob(
        ICoreScopeProvider scopeProvider,
        ILongRunningOperationRepository longRunningOperationRepository)
    {
        _scopeProvider = scopeProvider;
        _longRunningOperationRepository = longRunningOperationRepository;
    }

    /// <inheritdoc />
    public event EventHandler? PeriodChanged
    {
        add { }
        remove { }
    }

    /// <inheritdoc />
    public TimeSpan Period => TimeSpan.FromMinutes(2);

    /// <inheritdoc/>
    public TimeSpan Delay { get; } = TimeSpan.FromSeconds(10);

    /// <inheritdoc />
    public Task RunJobAsync()
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        _longRunningOperationRepository.CleanOperations(_deleteTime);
        scope.Complete();
        return Task.CompletedTask;
    }
}
