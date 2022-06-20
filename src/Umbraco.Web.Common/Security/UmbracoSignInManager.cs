using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Security;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Security;

/// <summary>
///     Abstract sign in manager implementation allowing modifying all defeault authentication schemes
/// </summary>
/// <typeparam name="TUser"></typeparam>
public abstract class UmbracoSignInManager<TUser> : SignInManager<TUser>
    where TUser : UmbracoIdentityUser
{
    // borrowed from https://github.com/dotnet/aspnetcore/blob/master/src/Identity/Core/src/SignInManager.cs
    protected const string UmbracoSignInMgrLoginProviderKey = "LoginProvider";

    // borrowed from https://github.com/dotnet/aspnetcore/blob/master/src/Identity/Core/src/SignInManager.cs
    protected const string UmbracoSignInMgrXsrfKey = "XsrfId";

    public UmbracoSignInManager(
        UserManager<TUser> userManager,
        IHttpContextAccessor contextAccessor,
        IUserClaimsPrincipalFactory<TUser> claimsFactory,
        IOptions<IdentityOptions> optionsAccessor,
        ILogger<SignInManager<TUser>> logger,
        IAuthenticationSchemeProvider schemes,
        IUserConfirmation<TUser> confirmation)
        : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
    {
    }

    protected abstract string AuthenticationType { get; }

    protected abstract string ExternalAuthenticationType { get; }

    protected abstract string TwoFactorAuthenticationType { get; }

    protected abstract string TwoFactorRememberMeAuthenticationType { get; }

    /// <inheritdoc />
    public override async Task<SignInResult> PasswordSignInAsync(TUser user, string password, bool isPersistent, bool lockoutOnFailure)
    {
        // override to handle logging/events
        SignInResult result = await base.PasswordSignInAsync(user, password, isPersistent, lockoutOnFailure);
        return await HandleSignIn(user, user.UserName, result);
    }

    /// <inheritdoc />
    public override async Task<ExternalLoginInfo?> GetExternalLoginInfoAsync(string? expectedXsrf = null)
    {
        // borrowed from https://github.com/dotnet/aspnetcore/blob/master/src/Identity/Core/src/SignInManager.cs#L422
        // to replace the auth scheme
        AuthenticateResult auth = await Context.AuthenticateAsync(ExternalAuthenticationType);
        IDictionary<string, string?>? items = auth.Properties?.Items;
        if (auth.Principal == null || items == null)
        {
            Logger.LogDebug(
                "The external login authentication failed. No user Principal or authentication items was resolved.");
            return null;
        }

        if (!items.ContainsKey(UmbracoSignInMgrLoginProviderKey))
        {
            throw new InvalidOperationException(
                $"The external login authenticated successfully but the key {UmbracoSignInMgrLoginProviderKey} was not found in the authentication properties. Ensure you call SignInManager.ConfigureExternalAuthenticationProperties before issuing a ChallengeResult.");
        }

        if (expectedXsrf != null)
        {
            if (!items.ContainsKey(UmbracoSignInMgrXsrfKey))
            {
                return null;
            }

            var userId = items[UmbracoSignInMgrXsrfKey];
            if (userId != expectedXsrf)
            {
                return null;
            }
        }

        var providerKey = auth.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var provider = items[UmbracoSignInMgrLoginProviderKey];
        if (providerKey == null || provider == null)
        {
            return null;
        }

        var providerDisplayName =
            (await GetExternalAuthenticationSchemesAsync()).FirstOrDefault(p => p.Name == provider)?.DisplayName ??
            provider;
        return new ExternalLoginInfo(auth.Principal, provider, providerKey, providerDisplayName)
        {
            AuthenticationTokens = auth.Properties?.GetTokens(),
            AuthenticationProperties = auth.Properties,
        };
    }

    /// <inheritdoc />
    public override async Task<TUser> GetTwoFactorAuthenticationUserAsync()
    {
        // borrowed from https://github.com/dotnet/aspnetcore/blob/master/src/Identity/Core/src/SignInManager.cs
        // replaced in order to use a custom auth type
        TwoFactorAuthenticationInfo? info = await RetrieveTwoFactorInfoAsync();
        if (info == null)
        {
            return null!;
        }

        return await UserManager.FindByIdAsync(info.UserId);
    }

    /// <inheritdoc />
    public override async Task<SignInResult> PasswordSignInAsync(string userName, string password, bool isPersistent, bool lockoutOnFailure)
    {
        // override to handle logging/events
        TUser? user = await UserManager.FindByNameAsync(userName);
        if (user == null)
        {
            return await HandleSignIn(null, userName, SignInResult.Failed);
        }

        return await PasswordSignInAsync(user, password, isPersistent, lockoutOnFailure);
    }

    /// <inheritdoc />
    public override bool IsSignedIn(ClaimsPrincipal principal)
    {
        // borrowed from https://github.com/dotnet/aspnetcore/blob/master/src/Identity/Core/src/SignInManager.cs#L126
        // replaced in order to use a custom auth type
        if (principal == null)
        {
            throw new ArgumentNullException(nameof(principal));
        }

        return principal.Identities.Any(i => i.AuthenticationType == AuthenticationType);
    }

    /// <inheritdoc />
    public override async Task<SignInResult> TwoFactorSignInAsync(string? provider, string? code, bool isPersistent, bool rememberClient)
    {
        // borrowed from https://github.com/dotnet/aspnetcore/blob/master/src/Identity/Core/src/SignInManager.cs#L552
        // replaced in order to use a custom auth type and to implement logging/events
        TwoFactorAuthenticationInfo? twoFactorInfo = await RetrieveTwoFactorInfoAsync();
        if (twoFactorInfo == null || twoFactorInfo.UserId == null)
        {
            return SignInResult.Failed;
        }

        TUser? user = await UserManager.FindByIdAsync(twoFactorInfo.UserId);
        if (user == null)
        {
            return SignInResult.Failed;
        }

        SignInResult? error = await PreSignInCheck(user);
        if (error != null)
        {
            return error;
        }

        if (await UserManager.VerifyTwoFactorTokenAsync(user, provider, code))
        {
            await DoTwoFactorSignInAsync(user, twoFactorInfo, isPersistent, rememberClient);
            return await HandleSignIn(user, user.UserName, SignInResult.Success);
        }

        // If the token is incorrect, record the failure which also may cause the user to be locked out
        await UserManager.AccessFailedAsync(user);
        return await HandleSignIn(user, user.UserName, SignInResult.Failed);
    }

    /// <inheritdoc />
    public override async Task RefreshSignInAsync(TUser user)
    {
        // borrowed from https://github.com/dotnet/aspnetcore/blob/master/src/Identity/Core/src/SignInManager.cs#L126
        // replaced in order to use a custom auth type
        AuthenticateResult auth = await Context.AuthenticateAsync(AuthenticationType);
        IList<Claim> claims = Array.Empty<Claim>();

        Claim? authenticationMethod = auth.Principal?.FindFirst(ClaimTypes.AuthenticationMethod);
        Claim? amr = auth.Principal?.FindFirst("amr");

        if (authenticationMethod != null || amr != null)
        {
            claims = new List<Claim>();
            if (authenticationMethod != null)
            {
                claims.Add(authenticationMethod);
            }

            if (amr != null)
            {
                claims.Add(amr);
            }
        }

        await SignInWithClaimsAsync(user, auth.Properties, claims);
    }

    /// <inheritdoc />
    public override async Task SignInWithClaimsAsync(TUser user, AuthenticationProperties? authenticationProperties, IEnumerable<Claim> additionalClaims)
    {
        // override to replace IdentityConstants.ApplicationScheme with custom AuthenticationType
        // code taken from aspnetcore: https://github.com/dotnet/aspnetcore/blob/master/src/Identity/Core/src/SignInManager.cs
        // we also override to set the current HttpContext principal since this isn't done by default

        // we also need to call our handle login to ensure all date/events are set
        await HandleSignIn(user, user.UserName, SignInResult.Success);

        ClaimsPrincipal? userPrincipal = await CreateUserPrincipalAsync(user);
        foreach (Claim claim in additionalClaims)
        {
            userPrincipal.Identities.First().AddClaim(claim);
        }

        // FYI (just for informational purposes):
        // This calls an ext method will eventually reaches `IAuthenticationService.SignInAsync`
        // which then resolves the `IAuthenticationSignInHandler` for the current scheme
        // by calling `IAuthenticationHandlerProvider.GetHandlerAsync(context, scheme);`
        // which then calls `IAuthenticationSignInHandler.SignInAsync` = CookieAuthenticationHandler.HandleSignInAsync

        // Also note, that when the CookieAuthenticationHandler sign in is successful we handle that event within our
        // own ConfigureUmbracoBackOfficeCookieOptions which assigns the current HttpContext.User to the IPrincipal created

        // Also note, this method gets called when performing 2FA logins
        await Context.SignInAsync(
            AuthenticationType,
            userPrincipal,
            authenticationProperties ?? new AuthenticationProperties());
    }

    /// <inheritdoc />
    public override async Task SignOutAsync()
    {
        // override to replace IdentityConstants.ApplicationScheme with custom auth types
        // code taken from aspnetcore: https://github.com/dotnet/aspnetcore/blob/master/src/Identity/Core/src/SignInManager.cs
        await Context.SignOutAsync(AuthenticationType);
        await Context.SignOutAsync(ExternalAuthenticationType);
        await Context.SignOutAsync(TwoFactorAuthenticationType);
    }

    /// <inheritdoc />
    public override async Task<bool> IsTwoFactorClientRememberedAsync(TUser user)
    {
        // borrowed from https://github.com/dotnet/aspnetcore/blob/master/src/Identity/Core/src/SignInManager.cs#L422
        // to replace the auth scheme
        var userId = await UserManager.GetUserIdAsync(user);
        AuthenticateResult result = await Context.AuthenticateAsync(TwoFactorRememberMeAuthenticationType);
        return result.Principal != null && result.Principal.FindFirstValue(ClaimTypes.Name) == userId;
    }

    /// <inheritdoc />
    public override async Task RememberTwoFactorClientAsync(TUser user)
    {
        // borrowed from https://github.com/dotnet/aspnetcore/blob/master/src/Identity/Core/src/SignInManager.cs#L422
        // to replace the auth scheme
        ClaimsPrincipal principal = await StoreRememberClient(user);
        await Context.SignInAsync(
            TwoFactorRememberMeAuthenticationType,
            principal,
            new AuthenticationProperties { IsPersistent = true });
    }

    /// <inheritdoc />
    public override async Task<SignInResult> TwoFactorRecoveryCodeSignInAsync(string recoveryCode)
    {
        // borrowed from https://github.com/dotnet/aspnetcore/blob/master/src/Identity/Core/src/SignInManager.cs#L422
        // to replace the auth scheme
        TwoFactorAuthenticationInfo? twoFactorInfo = await RetrieveTwoFactorInfoAsync();
        if (twoFactorInfo == null || twoFactorInfo.UserId == null)
        {
            return SignInResult.Failed;
        }

        TUser? user = await UserManager.FindByIdAsync(twoFactorInfo.UserId);
        if (user == null)
        {
            return SignInResult.Failed;
        }

        IdentityResult? result = await UserManager.RedeemTwoFactorRecoveryCodeAsync(user, recoveryCode);
        if (result.Succeeded)
        {
            await DoTwoFactorSignInAsync(user, twoFactorInfo, false, false);
            return SignInResult.Success;
        }

        // We don't protect against brute force attacks since codes are expected to be random.
        return SignInResult.Failed;
    }

    /// <inheritdoc />
    public override Task ForgetTwoFactorClientAsync() =>

        // borrowed from https://github.com/dotnet/aspnetcore/blob/master/src/Identity/Core/src/SignInManager.cs#L422
        // to replace the auth scheme
        Context.SignOutAsync(TwoFactorRememberMeAuthenticationType);

    /// <summary>
    ///     Called on any login attempt to update the AccessFailedCount and to raise events
    /// </summary>
    /// <param name="user"></param>
    /// <param name="username"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    protected virtual async Task<SignInResult> HandleSignIn(TUser? user, string? username, SignInResult result)
    {
        // TODO: Here I believe we can do all (or most) of the usermanager event raising so that it is not in the AuthenticationController
        if (username.IsNullOrWhiteSpace())
        {
            username = "UNKNOWN"; // could happen in 2fa or something else weird
        }

        if (result.Succeeded)
        {
            // track the last login date
            user!.LastLoginDateUtc = DateTime.UtcNow;
            if (user.AccessFailedCount > 0)
            {
                // we have successfully logged in, reset the AccessFailedCount
                user.AccessFailedCount = 0;
            }

            await UserManager.UpdateAsync(user);

            Logger.LogInformation("User: {UserName} logged in from IP address {IpAddress}", username, Context.Connection.RemoteIpAddress);
        }
        else if (result.IsLockedOut)
        {
            Logger.LogInformation(
                "Login attempt failed for username {UserName} from IP address {IpAddress}, the user is locked",
                username,
                Context.Connection.RemoteIpAddress);
        }
        else if (result.RequiresTwoFactor)
        {
            Logger.LogInformation(
                "Login attempt requires verification for username {UserName} from IP address {IpAddress}",
                username,
                Context.Connection.RemoteIpAddress);
        }
        else if (!result.Succeeded || result.IsNotAllowed)
        {
            Logger.LogInformation(
                "Login attempt failed for username {UserName} from IP address {IpAddress}",
                username,
                Context.Connection.RemoteIpAddress);
        }
        else
        {
            throw new ArgumentOutOfRangeException();
        }

        return result;
    }

    /// <inheritdoc />
    protected override async Task<SignInResult> SignInOrTwoFactorAsync(TUser user, bool isPersistent, string? loginProvider = null, bool bypassTwoFactor = false)
    {
        // borrowed from https://github.com/dotnet/aspnetcore/blob/master/src/Identity/Core/src/SignInManager.cs
        // to replace custom auth types
        if (!bypassTwoFactor && await IsTfaEnabled(user))
        {
            if (!await IsTwoFactorClientRememberedAsync(user))
            {
                // Store the userId for use after two factor check
                var userId = await UserManager.GetUserIdAsync(user);
                await Context.SignInAsync(TwoFactorAuthenticationType, StoreTwoFactorInfo(userId, loginProvider));
                return SignInResult.TwoFactorRequired;
            }
        }

        // Cleanup external cookie
        if (loginProvider != null)
        {
            await Context.SignOutAsync(ExternalAuthenticationType);
        }

        if (loginProvider == null)
        {
            await SignInWithClaimsAsync(user, isPersistent, new[] { new Claim("amr", "pwd") });
        }
        else
        {
            await SignInAsync(user, isPersistent, loginProvider);
        }

        return SignInResult.Success;
    }

    // borrowed from https://github.com/dotnet/aspnetcore/blob/master/src/Identity/Core/src/SignInManager.cs#L782
    // since it's not public
    private async Task<bool> IsTfaEnabled(TUser user)
        => UserManager.SupportsUserTwoFactor &&
           await UserManager.GetTwoFactorEnabledAsync(user) &&
           (await UserManager.GetValidTwoFactorProvidersAsync(user)).Count > 0;

    // borrowed from https://github.com/dotnet/aspnetcore/blob/master/src/Identity/Core/src/SignInManager.cs#L743
    // to replace custom auth types
    private ClaimsPrincipal StoreTwoFactorInfo(string userId, string? loginProvider)
    {
        var identity = new ClaimsIdentity(TwoFactorAuthenticationType);
        identity.AddClaim(new Claim(ClaimTypes.Name, userId));
        if (loginProvider != null)
        {
            identity.AddClaim(new Claim(ClaimTypes.AuthenticationMethod, loginProvider));
        }

        return new ClaimsPrincipal(identity);
    }

    // borrowed from https://github.com/dotnet/aspnetcore/blob/master/src/Identity/Core/src/SignInManager.cs
    // copy is required in order to use custom auth types
    private async Task<ClaimsPrincipal> StoreRememberClient(TUser user)
    {
        var userId = await UserManager.GetUserIdAsync(user);
        var rememberBrowserIdentity = new ClaimsIdentity(TwoFactorRememberMeAuthenticationType);
        rememberBrowserIdentity.AddClaim(new Claim(ClaimTypes.Name, userId));
        if (UserManager.SupportsUserSecurityStamp)
        {
            var stamp = await UserManager.GetSecurityStampAsync(user);
            rememberBrowserIdentity.AddClaim(new Claim(Options.ClaimsIdentity.SecurityStampClaimType, stamp));
        }

        return new ClaimsPrincipal(rememberBrowserIdentity);
    }

    // borrowed from https://github.com/dotnet/aspnetcore/blob/master/src/Identity/Core/src/SignInManager.cs
    // copy is required in order to use a custom auth type
    private async Task<TwoFactorAuthenticationInfo?> RetrieveTwoFactorInfoAsync()
    {
        AuthenticateResult result = await Context.AuthenticateAsync(TwoFactorAuthenticationType);
        if (result.Principal != null)
        {
            return new TwoFactorAuthenticationInfo
            {
                UserId = result.Principal.FindFirstValue(ClaimTypes.Name),
                LoginProvider = result.Principal.FindFirstValue(ClaimTypes.AuthenticationMethod),
            };
        }

        return null;
    }

    // borrowed from https://github.com/dotnet/aspnetcore/blob/master/src/Identity/Core/src/SignInManager.cs
    // copy is required in order to use custom auth types
    private async Task DoTwoFactorSignInAsync(TUser user, TwoFactorAuthenticationInfo twoFactorInfo, bool isPersistent, bool rememberClient)
    {
        // When token is verified correctly, clear the access failed count used for lockout
        await ResetLockout(user);

        var claims = new List<Claim> { new("amr", "mfa") };

        // Cleanup external cookie
        if (twoFactorInfo.LoginProvider != null)
        {
            claims.Add(new Claim(ClaimTypes.AuthenticationMethod, twoFactorInfo.LoginProvider));
            await Context.SignOutAsync(ExternalAuthenticationType);
        }

        // Cleanup two factor user id cookie
        await Context.SignOutAsync(TwoFactorAuthenticationType);
        if (rememberClient)
        {
            await RememberTwoFactorClientAsync(user);
        }

        await SignInWithClaimsAsync(user, isPersistent, claims);
    }

    // borrowed from https://github.com/dotnet/aspnetcore/blob/master/src/Identity/Core/src/SignInManager.cs#L891
    private class TwoFactorAuthenticationInfo
    {
        public string? UserId { get; set; }

        public string? LoginProvider { get; set; }
    }
}
