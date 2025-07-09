using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.HostedServices;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <inheritdoc />
internal class LongRunningOperationService : ILongRunningOperationService
{
    private readonly IBackgroundTaskQueue _backgroundTaskQueue;
    private readonly ILongRunningOperationRepository _repository;
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly ILogger<LongRunningOperationService> _logger;

    private readonly TimeSpan _timeToWaitBetweenBackgroundTaskStatusChecks = TimeSpan.FromSeconds(1);
    private readonly TimeSpan _defaultExpirationTime = TimeSpan.FromMinutes(5);

    private readonly TimeSpan _defaultExpirationTimeWhenInBackground = TimeSpan.FromMinutes(2);

    /// <summary>
    /// Initializes a new instance of the <see cref="LongRunningOperationService"/> class.
    /// </summary>
    /// <param name="backgroundTaskQueue">The background task queue to use for enqueuing long-running operations.</param>
    /// <param name="repository">The repository for tracking long-running operations.</param>
    /// <param name="scopeProvider">The scope provider for managing database transactions.</param>
    /// <param name="logger">The logger for logging information and errors related to long-running operations.</param>
    public LongRunningOperationService(
        IBackgroundTaskQueue backgroundTaskQueue,
        ILongRunningOperationRepository repository,
        ICoreScopeProvider scopeProvider,
        ILogger<LongRunningOperationService> logger)
    {
        _backgroundTaskQueue = backgroundTaskQueue;
        _repository = repository;
        _scopeProvider = scopeProvider;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<Attempt<Guid, LongRunningOperationEnqueueStatus>> Run(
        string type,
        Func<CancellationToken, Task> operation,
        bool allowConcurrentExecution = true,
        bool runInBackground = true,
        TimeSpan? expiryTimeout = null)
        => RunInner<object?>(
            type,
            async cancellationToken =>
            {
                await operation(cancellationToken);
                return null;
            },
            allowConcurrentExecution,
            runInBackground,
            expiryTimeout);

    /// <inheritdoc />
    public Task<Attempt<Guid, LongRunningOperationEnqueueStatus>> Run<T>(
        string type,
        Func<CancellationToken, Task<T>> operation,
        bool allowConcurrentExecution = true,
        bool runInBackground = true,
        TimeSpan? expiryTimeout = null)
        => RunInner(
            type,
            operation,
            allowConcurrentExecution,
            runInBackground,
            expiryTimeout);

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
        TimeSpan? expires = null)
    {
        Guid operationId;
        using (ICoreScope scope = _scopeProvider.CreateCoreScope())
        {
            if (!allowConcurrentExecution)
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

            expires ??= runInBackground ? _defaultExpirationTimeWhenInBackground : _defaultExpirationTime;

            operationId = Guid.CreateVersion7();
            _repository.Create(
                new LongRunningOperation
                {
                    Id = operationId,
                    Type = type,
                    Status = LongRunningOperationStatus.Enqueued,
                },
                expires.Value);
            scope.Complete();
        }

        if (runInBackground)
        {
            _logger.LogDebug("Enqueuing long-running operation {Type} with id {OperationId}.", type, operationId);
            _backgroundTaskQueue.QueueBackgroundWorkItem(async ct =>
            {
                await RunOperationInBackground(
                    operationId,
                    type,
                    operation,
                    expires.Value,
                    cancellationToken: ct);
            });
        }
        else
        {
            await RunOperation(type, operationId, operation, expires.Value);
        }

        return Attempt.SucceedWithStatus(LongRunningOperationEnqueueStatus.Success, operationId);
    }

    private async Task RunOperation<T>(
        string type,
        Guid operationId,
        Func<CancellationToken, Task<T>> operation,
        TimeSpan expires,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Running long-running operation {Type} with id {OperationId}.", type, operationId);
        using (ICoreScope scope = _scopeProvider.CreateCoreScope())
        {
            _repository.UpdateStatus(operationId, LongRunningOperationStatus.Running, expires);
            scope.Complete();
        }


        T result;
        try
        {
            result = await operation(cancellationToken);
        }
        catch (Exception)
        {
            _logger.LogDebug(
                "Finished long-running operation {Type} with id {OperationId} and status {Status}.",
                type,
                operationId,
                LongRunningOperationStatus.Failed);

            using ICoreScope scope = _scopeProvider.CreateCoreScope();
            _repository.UpdateStatus(operationId, LongRunningOperationStatus.Failed, expires);
            scope.Complete();

            throw;
        }

        using (ICoreScope scope = _scopeProvider.CreateCoreScope())
        {
            _logger.LogDebug("Finished long-running operation {Type} with id {OperationId} and status {Status}.", type, operationId, LongRunningOperationStatus.Success);
            _repository.UpdateStatus(operationId, LongRunningOperationStatus.Success, expires);

            if (result != null)
            {
                _repository.SetResult(operationId, result);
            }

            scope.Complete();
        }
    }

    private async Task RunOperationInBackground<T>(
        Guid operationId,
        string type,
        Func<CancellationToken, Task<T>> operation,
        TimeSpan expires,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Running long-running operation {Type} with id {OperationId}.", type, operationId);

        // Update the status to Running and increase the expiration time.
        using (ICoreScope scope = _scopeProvider.CreateCoreScope())
        {
            _repository.UpdateStatus(operationId, LongRunningOperationStatus.Running, expires);
            scope.Complete();
        }

        Task<T> task;
        using (ExecutionContext.SuppressFlow())
        {
            task = Task.Run(async () => await operation(cancellationToken), cancellationToken);
        }

        try
        {
            while (!task.IsCompleted)
            {
                // Update the status in the database and increase the expiration time.
                // That way, even if the status has not changed, we know that the operation is still being processed.
                using ICoreScope scope = _scopeProvider.CreateCoreScope();
                _repository.UpdateStatus(operationId, LongRunningOperationStatus.Running, expires);
                scope.Complete();

                await Task.WhenAny(task, Task.Delay(_timeToWaitBetweenBackgroundTaskStatusChecks, cancellationToken)).ConfigureAwait(false);
            }
        }
        catch (Exception)
        {
            // If an exception occurs, we update the status to Failed and rethrow the exception.
            _logger.LogDebug("Finished long-running operation {Type} with id {OperationId} and status {Status}.", type, operationId, LongRunningOperationStatus.Failed);
            using ICoreScope scope = _scopeProvider.CreateCoreScope();
            _repository.UpdateStatus(operationId, LongRunningOperationStatus.Failed, expires);
            scope.Complete();
            throw;
        }

        T result = await task.ConfigureAwait(false);
        _logger.LogDebug("Finished long-running operation {Type} with id {OperationId} and status {Status}.", type, operationId, LongRunningOperationStatus.Success);

        using (ICoreScope scope = _scopeProvider.CreateCoreScope())
        {
            _repository.UpdateStatus(operationId, LongRunningOperationStatus.Success, expires);
            if (result != null)
            {
                _repository.SetResult(operationId, result);
            }

            scope.Complete();
        }
    }

    private bool OperationOfTypeIsAlreadyRunning(string type)
        => _repository.GetByType(type, [LongRunningOperationStatus.Enqueued, LongRunningOperationStatus.Running]).Any();
}


