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


        public void Dispose()
        {
            _userManager?.Dispose();
        }
    }
}
