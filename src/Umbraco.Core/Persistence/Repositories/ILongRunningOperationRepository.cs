using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
/// Represents a repository for managing long-running operations.
/// </summary>
public interface ILongRunningOperationRepository
{
    /// <summary>
    /// Retrieves a long-running operation by its type and ID.
    /// </summary>
    /// <param name="type">The type of the long-running operation.</param>
    /// <param name="id">The unique identifier of the long-running operation.</param>
    /// <returns>The long-running operation if found; otherwise, null.</returns>
    LongRunningOperation? Get(string type, Guid id);

    /// <summary>
    /// Retrieves the latest long-running operation of a specific type.
    /// </summary>
    /// <param name="type">The type of the long-running operation.</param>
    /// <returns>The long-running operation if found; otherwise, null.</returns>
    LongRunningOperation? GetLatest(string type);

    /// <summary>
    /// Retrieves a long-running operation by its ID.
    /// </summary>
    /// <param name="operation">The operation to create.</param>
    public void Create(LongRunningOperation operation);

    /// <summary>
    /// Updates the status of a long-running operation identified by its ID.
    /// </summary>
    /// <param name="id">The unique identifier of the long-running operation.</param>
    /// <param name="status">The new status to set for the operation.</param>
    public void UpdateStatus(Guid id, LongRunningOperationStatus status);

    /// <summary>
    /// Gets the result of a long-running operation identified by its ID.
    /// </summary>
    /// <param name="id">The unique identifier of the long-running operation.</param>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <returns>The result of the operation if found; otherwise, default value of T.</returns>
    public T? GetResult<T>(Guid id);

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
    /// <param name="maxAge">The maximum age of operations to keep.</param>
    void CleanOperations(TimeSpan maxAge);
}
