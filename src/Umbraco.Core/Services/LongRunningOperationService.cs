using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <inheritdoc />
internal class LongRunningOperationService : ILongRunningOperationService
{
    private readonly ILongRunningOperationRepository _repository;
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<LongRunningOperationService> _logger;

    private readonly TimeSpan _timeToWaitBetweenBackgroundTaskStatusChecks = TimeSpan.FromSeconds(10);
    private readonly TimeSpan _defaultExpirationTime = TimeSpan.FromMinutes(2);

    /// <summary>
    /// Initializes a new instance of the <see cref="LongRunningOperationService"/> class.
    /// </summary>
    /// <param name="repository">The repository for tracking long-running operations.</param>
    /// <param name="scopeProvider">The scope provider for managing database transactions.</param>
    /// <param name="timeProvider">The time provider for getting the current UTC time.</param>
    /// <param name="logger">The logger for logging information and errors related to long-running operations.</param>
    public LongRunningOperationService(
        ILongRunningOperationRepository repository,
        ICoreScopeProvider scopeProvider,
        TimeProvider timeProvider,
        ILogger<LongRunningOperationService> logger)
    {
        _repository = repository;
        _scopeProvider = scopeProvider;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<Attempt<Guid, LongRunningOperationEnqueueStatus>> Run(
        string type,
        Func<CancellationToken, Task> operation,
        bool allowConcurrentExecution = false,
        bool runInBackground = true)
        => RunInner<object?>(
            type,
            async cancellationToken =>
            {
                await operation(cancellationToken);
                return null;
            },
            allowConcurrentExecution,
            runInBackground);

    /// <inheritdoc />
    public Task<Attempt<Guid, LongRunningOperationEnqueueStatus>> Run<T>(
        string type,
        Func<CancellationToken, Task<T>> operation,
        bool allowConcurrentExecution = false,
        bool runInBackground = true)
        => RunInner(
            type,
            operation,
            allowConcurrentExecution,
            runInBackground);

    /// <inheritdoc/>
    public Task<LongRunningOperationStatus?> GetStatus(Guid operationId)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        return Task.FromResult(_repository.GetStatus(operationId));
    }

    /// <inheritdoc/>
    public Task<IEnumerable<LongRunningOperation>> GetByType(string type, LongRunningOperationStatus[]? statuses = null)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        return Task.FromResult(
            _repository.GetByType(
                type,
                statuses ?? [LongRunningOperationStatus.Enqueued, LongRunningOperationStatus.Running]));
    }

    /// <inheritdoc />
    public Task<Attempt<TResult?, LongRunningOperationResultStatus>> GetResult<TResult>(Guid operationId)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        LongRunningOperation<TResult>? operation = _repository.Get<TResult>(operationId);
        if (operation?.Status is not LongRunningOperationStatus.Success)
        {
            return Task.FromResult(
                Attempt.FailWithStatus<TResult?, LongRunningOperationResultStatus>(
                    operation?.Status switch
                    {
                        LongRunningOperationStatus.Enqueued or LongRunningOperationStatus.Running => LongRunningOperationResultStatus.OperationPending,
                        LongRunningOperationStatus.Failed => LongRunningOperationResultStatus.OperationFailed,
                        null => LongRunningOperationResultStatus.OperationNotFound,
                        _ => throw new ArgumentOutOfRangeException(nameof(operation.Status), operation.Status, "Unhandled operation status."),
                    },
                    default));
        }

        return Task.FromResult(Attempt.SucceedWithStatus(LongRunningOperationResultStatus.Success, operation.Result));
    }

    private async Task<Attempt<Guid, LongRunningOperationEnqueueStatus>> RunInner<T>(
        string type,
        Func<CancellationToken, Task<T>> operation,
        bool allowConcurrentExecution = true,
        bool runInBackground = true,
        CancellationToken cancellationToken = default)
    {
        if (!runInBackground && _scopeProvider.Context is not null)
        {
            throw new InvalidOperationException("Long running operations cannot be executed in the foreground within an existing scope.");
        }

        Guid operationId;
        using (ICoreScope scope = _scopeProvider.CreateCoreScope())
        {
            if (allowConcurrentExecution is false)
            {
                // Acquire a write lock to ensure that no other operations of the same type can be enqueued while this one is being processed.
                // This is only needed if we do not allow multiple runs of the same type.
                scope.WriteLock(Constants.Locks.LongRunningOperations);
                if (OperationOfTypeIsAlreadyRunning(type))
                {
                    scope.Complete();
                    return Attempt.FailWithStatus(LongRunningOperationEnqueueStatus.AlreadyRunning, Guid.Empty);
                }
            }

            operationId = Guid.CreateVersion7();
            _repository.Create(
                new LongRunningOperation
                {
                    Id = operationId,
                    Type = type,
                    Status = LongRunningOperationStatus.Enqueued,
                },
                _timeProvider.GetUtcNow() + _defaultExpirationTime);
            scope.Complete();
        }

        if (runInBackground)
        {
            using (ExecutionContext.SuppressFlow())
            {
                _ = Task.Run(() => RunOperation(operationId, type, operation, cancellationToken), cancellationToken);
            }
        }
        else
        {
            await RunOperation(operationId, type, operation, cancellationToken);
        }

        return Attempt.SucceedWithStatus(LongRunningOperationEnqueueStatus.Success, operationId);
    }

    private async Task RunOperation<T>(
        Guid operationId,
        string type,
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Running long-running operation {Type} with id {OperationId}.", type, operationId);

        Task<T> operationTask = operation(cancellationToken);

        Task task;
        using (ExecutionContext.SuppressFlow())
        {
            task = Task.Run(
                () => TrackOperationStatus(operationId, type, operationTask),
                CancellationToken.None);
        }

        await task.ConfigureAwait(false);
    }

    private async Task TrackOperationStatus<T>(
        Guid operationId,
        string type,
        Task<T> operationTask)
    {
        _logger.LogDebug("Started tracking long-running operation {Type} with id {OperationId}.", type, operationId);

        try
        {
            while (operationTask.IsCompleted is false)
            {
                // Update the status in the database and increase the expiration time.
                // That way, even if the status has not changed, we know that the operation is still being processed.
                using (ICoreScope scope = _scopeProvider.CreateCoreScope())
                {
                    _repository.UpdateStatus(operationId, LongRunningOperationStatus.Running, _timeProvider.GetUtcNow() + _defaultExpirationTime);
                    scope.Complete();
                }

                await Task.WhenAny(operationTask, Task.Delay(_timeToWaitBetweenBackgroundTaskStatusChecks)).ConfigureAwait(false);
            }
        }
        catch (Exception)
        {
            // If an exception occurs, we update the status to Failed and rethrow the exception.
            _logger.LogDebug("Finished long-running operation {Type} with id {OperationId} and status {Status}.", type, operationId, LongRunningOperationStatus.Failed);
            using (ICoreScope scope = _scopeProvider.CreateCoreScope())
            {
                _repository.UpdateStatus(operationId, LongRunningOperationStatus.Failed, _timeProvider.GetUtcNow() + _defaultExpirationTime);
                scope.Complete();
            }

            throw;
        }

        _logger.LogDebug("Finished long-running operation {Type} with id {OperationId} and status {Status}.", type, operationId, LongRunningOperationStatus.Success);

        using (ICoreScope scope = _scopeProvider.CreateCoreScope())
        {
            _repository.UpdateStatus(operationId, LongRunningOperationStatus.Success, _timeProvider.GetUtcNow() + _defaultExpirationTime);
            if (operationTask.Result != null)
            {
                _repository.SetResult(operationId, operationTask.Result);
            }

            scope.Complete();
        }
    }

    private bool OperationOfTypeIsAlreadyRunning(string type)
        => _repository.GetByType(type, [LongRunningOperationStatus.Enqueued, LongRunningOperationStatus.Running]).Any();
}
