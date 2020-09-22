using System;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.Owin;
using Microsoft.Owin.Logging;
using Microsoft.Owin.Security;
using Umbraco.Core;
using Umbraco.Core.BackOffice;
using Umbraco.Core.Configuration.Models;

namespace Umbraco.Web.Security
{
    /// <summary>
    /// Custom sign in manager due to SignInManager not being .NET Standard.
    /// Code ported from Umbraco's BackOfficeSignInManager.
    /// Can be removed once the web project moves to .NET Core.
    /// </summary>
    public class BackOfficeSignInManager : IDisposable
    {
        private readonly IBackOfficeUserManager _userManager;
        private readonly IUserClaimsPrincipalFactory<BackOfficeIdentityUser> _claimsPrincipalFactory;
        private readonly IAuthenticationManager _authenticationManager;
        private readonly ILogger _logger;
        private readonly GlobalSettings _globalSettings;
        private readonly IOwinRequest _request;

        public BackOfficeSignInManager(
            IBackOfficeUserManager userManager,
            IUserClaimsPrincipalFactory<BackOfficeIdentityUser> claimsPrincipalFactory,
            IAuthenticationManager authenticationManager,
            ILogger logger,
            GlobalSettings globalSettings,
            IOwinRequest request)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _claimsPrincipalFactory = claimsPrincipalFactory ?? throw new ArgumentNullException(nameof(claimsPrincipalFactory));
            _authenticationManager = authenticationManager ?? throw new ArgumentNullException(nameof(authenticationManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _globalSettings = globalSettings ?? throw new ArgumentNullException(nameof(globalSettings));
            _request = request ?? throw new ArgumentNullException(nameof(request));
        }

        public async Task<ClaimsIdentity> CreateUserIdentityAsync(BackOfficeIdentityUser user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            var claimsPrincipal = await _claimsPrincipalFactory.CreateAsync(user);
            return claimsPrincipal.Identity as ClaimsIdentity;
        }

        public static BackOfficeSignInManager Create(IOwinContext context, GlobalSettings globalSettings, ILogger logger)
        {
            var userManager = context.GetBackOfficeUserManager();

            return new BackOfficeSignInManager(
                userManager,
                new BackOfficeClaimsPrincipalFactory<BackOfficeIdentityUser>(userManager, new OptionsWrapper<BackOfficeIdentityOptions>(userManager.Options)),
                context.Authentication,
                logger,
                globalSettings,
                context.Request);
        }

        /// <summary>
        /// Borrowed from Microsoft's underlying sign in manager which is not flexible enough to tell it to use a different cookie type
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="isPersistent"></param>
        /// <param name="shouldLockout"></param>
        /// <returns></returns>
        private async Task<SignInResult> PasswordSignInAsyncImpl(string userName, string password, bool isPersistent, bool shouldLockout)
        {
            var user = await _userManager.FindByNameAsync(userName);

            //if the user is null, create an empty one which can be used for auto-linking
            if (user == null) user = BackOfficeIdentityUser.CreateNew(_globalSettings, userName, null, _globalSettings.DefaultUILanguage);

            //check the password for the user, this will allow a developer to auto-link
            //an account if they have specified an IBackOfficeUserPasswordChecker
            if (await _userManager.CheckPasswordAsync(user, password))
            {
                //the underlying call to this will query the user by Id which IS cached!
                if (await _userManager.IsLockedOutAsync(user))
                {
                    return SignInResult.LockedOut;
                }

                // We need to verify that the user belongs to one or more groups that define content and media start nodes.
                // To do so we have to create the user claims identity and validate the calculated start nodes.
                var userIdentity = await CreateUserIdentityAsync(user);
                if (userIdentity is UmbracoBackOfficeIdentity backOfficeIdentity)
                {
                    if (backOfficeIdentity.StartContentNodes.Length == 0 || backOfficeIdentity.StartMediaNodes.Length == 0)
                    {
                        _logger.WriteCore(TraceEventType.Information, 0,
                            $"Login attempt failed for username {userName} from IP address {_request.RemoteIpAddress}, no content and/or media start nodes could be found for any of the user's groups", null, null);

                        // We will say its a sucessful login which it is, but they have no node access
                        return SignInResult.Success;
                    }
                }

                await _userManager.ResetAccessFailedCountAsync(user);
                return await SignInOrTwoFactor(user, isPersistent);
            }

            var requestContext = _request.Context;

            if (user.HasIdentity && shouldLockout)
            {
                // If lockout is requested, increment access failed count which might lock out the user
                await _userManager.AccessFailedAsync(user);
                if (await _userManager.IsLockedOutAsync(user))
                {
                    //at this point we've just locked the user out after too many failed login attempts

                    if (requestContext != null)
                    {
                        var backofficeUserManager = requestContext.GetBackOfficeUserManager();
                        if (backofficeUserManager != null) backofficeUserManager.RaiseAccountLockedEvent(_request.User, user.Id);
                    }

                    return SignInResult.LockedOut;
                }
            }

            if (requestContext != null)
            {
                var backofficeUserManager = requestContext.GetBackOfficeUserManager();
                if (backofficeUserManager != null)
                    backofficeUserManager.RaiseInvalidLoginAttemptEvent(_request.User, userName);
            }

            return SignInResult.Failed;
        }

        /// <summary>
        /// Borrowed from Microsoft's underlying sign in manager which is not flexible enough to tell it to use a different cookie type
        /// </summary>
        /// <param name="user"></param>
        /// <param name="isPersistent"></param>
        /// <returns></returns>
        private async Task<SignInResult> SignInOrTwoFactor(BackOfficeIdentityUser user, bool isPersistent)
        {
            var id = Convert.ToString(user.Id);
            if (await _userManager.GetTwoFactorEnabledAsync(user)
                && (await _userManager.GetValidTwoFactorProvidersAsync(user)).Count > 0)
            {
                var identity = new ClaimsIdentity(Constants.Security.BackOfficeTwoFactorAuthenticationType);
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, id));
                identity.AddClaim(new Claim(ClaimsIdentity.DefaultNameClaimType, user.UserName));
                _authenticationManager.SignIn(identity);
                return SignInResult.TwoFactorRequired;
            }
            await SignInAsync(user, isPersistent, false);
            return SignInResult.Success;
        }

        /// <summary>
        /// Creates a user identity and then signs the identity using the AuthenticationManager
        /// </summary>
        /// <param name="user"></param>
        /// <param name="isPersistent"></param>
        /// <param name="rememberBrowser"></param>
        /// <returns></returns>
        public async Task SignInAsync(BackOfficeIdentityUser user, bool isPersistent, bool rememberBrowser)
        {
            var userIdentity = await CreateUserIdentityAsync(user);

            // Clear any partial cookies from external or two factor partial sign ins
            _authenticationManager.SignOut(
                Constants.Security.BackOfficeExternalAuthenticationType,
                Constants.Security.BackOfficeTwoFactorAuthenticationType);

            var nowUtc = DateTime.Now.ToUniversalTime();

            if (rememberBrowser)
            {
                var rememberBrowserIdentity = _authenticationManager.CreateTwoFactorRememberBrowserIdentity(user.Id.ToString());
                _authenticationManager.SignIn(new AuthenticationProperties()
                {
                    IsPersistent = isPersistent,
                    AllowRefresh = true,
                    IssuedUtc = nowUtc,
                    ExpiresUtc = nowUtc.AddMinutes(_globalSettings.TimeOutInMinutes)
                }, userIdentity, rememberBrowserIdentity);
            }
            else
            {
                _authenticationManager.SignIn(new AuthenticationProperties()
                {
                    IsPersistent = isPersistent,
                    AllowRefresh = true,
                    IssuedUtc = nowUtc,
                    ExpiresUtc = nowUtc.AddMinutes(_globalSettings.TimeOutInMinutes)
                }, userIdentity);
            }

            //track the last login date
            user.LastLoginDateUtc = DateTime.UtcNow;
            if (user.AccessFailedCount > 0)
                //we have successfully logged in, reset the AccessFailedCount
                user.AccessFailedCount = 0;
            await _userManager.UpdateAsync(user);

            //set the current request's principal to the identity just signed in!
            _request.User = new ClaimsPrincipal(userIdentity);

            _logger.WriteCore(TraceEventType.Information, 0,
                string.Format(
                    "Login attempt succeeded for username {0} from IP address {1}",
                    user.UserName,
                    _request.RemoteIpAddress), null, null);
        }

        /// <summary>
        /// Get the user id that has been verified already or int.MinValue if the user has not been verified yet
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Replaces the underlying call which is not flexible and doesn't support a custom cookie
        /// </remarks>
        public async Task<string> GetVerifiedUserIdAsync()
        {
            var result = await _authenticationManager.AuthenticateAsync(Constants.Security.BackOfficeTwoFactorAuthenticationType);
            if (result != null && result.Identity != null && string.IsNullOrEmpty(result.Identity.GetUserId()) == false)
            {
                return result.Identity.GetUserId();
            }
            return null;
        }

        /// <summary>
        /// Get the username that has been verified already or null.
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetVerifiedUserNameAsync()
        {
            var result = await _authenticationManager.AuthenticateAsync(Constants.Security.BackOfficeTwoFactorAuthenticationType);
            if (result != null && result.Identity != null && string.IsNullOrEmpty(result.Identity.GetUserName()) == false)
            {
                return result.Identity.GetUserName();
            }
            return null;
        }

        /// <summary>
        /// Two factor verification step
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="code"></param>
        /// <param name="isPersistent"></param>
        /// <param name="rememberBrowser"></param>
        /// <returns></returns>
        /// <remarks>
        /// This is implemented because we cannot override GetVerifiedUserIdAsync and instead we have to shadow it
        /// so due to this and because we are using an INT as the TKey and not an object, it can never be null. Adding to that
        /// the default(int) value returned by the base class is always a valid user (i.e. the admin) so we just have to duplicate
        /// all of this code to check for int.MinValue
        /// </remarks>
        public async Task<SignInResult> TwoFactorSignInAsync(string provider, string code, bool isPersistent, bool rememberBrowser)
        {
            var userId = await GetVerifiedUserIdAsync();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return SignInResult.Failed;
            }
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return SignInResult.Failed;
            }
            if (await _userManager.IsLockedOutAsync(user))
            {
                return SignInResult.LockedOut;
            }
            if (await _userManager.VerifyTwoFactorTokenAsync(user, provider, code))
            {
                // When token is verified correctly, clear the access failed count used for lockout
                await _userManager.ResetAccessFailedCountAsync(user);
                await SignInAsync(user, isPersistent, rememberBrowser);
                return SignInResult.Success;
            }

            // If the token is incorrect, record the failure which also may cause the user to be locked out
            await _userManager.AccessFailedAsync(user);
            return SignInResult.Failed;
        }

        /// <summary>Send a two factor code to a user</summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        /// <remarks>
        /// This is implemented because we cannot override GetVerifiedUserIdAsync and instead we have to shadow it
        /// so due to this and because we are using an INT as the TKey and not an object, it can never be null. Adding to that
        /// the default(int) value returned by the base class is always a valid user (i.e. the admin) so we just have to duplicate
        /// all of this code to check for int.MinVale instead.
        /// </remarks>
        public Task<bool> SendTwoFactorCodeAsync(string provider)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _userManager?.Dispose();
        }
    }
}
