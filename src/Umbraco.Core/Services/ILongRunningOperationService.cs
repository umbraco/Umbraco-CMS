using Umbraco.Cms.Core.Models;
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
    /// <param name="allowConcurrentExecution">Whether to allow multiple instances of the same operation type to run concurrently.</param>
    /// <param name="runInBackground">Whether to run the operation in the background.</param>
    /// <returns>An <see cref="Attempt{TStatus}"/> indicating the status of the enqueue operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown if attempting to run an operation in the foreground within a scope.</exception>
    Task<Attempt<Guid, LongRunningOperationEnqueueStatus>> RunAsync(
        string type,
        Func<CancellationToken, Task> operation,
        bool allowConcurrentExecution = false,
        bool runInBackground = true);

    /// <summary>
    ///     Enqueues a long-running operation to be executed in the background.
    /// </summary>
    /// <param name="type">The type of the long-running operation, used for categorization.</param>
    /// <param name="operation">The operation to execute, which should accept a <see cref="CancellationToken"/>.</param>
    /// <param name="allowConcurrentExecution">Whether to allow multiple instances of the same operation type to run concurrently.</param>
    /// <param name="runInBackground">Whether to run the operation in the background.</param>
    /// <returns>An <see cref="Attempt{TStatus}"/> indicating the status of the enqueue operation.</returns>
    /// <typeparam name="T">The type of the result expected from the operation.</typeparam>
    /// <exception cref="InvalidOperationException">Thrown if attempting to run an operation in the foreground within a scope.</exception>
    Task<Attempt<Guid, LongRunningOperationEnqueueStatus>> RunAsync<T>(
        string type,
        Func<CancellationToken, Task<T>> operation,
        bool allowConcurrentExecution = false,
        bool runInBackground = true);

    /// <summary>
    /// Gets the status of a long-running operation by its unique identifier.
    /// </summary>
    /// <param name="operationId">The unique identifier for the operation.</param>
    /// <returns>True if the operation is running or enqueued; otherwise, false.</returns>
    Task<LongRunningOperationStatus?> GetStatusAsync(Guid operationId);

    /// <summary>
    /// Gets the active long-running operations of a specific type.
    /// </summary>
    /// <param name="type">The type of the long-running operation.</param>
    /// <param name="skip">Number of operations to skip.</param>
    /// <param name="take">Number of operations to take.</param>
    /// <param name="statuses">Optional array of statuses to filter the operations by. If null, only enqueued and running
    /// operations are returned.</param>
    /// <returns>True if the operation is running or enqueued; otherwise, false.</returns>
    Task<PagedModel<LongRunningOperation>> GetByTypeAsync(
        string type,
        int skip,
        int take,
        LongRunningOperationStatus[]? statuses = null);

    /// <summary>
    /// Gets the result of a long-running operation.
    /// </summary>
    /// <param name="operationId">The unique identifier of the long-running operation.</param>
    /// <typeparam name="TResult">The type of the result expected from the operation.</typeparam>
    /// <returns>An <see cref="Attempt{TResult}"/> containing the result of the operation
    /// and its status. If the operation is not found or has not completed, the result will be null.</returns>
    Task<Attempt<TResult?, LongRunningOperationResultStatus>> GetResultAsync<TResult>(Guid operationId);
}
