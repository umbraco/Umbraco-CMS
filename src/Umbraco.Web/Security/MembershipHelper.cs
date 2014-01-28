using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Security;
using Umbraco.Web.Models;

namespace Umbraco.Web.Security
{

    /// <summary>
    /// A helper class for handling Members
    /// </summary>
    public class MembershipHelper
    {
        private readonly ApplicationContext _applicationContext;
        private readonly HttpContextBase _httpContext;

        #region Constructors
        public MembershipHelper(ApplicationContext applicationContext, HttpContextBase httpContext)
        {
            if (applicationContext == null) throw new ArgumentNullException("applicationContext");
            if (httpContext == null) throw new ArgumentNullException("httpContext");
            _applicationContext = applicationContext;
            _httpContext = httpContext;
        }

        public MembershipHelper(UmbracoContext umbracoContext)
        {
            if (umbracoContext == null) throw new ArgumentNullException("umbracoContext");
            _httpContext = umbracoContext.HttpContext;
            _applicationContext = umbracoContext.Application;
        }
        #endregion

        /// <summary>
        /// Returns true if the current membership provider is the Umbraco built-in one.
        /// </summary>
        /// <returns></returns>
        public bool IsUmbracoMembershipProviderActive()
        {
            return Membership.Provider.IsUmbracoMembershipProvider();
        }

        /// <summary>
        /// Updates the currently logged in members profile
        /// </summary>
        /// <param name="model"></param>
        public void UpdateMemberProfile(ProfileModel model)
        {
            if (IsLoggedIn() == false)
            {
                throw new NotSupportedException("No member is currently logged in");
            }

            var member = GetCurrentMember();
            
            if (model.Name != null)
            {
                member.Name = model.Name;
            }
            member.Email = model.Email;
            member.Username = model.Email;

            if (model.MemberProperties != null)
            {
                foreach (var property in model.MemberProperties
                    //ensure the property they are posting exists
                    .Where(p => member.ContentType.PropertyTypeExists(p.Alias))
                    .Where(property => member.Properties.Contains(property.Alias))
                    //needs to be editable
                    .Where(p => member.ContentType.MemberCanEditProperty(p.Alias))
                    //needs to have a value
                    .Where(p => p.Value != null))
                {
                    member.Properties[property.Alias].Value = property.Value;
                }
            }

            _applicationContext.Services.MemberService.Save(member);

            //reset the FormsAuth cookie since the username might have changed
            FormsAuthentication.SetAuthCookie(member.Username, true);
        }

        /// <summary>
        /// Registers a new member
        /// </summary>
        /// <param name="model"></param>
        /// <param name="status"></param>
        /// <param name="logMemberIn">
        /// true to log the member in upon successful registration
        /// </param>
        /// <returns></returns>
        public MembershipUser RegisterMember(RegisterModel model, out MembershipCreateStatus status, bool logMemberIn = true)
        {
            model.Username = (model.UsernameIsEmail || model.Username == null) ? model.Email : model.Username;

            var membershipUser = Membership.CreateUser(model.Username, model.Password, model.Email,
                //TODO: Support q/a http://issues.umbraco.org/issue/U4-3213
                null, null,
                true, out status);

            if (status != MembershipCreateStatus.Success) return null;
            
            //update their real name 
            if (Membership.Provider.IsUmbracoMembershipProvider())
            {
                var member = _applicationContext.Services.MemberService.GetByUsername(membershipUser.UserName);
                member.Name = model.Name;

                if (model.MemberProperties != null)
                {
                    foreach (var property in model.MemberProperties.Where(p => p.Value != null)
                        .Where(property => member.Properties.Contains(property.Alias)))
                    {
                        member.Properties[property.Alias].Value = property.Value;
                    }
                }

                _applicationContext.Services.MemberService.Save(member);
            }
            else
            {
                //TODO: Support this scenario!
            }

            //Set member online
            Membership.GetUser(model.Username, true);

            //Log them in
            FormsAuthentication.SetAuthCookie(membershipUser.UserName, true);

            return membershipUser;
        }

        /// <summary>
        /// A helper method to perform the validation and logging in of a member - this is simply wrapping standard membership provider and asp.net forms auth logic.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool Login(string username, string password)
        {
            //Validate credentials
            if (Membership.ValidateUser(username, password) == false)
            {
                return false;
            }
            //Set member online
            var member = Membership.GetUser(username, true);
            if (member == null)
            {
                //this should not happen
                LogHelper.Warn<MembershipHelper>("The member validated but then no member was returned with the username " + username);                
                return false;
            }
            //Log them in
            FormsAuthentication.SetAuthCookie(member.UserName, true);
            return true;
        }

        /// <summary>
        /// Creates a new profile model filled in with the current members details if they are logged in.
        /// </summary>
        /// <returns></returns>
        public ProfileModel CreateProfileModel()
        {
            if (IsLoggedIn() == false)
            {
                return null;
            }

            if (Membership.Provider.IsUmbracoMembershipProvider())
            {
                var member = GetCurrentMember();
                //this shouldn't happen
                if (member == null) return null;

                var model = ProfileModel.CreateModel();
                model.Name = member.Name;
                model.Email = member.Email;

                var memberType = member.ContentType;

                foreach (var prop in memberType.PropertyTypes.Where(x => memberType.MemberCanEditProperty(x.Alias)))
                {
                    var value = string.Empty;
                    var propValue = member.Properties[prop.Alias];
                    if (propValue != null)
                    {
                        value = propValue.Value.ToString();
                    }

                    model.MemberProperties.Add(new UmbracoProperty
                    {
                        Alias = prop.Alias,
                        Name = prop.Name,
                        Value = value
                    });
                }
                return model;
            }

            //we can try to look up an associated member by the provider user key
            //TODO: Support this at some point!
            throw new NotSupportedException("Currently a member profile cannot be edited unless using the built-in Umbraco membership providers");
        }

        /// <summary>
        /// Creates a model to use for registering new members with custom member properties
        /// </summary>
        /// <param name="memberTypeAlias"></param>
        /// <returns></returns>
        public RegisterModel CreateRegistrationModel(string memberTypeAlias = null)
        {
            if (Membership.Provider.IsUmbracoMembershipProvider())
            {
                memberTypeAlias = memberTypeAlias ?? Constants.Conventions.MemberTypes.Member;
                var memberType = _applicationContext.Services.MemberTypeService.Get(memberTypeAlias);
                if (memberType == null)
                    throw new InvalidOperationException("Could not find a member type with alias " + memberTypeAlias);

                var props = memberType.PropertyTypes
                    .Where(x => memberType.MemberCanEditProperty(x.Alias))
                    .Select(prop => new UmbracoProperty
                    {
                        Alias = prop.Alias,
                        Name = prop.Name,
                        Value = string.Empty
                    }).ToList();

                var model = RegisterModel.CreateModel();
                model.MemberProperties = props;
                return model;
            }
            else
            {
                var model = RegisterModel.CreateModel();
                model.MemberTypeAlias = string.Empty;
                return model;
            }
        }

        /// <summary>
        /// Returns the login status model of the currently logged in member, if no member is logged in it returns null;
        /// </summary>
        /// <returns></returns>
        public LoginStatusModel GetCurrentLoginStatus()
        {
            var model = LoginStatusModel.CreateModel();

            if (IsLoggedIn() == false)
            {
                model.IsLoggedIn = false;
                return model;
            }
            
            if (Membership.Provider.IsUmbracoMembershipProvider())
            {
                var member = GetCurrentMember();
                //this shouldn't happen
                if (member == null) return model;
                model.Name = member.Name;
                model.Username = member.Username;
                model.Email = member.Email;
            }
            else
            {
                var member = Membership.GetUser();
                //this shouldn't happen
                if (member == null) return null;
                model.Name = member.UserName;
                model.Username = member.UserName;
                model.Email = member.Email;
            }

            model.IsLoggedIn = true;
            return model;
        }

        /// <summary>
        /// Check if a member is logged in
        /// </summary>
        /// <returns></returns>
        public bool IsLoggedIn()
        {
            return _httpContext.User != null && _httpContext.User.Identity.IsAuthenticated;
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
            
            if (IsLoggedIn() == false)
            {
                // If not logged on, not allowed
                allowAction = false;
            }
            else
            {
                string username;
                if (Membership.Provider.IsUmbracoMembershipProvider())
                {
                    var member = GetCurrentMember();
                    username = member.Username;
                    // If types defined, check member is of one of those types
                    var allowTypesList = allowTypes as IList<string> ?? allowTypes.ToList();
                    if (allowTypesList.Any(allowType => allowType != string.Empty))
                    {
                        // Allow only if member's type is in list
                        allowAction = allowTypesList.Select(x => x.ToLowerInvariant()).Contains(member.ContentType.Alias.ToLowerInvariant());
                    }

                    // If specific members defined, check member is of one of those
                    if (allowAction && allowMembers.Any())
                    {
                        // Allow only if member's Id is in the list
                        allowAction = allowMembers.Contains(member.Id);
                    }
                }
                else
                {
                    var member = Membership.GetUser();
                    username = member.UserName;
                }
                
                // If groups defined, check member is of one of those groups
                var allowGroupsList = allowGroups as IList<string> ?? allowGroups.ToList();
                if (allowAction && allowGroupsList.Any(allowGroup => allowGroup != string.Empty))
                {
                    // Allow only if member is assigned to a group in the list
                    var groups = Roles.GetRolesForUser(username);
                    allowAction = allowGroupsList.Select(s => s.ToLowerInvariant()).Intersect(groups.Select(myGroup => myGroup.ToLowerInvariant())).Any();
                }

                
            }

            return allowAction;
        }

        /// <summary>
        /// Updates a membership user with all of it's writable properties
        /// </summary>
        /// <param name="member"></param>
        /// <param name="provider"></param>
        /// <param name="email"></param>
        /// <param name="isApproved"></param>
        /// <param name="isLocked"></param>
        /// <param name="lastLoginDate"></param>
        /// <param name="lastActivityDate"></param>
        /// <param name="comment"></param>
        /// <returns></returns>
        internal MembershipUser UpdateMember(MembershipUser member, MembershipProvider provider,
            string email = null,
            bool? isApproved = null,
            bool? isLocked = null,
            DateTime? lastLoginDate = null,
            DateTime? lastActivityDate = null,
            string comment = null)
        {
            //set the writable properties
            if (email != null)
            {
                member.Email = email;
            }
            if (isApproved.HasValue)
            {
                member.IsApproved = isApproved.Value;
            }
            if (lastLoginDate.HasValue)
            {
                member.LastLoginDate = lastLoginDate.Value;
            }
            if (lastActivityDate.HasValue)
            {
                member.LastActivityDate = lastActivityDate.Value;
            }
            if (comment != null)
            {
                member.Comment = comment;
            }

            if (isLocked.HasValue)
            {
                //there is no 'setter' on IsLockedOut but you can ctor a new membership user with it set, so i guess that's what we'll do,
                // this does mean however if it was a typed membership user object that it will no longer be typed
                //membershipUser.IsLockedOut = true;
                member = new MembershipUser(member.ProviderName, member.UserName,
                    member.ProviderUserKey, member.Email, member.PasswordQuestion, member.Comment, member.IsApproved,
                    isLocked.Value,  //new value
                    member.CreationDate, member.LastLoginDate, member.LastActivityDate, member.LastPasswordChangedDate, member.LastLockoutDate);
            }

            provider.UpdateUser(member);

            return member;
        }

        /// <summary>
        /// Returns the currently logged in IMember object - this should never be exposed to the front-end since it's returning a business logic entity!
        /// </summary>
        /// <returns></returns>
        private IMember GetCurrentMember()
        {
            if (Membership.Provider.IsUmbracoMembershipProvider() == false)
            {
                throw new NotSupportedException("An IMember model can only be retreived when using the built-in Umbraco membership providers");
            }
            var member = _applicationContext.Services.MemberService.GetByUsername(_httpContext.User.Identity.Name);
            return member;
        }

    }
}
