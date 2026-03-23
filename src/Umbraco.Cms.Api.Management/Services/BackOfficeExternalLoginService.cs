using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Umbraco.Cms.Api.Management.Models;
using Umbraco.Cms.Api.Management.Security;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Services;

/// <summary>
/// Provides functionality for managing external login providers and authentication for the back office.
/// </summary>
public class BackOfficeExternalLoginService : IBackOfficeExternalLoginService
{
    private readonly IBackOfficeExternalLoginProviders _backOfficeExternalLoginProviders;
    private readonly IUserService _userService;
    private readonly IBackOfficeUserManager _backOfficeUserManager;
    private readonly IBackOfficeSignInManager _backOfficeSignInManager;
    private readonly IMemoryCache _memoryCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="BackOfficeExternalLoginService"/> class with the specified dependencies.
    /// </summary>
    /// <param name="backOfficeExternalLoginProviders">Provides external login providers for back office authentication.</param>
    /// <param name="userService">Service for managing back office users.</param>
    /// <param name="backOfficeUserManager">Manages back office user operations.</param>
    /// <param name="backOfficeSignInManager">Handles sign-in operations for back office users.</param>
    /// <param name="memoryCache">The memory cache used for caching authentication data.</param>
    public BackOfficeExternalLoginService(
        IBackOfficeExternalLoginProviders backOfficeExternalLoginProviders,
        IUserService userService,
        IBackOfficeUserManager backOfficeUserManager,
        IBackOfficeSignInManager backOfficeSignInManager,
        IMemoryCache memoryCache)
    {
        _backOfficeExternalLoginProviders = backOfficeExternalLoginProviders;
        _userService = userService;
        _backOfficeUserManager = backOfficeUserManager;
        _backOfficeSignInManager = backOfficeSignInManager;
        _memoryCache = memoryCache;
    }

    /// <summary>
    /// Asynchronously retrieves the external login providers and their statuses for a specified user.
    /// </summary>
    /// <param name="userKey">The unique identifier (key) of the user whose external login status is to be retrieved.</param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous operation, containing an <see cref="Attempt{IEnumerable{UserExternalLoginProviderModel}, ExternalLoginOperationStatus}"/>.
    /// On success, the result contains a collection of <see cref="UserExternalLoginProviderModel"/> objects describing each available external login provider and whether the user is linked to it.
    /// On failure, the result contains an appropriate <see cref="ExternalLoginOperationStatus"/> error status.
    /// </returns>
    public async Task<Attempt<IEnumerable<UserExternalLoginProviderModel>, ExternalLoginOperationStatus>> ExternalLoginStatusForUserAsync(Guid userKey)
    {
        IEnumerable<BackOfficeExternaLoginProviderScheme> providers =
            await _backOfficeExternalLoginProviders.GetBackOfficeProvidersAsync();

        Attempt<ICollection<IIdentityUserLogin>, UserOperationStatus> linkedLoginsAttempt =
            await _userService.GetLinkedLoginsAsync(userKey);

        if (linkedLoginsAttempt.Success is false)
        {
            return Attempt<IEnumerable<UserExternalLoginProviderModel>, ExternalLoginOperationStatus>.Fail(
                FromUserOperationStatusFailure(linkedLoginsAttempt.Status),
                Enumerable.Empty<UserExternalLoginProviderModel>());
        }

        // The use of SingleOrDefault below is allowed as there is a unique index on the linkedLogins result between user and provider
        IEnumerable<UserExternalLoginProviderModel> providerStatuses = providers.Select(
            providerScheme => new UserExternalLoginProviderModel(
                providerScheme.ExternalLoginProvider.AuthenticationType,
                linkedLoginsAttempt.Result.Any(linkedLogin =>
                    linkedLogin.LoginProvider == providerScheme.ExternalLoginProvider.AuthenticationType),
                providerScheme.ExternalLoginProvider.Options.AutoLinkOptions.AllowManualLinking,
                linkedLoginsAttempt.Result.SingleOrDefault(linkedLogin =>
                    linkedLogin.LoginProvider == providerScheme.ExternalLoginProvider.AuthenticationType)
                    ?.ProviderKey));

        return Attempt<IEnumerable<UserExternalLoginProviderModel>, ExternalLoginOperationStatus>.Succeed(
                ExternalLoginOperationStatus.Success, providerStatuses);
    }

    /// <summary>
    /// Asynchronously unlinks an external login from the specified back office user.
    /// </summary>
    /// <param name="claimsPrincipal">The <see cref="ClaimsPrincipal"/> representing the current authenticated user.</param>
    /// <param name="loginProvider">The name of the external login provider to unlink (e.g., "Google", "AzureAD").</param>
    /// <param name="providerKey">The unique key identifying the external login to be unlinked.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains an <see cref="Attempt{ExternalLoginOperationStatus}"/>,
    /// indicating whether the unlink operation succeeded and providing the status of the operation.
    /// </returns>
    public async Task<Attempt<ExternalLoginOperationStatus>> UnLinkLoginAsync(ClaimsPrincipal claimsPrincipal, string loginProvider, string providerKey)
    {
        var userId = claimsPrincipal.Identity?.GetUserId();
        if (userId is null)
        {
            return Attempt.Fail(ExternalLoginOperationStatus.IdentityNotFound);
        }

        BackOfficeIdentityUser? user = await _backOfficeUserManager.FindByIdAsync(userId);
        if (user is null)
        {
            return Attempt.Fail(ExternalLoginOperationStatus.UserNotFound);
        }

        AuthenticationScheme? authType = (await _backOfficeSignInManager.GetExternalAuthenticationSchemesAsync())
            .FirstOrDefault(x => x.Name == loginProvider);

        if (authType is null)
        {
            return Attempt.Fail(ExternalLoginOperationStatus.AuthenticationSchemeNotFound);
        }

        BackOfficeExternaLoginProviderScheme? opt = await _backOfficeExternalLoginProviders.GetAsync(authType.Name);
        if (opt is null)
        {
            return Attempt.Fail(ExternalLoginOperationStatus.AuthenticationOptionsNotFound);
        }

        if (opt.ExternalLoginProvider.Options.AutoLinkOptions.AllowManualLinking is false)
        {
            return Attempt.Fail(ExternalLoginOperationStatus.UnlinkingDisabled);
        }

        IEnumerable<IIdentityUserLogin> externalLogins = user.Logins.Where(l => l.LoginProvider == loginProvider);
        if (externalLogins.Any(l => l.ProviderKey == providerKey) is false)
        {
            return Attempt.Fail(ExternalLoginOperationStatus.InvalidProviderKey);
        }

        IdentityResult result = await _backOfficeUserManager.RemoveLoginAsync(user, loginProvider, providerKey);

        if (result.Succeeded is false)
        {
            return Attempt.Fail(ExternalLoginOperationStatus.Unknown);
        }

        await _backOfficeSignInManager.SignInAsync(user, true);
        return Attempt.Succeed(ExternalLoginOperationStatus.Success);
    }

    /// <summary>
    /// Handles the callback from an external login provider, attempting to link the external login to the currently authenticated back office user.
    /// </summary>
    /// <param name="httpContext">The current HTTP context containing authentication information from the external provider.</param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous operation. The result is an <see cref="Attempt{TResult, TStatus}"/> containing a collection of <see cref="IdentityError"/> objects if the operation fails, and an <see cref="ExternalLoginOperationStatus"/> indicating the outcome (such as Success, Unauthorized, UserNotFound, ExternalInfoNotFound, or IdentityFailure).
    /// </returns>
    public async Task<Attempt<IEnumerable<IdentityError>, ExternalLoginOperationStatus>> HandleLoginCallbackAsync(HttpContext httpContext)
    {
        AuthenticateResult cookieAuthenticatedUserAttempt =
            await httpContext.AuthenticateAsync(Constants.Security.BackOfficeAuthenticationType);

        if (cookieAuthenticatedUserAttempt.Succeeded is false)
        {
            return Attempt.FailWithStatus(ExternalLoginOperationStatus.Unauthorized, Enumerable.Empty<IdentityError>());
        }

        BackOfficeIdentityUser? user = await _backOfficeUserManager.GetUserAsync(cookieAuthenticatedUserAttempt.Principal);
        if (user is null)
        {
            return Attempt.FailWithStatus(ExternalLoginOperationStatus.UserNotFound, Enumerable.Empty<IdentityError>());
        }

        ExternalLoginInfo? info = await _backOfficeSignInManager.GetExternalLoginInfoAsync();

        if (info is null)
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

    /// <summary>
    /// Generates a secret for the specified external login provider based on the given claims principal.
    /// </summary>
    /// <param name="claimsPrincipal">The claims principal representing the user.</param>
    /// <param name="loginProvider">The external login provider identifier.</param>
    /// <returns>
    /// An <see cref="Attempt"/> containing the generated secret GUID if successful, or a failure status otherwise.
    /// </returns>
    public async Task<Attempt<Guid?, ExternalLoginOperationStatus>> GenerateLoginProviderSecretAsync(ClaimsPrincipal claimsPrincipal, string loginProvider)
    {
        if (claimsPrincipal.Identity is null)
        {
            return Attempt.FailWithStatus<Guid?, ExternalLoginOperationStatus>(ExternalLoginOperationStatus.IdentityNotFound, null);
        }

        IEnumerable<BackOfficeExternaLoginProviderScheme> configuredLoginProviders = await _backOfficeExternalLoginProviders.GetBackOfficeProvidersAsync();
        if (configuredLoginProviders.Any(provider => provider.ExternalLoginProvider.AuthenticationType.Equals(loginProvider))
            is false)
        {
            return Attempt.FailWithStatus<Guid?, ExternalLoginOperationStatus>(ExternalLoginOperationStatus.AuthenticationSchemeNotFound, null);
        }

        var userId = claimsPrincipal.Identity.GetUserId();
        if (userId is null)
        {
            return Attempt.FailWithStatus<Guid?, ExternalLoginOperationStatus>(ExternalLoginOperationStatus.IdentityNotFound, null);
        }

        var secret = Guid.NewGuid();
        _memoryCache.Set(secret, new LoginProviderUserLink { ClaimsPrincipalUserId = userId, LoginProvider = loginProvider }, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30) });

        return Attempt<Guid?, ExternalLoginOperationStatus>.Succeed(ExternalLoginOperationStatus.Success, secret);
    }

    /// <summary>
    /// Retrieves a <see cref="ClaimsPrincipal"/> associated with the specified login provider and link key, if available.
    /// </summary>
    /// <param name="loginProvider">The identifier of the external login provider.</param>
    /// <param name="linkKey">The unique key used to locate the user link in the cache.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that resolves to an <see cref="Attempt{T, TStatus}"/> containing the <see cref="ClaimsPrincipal"/> if successful, or an <see cref="ExternalLoginOperationStatus"/> indicating the failure reason.
    /// </returns>
    public async Task<Attempt<ClaimsPrincipal?, ExternalLoginOperationStatus>> ClaimsPrincipleFromLoginProviderLinkKeyAsync(
        string loginProvider,
        Guid linkKey)
    {
        LoginProviderUserLink? cachedSecretValue = _memoryCache.Get<LoginProviderUserLink>(linkKey);
        if (cachedSecretValue is null)
        {
            return Attempt.FailWithStatus<ClaimsPrincipal?, ExternalLoginOperationStatus>(ExternalLoginOperationStatus.UserSecretNotFound, null);
        }

        if (cachedSecretValue.LoginProvider.Equals(loginProvider) is false)
        {
            return Attempt.FailWithStatus<ClaimsPrincipal?, ExternalLoginOperationStatus>(
                ExternalLoginOperationStatus.InvalidSecret, null);
        }

        BackOfficeIdentityUser? user = await _backOfficeUserManager.FindByIdAsync(cachedSecretValue.ClaimsPrincipalUserId);
        if (user is null)
        {
            return Attempt.FailWithStatus<ClaimsPrincipal?, ExternalLoginOperationStatus>(
                ExternalLoginOperationStatus.IdentityNotFound, null);
        }

        ClaimsPrincipal claimsPrinciple = await _backOfficeSignInManager.CreateUserPrincipalAsync(user);

        _memoryCache.Remove(linkKey);
        return Attempt.SucceedWithStatus<ClaimsPrincipal?, ExternalLoginOperationStatus>(ExternalLoginOperationStatus.Success, claimsPrinciple);
    }

    private ExternalLoginOperationStatus FromUserOperationStatusFailure(UserOperationStatus userOperationStatus) =>
        userOperationStatus switch
        {
            UserOperationStatus.MissingUser => ExternalLoginOperationStatus.UserNotFound,
            _ => ExternalLoginOperationStatus.Unknown
        };
}
