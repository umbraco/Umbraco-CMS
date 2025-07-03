using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// A service for managing long-running operations that can be executed in the background.
/// </summary>
public interface ILongRunningOperationService
{
    /// <summary>
    ///     Enqueues a long-running operation to be executed in the background.
    /// </summary>
    /// <param name="type">The type of the long-running operation, used for categorization.</param>
    /// <param name="operation">The operation to execute, which should accept a <see cref="CancellationToken"/>.</param>
    /// <param name="allowMultipleRunsOfType">Whether to allow multiple instances of the same operation type to run concurrently.</param>
    /// <returns>An <see cref="Attempt{TStatus}"/> indicating the status of the enqueue operation.</returns>
    Task<Attempt<Guid, LongRunningOperationEnqueueStatus>> Run(
        string type,
        Func<CancellationToken, Task> operation,
        bool runInBackground = true,
        bool allowMultipleRunsOfType = true);

    /// <summary>
    ///     Enqueues a long-running operation to be executed in the background.
    /// </summary>
    /// <param name="type">The type of the long-running operation, used for categorization.</param>
    /// <param name="operation">The operation to execute, which should accept a <see cref="CancellationToken"/>.</param>
    /// <param name="allowMultipleRunsOfType">Whether to allow multiple instances of the same operation type to run concurrently.</param>
    /// <returns>An <see cref="Attempt{TStatus}"/> indicating the status of the enqueue operation.</returns>
    /// <typeparam name="T">The type of the result expected from the operation.</typeparam>
    Task<Attempt<Guid, LongRunningOperationEnqueueStatus>> Run<T>(
        string type,
        Func<CancellationToken, Task<T>> operation,
        bool runInBackground = true,
        bool allowMultipleRunsOfType = true);

    /// <summary>
    /// Checks if a long-running operation with the specified ID is currently running or enqueued.
    /// </summary>
    /// <param name="type">The type of the long-running operation.</param>
    /// <param name="operationId">The unique identifier for the operation.</param>
    /// <returns>True if the operation is running or enqueued; otherwise, false.</returns>
    Task<bool> IsRunning(string type, Guid operationId);

    /// <summary>
    /// Checks if a long-running operation with the specified type is currently running or enqueued.
    /// </summary>
    /// <param name="type">The type of the long-running operation.</param>
    /// <returns>True if the operation is running or enqueued; otherwise, false.</returns>
    Task<bool> IsRunning(string type);

    /// <summary>
    /// Gets the result of a long-running operation.
    /// </summary>
    /// <param name="operationId">The unique identifier of the long-running operation.</param>
    /// <typeparam name="TResult">The type of the result expected from the operation.</typeparam>
    /// <returns>An <see cref="Attempt{TResult}"/> containing the result of the operation
    /// and its status. If the operation is not found or has not completed, the result will be null.</returns>
    Task<Attempt<TResult?>> GetResult<TResult>(Guid operationId);
}
