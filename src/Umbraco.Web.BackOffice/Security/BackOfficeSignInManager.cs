using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Security
{
    using Constants = Core.Constants;

    /// <summary>
    /// The sign in manager for back office users
    /// </summary>
    public class BackOfficeSignInManager : UmbracoSignInManager<BackOfficeIdentityUser>, IBackOfficeSignInManager
    {
        private readonly BackOfficeUserManager _userManager;
        private readonly IBackOfficeExternalLoginProviders _externalLogins;
        private readonly IEventAggregator _eventAggregator;
        private readonly GlobalSettings _globalSettings;

        protected override string AuthenticationType => Constants.Security.BackOfficeAuthenticationType;

        protected override string ExternalAuthenticationType => Constants.Security.BackOfficeExternalAuthenticationType;

        protected override string TwoFactorAuthenticationType => Constants.Security.BackOfficeTwoFactorAuthenticationType;

        protected override string TwoFactorRememberMeAuthenticationType => Constants.Security.BackOfficeTwoFactorRememberMeAuthenticationType;

        public BackOfficeSignInManager(
            BackOfficeUserManager userManager,
            IHttpContextAccessor contextAccessor,
            IBackOfficeExternalLoginProviders externalLogins,
            IUserClaimsPrincipalFactory<BackOfficeIdentityUser> claimsFactory,
            IOptions<IdentityOptions> optionsAccessor,
            IOptions<GlobalSettings> globalSettings,
            ILogger<SignInManager<BackOfficeIdentityUser>> logger,
            IAuthenticationSchemeProvider schemes,
            IUserConfirmation<BackOfficeIdentityUser> confirmation,
            IEventAggregator eventAggregator)
            : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
        {
            _userManager = userManager;
            _externalLogins = externalLogins;
            _eventAggregator = eventAggregator;
            _globalSettings = globalSettings.Value;
        }

        [Obsolete("Use ctor with all params")]
        public BackOfficeSignInManager(
            BackOfficeUserManager userManager,
            IHttpContextAccessor contextAccessor,
            IBackOfficeExternalLoginProviders externalLogins,
            IUserClaimsPrincipalFactory<BackOfficeIdentityUser> claimsFactory,
            IOptions<IdentityOptions> optionsAccessor,
            IOptions<GlobalSettings> globalSettings,
            ILogger<SignInManager<BackOfficeIdentityUser>> logger,
            IAuthenticationSchemeProvider schemes,
            IUserConfirmation<BackOfficeIdentityUser> confirmation)
            : this(userManager, contextAccessor, externalLogins, claimsFactory, optionsAccessor, globalSettings, logger, schemes, confirmation, StaticServiceProvider.Instance.GetRequiredService<IEventAggregator>())
        {

        }

        /// <summary>
        /// Custom ExternalLoginSignInAsync overload for handling external sign in with auto-linking
        /// </summary>
        /// <param name="loginProvider"></param>
        /// <param name="providerKey"></param>
        /// <param name="isPersistent"></param>
        /// <param name="bypassTwoFactor"></param>
        /// <returns></returns>
        public async Task<SignInResult> ExternalLoginSignInAsync(ExternalLoginInfo loginInfo, bool isPersistent, bool bypassTwoFactor = false)
        {
            // borrowed from https://github.com/dotnet/aspnetcore/blob/master/src/Identity/Core/src/SignInManager.cs
            // to be able to deal with auto-linking and reduce duplicate lookups

            var autoLinkOptions = (await _externalLogins.GetAsync(loginInfo.LoginProvider))?.ExternalLoginProvider?.Options?.AutoLinkOptions;
            var user = await UserManager.FindByLoginAsync(loginInfo.LoginProvider, loginInfo.ProviderKey);
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

            var error = await PreSignInCheck(user);
            if (error != null)
            {
                return error;
            }
            return await SignInOrTwoFactorAsync(user, isPersistent, loginInfo.LoginProvider, bypassTwoFactor);
        }

        /// <summary>
        /// Configures the redirect URL and user identifier for the specified external login <paramref name="provider"/>.
        /// </summary>
        /// <param name="provider">The provider to configure.</param>
        /// <param name="redirectUrl">The external login URL users should be redirected to during the login flow.</param>
        /// <param name="userId">The current user's identifier, which will be used to provide CSRF protection.</param>
        /// <returns>A configured <see cref="AuthenticationProperties"/>.</returns>
        public override AuthenticationProperties ConfigureExternalAuthenticationProperties(string provider, string? redirectUrl, string? userId = null)
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

        public override Task<IEnumerable<AuthenticationScheme>> GetExternalAuthenticationSchemesAsync()
        {
            // TODO: We can filter these so that they only include the back office ones.
            // That can be done by either checking the scheme (maybe) or comparing it to what we have registered in the collection of BackOfficeExternalLoginProvider
            return base.GetExternalAuthenticationSchemesAsync();
        }

        /// <summary>
        /// Overridden to deal with events/notificiations
        /// </summary>
        /// <param name="user"></param>
        /// <param name="username"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        protected override async Task<SignInResult> HandleSignIn(BackOfficeIdentityUser? user, string? username, SignInResult result)
        {
            result = await base.HandleSignIn(user, username, result);

            if (result.Succeeded)
            {
                if (user != null)
                {
                    _userManager.NotifyLoginSuccess(Context.User, user.Id);
                }
            }
            else if (result.IsLockedOut)
            {
                _userManager.NotifyAccountLocked(Context.User, user?.Id);
            }
            else if (result.RequiresTwoFactor)
            {
                _userManager.NotifyLoginRequiresVerification(Context.User, user?.Id);
            }
            else if (!result.Succeeded || result.IsNotAllowed)
            {
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }

            return result;
        }

        /// <summary>
        /// Used for auto linking/creating user accounts for external logins
        /// </summary>
        /// <param name="loginInfo"></param>
        /// <param name="autoLinkOptions"></param>
        /// <returns></returns>
        private async Task<SignInResult> AutoLinkAndSignInExternalAccount(ExternalLoginInfo loginInfo, ExternalSignInAutoLinkOptions? autoLinkOptions)
        {
            // If there are no autolink options then the attempt is failed (user does not exist)
            if (autoLinkOptions == null || !autoLinkOptions.AutoLinkExternalAccount)
            {
                return SignInResult.Failed;
            }

            var email = loginInfo.Principal.FindFirstValue(ClaimTypes.Email);

            //we are allowing auto-linking/creating of local accounts
            if (email.IsNullOrWhiteSpace())
            {
                return AutoLinkSignInResult.FailedNoEmail;
            }
            else
            {
                //Now we need to perform the auto-link, so first we need to lookup/create a user with the email address
                var autoLinkUser = await UserManager.FindByEmailAsync(email);
                if (autoLinkUser != null)
                {
                    try
                    {
                        //call the callback if one is assigned
                        autoLinkOptions.OnAutoLinking?.Invoke(autoLinkUser, loginInfo);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "Could not link login provider {LoginProvider}.", loginInfo.LoginProvider);
                        return AutoLinkSignInResult.FailedException(ex.Message);
                    }

                    var shouldLinkUser = autoLinkOptions.OnExternalLogin == null || autoLinkOptions.OnExternalLogin(autoLinkUser, loginInfo);
                    if (shouldLinkUser)
                    {
                        return await LinkUser(autoLinkUser, loginInfo);
                    }
                    else
                    {
                        LogFailedExternalLogin(loginInfo, autoLinkUser);
                        return ExternalLoginSignInResult.NotAllowed;
                    }
                }
                else
                {
                    var name = loginInfo.Principal?.Identity?.Name;
                    if (name.IsNullOrWhiteSpace()) throw new InvalidOperationException("The Name value cannot be null");

                    autoLinkUser = BackOfficeIdentityUser.CreateNew(_globalSettings, email, email, autoLinkOptions.GetUserAutoLinkCulture(_globalSettings), name);

                    foreach (var userGroup in autoLinkOptions.DefaultUserGroups)
                    {
                        autoLinkUser.AddRole(userGroup);
                    }

                    //call the callback if one is assigned
                    try
                    {
                        autoLinkOptions.OnAutoLinking?.Invoke(autoLinkUser, loginInfo);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "Could not link login provider {LoginProvider}.", loginInfo.LoginProvider);
                        return AutoLinkSignInResult.FailedException(ex.Message);
                    }

                    var userCreationResult = await _userManager.CreateAsync(autoLinkUser);

                    if (!userCreationResult.Succeeded)
                    {
                        return AutoLinkSignInResult.FailedCreatingUser(userCreationResult.Errors.Select(x => x.Description).ToList());
                    }
                    else
                    {
                        var shouldLinkUser = autoLinkOptions.OnExternalLogin == null || autoLinkOptions.OnExternalLogin(autoLinkUser, loginInfo);
                        if (shouldLinkUser)
                        {
                            return await LinkUser(autoLinkUser, loginInfo);
                        }
                        else
                        {
                            LogFailedExternalLogin(loginInfo, autoLinkUser);
                            return ExternalLoginSignInResult.NotAllowed;
                        }
                    }
                }
            }
        }

        private async Task<SignInResult> LinkUser(BackOfficeIdentityUser autoLinkUser, ExternalLoginInfo loginInfo)
        {
            var existingLogins = await _userManager.GetLoginsAsync(autoLinkUser);
            var exists = existingLogins.FirstOrDefault(x => x.LoginProvider == loginInfo.LoginProvider && x.ProviderKey == loginInfo.ProviderKey);

            // if it already exists (perhaps it was added in the AutoLink callbak) then we just continue
            if (exists != null)
            {
                //sign in
                return await SignInOrTwoFactorAsync(autoLinkUser, isPersistent: false, loginInfo.LoginProvider);
            }

            var linkResult = await _userManager.AddLoginAsync(autoLinkUser, loginInfo);
            if (linkResult.Succeeded)
            {
                //we're good! sign in
                return await SignInOrTwoFactorAsync(autoLinkUser, isPersistent: false, loginInfo.LoginProvider);
            }

            //If this fails, we should really delete the user since it will be in an inconsistent state!
            var deleteResult = await _userManager.DeleteAsync(autoLinkUser);
            if (deleteResult.Succeeded)
            {
                var errors = linkResult.Errors.Select(x => x.Description).ToList();
                return AutoLinkSignInResult.FailedLinkingUser(errors);
            }
            else
            {
                //DOH! ... this isn't good, combine all errors to be shown
                var errors = linkResult.Errors.Concat(deleteResult.Errors).Select(x => x.Description).ToList();
                return AutoLinkSignInResult.FailedLinkingUser(errors);
            }
        }

        protected override async Task<SignInResult> SignInOrTwoFactorAsync(BackOfficeIdentityUser user, bool isPersistent,
            string? loginProvider = null, bool bypassTwoFactor = false)
        {
            var result = await base.SignInOrTwoFactorAsync(user, isPersistent, loginProvider, bypassTwoFactor);

            if (result.RequiresTwoFactor)
            {
                NotifyRequiresTwoFactor(user);
            }

            return result;
        }

        protected void NotifyRequiresTwoFactor(BackOfficeIdentityUser user) => Notify(user,
            (currentUser) => new UserTwoFactorRequestedNotification(currentUser.Key)
        );

        private T Notify<T>(BackOfficeIdentityUser currentUser, Func<BackOfficeIdentityUser, T> createNotification) where T : INotification
        {

            var notification = createNotification(currentUser);
            _eventAggregator.Publish(notification);
            return notification;
        }

        private void LogFailedExternalLogin(ExternalLoginInfo loginInfo, BackOfficeIdentityUser user) =>
            Logger.LogWarning("The AutoLinkOptions of the external authentication provider '{LoginProvider}' have refused the login based on the OnExternalLogin method. Affected user id: '{UserId}'", loginInfo.LoginProvider, user.Id);
    }
}
