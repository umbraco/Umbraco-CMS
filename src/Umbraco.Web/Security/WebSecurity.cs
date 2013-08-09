using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Security;
using umbraco;
using umbraco.DataLayer;
using umbraco.businesslogic.Exceptions;
using GlobalSettings = Umbraco.Core.Configuration.GlobalSettings;
using Member = umbraco.cms.businesslogic.member.Member;
using UmbracoSettings = Umbraco.Core.Configuration.UmbracoSettings;
using User = umbraco.BusinessLogic.User;

namespace Umbraco.Web.Security
{
    /// <summary>
    /// A utility class used for dealing with security in Umbraco
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
        public bool IsMemberAuthorized(
            bool allowAll = false,
            IEnumerable<string> allowTypes = null,
            IEnumerable<string> allowGroups = null,
            IEnumerable<int> allowMembers = null)
        {
            if (allowAll)
                return true;

            if (allowTypes == null)
                allowTypes = Enumerable.Empty<string>();
            if (allowGroups == null)
                allowGroups = Enumerable.Empty<string>();
            if (allowMembers == null)
                allowMembers = Enumerable.Empty<int>();
            
            // Allow by default
            var allowAction = true;
            
            // Get member details
            var member = Member.GetCurrentMember();
            if (member == null)
            {
                // If not logged on, not allowed
                allowAction = false;
            }
            else
            {
                // If types defined, check member is of one of those types
                var allowTypesList = allowTypes as IList<string> ?? allowTypes.ToList();
                if (allowTypesList.Any(allowType => allowType != string.Empty))
                {
                    // Allow only if member's type is in list
                    allowAction = allowTypesList.Select(x => x.ToLowerInvariant()).Contains(member.ContentType.Alias.ToLowerInvariant());
                }

                // If groups defined, check member is of one of those groups
                var allowGroupsList = allowGroups as IList<string> ?? allowGroups.ToList();
                if (allowAction && allowGroupsList.Any(allowGroup => allowGroup != string.Empty))
                {
                    // Allow only if member's type is in list
                    var groups = Roles.GetRolesForUser(member.LoginName);
                    allowAction = groups.Select(s => s.ToLowerInvariant()).Intersect(groups.Select(myGroup => myGroup.ToLowerInvariant())).Any();
                }

                // If specific members defined, check member is of one of those
                if (allowAction && allowMembers.Any())
                {
                    // Allow only if member's type is in list
                    allowAction = allowMembers.Contains(member.Id);
                }
            }

            return allowAction;
        }

        private static readonly int UmbracoTimeOutInMinutes = GlobalSettings.TimeOutInMinutes;

        private IUser _currentUser;

        /// <summary>
        /// Gets the current user.
        /// </summary>
        /// <value>The current user.</value>
        internal IUser CurrentUser
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
        public void PerformLogin(int userId)
        {
            var user = _applicationContext.Services.UserService.GetUserById(userId);
            PerformLogin(user);
        }

        internal void PerformLogin(IUser user)
        {
            _httpContext.CreateUmbracoAuthTicket(new UserData
            {
                Id = user.Id,
                AllowedApplications = user.AllowedSections.ToArray(),
                RealName = user.Name,
                //currently we only have one user type!
                Roles = new[] { user.UserType.Alias },
                StartContentNode = user.StartContentId,
                StartMediaNode = user.StartMediaId,
                Username = user.Username,
                Culture = ui.Culture(user.Language)
            });
            
            LogHelper.Info<WebSecurity>("User Id: {0} logged in", () => user.Id);
        }

        /// <summary>
        /// Clears the current login for the currently logged in user
        /// </summary>
        public void ClearCurrentLogin()
        {
            _httpContext.UmbracoLogout();
        }

        public void RenewLoginTimeout()
        {
            _httpContext.RenewUmbracoAuthTicket(UmbracoTimeOutInMinutes);
        }

        /// <summary>
        /// Validates credentials for a back office user
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        internal bool ValidateBackOfficeCredentials(string username, string password)
        {
            var membershipProvider = Membership.Providers[UmbracoSettings.DefaultBackofficeProvider];
            return membershipProvider != null && membershipProvider.ValidateUser(username, password);
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
            var userApps = _applicationContext.Services.UserService.GetUserSections(CurrentUser);
            return userApps.Any(uApp => uApp.InvariantEquals(app));
        }

        internal void UpdateLogin()
        {
            _httpContext.RenewUmbracoAuthTicket(UmbracoTimeOutInMinutes);
        }

        internal long GetTimeout()
        {
            var ticket = _httpContext.GetUmbracoAuthTicket();
            var ticks = ticket.Expiration.Ticks - DateTime.Now.Ticks;
            return ticks;
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
            var identity = _httpContext.GetCurrentIdentity();
            if (identity == null)
                return -1;
            return Convert.ToInt32(identity.Id);
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
            var ticket = _httpContext.GetUmbracoAuthTicket();
            if (ticket != null)
            {
                if (ticket.Expired == false)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Validates the current user
        /// </summary>
        /// <param name="throwExceptions">set to true if you want exceptions to be thrown if failed</param>
        /// <returns></returns>
        internal ValidateRequestAttempt ValidateCurrentUser(bool throwExceptions)
        {
            var ticket = _httpContext.GetUmbracoAuthTicket();

            if (ticket != null)
            {
                if (ticket.Expired == false)
                {
                    var user = User.GetUser(GetUserId());

                    // Check for console access
                    if (user.Disabled || (user.NoConsole && GlobalSettings.RequestIsInUmbracoApplication(_httpContext) && GlobalSettings.RequestIsLiveEditRedirector(_httpContext) == false))
                    {
                        if (throwExceptions) throw new ArgumentException("You have no priviledges to the umbraco console. Please contact your administrator");
                        return ValidateRequestAttempt.FailedNoPrivileges;
                    }
                    UpdateLogin();
                    return ValidateRequestAttempt.Success;
                }
                if (throwExceptions) throw new ArgumentException("User has timed out!!");
                return ValidateRequestAttempt.FailedTimedOut;
            }

            if (throwExceptions) throw new InvalidOperationException("The user has no umbraco contextid - try logging in");
            return ValidateRequestAttempt.FailedNoContextId;
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
            var uid = User.getUserId(username);
            if (uid < 0) return false;
            var usr = User.GetUser(uid);
            if (usr == null) return false;
            return UserHasAppAccess(app, usr);
        }

        [Obsolete("This is no longer used at all, it will always return a new GUID though if a user is logged in")]
        public string UmbracoUserContextId
        {
            get
            {
                return _httpContext.GetUmbracoAuthTicket() == null ? "" : Guid.NewGuid().ToString();
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
