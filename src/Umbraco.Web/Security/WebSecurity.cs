using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Services;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Security;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Umbraco.Core.Configuration;
using Umbraco.Core.Composing;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Identity;
using Current = Umbraco.Web.Composing.Current;

namespace Umbraco.Web.Security
{
    /// <summary>
    /// A utility class used for dealing with USER security in Umbraco
    /// </summary>
    public class WebSecurity
    {
        private readonly HttpContextBase _httpContext;
        private readonly IUserService _userService;
        private readonly IGlobalSettings _globalSettings;

        public WebSecurity(HttpContextBase httpContext, IUserService userService, IGlobalSettings globalSettings)
        {
            _httpContext = httpContext;
            _userService = userService;
            _globalSettings = globalSettings;
        }
        
        private IUser _currentUser;

        /// <summary>
        /// Gets the current user.
        /// </summary>
        /// <value>The current user.</value>
        public virtual IUser CurrentUser
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
                    var mgr = _httpContext.GetOwinContext().Get<BackOfficeSignInManager>();
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
            => _userManager ?? (_userManager = _httpContext.GetOwinContext().GetBackOfficeUserManager());

        /// <summary>
        /// Logs a user in.
        /// </summary>
        /// <param name="userId">The user Id</param>
        /// <returns>returns the number of seconds until their session times out</returns>
        public virtual double PerformLogin(int userId)
        {
            var owinCtx = _httpContext.GetOwinContext();
            //ensure it's done for owin too
            owinCtx.Authentication.SignOut(Constants.Security.BackOfficeExternalAuthenticationType);

            var user = UserManager.FindByIdAsync(userId).Result;

            SignInManager.SignInAsync(user, isPersistent: true, rememberBrowser: false).Wait();
            
            _httpContext.SetPrincipalForRequest(owinCtx.Request.User);
            
            return TimeSpan.FromMinutes(_globalSettings.TimeOutInMinutes).TotalSeconds;
        }
        
        /// <summary>
        /// Clears the current login for the currently logged in user
        /// </summary>
        public virtual void ClearCurrentLogin()
        {
            _httpContext.UmbracoLogout();
            _httpContext.GetOwinContext().Authentication.SignOut(
                Core.Constants.Security.BackOfficeAuthenticationType,
                Core.Constants.Security.BackOfficeExternalAuthenticationType);
        }

        /// <summary>
        /// Renews the user's login ticket
        /// </summary>
        public virtual void RenewLoginTimeout()
        {
            _httpContext.RenewUmbracoAuthTicket();
        }

        /// <summary>
        /// Validates credentials for a back office user
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        /// <remarks>
        /// This uses ASP.NET Identity to perform the validation
        /// </remarks>
        public virtual bool ValidateBackOfficeCredentials(string username, string password)
        {
            //find the user by username
            var user = UserManager.FindByNameAsync(username).Result;
            return user != null && UserManager.CheckPasswordAsync(user, password).Result;
        }
        
        /// <summary>
        /// Validates the current user to see if they have access to the specified app
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        internal bool ValidateUserApp(string app)
        {
            //if it is empty, don't validate
            if (app.IsNullOrWhiteSpace())
            {
                return true;
            }
            return CurrentUser.AllowedSections.Any(uApp => uApp.InvariantEquals(app));
        }

        /// <summary>
        /// Gets the current user's id.
        /// </summary>
        /// <returns></returns>
        public virtual Attempt<int> GetUserId()
        {
            var identity = _httpContext.GetCurrentIdentity(false);
            return identity == null ? Attempt.Fail<int>() : Attempt.Succeed(Convert.ToInt32(identity.Id));
        }

        /// <summary>
        /// Returns the current user's unique session id - used to mitigate csrf attacks or any other reason to validate a request
        /// </summary>
        /// <returns></returns>
        public virtual string GetSessionId()
        {
            var identity = _httpContext.GetCurrentIdentity(false);
            return identity?.SessionId;
        }
        
        /// <summary>
        /// Validates the currently logged in user and ensures they are not timed out
        /// </summary>
        /// <returns></returns>
        public virtual bool ValidateCurrentUser()
        {
            return ValidateCurrentUser(false, true) == ValidateRequestAttempt.Success;
        }

        /// <summary>
        /// Validates the current user assigned to the request and ensures the stored user data is valid
        /// </summary>
        /// <param name="throwExceptions">set to true if you want exceptions to be thrown if failed</param>
        /// <param name="requiresApproval">If true requires that the user is approved to be validated</param>
        /// <returns></returns>
        public virtual ValidateRequestAttempt ValidateCurrentUser(bool throwExceptions, bool requiresApproval = true)
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
            if (user == null || (requiresApproval && user.IsApproved == false) || (user.IsLockedOut && RequestIsInUmbracoApplication(_httpContext)))
            {
                if (throwExceptions) throw new ArgumentException("You have no privileges to the umbraco console. Please contact your administrator");
                return ValidateRequestAttempt.FailedNoPrivileges;
            }
            return ValidateRequestAttempt.Success;

        }

        private static bool RequestIsInUmbracoApplication(HttpContextBase context)
        {
            return context.Request.Path.ToLower().IndexOf(IOHelper.ResolveUrl(SystemDirectories.Umbraco).ToLower(), StringComparison.Ordinal) > -1;
        }

        /// <summary>
        /// Authorizes the full request, checks for SSL and validates the current user
        /// </summary>
        /// <param name="throwExceptions">set to true if you want exceptions to be thrown if failed</param>
        /// <returns></returns>
        internal ValidateRequestAttempt AuthorizeRequest(bool throwExceptions = false)
        {
            // check for secure connection
            if (_globalSettings.UseHttps && _httpContext.Request.IsSecureConnection == false)
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
        internal virtual bool UserHasSectionAccess(string section, IUser user)
        {
            return user.HasSectionAccess(section);
        }

        /// <summary>
        /// Checks if the specified user by username as access to the app
        /// </summary>
        /// <param name="section"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        internal bool UserHasSectionAccess(string section, string username)
        {
            var user = _userService.GetByUsername(username);
            if (user == null)
            {
                return false;
            }
            return user.HasSectionAccess(section);
        }

        /// <summary>
        /// Ensures that a back office user is logged in
        /// </summary>
        /// <returns></returns>
        public bool IsAuthenticated()
        {
            return _httpContext.User != null && _httpContext.User.Identity.IsAuthenticated && _httpContext.GetCurrentIdentity(false) != null;
        }
        
    }
}
