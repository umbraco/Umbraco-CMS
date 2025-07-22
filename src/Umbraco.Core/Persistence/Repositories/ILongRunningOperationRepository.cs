using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
/// Represents a repository for managing long-running operations.
/// </summary>
public interface ILongRunningOperationRepository
{
    /// <summary>
    /// Creates a new long-running operation.
    /// </summary>
    /// <param name="operation">The operation to create.</param>
    /// <param name="expirationDate">The date and time when the operation should be considered stale.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task CreateAsync(LongRunningOperation operation, DateTimeOffset expirationDate);

    /// <summary>
    /// Retrieves a long-running operation by its ID.
    /// </summary>
    /// <param name="id">The unique identifier of the long-running operation.</param>
    /// <returns>The long-running operation if found; otherwise, null.</returns>
    Task<LongRunningOperation?> GetAsync(Guid id);

    /// <summary>
    /// Retrieves a long-running operation by its ID.
    /// </summary>
    /// <typeparam name="T">The type of the result of the long-running operation.</typeparam>
    /// <param name="id">The unique identifier of the long-running operation.</param>
    /// <returns>The long-running operation if found; otherwise, null.</returns>
    Task<LongRunningOperation<T>?> GetAsync<T>(Guid id);

    /// <summary>
    /// Gets all long-running operations of a specific type, optionally filtered by their statuses.
    /// </summary>
    /// <param name="type">Type of the long-running operation.</param>
    /// <param name="statuses">Array of statuses to filter the operations by.</param>
    /// <param name="skip">Number of entries to skip.</param>
    /// <param name="take">Number of entries to take.</param>
    /// <returns>A paged model of <see cref="LongRunningOperation" /> objects.</returns>
    Task<PagedModel<LongRunningOperation>> GetByTypeAsync(string type, LongRunningOperationStatus[] statuses, int skip, int take);

    /// <summary>
    /// Gets the status of a long-running operation by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier for the operation.</param>
    /// <returns>The long-running operation if found; otherwise, null.</returns>
    Task<LongRunningOperationStatus?> GetStatusAsync(Guid id);

    /// <summary>
    /// Updates the status of a long-running operation identified by its ID.
    /// </summary>
    /// <param name="id">The unique identifier of the long-running operation.</param>
    /// <param name="status">The new status to set for the operation.</param>
    /// <param name="expirationDate">The date and time when the operation should be considered stale.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task UpdateStatusAsync(Guid id, LongRunningOperationStatus status, DateTimeOffset expirationDate);

    /// <summary>
    /// Sets the result of a long-running operation identified by its ID.
    /// </summary>
    /// <param name="id">The unique identifier of the long-running operation.</param>
    /// <param name="result">The result of the operation.</param>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task SetResultAsync<T>(Guid id, T result);

    /// <summary>
    /// Cleans up long-running operations that haven't been updated for a certain period of time.
    /// </summary>
    /// <param name="olderThan">The cutoff date and time for operations to be considered for deletion.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task CleanOperationsAsync(DateTimeOffset olderThan);
}
