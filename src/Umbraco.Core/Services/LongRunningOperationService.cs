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

    /// <summary>
    /// Initializes a new instance of the <see cref="LongRunningOperationService"/> class.
    /// </summary>
    /// <param name="backgroundTaskQueue">The background task queue to use for enqueuing long-running operations.</param>
    /// <param name="repository">The repository for tracking long-running operations.</param>
    /// <param name="scopeProvider">The scope provider for managing database transactions.</param>
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
        bool allowMultipleRunsOfType = true)
        => RunInner<object>(
            type,
            async cancellationToken =>
            {
                await operation(cancellationToken);
                return null!;
            },
            saveResult: false,
            runInBackground,
            allowMultipleRunsOfType);

    /// <inheritdoc />
    public Task<Attempt<Guid, LongRunningOperationEnqueueStatus>> Run<T>(
        string type,
        Func<CancellationToken, Task<T>> operation,
        bool runInBackground = true,
        bool allowMultipleRunsOfType = true)
        => RunInner(
            type,
            operation,
            saveResult: true,
            runInBackground,
            allowMultipleRunsOfType);

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
        bool saveResult,
        bool runInBackground = true,
        bool allowMultipleRunsOfType = true)
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

            operationId = Guid.CreateVersion7();
            _repository.Create(
                new LongRunningOperation
                {
                    Id = operationId,
                    Type = type,
                    Status = LongRunningOperationStatus.Enqueued,
                });
            scope.Complete();
        }


        if (runInBackground)
        {
            _backgroundTaskQueue.QueueBackgroundWorkItem(ct => RunInner(operationId, operation, saveResult, cancellationToken: ct));
        }
        else
        {
            await RunInner(operationId, operation, saveResult);
        }

        return Attempt.SucceedWithStatus(LongRunningOperationEnqueueStatus.Success, operationId);
    }

    private async Task RunInner<T>(
        Guid operationId,
        Func<CancellationToken, Task<T>> operation,
        bool saveResult = true,
        CancellationToken cancellationToken = default)
    {
        Task<T> task;
        using (ExecutionContext.SuppressFlow())
        {
            task = Task.Run(async () => await operation(cancellationToken), cancellationToken);
        }

        using (ICoreScope scope = _scopeProvider.CreateCoreScope())
        {
            _repository.UpdateStatus(operationId, LongRunningOperationStatus.Running);
            scope.Complete();
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
            _repository.UpdateStatus(operationId, status);
            scope.Complete();

            await Task.Delay(1000, cancellationToken); // Wait for 1 second before checking again
        }

        if (task.IsFaulted)
        {
            _logger.LogError(task.Exception, "An error occurred while running operation {OperationId}.", operationId);
        }

        using (ICoreScope scope = _scopeProvider.CreateCoreScope())
        {
            _repository.UpdateStatus(operationId, task.IsCompletedSuccessfully ? LongRunningOperationStatus.Success : LongRunningOperationStatus.Failed);
            if (saveResult && task.IsCompletedSuccessfully)
            {
                _repository.SetResult(operationId, task.Result);
            }

            scope.Complete();
        }
    }
}
