using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Security;
using Umbraco.Web.Models;
using Umbraco.Web.PublishedCache;
using Umbraco.Core.Cache;
using MPE = global::Umbraco.Core.Security.MembershipProviderExtensions;

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
            var provider = MPE.GetMembersMembershipProvider();
            return provider.IsUmbracoMembershipProvider();
        }

        /// <summary>
        /// Updates the currently logged in members profile
        /// </summary>
        /// <param name="model"></param>
        /// <returns>
        /// The updated MembershipUser object
        /// </returns>
        public Attempt<MembershipUser> UpdateMemberProfile(ProfileModel model)
        {
            if (IsLoggedIn() == false)
            {
                throw new NotSupportedException("No member is currently logged in");
            }

            //get the current membership user
            var provider = MPE.GetMembersMembershipProvider();
            var membershipUser = provider.GetCurrentUser();
            //NOTE: This should never happen since they are logged in
            if (membershipUser == null) throw new InvalidOperationException("Could not find member with username " + _httpContext.User.Identity.Name);

            try
            {
                //check if the email needs to change
                if (model.Email.InvariantEquals(membershipUser.Email) == false)
                {
                    //Use the membership provider to change the email since that is configured to do the checks to check for unique emails if that is configured.
                    var requiresUpdating = UpdateMember(membershipUser, provider, model.Email);
                    membershipUser = requiresUpdating.Result;
                }
            }
            catch (Exception ex)
            {
                //This will occur if an email already exists!
                return Attempt<MembershipUser>.Fail(ex);
            }

            var member = GetCurrentPersistedMember();

            //NOTE: If changing the username is a requirement, than that needs to be done via the IMember directly since MembershipProvider's natively do 
            // not support changing a username! 
            if (model.Name != null && member.Name != model.Name)
            {
                member.Name = model.Name;
            }

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

            return Attempt<MembershipUser>.Succeed(membershipUser);
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

            MembershipUser membershipUser;
            var provider = MPE.GetMembersMembershipProvider();
            //update their real name 
            if (provider.IsUmbracoMembershipProvider())
            {
                membershipUser = ((UmbracoMembershipProviderBase)provider).CreateUser(
                    model.MemberTypeAlias,
                    model.Username, model.Password, model.Email,
                    //TODO: Support q/a http://issues.umbraco.org/issue/U4-3213
                    null, null,
                    true, null, out status);

                if (status != MembershipCreateStatus.Success) return null;

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
                membershipUser = provider.CreateUser(model.Username, model.Password, model.Email,
                    //TODO: Support q/a http://issues.umbraco.org/issue/U4-3213
                    null, null,
                    true, null, out status);

                if (status != MembershipCreateStatus.Success) return null;
            }

            if (logMemberIn)
            {
                //Set member online
                provider.GetUser(model.Username, true);
    
                //Log them in
                FormsAuthentication.SetAuthCookie(membershipUser.UserName, model.CreatePersistentLoginCookie);
            }

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
            var provider = MPE.GetMembersMembershipProvider();
            //Validate credentials
            if (provider.ValidateUser(username, password) == false)
            {
                return false;
            }
            //Set member online
            var member = provider.GetUser(username, true);
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
        /// Logs out the current member
        /// </summary>
        public void Logout()
        {
            FormsAuthentication.SignOut();
        }

        #region Querying for front-end

        public IPublishedContent GetByProviderKey(object key)
        {
            return _applicationContext.ApplicationCache.RequestCache.GetCacheItem<IPublishedContent>(
                GetCacheKey("GetByProviderKey", key), () =>
                {
                    var provider = MPE.GetMembersMembershipProvider();
                    if (provider.IsUmbracoMembershipProvider() == false)
                    {
                        throw new NotSupportedException("Cannot access this method unless the Umbraco membership provider is active");
                    }

                    var result = _applicationContext.Services.MemberService.GetByProviderKey(key);
                    return result == null ? null : new MemberPublishedContent(result).CreateModel();
                });
        }

        public IPublishedContent GetById(int memberId)
        {
            return _applicationContext.ApplicationCache.RequestCache.GetCacheItem<IPublishedContent>(
                GetCacheKey("GetById", memberId), () =>
                {
                    var provider = MPE.GetMembersMembershipProvider();
                    if (provider.IsUmbracoMembershipProvider() == false)
                    {
                        throw new NotSupportedException("Cannot access this method unless the Umbraco membership provider is active");
                    }

                    var result = _applicationContext.Services.MemberService.GetById(memberId);
                    return result == null ? null : new MemberPublishedContent(result).CreateModel();
                });
        }

        public IPublishedContent GetByUsername(string username)
        {
            return _applicationContext.ApplicationCache.RequestCache.GetCacheItem<IPublishedContent>(
                GetCacheKey("GetByUsername", username), () =>
                {
                    var provider = MPE.GetMembersMembershipProvider();
                    if (provider.IsUmbracoMembershipProvider() == false)
                    {
                        throw new NotSupportedException("Cannot access this method unless the Umbraco membership provider is active");
                    }

                    var result = _applicationContext.Services.MemberService.GetByUsername(username);
                    return result == null ? null : new MemberPublishedContent(result).CreateModel();
                });
        }

        public IPublishedContent GetByEmail(string email)
        {
            return _applicationContext.ApplicationCache.RequestCache.GetCacheItem<IPublishedContent>(
                GetCacheKey("GetByEmail", email), () =>
                {
                    var provider = MPE.GetMembersMembershipProvider();
                    if (provider.IsUmbracoMembershipProvider() == false)
                    {
                        throw new NotSupportedException("Cannot access this method unless the Umbraco membership provider is active");
                    }

                    var result = _applicationContext.Services.MemberService.GetByEmail(email);
                    return result == null ? null : new MemberPublishedContent(result).CreateModel();
                });
        }

        /// <summary>
        /// Returns the currently logged in member as IPublishedContent
        /// </summary>
        /// <returns></returns>
        public IPublishedContent GetCurrentMember()
        {
            if (IsLoggedIn() == false)
            {
                return null;
            }
            var result = GetCurrentPersistedMember();
            return result == null ? null : new MemberPublishedContent(result).CreateModel();
        }

        /// <summary>
        /// Returns the currently logged in member id, -1 if they are not logged in
        /// </summary>
        /// <returns></returns>
        public int GetCurrentMemberId()
        {
            if (IsLoggedIn() == false)
            {
                return -1;
            }
            var result = GetCurrentMember();
            return result == null ? -1 : result.Id;
        }
        
        #endregion

        #region Model Creation methods for member data editing on the front-end
        /// <summary>
        /// Creates a new profile model filled in with the current members details if they are logged in which allows for editing
        /// profile properties
        /// </summary>
        /// <returns></returns>
        public ProfileModel GetCurrentMemberProfileModel()
        {
            if (IsLoggedIn() == false)
            {
                return null;
            }

            var provider = MPE.GetMembersMembershipProvider();

            if (provider.IsUmbracoMembershipProvider())
            {                
                var membershipUser = provider.GetCurrentUserOnline();
                var member = GetCurrentPersistedMember();
                //this shouldn't happen but will if the member is deleted in the back office while the member is trying
                // to use the front-end!
                if (member == null)
                {
                    //log them out since they've been removed
                    FormsAuthentication.SignOut();

                    return null;
                }

                var model = ProfileModel.CreateModel();
                model.Name = member.Name;
                model.MemberTypeAlias = member.ContentTypeAlias;

                model.Email = membershipUser.Email;
                model.UserName = membershipUser.UserName;
                model.PasswordQuestion = membershipUser.PasswordQuestion;
                model.Comment = membershipUser.Comment;
                model.IsApproved = membershipUser.IsApproved;
                model.IsLockedOut = membershipUser.IsLockedOut;
                model.LastLockoutDate = membershipUser.LastLockoutDate;
                model.CreationDate = membershipUser.CreationDate;
                model.LastLoginDate = membershipUser.LastLoginDate;
                model.LastActivityDate = membershipUser.LastActivityDate;
                model.LastPasswordChangedDate = membershipUser.LastPasswordChangedDate;


                var memberType = member.ContentType;

                var builtIns = Constants.Conventions.Member.GetStandardPropertyTypeStubs().Select(x => x.Key).ToArray();

                model.MemberProperties = GetMemberPropertiesViewModel(memberType, builtIns, member).ToList();

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
            var provider = MPE.GetMembersMembershipProvider();
            if (provider.IsUmbracoMembershipProvider())
            {
                memberTypeAlias = memberTypeAlias ?? Constants.Conventions.MemberTypes.DefaultAlias;
                var memberType = _applicationContext.Services.MemberTypeService.Get(memberTypeAlias);
                if (memberType == null)
                    throw new InvalidOperationException("Could not find a member type with alias " + memberTypeAlias);

                var builtIns = Constants.Conventions.Member.GetStandardPropertyTypeStubs().Select(x => x.Key).ToArray();
                var model = RegisterModel.CreateModel();
                model.MemberTypeAlias = memberTypeAlias;
                model.MemberProperties = GetMemberPropertiesViewModel(memberType, builtIns).ToList();
                return model;
            }
            else
            {
                var model = RegisterModel.CreateModel();
                model.MemberTypeAlias = string.Empty;
                return model;
            }
        }

        private IEnumerable<UmbracoProperty> GetMemberPropertiesViewModel(IMemberType memberType, IEnumerable<string> builtIns, IMember member = null)
        {
            var viewProperties = new List<UmbracoProperty>();

            foreach (var prop in memberType.PropertyTypes
                    .Where(x => builtIns.Contains(x.Alias) == false && memberType.MemberCanEditProperty(x.Alias)))
            {
                var value = string.Empty;
                if (member != null)
                {
                    var propValue = member.Properties[prop.Alias];
                    if (propValue != null && propValue.Value != null)
                    {
                        value = propValue.Value.ToString();
                    }    
                }

                var viewProperty = new UmbracoProperty
                {
                    Alias = prop.Alias,
                    Name = prop.Name,
                    Value = value
                };

                //TODO: Perhaps one day we'll ship with our own EditorTempates but for now developers 
                // can just render their own.

                ////This is a rudimentary check to see what data template we should render
                //// if developers want to change the template they can do so dynamically in their views or controllers 
                //// for a given property.
                ////These are the default built-in MVC template types: “Boolean”, “Decimal”, “EmailAddress”, “HiddenInput”, “Html”, “Object”, “String”, “Text”, and “Url”
                //// by default we'll render a text box since we've defined that metadata on the UmbracoProperty.Value property directly.
                //if (prop.DataTypeId == new Guid(Constants.PropertyEditors.TrueFalse))
                //{
                //    viewProperty.EditorTemplate = "UmbracoBoolean";
                //}
                //else
                //{                    
                //    switch (prop.DataTypeDatabaseType)
                //    {
                //        case DataTypeDatabaseType.Integer:
                //            viewProperty.EditorTemplate = "Decimal";
                //            break;
                //        case DataTypeDatabaseType.Ntext:
                //            viewProperty.EditorTemplate = "Text";
                //            break;
                //        case DataTypeDatabaseType.Date:
                //        case DataTypeDatabaseType.Nvarchar:
                //            break;
                //    }
                //}

                viewProperties.Add(viewProperty);
            }
            return viewProperties;
        }
        #endregion

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

            var provider = MPE.GetMembersMembershipProvider();

            if (provider.IsUmbracoMembershipProvider())
            {
                var member = GetCurrentPersistedMember();
                //this shouldn't happen but will if the member is deleted in the back office while the member is trying
                // to use the front-end!
                if (member == null)
                {
                    //log them out since they've been removed
                    FormsAuthentication.SignOut();
                    model.IsLoggedIn = false;
                    return model;
                }
                model.Name = member.Name;
                model.Username = member.Username;
                model.Email = member.Email;
            }
            else
            {
                var member = provider.GetCurrentUserOnline();
                //this shouldn't happen but will if the member is deleted in the back office while the member is trying
                // to use the front-end!
                if (member == null)
                {
                    //log them out since they've been removed
                    FormsAuthentication.SignOut();
                    model.IsLoggedIn = false;
                    return model;
                }
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
        /// Returns the currently logged in username
        /// </summary>
        public string CurrentUserName
        {
            get { return _httpContext.User.Identity.Name; }
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
                var provider = MPE.GetMembersMembershipProvider();

                string username;
                if (provider.IsUmbracoMembershipProvider())
                {
                    var member = GetCurrentPersistedMember();
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
                    var member = provider.GetCurrentUser();
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
        /// Changes password for a member/user given the membership provider name and the password change model
        /// </summary>
        /// <param name="username"></param>
        /// <param name="passwordModel"></param>
        /// <param name="membershipProviderName"></param>
        /// <returns></returns>
        public Attempt<PasswordChangedModel> ChangePassword(string username, ChangingPasswordModel passwordModel, string membershipProviderName)
        {
            var provider = Membership.Providers[membershipProviderName];
            if (provider == null)
            {
                throw new InvalidOperationException("Could not find provider with name " + membershipProviderName);
            }
            return ChangePassword(username, passwordModel, provider);
        }

        /// <summary>
        /// Changes password for a member/user given the membership provider and the password change model
        /// </summary>
        /// <param name="username"></param>
        /// <param name="passwordModel"></param>
        /// <param name="membershipProvider"></param>
        /// <returns></returns>        
        public Attempt<PasswordChangedModel> ChangePassword(string username, ChangingPasswordModel passwordModel, MembershipProvider membershipProvider)
        {
            // YES! It is completely insane how many options you have to take into account based on the membership provider. yikes!        

            if (passwordModel == null) throw new ArgumentNullException("passwordModel");
            if (membershipProvider == null) throw new ArgumentNullException("membershipProvider");

            //Are we resetting the password??
            if (passwordModel.Reset.HasValue && passwordModel.Reset.Value)
            {
                if (membershipProvider.EnablePasswordReset == false)
                {
                    return Attempt.Fail(new PasswordChangedModel { ChangeError = new ValidationResult("Password reset is not enabled", new[] { "resetPassword" }) });
                }
                if (membershipProvider.RequiresQuestionAndAnswer && passwordModel.Answer.IsNullOrWhiteSpace())
                {
                    return Attempt.Fail(new PasswordChangedModel { ChangeError = new ValidationResult("Password reset requires a password answer", new[] { "resetPassword" }) });
                }
                //ok, we should be able to reset it
                try
                {
                    var newPass = membershipProvider.ResetPassword(
                        username,
                        membershipProvider.RequiresQuestionAndAnswer ? passwordModel.Answer : null);

                    //return the generated pword
                    return Attempt.Succeed(new PasswordChangedModel { ResetPassword = newPass });
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
        /// Updates a membership user with all of it's writable properties
        /// </summary>
        /// <param name="member"></param>
        /// <param name="provider"></param>
        /// <param name="email"></param>
        /// <param name="isApproved"></param>
        /// <param name="lastLoginDate"></param>
        /// <param name="lastActivityDate"></param>
        /// <param name="comment"></param>
        /// <returns>
        /// Returns successful if the membershipuser required updating, otherwise returns failed if it didn't require updating.
        /// </returns>
        internal Attempt<MembershipUser> UpdateMember(MembershipUser member, MembershipProvider provider,
            string email = null,
            bool? isApproved = null,
            DateTime? lastLoginDate = null,
            DateTime? lastActivityDate = null,
            string comment = null)
        {
            var needsUpdating = new List<bool>();

            //set the writable properties
            if (email != null)
            {
                needsUpdating.Add(member.Email != email);
                member.Email = email;
            }
            if (isApproved.HasValue)
            {
                needsUpdating.Add(member.IsApproved != isApproved.Value);
                member.IsApproved = isApproved.Value;
            }
            if (lastLoginDate.HasValue)
            {
                needsUpdating.Add(member.LastLoginDate != lastLoginDate.Value);
                member.LastLoginDate = lastLoginDate.Value;
            }
            if (lastActivityDate.HasValue)
            {
                needsUpdating.Add(member.LastActivityDate != lastActivityDate.Value);
                member.LastActivityDate = lastActivityDate.Value;
            }
            if (comment != null)
            {
                needsUpdating.Add(member.Comment != comment);
                member.Comment = comment;
            }

            //Don't persist anything if nothing has changed
            if (needsUpdating.Any(x => x == true))
            {
                provider.UpdateUser(member);
                return Attempt<MembershipUser>.Succeed(member);
            }

            return Attempt<MembershipUser>.Fail(member);
        }
        
        /// <summary>
        /// Returns the currently logged in IMember object - this should never be exposed to the front-end since it's returning a business logic entity!
        /// </summary>
        /// <returns></returns>
        private IMember GetCurrentPersistedMember()
        {
            return _applicationContext.ApplicationCache.RequestCache.GetCacheItem<IMember>(
                GetCacheKey("GetCurrentPersistedMember"), () =>
                {
                    var provider = MPE.GetMembersMembershipProvider();

                    if (provider.IsUmbracoMembershipProvider() == false)
                    {
                        throw new NotSupportedException("An IMember model can only be retreived when using the built-in Umbraco membership providers");
                    }
                    var username = provider.GetCurrentUserName();
                    var member = _applicationContext.Services.MemberService.GetByUsername(username);
                    return member;
                });
        }

        private string GetCacheKey(string key, params object[] additional)
        {
            var sb = new StringBuilder(string.Format("{0}-{1}", typeof (MembershipHelper).Name, key));
            foreach (var s in additional)
            {
                sb.Append("-");
                sb.Append(s);
            }
            return sb.ToString();
        }

    }
}
