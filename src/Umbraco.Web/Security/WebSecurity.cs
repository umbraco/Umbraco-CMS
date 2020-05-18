using System;
using System.Security;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Services;
using Umbraco.Core.Models.Membership;
using Microsoft.Owin;
using Umbraco.Core.BackOffice;
using Umbraco.Core.Configuration;
using Umbraco.Core.Hosting;
using Umbraco.Core.Models;

namespace Umbraco.Web.Security
{

    /// <summary>
    /// A utility class used for dealing with USER security in Umbraco
    /// </summary>
    public class WebSecurity : IWebSecurity
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserService _userService;
        private readonly IGlobalSettings _globalSettings;
        private readonly IHostingEnvironment _hostingEnvironment;

        public WebSecurity(IHttpContextAccessor httpContextAccessor, IUserService userService, IGlobalSettings globalSettings, IHostingEnvironment hostingEnvironment)
        {
            _httpContextAccessor = httpContextAccessor;
            _userService = userService;
            _globalSettings = globalSettings;
            _hostingEnvironment = hostingEnvironment;
        }

        private IUser _currentUser;

        /// <summary>
        /// Gets the current user.
        /// </summary>
        /// <value>The current user.</value>
        public IUser CurrentUser
        {
            get
            {
                //only load it once per instance! (but make sure groups are loaded)
                if (_currentUser == null)
                {
                    var id = GetUserId();
                    _currentUser = id ? _userService.GetUserById(id.Result) : null;
                }

                return _currentUser;
            }
        }

        private BackOfficeSignInManager _signInManager;
        private BackOfficeSignInManager SignInManager
        {
            get
            {
                if (_signInManager == null)
                {
                    var mgr = _httpContextAccessor.GetRequiredHttpContext().GetOwinContext().Get<BackOfficeSignInManager>();
                    if (mgr == null)
                    {
                        throw new NullReferenceException("Could not resolve an instance of " + typeof(BackOfficeSignInManager) + " from the " + typeof(IOwinContext));
                    }
                    _signInManager = mgr;
                }
                return _signInManager;
            }
        }

        private BackOfficeUserManager<BackOfficeIdentityUser> _userManager;
        protected BackOfficeUserManager<BackOfficeIdentityUser> UserManager
            => _userManager ?? (_userManager = _httpContextAccessor.GetRequiredHttpContext().GetOwinContext().GetBackOfficeUserManager());

        [Obsolete("This needs to be removed, ASP.NET Identity should always be used for this operation, this is currently only used in the installer which needs to be updated")]
        public double PerformLogin(int userId)
        {
            var httpContext = _httpContextAccessor.GetRequiredHttpContext();
            var owinCtx = httpContext.GetOwinContext();
            //ensure it's done for owin too
            owinCtx.Authentication.SignOut(Constants.Security.BackOfficeExternalAuthenticationType);

            var user = UserManager.FindByIdAsync(userId.ToString()).Result;

            SignInManager.SignInAsync(user, isPersistent: true, rememberBrowser: false).Wait();

            httpContext.SetPrincipalForRequest(owinCtx.Request.User);

            return TimeSpan.FromMinutes(_globalSettings.TimeOutInMinutes).TotalSeconds;
        }

        [Obsolete("This needs to be removed, ASP.NET Identity should always be used for this operation, this is currently only used in the installer which needs to be updated")]
        public void ClearCurrentLogin()
        {
            var httpContext = _httpContextAccessor.GetRequiredHttpContext();
            httpContext.UmbracoLogout();
            httpContext.GetOwinContext().Authentication.SignOut(
                Core.Constants.Security.BackOfficeAuthenticationType,
                Core.Constants.Security.BackOfficeExternalAuthenticationType);
        }


        /// <summary>
        /// Gets the current user's id.
        /// </summary>
        /// <returns></returns>
        public Attempt<int> GetUserId()
        {
            var identity = _httpContextAccessor.GetRequiredHttpContext().GetCurrentIdentity(false);
            return identity == null ? Attempt.Fail<int>() : Attempt.Succeed(Convert.ToInt32(identity.Id));
        }

        /// <summary>
        /// Validates the currently logged in user and ensures they are not timed out
        /// </summary>
        /// <returns></returns>
        public bool ValidateCurrentUser()
        {
            return ValidateCurrentUser(false, true) == ValidateRequestAttempt.Success;
        }

        /// <summary>
        /// Validates the current user assigned to the request and ensures the stored user data is valid
        /// </summary>
        /// <param name="throwExceptions">set to true if you want exceptions to be thrown if failed</param>
        /// <param name="requiresApproval">If true requires that the user is approved to be validated</param>
        /// <returns></returns>
        public ValidateRequestAttempt ValidateCurrentUser(bool throwExceptions, bool requiresApproval = true)
        {
            //This will first check if the current user is already authenticated - which should be the case in nearly all circumstances
            // since the authentication happens in the Module, that authentication also checks the ticket expiry. We don't
            // need to check it a second time because that requires another decryption phase and nothing can tamper with it during the request.

            if (IsAuthenticated() == false)
            {
                //There is no user
                if (throwExceptions) throw new InvalidOperationException("The user has no umbraco contextid - try logging in");
                return ValidateRequestAttempt.FailedNoContextId;
            }

            var user = CurrentUser;

            // Check for console access
            if (user == null || (requiresApproval && user.IsApproved == false) || (user.IsLockedOut && RequestIsInUmbracoApplication(_httpContextAccessor, _globalSettings, _hostingEnvironment)))
            {
                if (throwExceptions) throw new ArgumentException("You have no privileges to the umbraco console. Please contact your administrator");
                return ValidateRequestAttempt.FailedNoPrivileges;
            }
            return ValidateRequestAttempt.Success;

        }

        private static bool RequestIsInUmbracoApplication(IHttpContextAccessor httpContextAccessor, IGlobalSettings globalSettings, IHostingEnvironment hostingEnvironment)
        {
            return httpContextAccessor.GetRequiredHttpContext().Request.Path.ToLower().IndexOf(hostingEnvironment.ToAbsolute(globalSettings.UmbracoPath).ToLower(), StringComparison.Ordinal) > -1;
        }

        /// <summary>
        /// Authorizes the full request, checks for SSL and validates the current user
        /// </summary>
        /// <param name="throwExceptions">set to true if you want exceptions to be thrown if failed</param>
        /// <returns></returns>
        public ValidateRequestAttempt AuthorizeRequest(bool throwExceptions = false)
        {
            // check for secure connection
            if (_globalSettings.UseHttps && _httpContextAccessor.GetRequiredHttpContext().Request.IsSecureConnection == false)
            {
                if (throwExceptions) throw new SecurityException("This installation requires a secure connection (via SSL). Please update the URL to include https://");
                return ValidateRequestAttempt.FailedNoSsl;
            }
            return ValidateCurrentUser(throwExceptions);
        }

        /// <summary>
        /// Checks if the specified user as access to the app
        /// </summary>
        /// <param name="section"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public bool UserHasSectionAccess(string section, IUser user)
        {
            return user.HasSectionAccess(section);
        }

        /// <summary>
        /// Ensures that a back office user is logged in
        /// </summary>
        /// <returns></returns>
        public bool IsAuthenticated()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            return httpContext?.User != null && httpContext.User.Identity.IsAuthenticated && httpContext.GetCurrentIdentity(false) != null;
        }

    }
}
