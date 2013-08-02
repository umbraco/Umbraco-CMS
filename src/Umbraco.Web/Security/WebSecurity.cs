using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Security;
using umbraco;
using umbraco.BusinessLogic;
using umbraco.DataLayer;
using umbraco.businesslogic.Exceptions;
using umbraco.cms.businesslogic.member;
using GlobalSettings = Umbraco.Core.Configuration.GlobalSettings;
using UmbracoSettings = Umbraco.Core.Configuration.UmbracoSettings;

namespace Umbraco.Web.Security
{
    /// <summary>
    /// A utility class used for dealing with security in Umbraco
    /// </summary>
    public class WebSecurity
    {
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

        /// <summary>
        /// Gets the SQL helper.
        /// </summary>
        /// <value>The SQL helper.</value>
        private ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

        private const long TicksPrMinute = 600000000;
        private static readonly int UmbracoTimeOutInMinutes = GlobalSettings.TimeOutInMinutes;

        private User _currentUser;

        /// <summary>
        /// Gets the current user.
        /// </summary>
        /// <value>The current user.</value>
        /// <remarks>
        /// This is internal because we don't want to expose the legacy User object on this class, instead we'll wait until IUser
        /// is public. If people want to reference the current user, they can reference it from the UmbracoContext.
        /// </remarks>
        internal User CurrentUser
        {
            get
            {
                //only load it once per instance!
                return _currentUser ?? (_currentUser = User.GetCurrent());
            }
        }

        /// <summary>
        /// Logs a user in.
        /// </summary>
        /// <param name="userId">The user Id</param>
        public void PerformLogin(int userId)
        {
            var retVal = Guid.NewGuid();
            SqlHelper.ExecuteNonQuery(
                                      "insert into umbracoUserLogins (contextID, userID, timeout) values (@contextId,'" + userId + "','" +
                                      (DateTime.Now.Ticks + (TicksPrMinute * UmbracoTimeOutInMinutes)) +
                                      "') ",
                                      SqlHelper.CreateParameter("@contextId", retVal));
            UmbracoUserContextId = retVal.ToString();

            LogHelper.Info<WebSecurity>("User Id: {0} logged in", () => userId);

        }

        /// <summary>
        /// Clears the current login for the currently logged in user
        /// </summary>
        public void ClearCurrentLogin()
        {
            // Added try-catch in case login doesn't exist in the database
            // Either due to old cookie or running multiple sessions on localhost with different port number
            try
            {
                SqlHelper.ExecuteNonQuery(
                "DELETE FROM umbracoUserLogins WHERE contextId = @contextId",
                SqlHelper.CreateParameter("@contextId", UmbracoUserContextId));
            }
            catch (Exception ex)
            {
                LogHelper.Error<WebSecurity>(string.Format("Login with contextId {0} didn't exist in the database", UmbracoUserContextId), ex);
            }

            //this clears the cookie
            UmbracoUserContextId = "";
        }

        public void RenewLoginTimeout()
        {
            // only call update if more than 1/10 of the timeout has passed
            SqlHelper.ExecuteNonQuery(
                "UPDATE umbracoUserLogins SET timeout = @timeout WHERE contextId = @contextId",
                SqlHelper.CreateParameter("@timeout", DateTime.Now.Ticks + (TicksPrMinute * UmbracoTimeOutInMinutes)),
                SqlHelper.CreateParameter("@contextId", UmbracoUserContextId));
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
            return CurrentUser.Applications.Any(uApp => uApp.alias == app);
        }

        internal void UpdateLogin(long timeout)
        {
            // only call update if more than 1/10 of the timeout has passed
            if (timeout - (((TicksPrMinute * UmbracoTimeOutInMinutes) * 0.8)) < DateTime.Now.Ticks)
                SqlHelper.ExecuteNonQuery(
                    "UPDATE umbracoUserLogins SET timeout = @timeout WHERE contextId = @contextId",
                    SqlHelper.CreateParameter("@timeout", DateTime.Now.Ticks + (TicksPrMinute * UmbracoTimeOutInMinutes)),
                    SqlHelper.CreateParameter("@contextId", UmbracoUserContextId));
        }

        internal long GetTimeout(string umbracoUserContextId)
        {
            return ApplicationContext.Current.ApplicationCache.GetCacheItem(
                CacheKeys.UserContextTimeoutCacheKey + umbracoUserContextId,
                new TimeSpan(0, UmbracoTimeOutInMinutes / 10, 0),
                () => GetTimeout(true));
        }

        internal long GetTimeout(bool byPassCache)
        {
            if (UmbracoSettings.KeepUserLoggedIn)
                RenewLoginTimeout();

            if (byPassCache)
            {
                return SqlHelper.ExecuteScalar<long>("select timeout from umbracoUserLogins where contextId=@contextId",
                                                          SqlHelper.CreateParameter("@contextId", new Guid(UmbracoUserContextId))
                                        );
            }

            return GetTimeout(UmbracoUserContextId);
        }

        /// <summary>
        /// Gets the user id.
        /// </summary>
        /// <param name="umbracoUserContextId">The umbraco user context ID.</param>
        /// <returns></returns>
        public int GetUserId(string umbracoUserContextId)
        {
            //need to parse to guid
            Guid guid;
            if (Guid.TryParse(umbracoUserContextId, out guid) == false)
            {
                return -1;
            }

            var id = ApplicationContext.Current.ApplicationCache.GetCacheItem(
                CacheKeys.UserContextCacheKey + umbracoUserContextId,
                new TimeSpan(0, UmbracoTimeOutInMinutes / 10, 0),
                () => SqlHelper.ExecuteScalar<int?>(
                    "select userID from umbracoUserLogins where contextID = @contextId",
                    SqlHelper.CreateParameter("@contextId", guid)));
            if (id == null)
                return -1;
            return id.Value;
        }

        /// <summary>
        /// Validates the user context ID.
        /// </summary>
        /// <param name="currentUmbracoUserContextId">The umbraco user context ID.</param>
        /// <returns></returns>
        public bool ValidateUserContextId(string currentUmbracoUserContextId)
        {
            if ((currentUmbracoUserContextId != ""))
            {
                int uid = GetUserId(currentUmbracoUserContextId);
                long timeout = GetTimeout(currentUmbracoUserContextId);

                if (timeout > DateTime.Now.Ticks)
                {
                    return true;
                }
                var user = User.GetUser(uid);
                LogHelper.Info(typeof(WebSecurity), "User {0} (Id:{1}) logged out", () => user.Name, () => user.Id);
            }
            return false;
        }

        /// <summary>
        /// Validates the current user
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="throwExceptions">set to true if you want exceptions to be thrown if failed</param>
        /// <returns></returns>
        internal ValidateRequestAttempt ValidateCurrentUser(HttpContextBase httpContext, bool throwExceptions = false)
        {
            if (UmbracoUserContextId != "")
            {
                var uid = GetUserId(UmbracoUserContextId);
                var timeout = GetTimeout(UmbracoUserContextId);

                if (timeout > DateTime.Now.Ticks)
                {
                    var user = User.GetUser(uid);

                    // Check for console access
                    if (user.Disabled || (user.NoConsole && GlobalSettings.RequestIsInUmbracoApplication(httpContext) && GlobalSettings.RequestIsLiveEditRedirector(httpContext) == false))
                    {
                        if (throwExceptions) throw new ArgumentException("You have no priviledges to the umbraco console. Please contact your administrator");
                        return ValidateRequestAttempt.FailedNoPrivileges;
                    }
                    UpdateLogin(timeout);
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
        /// <param name="httpContext"></param>
        /// <param name="throwExceptions">set to true if you want exceptions to be thrown if failed</param>
        /// <returns></returns>
        internal ValidateRequestAttempt AuthorizeRequest(HttpContextBase httpContext, bool throwExceptions = false)
        {
            // check for secure connection
            if (GlobalSettings.UseSSL && httpContext.Request.IsSecureConnection == false)
            {
                if (throwExceptions) throw new UserAuthorizationException("This installation requires a secure connection (via SSL). Please update the URL to include https://");
                return ValidateRequestAttempt.FailedNoSsl;
            }
            return ValidateCurrentUser(httpContext, throwExceptions);
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

        /// <summary>
        /// Gets or sets the umbraco user context ID.
        /// </summary>
        /// <value>The umbraco user context ID.</value>
        public string UmbracoUserContextId
        {
            get
            {
                var authTicket = HttpContext.Current.GetUmbracoAuthTicket();
                if (authTicket == null)
                {
                    return "";
                }
                var identity = authTicket.CreateUmbracoIdentity();
                if (identity == null)
                {
                    HttpContext.Current.UmbracoLogout();
                    return "";
                }
                return identity.UserContextId;
            }
            set
            {
                if (value.IsNullOrWhiteSpace())
                {
                    HttpContext.Current.UmbracoLogout();
                }
                else
                {
                    var uid = GetUserId(value);
                    if (uid == -1)
                    {
                        HttpContext.Current.UmbracoLogout();
                    }
                    else
                    {
                        var user = User.GetUser(uid);
                        HttpContext.Current.CreateUmbracoAuthTicket(
                            new UserData
                            {
                                Id = uid,
                                AllowedApplications = user.Applications.Select(x => x.alias).ToArray(),
                                Culture = ui.Culture(user),
                                RealName = user.Name,
                                Roles = new string[] { user.UserType.Alias },
                                StartContentNode = user.StartNodeId,
                                StartMediaNode = user.StartMediaId,
                                UserContextId = value,
                                Username = user.LoginName
                            });
                    }
                }
            }
        }

    }
}
