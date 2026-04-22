using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security.OperationStatus;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Infrastructure.Security;

namespace Umbraco.Cms.Core.Security;

/// <summary>
/// Provides functionality to manage client credentials associated with back office users in Umbraco.
/// </summary>
public sealed class BackOfficeUserClientCredentialsManager : ClientCredentialsManagerBase, IBackOfficeUserClientCredentialsManager
{
    private readonly IBackOfficeUserManager _backOfficeUserManager;
    private readonly IBackOfficeApplicationManager _backOfficeApplicationManager;
    private readonly IUserService _userService;

    protected override string ClientIdPrefix => Constants.OAuthClientIds.BackOffice;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Core.Security.BackOfficeUserClientCredentialsManager"/> class,
    /// which manages client credentials for back office users.
    /// </summary>
    /// <param name="backOfficeUserManager">The manager responsible for handling back office user operations.</param>
    /// <param name="backOfficeApplicationManager">The manager responsible for handling back office application operations.</param>
    /// <param name="userService">The service used for user-related operations.</param>
    public BackOfficeUserClientCredentialsManager(
        IBackOfficeUserManager backOfficeUserManager,
        IBackOfficeApplicationManager backOfficeApplicationManager,
        IUserService userService)
    {
        _backOfficeUserManager = backOfficeUserManager;
        _userService = userService;
        _backOfficeApplicationManager = backOfficeApplicationManager;
    }

    /// <summary>
    /// Asynchronously saves the client credentials for a specified back office user.
    /// </summary>
    /// <param name="userKey">The unique identifier (<see cref="Guid"/>) of the back office user.</param>
    /// <param name="clientId">The client identifier to associate with the user. This value will be sanitized before use.</param>
    /// <param name="clientSecret">The client secret to associate with the client identifier.</param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous operation. The result contains an <see cref="Attempt{BackOfficeUserClientCredentialsOperationStatus}"/> indicating whether the credentials were saved successfully or if an error occurred (such as invalid user, duplicate client ID, or invalid client ID).
    /// </returns>
    public async Task<Attempt<BackOfficeUserClientCredentialsOperationStatus>> SaveAsync(Guid userKey, string clientId, string clientSecret)
    {
        clientId = SafeClientId(clientId);
        UserClientCredentialsOperationStatus result = await _userService.AddClientIdAsync(userKey, clientId);

        if (result != UserClientCredentialsOperationStatus.Success)
        {
            return result switch
            {
                UserClientCredentialsOperationStatus.InvalidUser => Attempt.Fail(BackOfficeUserClientCredentialsOperationStatus.InvalidUser),
                UserClientCredentialsOperationStatus.DuplicateClientId => Attempt.Fail(BackOfficeUserClientCredentialsOperationStatus.DuplicateClientId),
                UserClientCredentialsOperationStatus.InvalidClientId => Attempt.Fail(BackOfficeUserClientCredentialsOperationStatus.InvalidClientId),
                _ => throw new ArgumentOutOfRangeException($"Unsupported client ID operation status: {result}")
            };
        }

        await _backOfficeApplicationManager.EnsureBackOfficeClientCredentialsApplicationAsync(clientId, clientSecret);

        return Attempt.Succeed(BackOfficeUserClientCredentialsOperationStatus.Success);
    }

    /// <summary>
    /// Deletes the client credentials associated with the specified back office user and client ID.
    /// </summary>
    /// <param name="userKey">The unique identifier of the back office user.</param>
    /// <param name="clientId">The client ID of the credentials to delete.</param>
    /// <returns>An <see cref="Attempt\{BackOfficeUserClientCredentialsOperationStatus\}"/> indicating the result of the delete operation.</returns>
    public async Task<Attempt<BackOfficeUserClientCredentialsOperationStatus>> DeleteAsync(Guid userKey, string clientId)
    {
        clientId = SafeClientId(clientId);

        await _backOfficeApplicationManager.DeleteBackOfficeClientCredentialsApplicationAsync(clientId);
        await _userService.RemoveClientIdAsync(userKey, clientId);

        return Attempt.Succeed(BackOfficeUserClientCredentialsOperationStatus.Success);
    }

    /// <summary>
    /// Asynchronously finds a back office user associated with the specified client ID.
    /// </summary>
    /// <param name="clientId">The client ID used to locate the back office user.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the <see cref="BackOfficeIdentityUser"/> if a matching and approved user is found; otherwise, <c>null</c>.</returns>
    public async Task<BackOfficeIdentityUser?> FindUserAsync(string clientId)
    {
        IUser? user = await _userService.FindByClientIdAsync(SafeClientId(clientId));
        if (user is null || user.IsApproved is false)
        {
            return null;
        }

        return await _backOfficeUserManager.FindByNameAsync(user.Username);
    }

    /// <summary>
    /// Gets the client IDs associated with the specified back office user.
    /// </summary>
    /// <param name="userKey">The unique identifier of the back office user.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable of client ID strings.</returns>
    public async Task<IEnumerable<string>> GetClientIdsAsync(Guid userKey)
        => await _userService.GetClientIdsAsync(userKey);
}
