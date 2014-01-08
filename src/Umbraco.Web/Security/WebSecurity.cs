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
                    // Allow only if member is assigned to a group in the list
                    var groups = Roles.GetRolesForUser(member.LoginName);
                    allowAction = allowGroupsList.Select(s => s.ToLowerInvariant()).Intersect(groups.Select(myGroup => myGroup.ToLowerInvariant())).Any();
                }

                // If specific members defined, check member is of one of those
                if (allowAction && allowMembers.Any())
                {
                    // Allow only if member's Id is in the list
                    allowAction = allowMembers.Contains(member.Id);
                }
            }

            return allowAction;
        }

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
        internal FormsAuthenticationTicket PerformLogin(IUser user)
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
                Culture = ui.Culture(user.Language)
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
        internal bool ValidateBackOfficeCredentials(string username, string password)
        {
            var membershipProvider = Membership.Providers[UmbracoConfig.For.UmbracoSettings().Providers.DefaultBackOfficeUserProvider];
            return membershipProvider != null && membershipProvider.ValidateUser(username, password);
        }

        /// <summary>
        /// Returns the MembershipUser from the back office membership provider
        /// </summary>
        /// <param name="username"></param>
        /// <param name="setOnline"></param>
        /// <returns></returns>
        internal MembershipUser GetBackOfficeMembershipUser(string username, bool setOnline)
        {
            var membershipProvider = Membership.Providers[UmbracoConfig.For.UmbracoSettings().Providers.DefaultBackOfficeUserProvider];
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

            if (membershipUser == null)
            {
                throw new InvalidOperationException(
                    "The username & password validated but the membership provider '" +
                    Membership.Providers[UmbracoConfig.For.UmbracoSettings().Providers.DefaultBackOfficeUserProvider].Name +
                    "' did not return a MembershipUser with the username supplied");
            }

            //regarldess of the membership provider used, see if this user object already exists in the umbraco data
            var user = _applicationContext.Services.UserService.GetByUsername(membershipUser.UserName);

            //we're using the built-in membership provider so the user will already be available
            if (Membership.Providers[UmbracoConfig.For.UmbracoSettings().Providers.DefaultBackOfficeUserProvider] is UsersMembershipProvider)
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
                Password = Guid.NewGuid().ToString("N"), //Need to set this to something - will not be used though
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
        /// Changes password for a member/user given the membership provider and the password change model
        /// </summary>
        /// <param name="username"></param>
        /// <param name="passwordModel"></param>
        /// <param name="membershipProvider"></param>
        /// <returns></returns>
        /// <remarks>
        /// YES! It is completely insane how many options you have to take into account based on the membership provider. yikes!        
        /// </remarks>
        internal Attempt<PasswordChangedModel> ChangePassword(string username, ChangingPasswordModel passwordModel, MembershipProvider membershipProvider)
        {
            if (passwordModel == null) throw new ArgumentNullException("passwordModel");
            if (membershipProvider == null) throw new ArgumentNullException("membershipProvider");
            
            //Are we resetting the password??
            if (passwordModel.Reset.HasValue && passwordModel.Reset.Value)
            {
                if (membershipProvider.EnablePasswordReset == false)
                {
                    return Attempt.Fail(new PasswordChangedModel {ChangeError = new ValidationResult("Password reset is not enabled", new[] {"resetPassword"})});
                }
                if (membershipProvider.RequiresQuestionAndAnswer && passwordModel.Answer.IsNullOrWhiteSpace())
                {
                    return Attempt.Fail(new PasswordChangedModel {ChangeError = new ValidationResult("Password reset requires a password answer", new[] {"resetPassword"})});                    
                }
                //ok, we should be able to reset it
                try
                {
                    var newPass = membershipProvider.ResetPassword(
                        username,
                        membershipProvider.RequiresQuestionAndAnswer ? passwordModel.Answer : null);

                    //return the generated pword
                    return Attempt.Succeed(new PasswordChangedModel {ResetPassword = newPass});
                }
                catch (Exception ex)
                {
                    LogHelper.WarnWithException<WebSecurity>("Could not reset member password", ex);
                    return Attempt.Fail(new PasswordChangedModel { ChangeError = new ValidationResult("Could not reset password, error: " + ex.Message + " (see log for full details)", new[] { "resetPassword" }) });
                }
            }

            //we're not resetting it so we need to try to change it.

            if (passwordModel.NewPassword.IsNullOrWhiteSpace())
            {
                return Attempt.Fail(new PasswordChangedModel { ChangeError = new ValidationResult("Cannot set an empty password", new[] { "value" }) });
            }

            //This is an edge case and is only necessary for backwards compatibility:
            var umbracoBaseProvider = membershipProvider as MembershipProviderBase;
            if (umbracoBaseProvider != null && umbracoBaseProvider.AllowManuallyChangingPassword)
            {
                //this provider allows manually changing the password without the old password, so we can just do it
                try
                {
                    var result = umbracoBaseProvider.ChangePassword(username, "", passwordModel.NewPassword);
                    return result == false
                        ? Attempt.Fail(new PasswordChangedModel { ChangeError = new ValidationResult("Could not change password, invalid username or password", new[] { "value" }) })
                        : Attempt.Succeed(new PasswordChangedModel());
                }
                catch (Exception ex)
                {
                    LogHelper.WarnWithException<WebSecurity>("Could not change member password", ex);
                    return Attempt.Fail(new PasswordChangedModel { ChangeError = new ValidationResult("Could not change password, error: " + ex.Message + " (see log for full details)", new[] { "value" }) });
                }
            }

            //The provider does not support manually chaning the password but no old password supplied - need to return an error
            if (passwordModel.OldPassword.IsNullOrWhiteSpace() && membershipProvider.EnablePasswordRetrieval == false)
            {
                //if password retrieval is not enabled but there is no old password we cannot continue
                return Attempt.Fail(new PasswordChangedModel { ChangeError = new ValidationResult("Password cannot be changed without the old password", new[] { "value" }) });
            }

            if (passwordModel.OldPassword.IsNullOrWhiteSpace() == false)
            {
                //if an old password is suplied try to change it

                try
                {
                    var result = membershipProvider.ChangePassword(username, passwordModel.OldPassword, passwordModel.NewPassword);
                    return result == false
                        ? Attempt.Fail(new PasswordChangedModel { ChangeError = new ValidationResult("Could not change password, invalid username or password", new[] { "value" }) })
                        : Attempt.Succeed(new PasswordChangedModel());
                }
                catch (Exception ex)
                {
                    LogHelper.WarnWithException<WebSecurity>("Could not change member password", ex);
                    return Attempt.Fail(new PasswordChangedModel { ChangeError = new ValidationResult("Could not change password, error: " + ex.Message + " (see log for full details)", new[] { "value" }) });
                }
            }

            if (membershipProvider.EnablePasswordRetrieval == false)
            {
                //we cannot continue if we cannot get the current password
                return Attempt.Fail(new PasswordChangedModel { ChangeError = new ValidationResult("Password cannot be changed without the old password", new[] { "value" }) });
            }
            if (membershipProvider.RequiresQuestionAndAnswer && passwordModel.Answer.IsNullOrWhiteSpace())
            {
                //if the question answer is required but there isn't one, we cannot continue
                return Attempt.Fail(new PasswordChangedModel { ChangeError = new ValidationResult("Password cannot be changed without the password answer", new[] { "value" }) });
            }

            //lets try to get the old one so we can change it
            try
            {
                var oldPassword = membershipProvider.GetPassword(
                    username,
                    membershipProvider.RequiresQuestionAndAnswer ? passwordModel.Answer : null);

                try
                {
                    var result = membershipProvider.ChangePassword(username, oldPassword, passwordModel.NewPassword);
                    return result == false
                        ? Attempt.Fail(new PasswordChangedModel { ChangeError = new ValidationResult("Could not change password", new[] { "value" }) })
                        : Attempt.Succeed(new PasswordChangedModel());
                }
                catch (Exception ex1)
                {
                    LogHelper.WarnWithException<WebSecurity>("Could not change member password", ex1);
                            return Attempt.Fail(new PasswordChangedModel { ChangeError = new ValidationResult("Could not change password, error: " + ex1.Message + " (see log for full details)", new[] { "value" }) });                            
                }

            }
            catch (Exception ex2)
            {
                LogHelper.WarnWithException<WebSecurity>("Could not retrieve member password", ex2);
                        return Attempt.Fail(new PasswordChangedModel { ChangeError = new ValidationResult("Could not change password, error: " + ex2.Message + " (see log for full details)", new[] { "value" }) });                        
            }
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
            var identity = _httpContext.GetCurrentIdentity(true);
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
            var identity = _httpContext.GetCurrentIdentity(true);
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
                    var user = CurrentUser;

                    // Check for console access
                    if (user.IsApproved == false || (user.IsLockedOut && GlobalSettings.RequestIsInUmbracoApplication(_httpContext)))
                    {
                        if (throwExceptions) throw new ArgumentException("You have no priviledges to the umbraco console. Please contact your administrator");
                        return ValidateRequestAttempt.FailedNoPrivileges;
                    }                    
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
        internal bool UserHasAppAccess(string app, IUser user)
        {
            var apps = _applicationContext.Services.UserService.GetUserSections(user);
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
