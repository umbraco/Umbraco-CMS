using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;
using LightInject;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Security;
using Umbraco.Web.Models;
using Umbraco.Web.PublishedCache;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;
using Umbraco.Web.Editors;
using Umbraco.Web.Security.Providers;
using Umbraco.Web.Routing;
using MPE = global::Umbraco.Core.Security.MembershipProviderExtensions;

namespace Umbraco.Web.Security
{
    /// <summary>
    /// A helper class for handling Members
    /// </summary>
    public class MembershipHelper
    {
        private readonly MembershipProvider _membershipProvider;
        private readonly RoleProvider _roleProvider;
        private readonly HttpContextBase _httpContext;
        private readonly IPublishedMemberCache _memberCache;
        private readonly UmbracoContext _umbracoContext;

        [Inject]
        private IMemberService MemberService { get; set; }

        [Inject]
        private IMemberTypeService MemberTypeService { get; set; }

        [Inject]
        private IUserService UserService { get; set; }

        [Inject]
        private IPublicAccessService PublicAccessService { get; set; }

        [Inject]
        private CacheHelper ApplicationCache { get; set; }

        [Inject]
        private ILogger Logger { get; set; }

        [Inject]
        private PublishedRouter Router { get; set; }

        #region Constructors

        // used here and there for IMember operations (not front-end stuff, no need for _memberCache)


        // used everywhere
        public MembershipHelper(UmbracoContext umbracoContext)
            : this(umbracoContext, MPE.GetMembersMembershipProvider(), Roles.Enabled ? Roles.Provider : new MembersRoleProvider(Current.Services.MemberService))
        { }

        // used in tests and (this)
        public MembershipHelper(UmbracoContext umbracoContext, MembershipProvider membershipProvider, RoleProvider roleProvider)
        {
            if (umbracoContext == null) throw new ArgumentNullException(nameof(umbracoContext));
            if (membershipProvider == null) throw new ArgumentNullException(nameof(membershipProvider));
            if (roleProvider == null) throw new ArgumentNullException(nameof(roleProvider));

            _httpContext = umbracoContext.HttpContext;
            _umbracoContext = umbracoContext;
            _membershipProvider = membershipProvider;
            _roleProvider = roleProvider;
            _memberCache = umbracoContext.PublishedSnapshot.Members;

            // helpers are *not* instanciated by the container so we have to
            // get our dependencies injected manually, through properties.
            Current.Container.InjectProperties(this);
        }

        #endregion

        protected IPublishedMemberCache MemberCache
        {
            get
            {
                if (_memberCache == null)
                    throw new InvalidOperationException("No MemberCache.");
                return _memberCache;
            }
        }

        /// <summary>
        /// Check if a document object is protected by the "Protect Pages" functionality in umbraco
        /// </summary>
        /// <param name="path">The full path of the document object to check</param>
        /// <returns>True if the document object is protected</returns>
        public virtual bool IsProtected(string path)
        {
            //this is a cached call
            return PublicAccessService.IsProtected(path);
        }

        /// <summary>
        /// Check if the current user has access to a document
        /// </summary>
        /// <param name="path">The full path of the document object to check</param>
        /// <returns>True if the current user has access or if the current document isn't protected</returns>
        public virtual bool MemberHasAccess(string path)
        {
            //cache this in the request cache
            return ApplicationCache.RequestCache.GetCacheItem<bool>(string.Format("{0}.{1}-{2}", typeof(MembershipHelper), "MemberHasAccess", path), () =>
            {
                if (IsProtected(path))
                {
                    return IsLoggedIn() && HasAccess(path, Roles.Provider);
                }
                return true;
            });
        }

        /// <summary>
        /// This will check if the member has access to this path
        /// </summary>
        /// <param name="path"></param>
        /// <param name="roleProvider"></param>
        /// <returns></returns>
        /// <remarks>
        /// This is essentially the same as the PublicAccessServiceExtensions.HasAccess however this will use the PCR cache
        /// of the already looked up roles for the member so this doesn't need to happen more than once.
        /// This does a safety check in case of things like unit tests where there is no PCR and if that is the case it will use
        /// lookup the roles directly.
        /// </remarks>
        private bool HasAccess(string path, RoleProvider roleProvider)
        {
            return _umbracoContext.PublishedRequest == null
                ? PublicAccessService.HasAccess(path, CurrentUserName, roleProvider.GetRolesForUser)
                : PublicAccessService.HasAccess(path, CurrentUserName, Router.GetRolesForLogin);
        }

        /// <summary>
        /// Returns true if the current membership provider is the Umbraco built-in one.
        /// </summary>
        /// <returns></returns>
        public bool IsUmbracoMembershipProviderActive()
        {
            var provider = _membershipProvider;
            return provider.IsUmbracoMembershipProvider();
        }

        /// <summary>
        /// Updates the currently logged in members profile
        /// </summary>
        /// <param name="model"></param>
        /// <returns>
        /// The updated MembershipUser object
        /// </returns>
        public virtual Attempt<MembershipUser> UpdateMemberProfile(ProfileModel model)
        {
            if (IsLoggedIn() == false)
            {
                throw new NotSupportedException("No member is currently logged in");
            }

            //get the current membership user
            var provider = _membershipProvider;
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
                    .Where(p => member.ContentType.MemberCanEditProperty(p.Alias)))
                {
                    member.Properties[property.Alias].SetValue(property.Value);
                }
            }

            MemberService.Save(member);

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
        public virtual MembershipUser RegisterMember(RegisterModel model, out MembershipCreateStatus status, bool logMemberIn = true)
        {
            model.Username = (model.UsernameIsEmail || model.Username == null) ? model.Email : model.Username;

            MembershipUser membershipUser;
            var provider = _membershipProvider;
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

                var member = MemberService.GetByUsername(membershipUser.UserName);
                member.Name = model.Name;

                if (model.MemberProperties != null)
                {
                    foreach (var property in model.MemberProperties.Where(p => p.Value != null)
                        .Where(property => member.Properties.Contains(property.Alias)))
                    {
                        member.Properties[property.Alias].SetValue(property.Value);
                    }
                }

                MemberService.Save(member);
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
        public virtual bool Login(string username, string password)
        {
            var provider = _membershipProvider;
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
                Current.Logger.Warn<MembershipHelper>("The member validated but then no member was returned with the username {Username}", username);
                return false;
            }
            //Log them in
            FormsAuthentication.SetAuthCookie(member.UserName, true);
            return true;
        }

        /// <summary>
        /// Logs out the current member
        /// </summary>
        public virtual void Logout()
        {
            FormsAuthentication.SignOut();
        }

        #region Querying for front-end

        public virtual IPublishedContent GetByProviderKey(object key)
        {
            return MemberCache.GetByProviderKey(key);
        }

        public virtual IPublishedContent GetById(int memberId)
        {
            return MemberCache.GetById(memberId);
        }

        public virtual IPublishedContent GetByUsername(string username)
        {
            return MemberCache.GetByUsername(username);
        }

        public virtual IPublishedContent GetByEmail(string email)
        {
            return MemberCache.GetByEmail(email);
        }

        public virtual IPublishedContent Get(Udi udi)
        {
            var guidUdi = udi as GuidUdi;
            if (guidUdi == null) return null;

            var umbracoType = Constants.UdiEntityType.ToUmbracoObjectType(udi.EntityType);

            var entityService = Current.Services.EntityService;
            switch (umbracoType)
            {
                case UmbracoObjectTypes.Member:
                    // fixme - need to implement Get(guid)!
                    var memberAttempt = entityService.GetId(guidUdi.Guid, umbracoType);
                    if (memberAttempt.Success)
                        return GetById(memberAttempt.Result);
                    break;
            }

            return null;
        }

        /// <summary>
        /// Returns the currently logged in member as IPublishedContent
        /// </summary>
        /// <returns></returns>
        public virtual IPublishedContent GetCurrentMember()
        {
            if (IsLoggedIn() == false)
            {
                return null;
            }
            var result = GetCurrentPersistedMember();
            return result == null ? null : MemberCache.GetByMember(result);
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
            return result?.Id ?? -1;
        }

        #endregion

        #region Model Creation methods for member data editing on the front-end
        /// <summary>
        /// Creates a new profile model filled in with the current members details if they are logged in which allows for editing
        /// profile properties
        /// </summary>
        /// <returns></returns>
        public virtual ProfileModel GetCurrentMemberProfileModel()
        {
            if (IsLoggedIn() == false)
            {
                return null;
            }

            var provider = _membershipProvider;

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
        public virtual RegisterModel CreateRegistrationModel(string memberTypeAlias = null)
        {
            var provider = _membershipProvider;
            if (provider.IsUmbracoMembershipProvider())
            {
                memberTypeAlias = memberTypeAlias ?? Constants.Conventions.MemberTypes.DefaultAlias;
                var memberType = MemberTypeService.Get(memberTypeAlias);
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
                    .Where(x => builtIns.Contains(x.Alias) == false && memberType.MemberCanEditProperty(x.Alias))
                    .OrderBy(p => p.SortOrder))
            {
                var value = string.Empty;
                if (member != null)
                {
                    var propValue = member.Properties[prop.Alias];
                    if (propValue != null && propValue.GetValue() != null)
                    {
                        value = propValue.GetValue().ToString();
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
        public virtual LoginStatusModel GetCurrentLoginStatus()
        {
            var model = LoginStatusModel.CreateModel();

            if (IsLoggedIn() == false)
            {
                model.IsLoggedIn = false;
                return model;
            }

            var provider = _membershipProvider;

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
        public string CurrentUserName => _httpContext.User.Identity.Name;

        /// <summary>
        /// Returns true or false if the currently logged in member is authorized based on the parameters provided
        /// </summary>
        /// <param name="allowAll"></param>
        /// <param name="allowTypes"></param>
        /// <param name="allowGroups"></param>
        /// <param name="allowMembers"></param>
        /// <returns></returns>
        public virtual bool IsMemberAuthorized(
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
                var provider = _membershipProvider;

                string username;
                
                if (provider.IsUmbracoMembershipProvider())
                {
                    var member = GetCurrentPersistedMember();
                    // If a member could not be resolved from the provider, we are clearly not authorized and can break right here
                    if (member == null)
                        return false;
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
                    // If a member could not be resolved from the provider, we are clearly not authorized and can break right here
                    if (member == null)
                        return false;
                    username = member.UserName;
                }

                // If groups defined, check member is of one of those groups
                var allowGroupsList = allowGroups as IList<string> ?? allowGroups.ToList();
                if (allowAction && allowGroupsList.Any(allowGroup => allowGroup != string.Empty))
                {
                    // Allow only if member is assigned to a group in the list
                    var groups = _roleProvider.GetRolesForUser(username);
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
        public virtual Attempt<PasswordChangedModel> ChangePassword(string username, ChangingPasswordModel passwordModel, string membershipProviderName)
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
        public virtual Attempt<PasswordChangedModel> ChangePassword(string username, ChangingPasswordModel passwordModel, MembershipProvider membershipProvider)
        {
            var passwordChanger = new PasswordChanger(Logger, UserService, Web.Composing.Current.UmbracoContext.HttpContext);
            return passwordChanger.ChangePasswordWithMembershipProvider(username, passwordModel, membershipProvider);
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
            var update = false;

            if (email != null)
            {
                if (member.Email != email) update = true;
                member.Email = email;
            }
            if (isApproved.HasValue)
            {
                if (member.IsApproved != isApproved.Value) update = true;
                member.IsApproved = isApproved.Value;
            }
            if (lastLoginDate.HasValue)
            {
                if (member.LastLoginDate != lastLoginDate.Value) update = true;
                member.LastLoginDate = lastLoginDate.Value;
            }
            if (lastActivityDate.HasValue)
            {
                if (member.LastActivityDate != lastActivityDate.Value) update = true;
                member.LastActivityDate = lastActivityDate.Value;
            }
            if (comment != null)
            {
                if (member.Comment != comment) update = true;
                member.Comment = comment;
            }

            if (update == false)
                return Attempt<MembershipUser>.Fail(member);

            provider.UpdateUser(member);
            return Attempt<MembershipUser>.Succeed(member);
        }

        /// <summary>
        /// Returns the currently logged in IMember object - this should never be exposed to the front-end since it's returning a business logic entity!
        /// </summary>
        /// <returns></returns>
        private IMember GetCurrentPersistedMember()
        {
            return ApplicationCache.RequestCache.GetCacheItem<IMember>(
                GetCacheKey("GetCurrentPersistedMember"), () =>
                {
                    var provider = _membershipProvider;

                    if (provider.IsUmbracoMembershipProvider() == false)
                    {
                        throw new NotSupportedException("An IMember model can only be retreived when using the built-in Umbraco membership providers");
                    }
                    var username = provider.GetCurrentUserName();
                    var member = MemberService.GetByUsername(username);
                    return member;
                });
        }

        private static string GetCacheKey(string key, params object[] additional)
        {
            var sb = new StringBuilder();
            sb.Append(typeof (MembershipHelper).Name);
            sb.Append("-");
            sb.Append(key);
            foreach (var s in additional)
            {
                sb.Append("-");
                sb.Append(s);
            }
            return sb.ToString();
        }

    }
}
