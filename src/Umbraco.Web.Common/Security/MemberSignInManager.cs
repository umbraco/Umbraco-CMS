using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
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

namespace Umbraco.Cms.Web.Common.Security
{

    /// <summary>
    /// The sign in manager for members
    /// </summary>
    public class MemberSignInManager : UmbracoSignInManager<MemberIdentityUser>, IMemberSignInManagerExternalLogins
    {
        private readonly IMemberExternalLoginProviders _memberExternalLoginProviders;
        private readonly IEventAggregator _eventAggregator;

        public MemberSignInManager(
            UserManager<MemberIdentityUser> memberManager,
            IHttpContextAccessor contextAccessor,
            IUserClaimsPrincipalFactory<MemberIdentityUser> claimsFactory,
            IOptions<IdentityOptions> optionsAccessor,
            ILogger<SignInManager<MemberIdentityUser>> logger,
            IAuthenticationSchemeProvider schemes,
            IUserConfirmation<MemberIdentityUser> confirmation,
            IMemberExternalLoginProviders memberExternalLoginProviders,
            IEventAggregator eventAggregator) :
            base(memberManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
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
            IUserConfirmation<MemberIdentityUser> confirmation) :
            this(memberManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation,
                StaticServiceProvider.Instance.GetRequiredService<IMemberExternalLoginProviders>(),
                StaticServiceProvider.Instance.GetRequiredService<IEventAggregator>())
        { }

        // use default scheme for members
        protected override string AuthenticationType => IdentityConstants.ApplicationScheme;

        // use default scheme for members
        protected override string ExternalAuthenticationType => IdentityConstants.ExternalScheme;

        // use default scheme for members
        protected override string TwoFactorAuthenticationType => IdentityConstants.TwoFactorUserIdScheme;

        // use default scheme for members
        protected override string TwoFactorRememberMeAuthenticationType => IdentityConstants.TwoFactorRememberMeScheme;

        /// <inheritdoc />
        public override async Task<ExternalLoginInfo> GetExternalLoginInfoAsync(string expectedXsrf = null)
        {
            // borrowed from https://github.com/dotnet/aspnetcore/blob/master/src/Identity/Core/src/SignInManager.cs#L422
            // to replace the auth scheme

            var auth = await Context.AuthenticateAsync(ExternalAuthenticationType);
            var items = auth?.Properties?.Items;
            if (auth?.Principal == null || items == null)
            {
                Logger.LogDebug(auth?.Failure ?? new NullReferenceException("Context.AuthenticateAsync(ExternalAuthenticationType) is null"),
                    "The external login authentication failed. No user Principal or authentication items was resolved.");
                return null;
            }

            if (!items.ContainsKey(UmbracoSignInMgrLoginProviderKey))
            {
                throw new InvalidOperationException($"The external login authenticated successfully but the key {UmbracoSignInMgrLoginProviderKey} was not found in the authentication properties. Ensure you call SignInManager.ConfigureExternalAuthenticationProperties before issuing a ChallengeResult.");
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
            if (providerKey == null || items[UmbracoSignInMgrLoginProviderKey] is not string provider)
            {
                return null;
            }

            var providerDisplayName = (await GetExternalAuthenticationSchemesAsync()).FirstOrDefault(p => p.Name == provider)?.DisplayName ?? provider;
            return new ExternalLoginInfo(auth.Principal, provider, providerKey, providerDisplayName)
            {
                AuthenticationTokens = auth.Properties.GetTokens(),
                AuthenticationProperties = auth.Properties
            };
        }

        /// <summary>
        /// Custom ExternalLoginSignInAsync overload for handling external sign in with auto-linking
        /// </summary>
        public async Task<SignInResult> ExternalLoginSignInAsync(ExternalLoginInfo loginInfo, bool isPersistent, bool bypassTwoFactor = false)
        {
            // borrowed from https://github.com/dotnet/aspnetcore/blob/master/src/Identity/Core/src/SignInManager.cs
            // to be able to deal with auto-linking and reduce duplicate lookups

            var autoLinkOptions = (await _memberExternalLoginProviders.GetAsync(loginInfo.LoginProvider))?.ExternalLoginProvider?.Options?.AutoLinkOptions;
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
        /// Used for auto linking/creating user accounts for external logins
        /// </summary>
        /// <param name="loginInfo"></param>
        /// <param name="autoLinkOptions"></param>
        /// <returns></returns>
        private async Task<SignInResult> AutoLinkAndSignInExternalAccount(ExternalLoginInfo loginInfo, MemberExternalSignInAutoLinkOptions autoLinkOptions)
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

                    autoLinkUser = MemberIdentityUser.CreateNew(email, email, autoLinkOptions.DefaultMemberTypeAlias, autoLinkOptions.DefaultIsApproved, name);

                    foreach (var userGroup in autoLinkOptions.DefaultMemberGroups)
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

                    var userCreationResult = await UserManager.CreateAsync(autoLinkUser);

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

        // TODO in v10 we can share this with backoffice by moving the backoffice into common.
        public class ExternalLoginSignInResult : SignInResult
        {
            public static ExternalLoginSignInResult NotAllowed { get; } = new ExternalLoginSignInResult()
            {
                Succeeded = false
            };
        }
        // TODO in v10 we can share this with backoffice by moving the backoffice into common.
        public class AutoLinkSignInResult : SignInResult
        {
            public static AutoLinkSignInResult FailedNotLinked { get; } = new AutoLinkSignInResult()
            {
                Succeeded = false
            };

            public static AutoLinkSignInResult FailedNoEmail { get; } = new AutoLinkSignInResult()
            {
                Succeeded = false
            };

            public static AutoLinkSignInResult FailedException(string error) => new AutoLinkSignInResult(new[] { error })
            {
                Succeeded = false
            };

            public static AutoLinkSignInResult FailedCreatingUser(IReadOnlyCollection<string> errors) => new AutoLinkSignInResult(errors)
            {
                Succeeded = false
            };

            public static AutoLinkSignInResult FailedLinkingUser(IReadOnlyCollection<string> errors) => new AutoLinkSignInResult(errors)
            {
                Succeeded = false
            };

            public AutoLinkSignInResult(IReadOnlyCollection<string> errors)
            {
                Errors = errors ?? throw new ArgumentNullException(nameof(errors));
            }

            public AutoLinkSignInResult()
            {
            }

            public IReadOnlyCollection<string> Errors { get; } = Array.Empty<string>();
        }

        /// <inheritdoc />
        public override AuthenticationProperties ConfigureExternalAuthenticationProperties(string provider, string redirectUrl, string userId = null)
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

        /// <inheritdoc />
        public override Task<IEnumerable<AuthenticationScheme>> GetExternalAuthenticationSchemesAsync()
        {
            // That can be done by either checking the scheme (maybe) or comparing it to what we have registered in the collection of BackOfficeExternalLoginProvider
            return base.GetExternalAuthenticationSchemesAsync();
        }

        private async Task<SignInResult> LinkUser(MemberIdentityUser autoLinkUser, ExternalLoginInfo loginInfo)
        {
            var existingLogins = await UserManager.GetLoginsAsync(autoLinkUser);
            var exists = existingLogins.FirstOrDefault(x => x.LoginProvider == loginInfo.LoginProvider && x.ProviderKey == loginInfo.ProviderKey);

            // if it already exists (perhaps it was added in the AutoLink callbak) then we just continue
            if (exists != null)
            {
                //sign in
                return await SignInOrTwoFactorAsync(autoLinkUser, isPersistent: false, loginInfo.LoginProvider);
            }

            var linkResult = await UserManager.AddLoginAsync(autoLinkUser, loginInfo);
            if (linkResult.Succeeded)
            {
                //we're good! sign in
                return await SignInOrTwoFactorAsync(autoLinkUser, isPersistent: false, loginInfo.LoginProvider);
            }

            //If this fails, we should really delete the user since it will be in an inconsistent state!
            var deleteResult = await UserManager.DeleteAsync(autoLinkUser);
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

        private void LogFailedExternalLogin(ExternalLoginInfo loginInfo, MemberIdentityUser user) =>
            Logger.LogWarning("The AutoLinkOptions of the external authentication provider '{LoginProvider}' have refused the login based on the OnExternalLogin method. Affected user id: '{UserId}'", loginInfo.LoginProvider, user.Id);

        protected override async Task<SignInResult> SignInOrTwoFactorAsync(MemberIdentityUser user, bool isPersistent,
            string loginProvider = null, bool bypassTwoFactor = false)
        {
            var result = await base.SignInOrTwoFactorAsync(user, isPersistent, loginProvider, bypassTwoFactor);

            if (result.RequiresTwoFactor)
            {
                NotifyRequiresTwoFactor(user);
            }

            return result;
        }

        protected void NotifyRequiresTwoFactor(MemberIdentityUser user) => Notify(user,
            (currentUser) => new MemberTwoFactorRequestedNotification(currentUser.Key)
        );

        private T Notify<T>(MemberIdentityUser currentUser, Func<MemberIdentityUser, T> createNotification) where T : INotification
        {

            var notification = createNotification(currentUser);
            _eventAggregator.Publish(notification);
            return notification;
        }
    }
}
