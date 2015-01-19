using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Security;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Security;
using Umbraco.Web.Models;
using Umbraco.Web.Models.ContentEditing;
using umbraco;
using umbraco.DataLayer;
using umbraco.businesslogic.Exceptions;
using umbraco.providers;
using GlobalSettings = Umbraco.Core.Configuration.GlobalSettings;
using Member = umbraco.cms.businesslogic.member.Member;
using User = umbraco.BusinessLogic.User;

namespace Umbraco.Web.Security
{
    /// <summary>
    /// A utility class used for dealing with USER security in Umbraco
    /// </summary>
    public class WebSecurity : DisposableObject
    {
        private HttpContextBase _httpContext;
        private ApplicationContext _applicationContext;

        public WebSecurity(HttpContextBase httpContext, ApplicationContext applicationContext)
        {
            _httpContext = httpContext;
            _applicationContext = applicationContext;
            //This ensures the dispose method is called when the request terminates, though
            // we also ensure this happens in the Umbraco module because the UmbracoContext is added to the
            // http context items.
            _httpContext.DisposeOnPipelineCompleted(this);
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
            if (HttpContext.Current == null || ApplicationContext.Current == null)
            {
                return false;
            }
            var helper = new MembershipHelper(ApplicationContext.Current, new HttpContextWrapper(HttpContext.Current));
            return helper.IsMemberAuthorized(allowAll, allowTypes, allowGroups, allowMembers);
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
                //only load it once per instance!
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

        /// <summary>
        /// Logs a user in.
        /// </summary>
        /// <param name="userId">The user Id</param>
        /// <returns>returns the number of seconds until their session times out</returns>
        public double PerformLogin(int userId)
        {
            var user = _applicationContext.Services.UserService.GetUserById(userId);
            return PerformLogin(user).GetRemainingAuthSeconds();
        }

        /// <summary>
        /// Logs the user in
        /// </summary>
        /// <param name="user"></param>
        /// <returns>returns the number of seconds until their session times out</returns>
        public FormsAuthenticationTicket PerformLogin(IUser user)
        {
            var ticket = _httpContext.CreateUmbracoAuthTicket(new UserData(Guid.NewGuid().ToString("N"))
            {
                Id = user.Id,
                AllowedApplications = user.AllowedSections.ToArray(),
                RealName = user.Name,
                //currently we only have one user type!
                Roles = new[] { user.UserType.Alias },
                StartContentNode = user.StartContentId,
                StartMediaNode = user.StartMediaId,
                Username = user.Username,
                Culture = ui.Culture(user)
            });
            
            LogHelper.Info<WebSecurity>("User Id: {0} logged in", () => user.Id);

            return ticket;
        }

        /// <summary>
        /// Clears the current login for the currently logged in user
        /// </summary>
        public void ClearCurrentLogin()
        {
            _httpContext.UmbracoLogout();
        }

        /// <summary>
        /// Renews the user's login ticket
        /// </summary>
        public void RenewLoginTimeout()
        {
            _httpContext.RenewUmbracoAuthTicket();
        }

        /// <summary>
        /// Validates credentials for a back office user
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool ValidateBackOfficeCredentials(string username, string password)
        {
            var membershipProvider = Core.Security.MembershipProviderExtensions.GetUsersMembershipProvider();
            return membershipProvider != null && membershipProvider.ValidateUser(username, password);
        }
        
        /// <summary>
        /// Returns the MembershipUser from the back office membership provider
        /// </summary>
        /// <param name="username"></param>
        /// <param name="setOnline"></param>
        /// <returns></returns>
        public MembershipUser GetBackOfficeMembershipUser(string username, bool setOnline)
        {
            var membershipProvider = Core.Security.MembershipProviderExtensions.GetUsersMembershipProvider();
            return membershipProvider != null ? membershipProvider.GetUser(username, setOnline) : null;
        }

        /// <summary>
        /// Returns the back office IUser instance for the username specified
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        /// <remarks>
        /// This will return an Iuser instance no matter what membership provider is installed for the back office, it will automatically
        /// create any missing Iuser accounts if one is not found and a custom membership provider is being used. 
        /// </remarks>
        internal IUser GetBackOfficeUser(string username)
        {
            //get the membership user (set user to be 'online' in the provider too)
            var membershipUser = GetBackOfficeMembershipUser(username, true);
            var provider = Core.Security.MembershipProviderExtensions.GetUsersMembershipProvider();

            if (membershipUser == null)
            {
                throw new InvalidOperationException(
                    "The username & password validated but the membership provider '" +
                    provider.Name +
                    "' did not return a MembershipUser with the username supplied");
            }

            //regarldess of the membership provider used, see if this user object already exists in the umbraco data
            var user = _applicationContext.Services.UserService.GetByUsername(membershipUser.UserName);

            //we're using the built-in membership provider so the user will already be available
            if (provider.IsUmbracoUsersProvider())
            {
                if (user == null)
                {
                    //this should never happen
                    throw new InvalidOperationException("The user '" + username + "' could not be found in the Umbraco database");
                }
                return user;
            }

            //we are using a custom membership provider for the back office, in this case we need to create user accounts for the logged in member.
            //if we already have a user object in Umbraco we don't need to do anything, otherwise we need to create a mapped Umbraco account.
            if (user != null) return user;

            //we need to create an Umbraco IUser of a 'writer' type with access to only content - this was how v6 operates.
            var writer = _applicationContext.Services.UserService.GetUserTypeByAlias("writer");
            
            var email = membershipUser.Email;
            if (email.IsNullOrWhiteSpace())
            {
                //in some cases if there is no email we have to generate one since it is required!
                email = Guid.NewGuid().ToString("N") + "@example.com";
            }

            user = new Core.Models.Membership.User(writer)
            {
                Email = email,
                Language = GlobalSettings.DefaultUILanguage,
                Name = membershipUser.UserName,
                RawPasswordValue = Guid.NewGuid().ToString("N"), //Need to set this to something - will not be used though
                DefaultPermissions = writer.Permissions,
                Username = membershipUser.UserName,
                StartContentId = -1,
                StartMediaId = -1,
                IsLockedOut = false,
                IsApproved = true
            };
            user.AddAllowedSection("content");

            _applicationContext.Services.UserService.Save(user);

            return user;
        }

        /// <summary>
        /// Validates the user node tree permissions.
        /// </summary>
        /// <param name="umbracoUser"></param>
        /// <param name="path">The path.</param>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        internal bool ValidateUserNodeTreePermissions(User umbracoUser, string path, string action)
        {
            var permissions = umbracoUser.GetPermissions(path);
            if (permissions.IndexOf(action, StringComparison.Ordinal) > -1 && (path.Contains("-20") || ("," + path + ",").Contains("," + umbracoUser.StartNodeId + ",")))
                return true;

            var user = umbracoUser;
            LogHelper.Info<WebSecurity>("User {0} has insufficient permissions in UmbracoEnsuredPage: '{1}', '{2}', '{3}'", () => user.Name, () => path, () => permissions, () => action);
            return false;
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
        public int GetUserId()
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
        public string GetSessionId()
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
        public bool ValidateCurrentUser()
        {
            var result = ValidateCurrentUser(false);
            return result == ValidateRequestAttempt.Success; 
        }

        /// <summary>
        /// Validates the current user assigned to the request and ensures the stored user data is valid
        /// </summary>
        /// <param name="throwExceptions">set to true if you want exceptions to be thrown if failed</param>
        /// <returns></returns>
        internal ValidateRequestAttempt ValidateCurrentUser(bool throwExceptions)
        {
            //This will first check if the current user is already authenticated - which should be the case in nearly all circumstances
            // since the authentication happens in the Module, that authentication also checks the ticket expiry. We don't 
            // need to check it a second time because that requires another decryption phase and nothing can tamper with it during the request.

            if (_httpContext.User.Identity.IsAuthenticated == false) 
            {
                //There is no user
                if (throwExceptions) throw new InvalidOperationException("The user has no umbraco contextid - try logging in");
                return ValidateRequestAttempt.FailedNoContextId;
            }

            var user = CurrentUser;

            // Check for console access
            if (user == null || user.IsApproved == false || (user.IsLockedOut && GlobalSettings.RequestIsInUmbracoApplication(_httpContext)))
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
        /// <param name="app"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        internal bool UserHasAppAccess(string app, IUser user)
        {
            var apps = user.AllowedSections;
            return apps.Any(uApp => uApp.InvariantEquals(app));
        }

        [Obsolete("Do not use this method if you don't have to, use the overload with IUser instead")]
        internal bool UserHasAppAccess(string app, User user)
        {
            return user.Applications.Any(uApp => uApp.alias == app);
        }

        /// <summary>
        /// Checks if the specified user by username as access to the app
        /// </summary>
        /// <param name="app"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        internal bool UserHasAppAccess(string app, string username)
        {
            var user = _applicationContext.Services.UserService.GetByUsername(username);
            if (user == null)
            {
                return false;
            }
            return UserHasAppAccess(app, user);
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
        
        protected override void DisposeResources()
        {
            _httpContext = null;
            _applicationContext = null;
        }
    }
}
