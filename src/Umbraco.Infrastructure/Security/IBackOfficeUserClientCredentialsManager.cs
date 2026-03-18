using Umbraco.Cms.Core.Security.OperationStatus;

namespace Umbraco.Cms.Core.Security;

/// <summary>
/// Provides methods for managing client credentials associated with back office users.
/// </summary>
public interface IBackOfficeUserClientCredentialsManager
{
    /// <summary>
    /// Asynchronously finds the back office user associated with the specified client ID.
    /// </summary>
    /// <param name="clientId">The client ID to search for.</param>
    /// <returns>A task representing the asynchronous operation, with the result containing the <see cref="BackOfficeIdentityUser"/> if found; otherwise, <c>null</c>.</returns>
    Task<BackOfficeIdentityUser?> FindUserAsync(string clientId);

    /// <summary>
    /// Asynchronously retrieves the client IDs associated with the specified back office user.
    /// </summary>
    /// <param name="userKey">The unique identifier of the back office user.</param>
    /// <returns>A task representing the asynchronous operation, containing an enumerable of client ID strings.</returns>
    Task<IEnumerable<string>> GetClientIdsAsync(Guid userKey);

    /// <summary>
    /// Asynchronously saves client credentials for the specified back office user.
    /// </summary>
    /// <param name="userKey">The unique identifier of the back office user.</param>
    /// <param name="clientId">The client identifier to save.</param>
    /// <param name="clientSecret">The client secret to save.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation, containing an <see cref="Attempt{BackOfficeUserClientCredentialsOperationStatus}"/> indicating the result of the operation.</returns>
    Task<Attempt<BackOfficeUserClientCredentialsOperationStatus>> SaveAsync(Guid userKey, string clientId, string clientSecret);

    /// <summary>
    /// Deletes the client credentials associated with the specified back office user.
    /// </summary>
    /// <param name="userKey">The unique identifier of the back office user whose credentials are to be deleted.</param>
    /// <param name="clientId">The identifier of the client credentials to delete.</param>
    /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous delete operation. The task result contains an <see cref="Attempt{BackOfficeUserClientCredentialsOperationStatus}"/> indicating the outcome of the operation.</returns>
    Task<Attempt<BackOfficeUserClientCredentialsOperationStatus>> DeleteAsync(Guid userKey, string clientId);
}
