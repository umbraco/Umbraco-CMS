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
using Umbraco.Web.PublishedCache;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;
using Umbraco.Web.Editors;

namespace Umbraco.Web.Security
{
    /// <summary>
    /// A helper class for handling Members
    /// </summary>
    public class MembershipHelper
    {
        private readonly MembershipProvider _membershipProvider;
        private readonly RoleProvider _roleProvider;
        private readonly IMemberService _memberService;
        private readonly IMemberTypeService _memberTypeService;
        private readonly IUserService _userService;
        private readonly IPublicAccessService _publicAccessService;
        private readonly AppCaches _appCaches;
        private readonly ILogger _logger;

        #region Constructors

        public MembershipHelper
        (
            HttpContextBase httpContext,
            IPublishedMemberCache memberCache,
            MembershipProvider membershipProvider,
            RoleProvider roleProvider,
            IMemberService memberService,
            IMemberTypeService memberTypeService,
            IUserService userService,
            IPublicAccessService publicAccessService,
            AppCaches appCaches,
            ILogger logger
        )
        {
            HttpContext = httpContext;
            MemberCache = memberCache;
            _memberService = memberService;
            _memberTypeService = memberTypeService;
            _userService = userService;
            _publicAccessService = publicAccessService;
            _appCaches = appCaches;
            _logger = logger;

            _membershipProvider = membershipProvider ?? throw new ArgumentNullException(nameof(membershipProvider));
            _roleProvider = roleProvider ?? throw new ArgumentNullException(nameof(roleProvider));
        }

        #endregion

        protected HttpContextBase HttpContext { get; }
        protected IPublishedMemberCache MemberCache { get; }

        /// <summary>
        /// Check if a document object is protected by the "Protect Pages" functionality in umbraco
        /// </summary>
        /// <param name="path">The full path of the document object to check</param>
        /// <returns>True if the document object is protected</returns>
        public virtual bool IsProtected(string path)
        {
            //this is a cached call
            return _publicAccessService.IsProtected(path);
        }

        public virtual IDictionary<string, bool> IsProtected(IEnumerable<string> paths)
        {
            var result = new Dictionary<string, bool>();
            foreach (var path in paths)
            {
                //this is a cached call
                result[path] = _publicAccessService.IsProtected(path);
            }
            return result;
        }

        /// <summary>
        /// Check if the current user has access to a document
        /// </summary>
        /// <param name="path">The full path of the document object to check</param>
        /// <returns>True if the current user has access or if the current document isn't protected</returns>
        public virtual bool MemberHasAccess(string path)
        {
            if (IsProtected(path))
            {
                return IsLoggedIn() && HasAccess(path, Roles.Provider);
            }
            return true;
        }

        /// <summary>
        /// Checks if the current user has access to the paths
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        public virtual IDictionary<string, bool> MemberHasAccess(IEnumerable<string> paths)
        {
            var protectedPaths = IsProtected(paths);

            var pathsWithProtection = protectedPaths.Where(x => x.Value).Select(x => x.Key);
            var pathsWithAccess = HasAccess(pathsWithProtection, Roles.Provider);

            var result = new Dictionary<string, bool>();
            foreach(var path in paths)
            {
                pathsWithAccess.TryGetValue(path, out var hasAccess);
                // if it's not found it's false anyways
                result[path] = !pathsWithProtection.Contains(path) || hasAccess;
            }
            return result;
        }

        /// <summary>
        /// This will check if the member has access to this path
        /// </summary>
        /// <param name="path"></param>
        /// <param name="roleProvider"></param>
        /// <returns></returns>
        private bool HasAccess(string path, RoleProvider roleProvider)
        {
            return _publicAccessService.HasAccess(path, CurrentUserName, roleProvider.GetRolesForUser);
        }

        private IDictionary<string, bool> HasAccess(IEnumerable<string> paths, RoleProvider roleProvider)
        {
            // ensure we only lookup user roles once
            string[] userRoles = null;
            string[] getUserRoles(string username)
            {
                if (userRoles != null) return userRoles;
                userRoles = roleProvider.GetRolesForUser(username).ToArray();
                return userRoles;
            }

            var result = new Dictionary<string, bool>();
            foreach (var path in paths)
            {
                result[path] = IsLoggedIn() && _publicAccessService.HasAccess(path, CurrentUserName, getUserRoles);
            }
            return result;
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
            if (membershipUser == null) throw new InvalidOperationException("Could not find member with username " + HttpContext.User.Identity.Name);

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

            var memberType = _memberTypeService.Get(member.ContentTypeId);

            if (model.MemberProperties != null)
            {
                foreach (var property in model.MemberProperties
                    //ensure the property they are posting exists
                    .Where(p => memberType.PropertyTypeExists(p.Alias))
                    .Where(property => member.Properties.Contains(property.Alias))
                    //needs to be editable
                    .Where(p => memberType.MemberCanEditProperty(p.Alias)))
                {
                    member.Properties[property.Alias].SetValue(property.Value);
                }
            }

            _memberService.Save(member);

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
                    // TODO: Support q/a http://issues.umbraco.org/issue/U4-3213
                    null, null,
                    true, null, out status);

                if (status != MembershipCreateStatus.Success) return null;

                var member = _memberService.GetByUsername(membershipUser.UserName);
                member.Name = model.Name;

                if (model.MemberProperties != null)
                {
                    foreach (var property in model.MemberProperties.Where(p => p.Value != null)
                        .Where(property => member.Properties.Contains(property.Alias)))
                    {
                        member.Properties[property.Alias].SetValue(property.Value);
                    }
                }

                _memberService.Save(member);
            }
            else
            {
                membershipUser = provider.CreateUser(model.Username, model.Password, model.Email,
                    // TODO: Support q/a http://issues.umbraco.org/issue/U4-3213
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
            // Get the member, do not set to online - this is done implicitly as part of ValidateUser which is consistent with
            // how the .NET framework SqlMembershipProvider works. Passing in true will just cause more unnecessary SQL queries/locks.
            var member = provider.GetUser(username, false);
            if (member == null)
            {
                //this should not happen
                Current.Logger.Warn<MembershipHelper, string>("The member validated but then no member was returned with the username {Username}", username);
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

        public virtual IEnumerable<IPublishedContent> GetByProviderKeys(IEnumerable<object> keys)
        {
            return keys?.Select(GetByProviderKey).WhereNotNull() ?? Enumerable.Empty<IPublishedContent>();
        }

        public virtual IPublishedContent GetById(int memberId)
        {
            return MemberCache.GetById(memberId);
        }

        public virtual IEnumerable<IPublishedContent> GetByIds(IEnumerable<int> memberIds)
        {
            return memberIds?.Select(GetById).WhereNotNull() ?? Enumerable.Empty<IPublishedContent>();
        }

        public virtual IPublishedContent GetById(Guid memberId)
        {
            return GetByProviderKey(memberId);
        }

        public virtual IEnumerable<IPublishedContent> GetByIds(IEnumerable<Guid> memberIds)
        {
            return GetByProviderKeys(memberIds.OfType<object>());
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
                    // TODO: need to implement Get(guid)!
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


                var memberType = _memberTypeService.Get(member.ContentTypeId);

                var builtIns = Constants.Conventions.Member.GetStandardPropertyTypeStubs().Select(x => x.Key).ToArray();

                model.MemberProperties = GetMemberPropertiesViewModel(memberType, builtIns, member).ToList();

                return model;
            }

            //we can try to look up an associated member by the provider user key
            // TODO: Support this at some point!
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
                var memberType = _memberTypeService.Get(memberTypeAlias);
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

                // TODO: Perhaps one day we'll ship with our own EditorTempates but for now developers
                // can just render their own.

                ////This is a rudimentary check to see what data template we should render
                //// if developers want to change the template they can do so dynamically in their views or controllers
                //// for a given property.
                ////These are the default built-in MVC template types: “Boolean”, “Decimal”, “EmailAddress”, “HiddenInput”, “HTML”, “Object”, “String”, “Text”, and “Url”
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
        /// Gets the current user's roles.
        /// </summary>
        /// <remarks>Roles are cached per user name, at request level.</remarks>
        public IEnumerable<string> GetCurrentUserRoles()
            => GetUserRoles(CurrentUserName);

        /// <summary>
        /// Gets a user's roles.
        /// </summary>
        /// <remarks>Roles are cached per user name, at request level.</remarks>
        public IEnumerable<string> GetUserRoles(string userName)
        {
            // optimize by caching per-request (v7 cached per PublishedRequest, in PublishedRouter)
            var key = "Umbraco.Web.Security.MembershipHelper__Roles__" + userName;
            return _appCaches.RequestCache.GetCacheItem(key, () => Roles.Provider.GetRolesForUser(userName));
        }

        /// <summary>
        /// Returns the login status model of the currently logged in member.
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
            return HttpContext.User != null && HttpContext.User.Identity.IsAuthenticated;
        }

        /// <summary>
        /// Returns the currently logged in username
        /// </summary>
        public string CurrentUserName => HttpContext.User.Identity.Name;

        /// <summary>
        /// Returns true or false if the currently logged in member is authorized based on the parameters provided
        /// </summary>
        /// <param name="allowTypes"></param>
        /// <param name="allowGroups"></param>
        /// <param name="allowMembers"></param>
        /// <returns></returns>
        public virtual bool IsMemberAuthorized(
            IEnumerable<string> allowTypes = null,
            IEnumerable<string> allowGroups = null,
            IEnumerable<int> allowMembers = null)
        {
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
            var passwordChanger = new PasswordChanger(_logger, _userService, HttpContext);
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
        /// Returns successful if the membership user required updating, otherwise returns failed if it didn't require updating.
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
            var provider = _membershipProvider;

            if (provider.IsUmbracoMembershipProvider() == false)
            {
                throw new NotSupportedException("An IMember model can only be retrieved when using the built-in Umbraco membership providers");
            }
            var username = provider.GetCurrentUserName();

            // The result of this is cached by the MemberRepository
            var member = _memberService.GetByUsername(username);
            return member;
        }

    }
}
