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

        public BackOfficeSignInManager(BackOfficeUserManager userManager, IAuthenticationManager authenticationManager, ILogger logger, IOwinRequest request)
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
            return user.GenerateUserIdentityAsync((BackOfficeUserManager)UserManager);
        }

        public static BackOfficeSignInManager Create(IdentityFactoryOptions<BackOfficeSignInManager> options, IOwinContext context, ILogger logger)
        {
            return new BackOfficeSignInManager(
                context.GetUserManager<BackOfficeUserManager>(), 
                context.Authentication,
                logger,
                context.Request);
        }

        /// <summary>
        /// Sign in the user in using the user name and password
        /// </summary>
        /// <param name="userName"/><param name="password"/><param name="isPersistent"/><param name="shouldLockout"/>
        /// <returns/>
        public async override Task<SignInStatus> PasswordSignInAsync(string userName, string password, bool isPersistent, bool shouldLockout)
        {
            var result = await base.PasswordSignInAsync(userName, password, isPersistent, shouldLockout);

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
                            "Login attempt failed for username {0} from IP address {1}, the user requires verification",
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

            _logger.WriteCore(TraceEventType.Information, 0,
                string.Format(
                    "Login attempt succeeded for username {0} from IP address {1}",
                    user.UserName,
                    _request.RemoteIpAddress), null, null);
        }
    }
}