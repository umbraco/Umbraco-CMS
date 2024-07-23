using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security.OperationStatus;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Security;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Security;

public class BackOfficeUserClientCredentialsManager : IBackOfficeUserClientCredentialsManager
{
    private readonly IBackOfficeUserManager _backOfficeUserManager;
    private readonly IBackOfficeApplicationManager _backOfficeApplicationManager;
    private readonly IUserService _userService;

    public BackOfficeUserClientCredentialsManager(
        IBackOfficeUserManager backOfficeUserManager,
        IBackOfficeApplicationManager backOfficeApplicationManager,
        IUserService userService)
    {
        _backOfficeUserManager = backOfficeUserManager;
        _userService = userService;
        _backOfficeApplicationManager = backOfficeApplicationManager;
    }

    public async Task<Attempt<BackOfficeUserClientCredentialsOperationStatus>> SaveAsync(Guid userKey, string clientId, string clientSecret)
    {
        if (IsReservedClientId(clientId))
        {
            return Attempt.Fail(BackOfficeUserClientCredentialsOperationStatus.ReservedClientId);
        }

        if (await _userService.AddClientIdAsync(userKey, clientId) is false)
        {
            return Attempt.Fail(BackOfficeUserClientCredentialsOperationStatus.DuplicateClientId);
        }

        await _backOfficeApplicationManager.EnsureBackOfficeClientCredentialsApplicationAsync(clientId, clientSecret);

        return Attempt.Succeed(BackOfficeUserClientCredentialsOperationStatus.Success);
    }

    public async Task<Attempt<BackOfficeUserClientCredentialsOperationStatus>> DeleteAsync(Guid userKey, string clientId)
    {
        if (IsReservedClientId(clientId))
        {
            return Attempt.Fail(BackOfficeUserClientCredentialsOperationStatus.ReservedClientId);
        }

        await _backOfficeApplicationManager.DeleteBackOfficeClientCredentialsApplicationAsync(clientId);
        await _userService.RemoveClientIdAsync(userKey, clientId);

        return Attempt.Succeed(BackOfficeUserClientCredentialsOperationStatus.Success);
    }

    public async Task<BackOfficeIdentityUser?> FindUserAsync(string clientId)
    {
        IUser? user = await _userService.FindByClientIdAsync(clientId);
        if (user is null || user.IsApproved is false)
        {
            return null;
        }

        return await _backOfficeUserManager.FindByNameAsync(user.Username);
    }

    private static bool IsReservedClientId(string clientId)
        => clientId.InvariantEquals(Constants.OAuthClientIds.BackOffice)
           || clientId.InvariantEquals(Constants.OAuthClientIds.Postman)
           || clientId.InvariantEquals(Constants.OAuthClientIds.Swagger);
}
