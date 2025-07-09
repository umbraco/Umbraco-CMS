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
    /// <param name="expiryTimeout">The time span after which the operation is considered expired if it hasn't been updated in that time.</param>
    public void Create(LongRunningOperation operation, TimeSpan expiryTimeout);

    /// <summary>
    /// Retrieves a long-running operation by its ID.
    /// </summary>
    /// <param name="id">The unique identifier of the long-running operation.</param>
    /// <returns>The long-running operation if found; otherwise, null.</returns>
    LongRunningOperation? Get(Guid id);

    /// <summary>
    /// Retrieves a long-running operation by its ID.
    /// </summary>
    /// <typeparam name="T">The type of the result of the long-running operation.</typeparam>
    /// <param name="id">The unique identifier of the long-running operation.</param>
    /// <returns>The long-running operation if found; otherwise, null.</returns>
    LongRunningOperation<T>? Get<T>(Guid id);

    /// <summary>
    /// Gets all long-running operations of a specific type, optionally filtered by their statuses.
    /// </summary>
    /// <param name="type">Type of the long-running operation.</param>
    /// <param name="statuses">Array of statuses to filter the operations by.</param>
    /// <returns>A collection of long-running operations matching the specified type and statuses.</returns>
    IEnumerable<LongRunningOperation> GetByType(string type, LongRunningOperationStatus[] statuses);

    /// <summary>
    /// Gets the status of a long-running operation by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier for the operation.</param>
    /// <returns>The long-running operation if found; otherwise, null.</returns>
    LongRunningOperationStatus? GetStatus(Guid id);

    /// <summary>
    /// Updates the status of a long-running operation identified by its ID.
    /// </summary>
    /// <param name="id">The unique identifier of the long-running operation.</param>
    /// <param name="status">The new status to set for the operation.</param>
    /// <param name="expiryTimeout">The time span after which the operation is considered expired if its status hasn't been updated.</param>
    public void UpdateStatus(Guid id, LongRunningOperationStatus status, TimeSpan expiryTimeout);

    /// <summary>
    /// Sets the result of a long-running operation identified by its ID.
    /// </summary>
    /// <param name="id">The unique identifier of the long-running operation.</param>
    /// <param name="result">The result of the operation.</param>
    /// <typeparam name="T">The type of the result.</typeparam>
    public void SetResult<T>(Guid id, T result);

    /// <summary>
    /// Cleans up long-running operations that haven't been updated for a certain period of time.
    /// </summary>
    /// <param name="maxAgeOfOperations">Maximum age of operations to keep.</param>
    void CleanOperations(TimeSpan maxAgeOfOperations);
}
