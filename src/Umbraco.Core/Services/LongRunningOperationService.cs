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
        bool runInBackground = true,
        bool allowMultipleRunsOfType = true,
        TimeSpan? expires = null)
        => RunInner<object>(
            type,
            async cancellationToken =>
            {
                await operation(cancellationToken);
                return null!;
            },
            runInBackground,
            allowMultipleRunsOfType,
            expires);

    /// <inheritdoc />
    public Task<Attempt<Guid, LongRunningOperationEnqueueStatus>> Run<T>(
        string type,
        Func<CancellationToken, Task<T>> operation,
        bool runInBackground = true,
        bool allowMultipleRunsOfType = true,
        TimeSpan? expires = null)
        => RunInner(
            type,
            operation,
            runInBackground,
            allowMultipleRunsOfType,
            expires);

    /// <inheritdoc />
    public Task<bool> IsRunning(string type, Guid operationId)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        LongRunningOperation? operation = _repository.Get(type, operationId);
        return Task.FromResult(IsRunning(operation));
    }

    /// <inheritdoc/>
    public Task<bool> IsRunning(string type)
        => Task.FromResult(IsRunningSync(type));

    /// <inheritdoc />
    public Task<Attempt<TResult?>> GetResult<TResult>(Guid operationId)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        TResult? result = _repository.GetResult<TResult>(operationId);
        return Task.FromResult(
            result == null
            ? Attempt.Fail(result)
            : Attempt.Succeed(result));
    }

    private static bool IsRunning(LongRunningOperation? operation) =>
        operation is { Status: LongRunningOperationStatus.Enqueued or LongRunningOperationStatus.Running };

    private bool IsRunningSync(string type)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        LongRunningOperation? operation = _repository.GetLatest(type);
        return IsRunning(operation);
    }

    private async Task<Attempt<Guid, LongRunningOperationEnqueueStatus>> RunInner<T>(
        string type,
        Func<CancellationToken, Task<T>> operation,
        bool runInBackground = true,
        bool allowMultipleRunsOfType = true,
        TimeSpan? expires = null)
    {
        if (!allowMultipleRunsOfType && IsRunningSync(type))
        {
            // If an operation of the type is already enqueued or running, we return an attempt indicating that it is already running.
            // This prevents multiple enqueues of the same operation.
            // TODO: Should we return the id of the already running operation?
            return Attempt.FailWithStatus(LongRunningOperationEnqueueStatus.AlreadyRunning, Guid.Empty);
        }

        Guid operationId;
        using (ICoreScope scope = _scopeProvider.CreateCoreScope())
        {
            if (!allowMultipleRunsOfType)
            {
                // Acquire a write lock to ensure that no other operations of the same type can be enqueued while this one is being processed.
                // This is only needed if we do not allow multiple runs of the same type.
                scope.WriteLock(Constants.Locks.LongRunningOperations);
                if (IsRunningSync(type))
                {
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
            _logger.LogInformation("Enqueuing long-running operation {Type} with id {OperationId}.", type, operationId);
            _backgroundTaskQueue.QueueBackgroundWorkItem(async ct =>
            {
                await RunOperationInBackground(
                    operationId,
                    operation,
                    expires.Value,
                    cancellationToken: ct);
                _logger.LogInformation("Finished long-running operation {Type} with id {OperationId}.", type, operationId);
            });
        }
        else
        {
            await RunOperation(operationId, operation, expires.Value);
        }

        return Attempt.SucceedWithStatus(LongRunningOperationEnqueueStatus.Success, operationId);
    }

    private async Task RunOperation<T>(
        Guid operationId,
        Func<CancellationToken, Task<T>> operation,
        TimeSpan expires,
        CancellationToken cancellationToken = default)
    {
        using (ICoreScope scope = _scopeProvider.CreateCoreScope())
        {
            _repository.UpdateStatus(operationId, LongRunningOperationStatus.Running, expires);
            scope.Complete();
        }

        using (ICoreScope scope = _scopeProvider.CreateCoreScope())
        {
            try
            {
                T result = await operation(cancellationToken);
                _repository.UpdateStatus(operationId, LongRunningOperationStatus.Success, expires);

                if (result != null)
                {
                    _repository.SetResult(operationId, result, expires);
                }
            }
            catch (Exception)
            {
                _repository.UpdateStatus(operationId, LongRunningOperationStatus.Failed, expires);
                throw;
            }
            finally
            {
                scope.Complete();
            }
        }
    }

    private async Task RunOperationInBackground<T>(
        Guid operationId,
        Func<CancellationToken, Task<T>> operation,
        TimeSpan expires,
        CancellationToken cancellationToken = default)
    {
        Task<T> task;
        using (ExecutionContext.SuppressFlow())
        {
            task = Task.Run(async () => await operation(cancellationToken), cancellationToken);
        }

        while (true)
        {
            TaskStatus taskStatus = task.Status;
            if (task.IsCompleted)
            {
                break;
            }

            LongRunningOperationStatus status = taskStatus switch
            {
                TaskStatus.Running or TaskStatus.WaitingForChildrenToComplete => LongRunningOperationStatus.Running,
                _ => LongRunningOperationStatus.Enqueued,
            };

            // Update the status in the database, which will also update the update date.
            // That way, even if the status has not changed, we know that the operation is still being processed.
            using ICoreScope scope = _scopeProvider.CreateCoreScope();
            _repository.UpdateStatus(operationId, status, expires);
            scope.Complete();

            await Task.Delay(_timeToWaitBetweenBackgroundTaskStatusChecks, cancellationToken);
        }

        using (ICoreScope scope = _scopeProvider.CreateCoreScope())
        {
            _repository.UpdateStatus(operationId, task.IsCompletedSuccessfully ? LongRunningOperationStatus.Success : LongRunningOperationStatus.Failed, expires);
            if (task.Result != null)
            {
                _repository.SetResult(operationId, task.Result, expires);
            }

            scope.Complete();
        }

        if (task.IsFaulted)
        {
            throw task.Exception;
        }
    }
}

