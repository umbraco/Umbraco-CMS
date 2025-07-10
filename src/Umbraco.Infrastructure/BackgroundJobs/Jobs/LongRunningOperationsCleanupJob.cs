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
    private readonly TimeProvider _timeProvider;

    private readonly TimeSpan _deleteTime = TimeSpan.FromDays(1);

    /// <summary>
    /// Initializes a new instance of the <see cref="LongRunningOperationsCleanupJob"/> class.
    /// </summary>
    /// <param name="scopeProvider">The scope provider for managing database transactions.</param>
    /// <param name="longRunningOperationRepository">The repository for managing long-running operations.</param>
    /// <param name="timeProvider">The time provider for getting the current time.</param>
    public LongRunningOperationsCleanupJob(
        ICoreScopeProvider scopeProvider,
        ILongRunningOperationRepository longRunningOperationRepository,
        TimeProvider timeProvider)
    {
        _scopeProvider = scopeProvider;
        _longRunningOperationRepository = longRunningOperationRepository;
        _timeProvider = timeProvider;
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
    public async Task RunJobAsync()
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        await _longRunningOperationRepository.CleanOperationsAsync(_timeProvider.GetUtcNow() - _deleteTime);
        scope.Complete();
    }
}
