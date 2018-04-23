using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Security;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Security;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using umbraco.businesslogic.Exceptions;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Identity;
using Umbraco.Web.Models.ContentEditing;
using GlobalSettings = Umbraco.Core.Configuration.GlobalSettings;
using User = umbraco.BusinessLogic.User;

namespace Umbraco.Web.Security
{
    /// <summary>
    /// A utility class used for dealing with USER security in Umbraco
    /// </summary>
    public class WebSecurity : DisposableObjectSlim
    {
        private HttpContextBase _httpContext;
        private ApplicationContext _applicationContext;

        public WebSecurity(HttpContextBase httpContext, ApplicationContext applicationContext)
        {
            _httpContext = httpContext;
            _applicationContext = applicationContext;            
        }
        
        /// <summary>
        /// Returns true or false if the currently logged in member is authorized based on the parameters provided
        /// </summary>
        /// <param name="allowAll"></param>
        /// <param name="allowTypes"></param>
        /// <param name="allowGroups"></param>
        /// <param name="allowMembers"></param>
        /// <returns></returns>
        [Obsolete("Use MembershipHelper.IsMemberAuthorized instead")]
        public bool IsMemberAuthorized(
            bool allowAll = false,
            IEnumerable<string> allowTypes = null,
            IEnumerable<string> allowGroups = null,
            IEnumerable<int> allowMembers = null)
        {
            if (UmbracoContext.Current == null)
            {
                return false;
            }
            var helper = new MembershipHelper(UmbracoContext.Current);
            return helper.IsMemberAuthorized(allowAll, allowTypes, allowGroups, allowMembers);
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
                    if (id == -1)
                    {
                        return null;
                    }
                    _currentUser = _applicationContext.Services.UserService.GetUserById(id);
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
        {
            get { return _userManager ?? (_userManager = _httpContext.GetOwinContext().GetBackOfficeUserManager()); }
        }        

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
            
            return TimeSpan.FromMinutes(GlobalSettings.TimeOutInMinutes).TotalSeconds;
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
        
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Back office users shouldn't be resolved from the membership provider, they should be resolved usign the BackOfficeUserManager or the IUserService")]
        public virtual MembershipUser GetBackOfficeMembershipUser(string username, bool setOnline)
        {
            var membershipProvider = Core.Security.MembershipProviderExtensions.GetUsersMembershipProvider();
            return membershipProvider != null ? membershipProvider.GetUser(username, setOnline) : null;
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
        /// Gets the user id.
        /// </summary>
        /// <param name="umbracoUserContextId">This is not used</param>
        /// <returns></returns>
        [Obsolete("This method is no longer used, use the GetUserId() method without parameters instead")]
        public int GetUserId(string umbracoUserContextId)
        {           
            return GetUserId();
        }

        /// <summary>
        /// Gets the currnet user's id.
        /// </summary>
        /// <returns></returns>
        public virtual int GetUserId()
        {
            var identity = _httpContext.GetCurrentIdentity(false);
            if (identity == null)
                return -1;
            return Convert.ToInt32(identity.Id);
        }

        /// <summary>
        /// Returns the current user's unique session id - used to mitigate csrf attacks or any other reason to validate a request
        /// </summary>
        /// <returns></returns>
        public virtual string GetSessionId()
        {
            var identity = _httpContext.GetCurrentIdentity(false);
            if (identity == null)
                return null;
            return identity.SessionId;
        }

        /// <summary>
        /// Validates the user context ID.
        /// </summary>
        /// <param name="currentUmbracoUserContextId">This doesn't do anything</param>
        /// <returns></returns>
        [Obsolete("This method is no longer used, use the ValidateCurrentUser() method instead")]
        public bool ValidateUserContextId(string currentUmbracoUserContextId)
        {
            return ValidateCurrentUser();
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
            if (user == null || (requiresApproval && user.IsApproved == false) || (user.IsLockedOut && GlobalSettings.RequestIsInUmbracoApplication(_httpContext)))
            {
                if (throwExceptions) throw new ArgumentException("You have no priviledges to the umbraco console. Please contact your administrator");
                return ValidateRequestAttempt.FailedNoPrivileges;
            }
            return ValidateRequestAttempt.Success;

        }

        /// <summary>
        /// Authorizes the full request, checks for SSL and validates the current user
        /// </summary>
        /// <param name="throwExceptions">set to true if you want exceptions to be thrown if failed</param>
        /// <returns></returns>
        internal ValidateRequestAttempt AuthorizeRequest(bool throwExceptions = false)
        {
            // check for secure connection
            if (GlobalSettings.UseSSL && _httpContext.Request.IsSecureConnection == false)
            {
                if (throwExceptions) throw new UserAuthorizationException("This installation requires a secure connection (via SSL). Please update the URL to include https://");
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

        [Obsolete("Do not use this method if you don't have to, use the overload with IUser instead")]
        internal bool UserHasSectionAccess(string section, User user)
        {
            return user.Applications.Any(uApp => uApp.alias == section);
        }

        /// <summary>
        /// Checks if the specified user by username as access to the app
        /// </summary>
        /// <param name="section"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        internal bool UserHasSectionAccess(string section, string username)
        {
            var user = _applicationContext.Services.UserService.GetByUsername(username);
            if (user == null)
            {
                return false;
            }
            return user.HasSectionAccess(section);
        }

        [Obsolete("Returns the current user's unique umbraco sesion id - this cannot be set and isn't intended to be used in your code")]
        public string UmbracoUserContextId
        {
            get
            {
                return _httpContext.GetUmbracoAuthTicket() == null ? "" : GetSessionId();                
            }
            set
            {
            }
        }

        /// <summary>
        /// Ensures that a back office user is logged in
        /// </summary>
        /// <returns></returns>
        public bool IsAuthenticated()
        {
            return _httpContext.User.Identity.IsAuthenticated && _httpContext.GetCurrentIdentity(false) != null;
        }

        protected override void DisposeResources()
        {
            _httpContext = null;
        }

        
    }
}
