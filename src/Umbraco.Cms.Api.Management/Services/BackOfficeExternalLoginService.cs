using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Umbraco.Cms.Api.Management.Security;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Services;

public class BackOfficeExternalLoginService : IBackOfficeExternalLoginService
{
    private readonly IBackOfficeExternalLoginProviders _backOfficeExternalLoginProviders;
    private readonly IUserService _userService;
    private readonly IBackOfficeUserManager _backOfficeUserManager;
    private readonly IBackOfficeSignInManager _backOfficeSignInManager;

    public BackOfficeExternalLoginService(
        IBackOfficeExternalLoginProviders backOfficeExternalLoginProviders,
        IUserService userService,
        IBackOfficeUserManager backOfficeUserManager,
        IBackOfficeSignInManager backOfficeSignInManager)
    {
        _backOfficeExternalLoginProviders = backOfficeExternalLoginProviders;
        _userService = userService;
        _backOfficeUserManager = backOfficeUserManager;
        _backOfficeSignInManager = backOfficeSignInManager;
    }

    public async Task<Attempt<IEnumerable<UserExternalLoginProviderModel>, ExternalLoginOperationStatus>>
        ExternalLoginStatusForUserAsync(Guid userid)
    {
        IEnumerable<BackOfficeExternaLoginProviderScheme> providers =
            await _backOfficeExternalLoginProviders.GetBackOfficeProvidersAsync();

        Attempt<ICollection<IIdentityUserLogin>, UserOperationStatus> linkedLoginsAttempt =
            await _userService.GetLinkedLoginsAsync(userid);
        if (linkedLoginsAttempt.Success is false)
        {
            return Attempt<IEnumerable<UserExternalLoginProviderModel>, ExternalLoginOperationStatus>.Fail(
                FromUserOperationStatusFailure(linkedLoginsAttempt.Status),
                Enumerable.Empty<UserExternalLoginProviderModel>());
        }

        IEnumerable<UserExternalLoginProviderModel> providerStatuses = providers.Select(
            providerScheme => new UserExternalLoginProviderModel(
                providerScheme.ExternalLoginProvider.AuthenticationType,
                linkedLoginsAttempt.Result.Any(linkedLogin =>
                    linkedLogin.LoginProvider == providerScheme.ExternalLoginProvider.AuthenticationType),
                providerScheme.ExternalLoginProvider.Options.AutoLinkOptions.AllowManualLinking));

        return Attempt<IEnumerable<UserExternalLoginProviderModel>, ExternalLoginOperationStatus>.Succeed(
                ExternalLoginOperationStatus.Success, providerStatuses);
    }

    public async Task<Attempt<ExternalLoginOperationStatus>> UnLinkLoginAsync(ClaimsPrincipal claimsPrincipal, string loginProvider, string providerKey)
    {
        var userId = claimsPrincipal.Identity?.GetUserId();
        if (userId is null)
        {
            return Attempt.Fail(ExternalLoginOperationStatus.IdentityNotFound);
        }

        BackOfficeIdentityUser? user = await _backOfficeUserManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Attempt.Fail(ExternalLoginOperationStatus.UserNotFound);
        }

        AuthenticationScheme? authType = (await _backOfficeSignInManager.GetExternalAuthenticationSchemesAsync())
            .FirstOrDefault(x => x.Name == loginProvider);

        if (authType == null)
        {
            return Attempt.Fail(ExternalLoginOperationStatus.AuthenticationSchemeNotFound);
        }

        BackOfficeExternaLoginProviderScheme? opt = await _backOfficeExternalLoginProviders.GetAsync(authType.Name);
        if (opt == null)
        {
            return Attempt.Fail(ExternalLoginOperationStatus.AuthenticationOptionsNotFound);
        }

        if (!opt.ExternalLoginProvider.Options.AutoLinkOptions.AllowManualLinking)
        {
            return Attempt.Fail(ExternalLoginOperationStatus.UnlinkingDisabled);
        }

        IEnumerable<IIdentityUserLogin> externalLogins = user.Logins.Where(l => l.LoginProvider == loginProvider);
        if (externalLogins.Any(l => l.ProviderKey == providerKey) == false)
        {
            return Attempt.Fail(ExternalLoginOperationStatus.InvalidProviderKey);
        }

        IdentityResult result = await _backOfficeUserManager.RemoveLoginAsync(
            user,
            loginProvider,
            providerKey);

        if (result.Succeeded is false)
        {
            return Attempt.Fail(ExternalLoginOperationStatus.Unknown);
        }

        await _backOfficeSignInManager.SignInAsync(user, true);
        return Attempt.Succeed(ExternalLoginOperationStatus.Success);
    }

    public async Task<Attempt<IEnumerable<IdentityError>, ExternalLoginOperationStatus>> HandleLoginCallbackAsync(HttpContext httpContext)
    {
        AuthenticateResult cookieAuthenticatedUserAttempt =
            await httpContext.AuthenticateAsync(Constants.Security.BackOfficeAuthenticationType);

        if (cookieAuthenticatedUserAttempt.Succeeded == false)
        {
            return Attempt.FailWithStatus(ExternalLoginOperationStatus.Unauthorized, Enumerable.Empty<IdentityError>());
        }

        BackOfficeIdentityUser? user = await _backOfficeUserManager.GetUserAsync(cookieAuthenticatedUserAttempt.Principal);
        if (user == null)
        {
            return Attempt.FailWithStatus(ExternalLoginOperationStatus.UserNotFound, Enumerable.Empty<IdentityError>());
        }

        ExternalLoginInfo? info =
            await _backOfficeSignInManager.GetExternalLoginInfoAsync();

        if (info == null)
        {
            return Attempt.FailWithStatus(ExternalLoginOperationStatus.ExternalInfoNotFound, Enumerable.Empty<IdentityError>());
        }

        IdentityResult addLoginResult = await _backOfficeUserManager.AddLoginAsync(user, info);
        if (addLoginResult.Succeeded is false)
        {
            return Attempt.FailWithStatus(ExternalLoginOperationStatus.IdentityFailure, addLoginResult.Errors);
        }

        // Update any authentication tokens if succeeded
        await _backOfficeSignInManager.UpdateExternalAuthenticationTokensAsync(info);
        return Attempt.SucceedWithStatus(ExternalLoginOperationStatus.Success, Enumerable.Empty<IdentityError>());
    }

    private ExternalLoginOperationStatus FromUserOperationStatusFailure(UserOperationStatus userOperationStatus) =>
        userOperationStatus switch
        {
            UserOperationStatus.MissingUser => ExternalLoginOperationStatus.UserNotFound,
            _ => ExternalLoginOperationStatus.Unknown
        };
}
