﻿using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security.OperationStatus;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Security;

namespace Umbraco.Cms.Core.Security;

public sealed class BackOfficeUserClientCredentialsManager : ClientCredentialsManagerBase, IBackOfficeUserClientCredentialsManager
{
    private readonly IBackOfficeUserManager _backOfficeUserManager;
    private readonly IBackOfficeApplicationManager _backOfficeApplicationManager;
    private readonly IUserService _userService;

    protected override string ClientIdPrefix => Constants.OAuthClientIds.BackOffice;

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
        clientId = SafeClientId(clientId);

        if (await _userService.AddClientIdAsync(userKey, clientId) is false)
        {
            return Attempt.Fail(BackOfficeUserClientCredentialsOperationStatus.DuplicateClientId);
        }

        await _backOfficeApplicationManager.EnsureBackOfficeClientCredentialsApplicationAsync(clientId, clientSecret);

        return Attempt.Succeed(BackOfficeUserClientCredentialsOperationStatus.Success);
    }

    public async Task<Attempt<BackOfficeUserClientCredentialsOperationStatus>> DeleteAsync(Guid userKey, string clientId)
    {
        clientId = SafeClientId(clientId);

        await _backOfficeApplicationManager.DeleteBackOfficeClientCredentialsApplicationAsync(clientId);
        await _userService.RemoveClientIdAsync(userKey, clientId);

        return Attempt.Succeed(BackOfficeUserClientCredentialsOperationStatus.Success);
    }

    public async Task<BackOfficeIdentityUser?> FindUserAsync(string clientId)
    {
        IUser? user = await _userService.FindByClientIdAsync(SafeClientId(clientId));
        if (user is null || user.IsApproved is false)
        {
            return null;
        }

        return await _backOfficeUserManager.FindByNameAsync(user.Username);
    }
}
