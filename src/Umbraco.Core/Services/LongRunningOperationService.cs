using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <inheritdoc />
internal class LongRunningOperationService : ILongRunningOperationService
{
    private readonly IOptions<LongRunningOperationsSettings> _options;
    private readonly ILongRunningOperationRepository _repository;
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<LongRunningOperationService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LongRunningOperationService"/> class.
    /// </summary>
    /// <param name="options">The options for long-running operations settings.</param>
    /// <param name="repository">The repository for tracking long-running operations.</param>
    /// <param name="scopeProvider">The scope provider for managing database transactions.</param>
    /// <param name="timeProvider">The time provider for getting the current UTC time.</param>
    /// <param name="logger">The logger for logging information and errors related to long-running operations.</param>
    public LongRunningOperationService(
        IOptions<LongRunningOperationsSettings> options,
        ILongRunningOperationRepository repository,
        ICoreScopeProvider scopeProvider,
        TimeProvider timeProvider,
        ILogger<LongRunningOperationService> logger)
    {
        _options = options;
        _repository = repository;
        _scopeProvider = scopeProvider;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<Attempt<Guid, LongRunningOperationEnqueueStatus>> RunAsync(
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
    public Task<Attempt<Guid, LongRunningOperationEnqueueStatus>> RunAsync<T>(
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
    public async Task<LongRunningOperationStatus?> GetStatusAsync(Guid operationId)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        return await _repository.GetStatusAsync(operationId);
    }

    /// <inheritdoc/>
    public async Task<PagedModel<LongRunningOperation>> GetByTypeAsync(
        string type,
        int skip,
        int take,
        LongRunningOperationStatus[]? statuses = null)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        return await _repository.GetByTypeAsync(
                type,
                statuses ?? [LongRunningOperationStatus.Enqueued, LongRunningOperationStatus.Running],
                skip,
                take);
    }

    /// <inheritdoc />
    public async Task<Attempt<TResult?, LongRunningOperationResultStatus>> GetResultAsync<TResult>(Guid operationId)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        LongRunningOperation<TResult>? operation = await _repository.GetAsync<TResult>(operationId);
        if (operation?.Status is not LongRunningOperationStatus.Success)
        {
            return Attempt.FailWithStatus<TResult?, LongRunningOperationResultStatus>(
                operation?.Status switch
                {
                    LongRunningOperationStatus.Enqueued or LongRunningOperationStatus.Running => LongRunningOperationResultStatus.OperationPending,
                    LongRunningOperationStatus.Failed => LongRunningOperationResultStatus.OperationFailed,
                    null => LongRunningOperationResultStatus.OperationNotFound,
                    _ => throw new ArgumentOutOfRangeException(nameof(operation.Status), operation.Status, "Unhandled operation status."),
                },
                default);
        }

        return Attempt.SucceedWithStatus(LongRunningOperationResultStatus.Success, operation.Result);
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
                scope.EagerWriteLock(Constants.Locks.LongRunningOperations);
                if (await IsAlreadyRunning(type))
                {
                    scope.Complete();
                    return Attempt.FailWithStatus(LongRunningOperationEnqueueStatus.AlreadyRunning, Guid.Empty);
                }
            }

            operationId = Guid.CreateVersion7();
            await _repository.CreateAsync(
                new LongRunningOperation
                {
                    Id = operationId,
                    Type = type,
                    Status = LongRunningOperationStatus.Enqueued,
                },
                _timeProvider.GetUtcNow() + _options.Value.ExpirationTime);
            scope.Complete();
        }

        if (runInBackground)
        {
            using (ExecutionContext.SuppressFlow())
            {
                _ = Task.Run(
                    async () =>
                    {
                        try
                        {
                            await RunOperation(operationId, type, operation, cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "An error occurred while running long-running background operation {Type} with id {OperationId}.", type, operationId);
                        }
                    },
                    cancellationToken);
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
                    await _repository.UpdateStatusAsync(operationId, LongRunningOperationStatus.Running, _timeProvider.GetUtcNow() + _options.Value.ExpirationTime);
                    scope.Complete();
                }

                await Task.WhenAny(operationTask, Task.Delay(_options.Value.TimeBetweenStatusChecks)).ConfigureAwait(false);
            }
        }
        catch (Exception)
        {
            // If an exception occurs, we update the status to Failed and rethrow the exception.
            _logger.LogDebug("Finished long-running operation {Type} with id {OperationId} and status {Status}.", type, operationId, LongRunningOperationStatus.Failed);
            using (ICoreScope scope = _scopeProvider.CreateCoreScope())
            {
                await _repository.UpdateStatusAsync(operationId, LongRunningOperationStatus.Failed, _timeProvider.GetUtcNow() + _options.Value.ExpirationTime);
                scope.Complete();
            }

            throw;
        }

        _logger.LogDebug("Finished long-running operation {Type} with id {OperationId} and status {Status}.", type, operationId, LongRunningOperationStatus.Success);

        using (ICoreScope scope = _scopeProvider.CreateCoreScope())
        {
            await _repository.UpdateStatusAsync(operationId, LongRunningOperationStatus.Success, _timeProvider.GetUtcNow() + _options.Value.ExpirationTime);
            if (operationTask.Result != null)
            {
                await _repository.SetResultAsync(operationId, operationTask.Result);
            }

            scope.Complete();
        }
    }

    private async Task<bool> IsAlreadyRunning(string type)
        => (await _repository.GetByTypeAsync(type, [LongRunningOperationStatus.Enqueued, LongRunningOperationStatus.Running], 0, 0)).Total != 0;
}
