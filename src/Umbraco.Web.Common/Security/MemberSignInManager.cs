using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Security;

/// <summary>
///     The sign in manager for members
/// </summary>
public class MemberSignInManager : UmbracoSignInManager<MemberIdentityUser>, IMemberSignInManager
{
    private readonly IEventAggregator _eventAggregator;
    private readonly IMemberExternalLoginProviders _memberExternalLoginProviders;

    public MemberSignInManager(
        UserManager<MemberIdentityUser> memberManager,
        IHttpContextAccessor contextAccessor,
        IUserClaimsPrincipalFactory<MemberIdentityUser> claimsFactory,
        IOptions<IdentityOptions> optionsAccessor,
        ILogger<SignInManager<MemberIdentityUser>> logger,
        IAuthenticationSchemeProvider schemes,
        IUserConfirmation<MemberIdentityUser> confirmation,
        IMemberExternalLoginProviders memberExternalLoginProviders,
        IEventAggregator eventAggregator)
        : base(memberManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
    {
        _memberExternalLoginProviders = memberExternalLoginProviders;
        _eventAggregator = eventAggregator;
    }

    [Obsolete("Use ctor with all params")]
    public MemberSignInManager(
        UserManager<MemberIdentityUser> memberManager,
        IHttpContextAccessor contextAccessor,
        IUserClaimsPrincipalFactory<MemberIdentityUser> claimsFactory,
        IOptions<IdentityOptions> optionsAccessor,
        ILogger<SignInManager<MemberIdentityUser>> logger,
        IAuthenticationSchemeProvider schemes,
        IUserConfirmation<MemberIdentityUser> confirmation)
        : this(
            memberManager,
            contextAccessor,
            claimsFactory,
            optionsAccessor,
            logger,
            schemes,
            confirmation,
            StaticServiceProvider.Instance.GetRequiredService<IMemberExternalLoginProviders>(),
            StaticServiceProvider.Instance.GetRequiredService<IEventAggregator>())
    {
    }

    // use default scheme for members
    protected override string AuthenticationType => IdentityConstants.ApplicationScheme;

    // use default scheme for members
    protected override string ExternalAuthenticationType => IdentityConstants.ExternalScheme;

    // use default scheme for members
    protected override string TwoFactorAuthenticationType => IdentityConstants.TwoFactorUserIdScheme;

    // use default scheme for members
    protected override string TwoFactorRememberMeAuthenticationType => IdentityConstants.TwoFactorRememberMeScheme;

    public override async Task<ExternalLoginInfo?> GetExternalLoginInfoAsync(string? expectedXsrf = null)
    {
        // borrowed from https://github.com/dotnet/aspnetcore/blob/master/src/Identity/Core/src/SignInManager.cs#L422
        // to replace the auth scheme
        AuthenticateResult auth = await Context.AuthenticateAsync(ExternalAuthenticationType);
        IDictionary<string, string?>? items = auth.Properties?.Items;
        if (auth.Principal == null || items == null)
        {
            Logger.LogDebug(
                auth.Failure ??
                new NullReferenceException("Context.AuthenticateAsync(ExternalAuthenticationType) is null"),
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
        if (providerKey == null || provider is null)
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

    /// <summary>
    ///     Custom ExternalLoginSignInAsync overload for handling external sign in with auto-linking
    /// </summary>
    public async Task<SignInResult> ExternalLoginSignInAsync(ExternalLoginInfo loginInfo, bool isPersistent, bool bypassTwoFactor = false)
    {
        // borrowed from https://github.com/dotnet/aspnetcore/blob/master/src/Identity/Core/src/SignInManager.cs
        // to be able to deal with auto-linking and reduce duplicate lookups
        MemberExternalSignInAutoLinkOptions? autoLinkOptions =
            (await _memberExternalLoginProviders.GetAsync(loginInfo.LoginProvider))?.ExternalLoginProvider.Options.AutoLinkOptions;
        MemberIdentityUser? user = await UserManager.FindByLoginAsync(loginInfo.LoginProvider, loginInfo.ProviderKey);
        if (user == null)
        {
            // user doesn't exist so see if we can auto link
            return await AutoLinkAndSignInExternalAccount(loginInfo, autoLinkOptions);
        }

        if (autoLinkOptions != null && autoLinkOptions.OnExternalLogin != null)
        {
            var shouldSignIn = autoLinkOptions.OnExternalLogin(user, loginInfo);
            if (shouldSignIn == false)
            {
                LogFailedExternalLogin(loginInfo, user);
                return ExternalLoginSignInResult.NotAllowed;
            }
        }

        SignInResult? error = await PreSignInCheck(user);
        if (error != null)
        {
            return error;
        }

        return await SignInOrTwoFactorAsync(user, isPersistent, loginInfo.LoginProvider, bypassTwoFactor);
    }

    public override AuthenticationProperties ConfigureExternalAuthenticationProperties(
        string provider,
        string redirectUrl,
        string? userId = null)
    {
        // borrowed from https://github.com/dotnet/aspnetcore/blob/master/src/Identity/Core/src/SignInManager.cs
        // to be able to use our own XsrfKey/LoginProviderKey because the default is private :/
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        properties.Items[UmbracoSignInMgrLoginProviderKey] = provider;
        if (userId != null)
        {
            properties.Items[UmbracoSignInMgrXsrfKey] = userId;
        }

        return properties;
    }


    protected override async Task<SignInResult> SignInOrTwoFactorAsync(MemberIdentityUser user, bool isPersistent, string? loginProvider = null, bool bypassTwoFactor = false)
    {
        SignInResult result = await base.SignInOrTwoFactorAsync(user, isPersistent, loginProvider, bypassTwoFactor);

        if (result.RequiresTwoFactor)
        {
            NotifyRequiresTwoFactor(user);
        }

        return result;
    }

    /// <summary>
    ///     Used for auto linking/creating user accounts for external logins
    /// </summary>
    /// <param name="loginInfo"></param>
    /// <param name="autoLinkOptions"></param>
    /// <returns></returns>
    private async Task<SignInResult> AutoLinkAndSignInExternalAccount(
        ExternalLoginInfo loginInfo,
        MemberExternalSignInAutoLinkOptions? autoLinkOptions)
    {
        // If there are no autolink options then the attempt is failed (user does not exist)
        if (autoLinkOptions == null || !autoLinkOptions.AutoLinkExternalAccount)
        {
            return SignInResult.Failed;
        }

        var email = loginInfo.Principal.FindFirstValue(ClaimTypes.Email);

        // we are allowing auto-linking/creating of local accounts
        if (email.IsNullOrWhiteSpace())
        {
            return AutoLinkSignInResult.FailedNoEmail;
        }

        // Now we need to perform the auto-link, so first we need to lookup/create a user with the email address
        MemberIdentityUser? autoLinkUser = await UserManager.FindByEmailAsync(email);
        if (autoLinkUser != null)
        {
            try
            {
                // call the callback if one is assigned
                autoLinkOptions.OnAutoLinking?.Invoke(autoLinkUser, loginInfo);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Could not link login provider {LoginProvider}.", loginInfo.LoginProvider);
                return AutoLinkSignInResult.FailedException(ex.Message);
            }

            var shouldLinkUser = autoLinkOptions.OnExternalLogin == null ||
                                 autoLinkOptions.OnExternalLogin(autoLinkUser, loginInfo);
            if (shouldLinkUser)
            {
                return await LinkUser(autoLinkUser, loginInfo);
            }

            LogFailedExternalLogin(loginInfo, autoLinkUser);
            return ExternalLoginSignInResult.NotAllowed;
        }

        var name = loginInfo.Principal?.Identity?.Name;
        if (name.IsNullOrWhiteSpace())
        {
            throw new InvalidOperationException("The Name value cannot be null");
        }

        autoLinkUser = MemberIdentityUser.CreateNew(email, email, autoLinkOptions.DefaultMemberTypeAlias, autoLinkOptions.DefaultIsApproved, name);

        foreach (var userGroup in autoLinkOptions.DefaultMemberGroups)
        {
            autoLinkUser.AddRole(userGroup);
        }

        // call the callback if one is assigned
        try
        {
            autoLinkOptions.OnAutoLinking?.Invoke(autoLinkUser, loginInfo);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Could not link login provider {LoginProvider}.", loginInfo.LoginProvider);
            return AutoLinkSignInResult.FailedException(ex.Message);
        }

        IdentityResult? userCreationResult = await UserManager.CreateAsync(autoLinkUser);

        if (!userCreationResult.Succeeded)
        {
            return AutoLinkSignInResult.FailedCreatingUser(
                userCreationResult.Errors.Select(x => x.Description).ToList());
        }

        {
            var shouldLinkUser = autoLinkOptions.OnExternalLogin == null ||
                                 autoLinkOptions.OnExternalLogin(autoLinkUser, loginInfo);
            if (shouldLinkUser)
            {
                return await LinkUser(autoLinkUser, loginInfo);
            }

            LogFailedExternalLogin(loginInfo, autoLinkUser);
            return ExternalLoginSignInResult.NotAllowed;
        }
    }

    private async Task<SignInResult> LinkUser(MemberIdentityUser autoLinkUser, ExternalLoginInfo loginInfo)
    {
        IList<UserLoginInfo>? existingLogins = await UserManager.GetLoginsAsync(autoLinkUser);
        UserLoginInfo? exists = existingLogins.FirstOrDefault(x =>
            x.LoginProvider == loginInfo.LoginProvider && x.ProviderKey == loginInfo.ProviderKey);

        // if it already exists (perhaps it was added in the AutoLink callbak) then we just continue
        if (exists != null)
        {
            // sign in
            return await SignInOrTwoFactorAsync(autoLinkUser, false, loginInfo.LoginProvider);
        }

        IdentityResult? linkResult = await UserManager.AddLoginAsync(autoLinkUser, loginInfo);
        if (linkResult.Succeeded)
        {
            // we're good! sign in
            return await SignInOrTwoFactorAsync(autoLinkUser, false, loginInfo.LoginProvider);
        }

        // If this fails, we should disapprove the member,as it is now in an inconsistent state.
        return await HandleFailedLinkingUser(autoLinkUser, linkResult);
    }

    protected Task<AutoLinkSignInResult> HandleFailedLinkingUser(MemberIdentityUser autoLinkUser, IdentityResult linkResult)
    {
        var errors = linkResult.Errors.Select(x => x.Description).ToList();

        Logger.LogError("Failed to external link user. The following errors happened: {errors}", errors);
        return Task.FromResult(AutoLinkSignInResult.FailedLinkingUser(errors));
    }

    private void LogFailedExternalLogin(ExternalLoginInfo loginInfo, MemberIdentityUser user) =>
        Logger.LogWarning(
            "The AutoLinkOptions of the external authentication provider '{LoginProvider}' have refused the login based on the OnExternalLogin method. Affected user id: '{UserId}'",
            loginInfo.LoginProvider,
            user.Id);

    protected void NotifyRequiresTwoFactor(MemberIdentityUser user) => Notify(
        user,
        currentUser => new MemberTwoFactorRequestedNotification(currentUser.Key));

    private T Notify<T>(MemberIdentityUser currentUser, Func<MemberIdentityUser, T> createNotification)
        where T : INotification
    {
        T notification = createNotification(currentUser);
        _eventAggregator.Publish(notification);
        return notification;
    }

    // TODO in v10 we can share this with backoffice by moving the backoffice into common.
    public class ExternalLoginSignInResult : SignInResult
    {
        public static new ExternalLoginSignInResult NotAllowed { get; } = new() { Succeeded = false };
    }

    // TODO in v10 we can share this with backoffice by moving the backoffice into common.
    public class AutoLinkSignInResult : SignInResult
    {
        public AutoLinkSignInResult(IReadOnlyCollection<string> errors) =>
            Errors = errors ?? throw new ArgumentNullException(nameof(errors));

        public AutoLinkSignInResult()
        {
        }

        public static AutoLinkSignInResult FailedNotLinked { get; } = new() { Succeeded = false };

        public static AutoLinkSignInResult FailedNoEmail { get; } = new() { Succeeded = false };

        public IReadOnlyCollection<string> Errors { get; } = Array.Empty<string>();

        public static AutoLinkSignInResult FailedException(string error) => new(new[] { error }) { Succeeded = false };

        public static AutoLinkSignInResult FailedCreatingUser(IReadOnlyCollection<string> errors) =>
            new(errors) { Succeeded = false };

        public static AutoLinkSignInResult FailedLinkingUser(IReadOnlyCollection<string> errors) =>
            new(errors) { Succeeded = false };
    }
}
