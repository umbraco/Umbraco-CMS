using System;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Logging;
using Microsoft.Owin.Security;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models.Identity;

namespace Umbraco.Core.Security
{
    public class BackOfficeSignInManager : SignInManager<BackOfficeIdentityUser, int>
    {
        private readonly ILogger _logger;
        private readonly IOwinRequest _request;

        public BackOfficeSignInManager(UserManager<BackOfficeIdentityUser, int> userManager, IAuthenticationManager authenticationManager, ILogger logger, IOwinRequest request)
            : base(userManager, authenticationManager)
        {
            if (logger == null) throw new ArgumentNullException("logger");
            if (request == null) throw new ArgumentNullException("request");
            _logger = logger;
            _request = request;
            AuthenticationType = Constants.Security.BackOfficeAuthenticationType;
        }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(BackOfficeIdentityUser user)
        {
            return user.GenerateUserIdentityAsync((BackOfficeUserManager<BackOfficeIdentityUser>)UserManager);
        }

        public static BackOfficeSignInManager Create(IdentityFactoryOptions<BackOfficeSignInManager> options, IOwinContext context, ILogger logger)
        {
            return new BackOfficeSignInManager(
                context.GetBackOfficeUserManager(),
                context.Authentication,
                logger,
                context.Request);
        }

        /// <summary>
        /// Sign in the user in using the user name and password
        /// </summary>
        /// <param name="userName"/><param name="password"/><param name="isPersistent"/><param name="shouldLockout"/>
        /// <returns/>
        public override async Task<SignInStatus> PasswordSignInAsync(string userName, string password, bool isPersistent, bool shouldLockout)
        {
            var result = await PasswordSignInAsyncImpl(userName, password, isPersistent, shouldLockout);
            
            switch (result)
            {
                case SignInStatus.Success:
                    _logger.WriteCore(TraceEventType.Information, 0,
                        string.Format(
                            "User: {0} logged in from IP address {1}",
                            userName,
                            _request.RemoteIpAddress), null, null);
                    break;
                case SignInStatus.LockedOut:
                    _logger.WriteCore(TraceEventType.Information, 0,
                        string.Format(
                            "Login attempt failed for username {0} from IP address {1}, the user is locked",
                            userName,
                            _request.RemoteIpAddress), null, null);
                    break;
                case SignInStatus.RequiresVerification:
                    _logger.WriteCore(TraceEventType.Information, 0,
                        string.Format(
                            "Login attempt requires verification for username {0} from IP address {1}",
                            userName,
                            _request.RemoteIpAddress), null, null);
                    break;
                case SignInStatus.Failure:
                    _logger.WriteCore(TraceEventType.Information, 0,
                        string.Format(
                            "Login attempt failed for username {0} from IP address {1}",
                            userName,
                            _request.RemoteIpAddress), null, null);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return result;
        }

        /// <summary>
        /// Borrowed from Micorosoft's underlying sign in manager which is not flexible enough to tell it to use a different cookie type
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="isPersistent"></param>
        /// <param name="shouldLockout"></param>
        /// <returns></returns>
        private async Task<SignInStatus> PasswordSignInAsyncImpl(string userName, string password, bool isPersistent, bool shouldLockout)
        {
            if (UserManager == null)
            {
                return SignInStatus.Failure;
            }
            var user = await UserManager.FindByNameAsync(userName);
            if (user == null)
            {
                return SignInStatus.Failure;
            }
            if (await UserManager.IsLockedOutAsync(user.Id))
            {
                return SignInStatus.LockedOut;
            }
            if (await UserManager.CheckPasswordAsync(user, password))
            {
                await UserManager.ResetAccessFailedCountAsync(user.Id);
                return await SignInOrTwoFactor(user, isPersistent);
            }
            if (shouldLockout)
            {
                // If lockout is requested, increment access failed count which might lock out the user
                await UserManager.AccessFailedAsync(user.Id);
                if (await UserManager.IsLockedOutAsync(user.Id))
                {
                    return SignInStatus.LockedOut;
                }
            }
            return SignInStatus.Failure;
        }

        /// <summary>
        /// Borrowed from Micorosoft's underlying sign in manager which is not flexible enough to tell it to use a different cookie type
        /// </summary>
        /// <param name="user"></param>
        /// <param name="isPersistent"></param>
        /// <returns></returns>
        private async Task<SignInStatus> SignInOrTwoFactor(BackOfficeIdentityUser user, bool isPersistent)
        {
            var id = Convert.ToString(user.Id);
            if (await UserManager.GetTwoFactorEnabledAsync(user.Id)
                && (await UserManager.GetValidTwoFactorProvidersAsync(user.Id)).Count > 0)
            {
                var identity = new ClaimsIdentity(Constants.Security.BackOfficeTwoFactorAuthenticationType);
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, id));
                identity.AddClaim(new Claim(ClaimsIdentity.DefaultNameClaimType, user.UserName));
                AuthenticationManager.SignIn(identity);
                return SignInStatus.RequiresVerification;
            }
            await SignInAsync(user, isPersistent, false);
            return SignInStatus.Success;
        }

        /// <summary>
        /// Creates a user identity and then signs the identity using the AuthenticationManager
        /// </summary>
        /// <param name="user"></param>
        /// <param name="isPersistent"></param>
        /// <param name="rememberBrowser"></param>
        /// <returns></returns>
        public override async Task SignInAsync(BackOfficeIdentityUser user, bool isPersistent, bool rememberBrowser)
        {
            var userIdentity = await CreateUserIdentityAsync(user);

            // Clear any partial cookies from external or two factor partial sign ins
            AuthenticationManager.SignOut(
                Constants.Security.BackOfficeExternalAuthenticationType,
                Constants.Security.BackOfficeTwoFactorAuthenticationType);

            var nowUtc = DateTime.Now.ToUniversalTime();

            if (rememberBrowser)
            {
                var rememberBrowserIdentity = AuthenticationManager.CreateTwoFactorRememberBrowserIdentity(ConvertIdToString(user.Id));
                AuthenticationManager.SignIn(new AuthenticationProperties()
                {
                    IsPersistent = isPersistent,
                    AllowRefresh = true,
                    IssuedUtc = nowUtc,
                    ExpiresUtc = nowUtc.AddMinutes(GlobalSettings.TimeOutInMinutes)
                }, userIdentity, rememberBrowserIdentity);                
            }
            else
            {
                AuthenticationManager.SignIn(new AuthenticationProperties()
                {
                    IsPersistent = isPersistent,
                    AllowRefresh = true,
                    IssuedUtc = nowUtc,
                    ExpiresUtc = nowUtc.AddMinutes(GlobalSettings.TimeOutInMinutes)
                }, userIdentity);
            }

            //track the last login date
            user.LastLoginDateUtc = DateTime.UtcNow;
            user.AccessFailedCount = 0;
            await UserManager.UpdateAsync(user);

            _logger.WriteCore(TraceEventType.Information, 0,
                string.Format(
                    "Login attempt succeeded for username {0} from IP address {1}",
                    user.UserName,
                    _request.RemoteIpAddress), null, null);
        }

        /// <summary>
        /// Get the user id that has been verified already or -1.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Replaces the underlying call which is not flexible and doesn't support a custom cookie
        /// </remarks>
        public new async Task<int> GetVerifiedUserIdAsync()
        {
            var result = await AuthenticationManager.AuthenticateAsync(Constants.Security.BackOfficeTwoFactorAuthenticationType);
            if (result != null && result.Identity != null && string.IsNullOrEmpty(result.Identity.GetUserId()) == false)
            {
                return ConvertIdFromString(result.Identity.GetUserId());
            }
            return -1;
        }

        /// <summary>
        /// Get the username that has been verified already or null.
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetVerifiedUserNameAsync()
        {
            var result = await AuthenticationManager.AuthenticateAsync(Constants.Security.BackOfficeTwoFactorAuthenticationType);
            if (result != null && result.Identity != null && string.IsNullOrEmpty(result.Identity.GetUserName()) == false)
            {
                return result.Identity.GetUserName();
            }
            return null;
        }
    }
}
