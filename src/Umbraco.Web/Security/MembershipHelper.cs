using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Models.Security;
using Umbraco.Cms.Core.Net;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Web.Security
{
    /// <summary>
    /// Helper class containing logic relating to the built-in Umbraco members macros and controllers for:
    /// - Registration
    /// - Updating
    /// - Logging in
    /// - Current status
    /// </summary>
    public class MembershipHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMemberService _memberService;
        private readonly IMemberTypeService _memberTypeService;
        private readonly IPublicAccessService _publicAccessService;
        private readonly AppCaches _appCaches;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;
        private readonly IShortStringHelper _shortStringHelper;
        private readonly IEntityService _entityService;
        private readonly IMemberManager _memberManager;
        private readonly IIpResolver _ipResolver;

        //TODO: use from identity
        private readonly bool _enablePasswordRetrieval = false;
        private readonly bool _requiresUniqueEmail = true;
        private readonly int _maxInvalidPasswordAttempts = 10;
            
        #region Constructors

        public MembershipHelper
        (
            IHttpContextAccessor httpContextAccessor,
            IPublishedMemberCache memberCache,
            IMemberService memberService,
            IMemberTypeService memberTypeService,
            IPublicAccessService publicAccessService,
            AppCaches appCaches,
            ILoggerFactory loggerFactory,
            IShortStringHelper shortStringHelper,
            IEntityService entityService,
            IIpResolver ipResolver,
            IMemberManager memberManager
        )
        {
            MemberCache = memberCache;
            _httpContextAccessor = httpContextAccessor;
            _memberService = memberService;
            _memberTypeService = memberTypeService;
            _publicAccessService = publicAccessService;
            _appCaches = appCaches;
            _loggerFactory = loggerFactory;
            _logger = _loggerFactory.CreateLogger<MembershipHelper>();
            _shortStringHelper = shortStringHelper;
            _entityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
            _ipResolver = ipResolver ?? throw new ArgumentNullException(nameof(ipResolver));
            _memberManager = memberManager ?? throw new ArgumentNullException(nameof(memberManager));
        }

        #endregion

        protected IPublishedMemberCache MemberCache { get; }

        /// <summary>
        /// Check if a document object is protected by the "Protect Pages" functionality in umbraco
        /// The call is a cached call
        /// </summary>
        /// <param name="path">The full path of the document object to check</param>
        /// <returns>True if the document object is protected</returns>
        public virtual bool IsProtected(string path) => _publicAccessService.IsProtected(path);

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
        public virtual async Task<bool> MemberHasAccess(string path)
        {
            if (IsProtected(path))
            {
                return IsLoggedIn() && await HasAccess(path);
            }
            return true;
        }

        /// <summary>
        /// Checks if the current user has access to the paths
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        public virtual async Task<IDictionary<string, bool>> MemberHasAccess(IEnumerable<string> paths)
        {
            IDictionary<string, bool> protectedPaths = IsProtected(paths);
            IEnumerable<string> pathsWithProtection = protectedPaths.Where(x => x.Value).Select(x => x.Key);
            IDictionary<string, bool> pathsWithAccess = await HasAccess(pathsWithProtection);

            var result = new Dictionary<string, bool>();
            foreach (var path in paths)
            {
                pathsWithAccess.TryGetValue(path, out var hasAccess);
                // if it's not found it's false anyways
                result[path] = !pathsWithProtection.Contains(path) || hasAccess;
            }
            return result;
        }

        /// <summary>
        /// Updates the currently logged in members profile
        /// </summary>
        /// <param name="model"></param>
        /// <returns>
        /// The updated MembershipUser object
        /// </returns>
        public virtual async Task<Attempt<MemberIdentityUser>> UpdateMemberProfile(ProfileModel model)
        {
            if (IsLoggedIn() == false)
            {
                throw new NotSupportedException("No member is currently logged in");
            }

            MemberIdentityUser membershipUser = await GetCurrentUser();

            try
            {
                //check if the email needs to change
                if (model.Email.InvariantEquals(membershipUser.Email) == false)
                {
                    //Use identity to change the email since that is configured to do the checks to check for unique emails if that is configured.
                    //TODO: come back to UpdateMember
                    Attempt<MemberIdentityUser> requiresUpdating = UpdateMember(membershipUser, model.Email);
                    membershipUser = requiresUpdating.Result;
                }
            }
            catch (Exception ex)
            {
                //This will occur if an email already exists!
                return Attempt<MemberIdentityUser>.Fail(ex);
            }

            //TODO: come back to this
            IMember member = GetCurrentPersistedMember();

            //NOTE: If changing the username is a requirement, than that needs to be done via the IMember directly since MembershipProvider's natively do
            // not support changing a username!
            if (model.Name != null && membershipUser.Name != model.Name)
            {
                membershipUser.Name = model.Name;
            }

            IMemberType memberType = _memberTypeService.Get(member.ContentTypeId);

            if (model.MemberProperties != null)
            {
                foreach (UmbracoProperty property in model.MemberProperties
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

            //TODO: Replace with Identity SignInManager
            //reset the FormsAuth cookie since the username might have changed
            //FormsAuthentication.SetAuthCookie(member.Username, true);

            return Attempt<MemberIdentityUser>.Succeed(membershipUser);
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
        public virtual async Task<Member> RegisterMember(RegisterModel model, bool logMemberIn = true)
        {
            //TODO: This no longer has an out parameter as it is an aSync method. Is that ok? 
            model.Username = (model.UsernameIsEmail || model.Username == null) ? model.Email : model.Username;

            var identityMember = MemberIdentityUser.CreateNew(
                model.Username,
                model.Email,
                model.MemberTypeAlias,
                model.Name);

            IdentityResult created = await _memberManager.CreateAsync(identityMember, model.Password);

            if (!created.Succeeded)
            {
                return null;
            }

            var member = (Member)_memberService.GetByUsername(identityMember.UserName);

            if (model.MemberProperties != null)
            {
                foreach (UmbracoProperty property in model.MemberProperties.Where(p => p.Value != null)
                    .Where(property => member.Properties.Contains(property.Alias)))
                {
                    member.Properties[property.Alias].SetValue(property.Value);
                }
            }

            _memberService.Save(member);

            if (logMemberIn)
            {
                //TODO: come back to this
                SetMemberOnline(identityMember, member);
            }

            return member;
        }

        /// <summary>
        /// A helper method to perform the validation and logging in of a member - this is simply wrapping standard membership provider and asp.net forms auth logic.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public virtual async Task<bool> Login(string username, string password)
        {
            //TODO: move this logic to the manager
            MemberIdentityUser member = await _memberManager.FindByNameAsync(username);
            if (member == null)
            {
                _logger.LogWarning($"Login attempt failed for member username {0} from IP address {1}, the user does not exist", username, _ipResolver.GetCurrentRequestIpAddress());
                return false;
            }

            if (member.IsApproved == false)
            {
                _logger.LogWarning($"Validation failed for member username {0} from IP address {1}, the user is not approved", username, _ipResolver.GetCurrentRequestIpAddress());
                return false;
            }

            if (member.IsLockedOut)
            {
                _logger.LogWarning($"Login attempt failed for member username {0} from IP address {1}, the user is locked out", username, _ipResolver.GetCurrentRequestIpAddress());
                return false;
            }

            //Validate credentials
            bool authenticated = await _memberManager.CheckPasswordAsync(member, password);

            if (authenticated == false)
            {
                //TODO: Is this too explicit?
                _logger.LogWarning($"The password was not valid for member user {0} from IP address {1}", username, _ipResolver.GetCurrentRequestIpAddress());

                // TODO: Increment login attempts - lock if too many.
                // TODO: Should we do this in Identity layer instead?
                var count = member.AccessFailedCount;
                count++;
                member.AccessFailedCount = count;

                if (count >= _maxInvalidPasswordAttempts)
                {
                    member.LockoutEnabled = true;
                    //member.LastLockoutDate = DateTime.Now;
                    _logger.LogWarning($"Login attempt failed for username {0} from IP address {1}, the user is now locked out, max invalid password attempts exceeded", username, _ipResolver.GetCurrentRequestIpAddress());
                }
                else
                {
                    _logger.LogWarning($"Login attempt failed for username {0} from IP address {1}", username, _ipResolver.GetCurrentRequestIpAddress());
                }
            }
            else
            {
                if (member.AccessFailedCount > 0)
                {
                    //we have successfully logged in, reset the AccessFailedCount
                    member.AccessFailedCount = 0;
                    return false;
                }

                _logger.LogWarning($"Login attempt succeeded for member username {0} from IP address {1}", username, _ipResolver.GetCurrentRequestIpAddress());
            }

            //TODO: old comment, what is still needed in .NET Core?
            // don't raise events for this! It just sets the member dates, if we do raise events this will
            // cause all distributed cache to execute - which will clear out some caches we don't want.
            // http://issues.umbraco.org/issue/U4-3451
            // TODO: In v8 we aren't going to have an overload to disable events, so we'll need to make a different method
            // for this type of thing (i.e. UpdateLastLogin or similar).

            member.LastLoginDateUtc = DateTime.Now;

            await _memberManager.UpdateAsync(member);

            //TODO: do we still need to worry about full save/speed?
            //bool requiresFullSave = false;
            //set the last login date without full save (fast, no locks)
            //_memberService.SetLastLogin(member.UserName, member.LastLoginDateUtc.GetValueOrDefault());

            //TODO: replace
            // Get the member, do not set to online - this is done implicitly as part of ValidateUser which is consistent with
            // how the .NET framework SqlMembershipProvider works. Passing in true will just cause more unnecessary SQL queries/locks.
            MemberIdentityUser identityMember = await GetUser(username, false);
            if (identityMember == null)
            {
                //this should not happen
                _logger.LogWarning("The member validated but then no member was returned with the username {Username}", username);
                return false;
            }

            //Log them in
            //TODO: login user in using SignInManager
            //FormsAuthentication.SetAuthCookie(member.UserName, true);
            return true;
        }

        #region Querying for front-end


        /// <summary>
        /// Logs out the current member
        /// </summary>
        public virtual void Logout()
        {
            //TODO: Use members SignInManager
            //FormsAuthentication.SignOut();
        }

        public virtual IPublishedContent GetByProviderKey(object key) => MemberCache.GetByProviderKey(key);

        public virtual IEnumerable<IPublishedContent> GetByProviderKeys(IEnumerable<object> keys) => keys?.Select(GetByProviderKey).WhereNotNull() ?? Enumerable.Empty<IPublishedContent>();

        public virtual IPublishedContent GetById(int memberId) => MemberCache.GetById(memberId);

        public virtual IEnumerable<IPublishedContent> GetByIds(IEnumerable<int> memberIds) => memberIds?.Select(GetById).WhereNotNull() ?? Enumerable.Empty<IPublishedContent>();

        public virtual IPublishedContent GetById(Guid memberId) => GetByProviderKey(memberId);

        public virtual IEnumerable<IPublishedContent> GetByIds(IEnumerable<Guid> memberIds) => GetByProviderKeys(memberIds.OfType<object>());

        public virtual IPublishedContent GetByUsername(string username) => MemberCache.GetByUsername(username);

        public virtual IPublishedContent GetByEmail(string email) => MemberCache.GetByEmail(email);

        public virtual IPublishedContent Get(Udi udi)
        {
            var guidUdi = udi as GuidUdi;
            if (guidUdi == null)
            {
                return null;
            }

            UmbracoObjectTypes umbracoType = UdiEntityTypeHelper.ToUmbracoObjectType(udi.EntityType);

            switch (umbracoType)
            {
                case UmbracoObjectTypes.Member:
                    // TODO: need to implement Get(guid)!
                    Attempt<int> memberAttempt = _entityService.GetId(guidUdi.Guid, umbracoType);
                    if (memberAttempt.Success)
                    {
                        return GetById(memberAttempt.Result);
                    }

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

            //TODO: revisit this
            IMember result = GetCurrentPersistedMember();
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

            //TODO: revisit this
            IPublishedContent result = GetCurrentMember();
            return result?.Id ?? -1;
        }

        #endregion

        #region Model Creation methods for member data editing on the front-end
        /// <summary>
        /// Creates a new profile model filled in with the current members details if they are logged in which allows for editing
        /// profile properties
        /// </summary>
        /// <returns></returns>
        public virtual async Task<ProfileModel> GetCurrentMemberProfileModel()
        {
            if (IsLoggedIn() == false)
            {
                return null;
            }

            MemberIdentityUser membershipUser = await GetCurrentUserOnline();
            IMember member = GetCurrentPersistedMember();

            //this shouldn't happen but will if the member is deleted in the back office while the member is trying
            // to use the front-end!
            if (member == null)
            {
                //TODO: Replace with identity SignInmanager
                //log them out since they've been removed
                //FormsAuthentication.SignOut();

                return null;
            }

            var model = new ProfileModel
            {
                Name = member.Name,
                MemberTypeAlias = member.ContentTypeAlias,

                //TODO: we don't have all these properties on members identity
                // Comment, LastLockoutDate, CreationDate, LastActivityDate
                Email = membershipUser.Email,
                UserName = membershipUser.UserName,
                //Comment = membershipUser.Comment,
                IsApproved = membershipUser.IsApproved,
                IsLockedOut = membershipUser.IsLockedOut,
                //LastLockoutDate = membershipUser.LastLockoutDate,
                //CreationDate = membershipUser.CreationDate,
                LastLoginDate = membershipUser.LastLoginDateUtc.GetValueOrDefault(),
                //LastActivityDate = membershipUser.LastActivityDate,
                LastPasswordChangedDate = membershipUser.LastPasswordChangeDateUtc.GetValueOrDefault()
            };

            IMemberType memberType = _memberTypeService.Get(member.ContentTypeId);

            var builtIns = ConventionsHelper.GetStandardPropertyTypeStubs(_shortStringHelper).Select(x => x.Key).ToArray();

            model.MemberProperties = GetMemberPropertiesViewModel(memberType, builtIns, member).ToList();

            return model;
        }

        /// <summary>
        /// Creates a model to use for registering new members with custom member properties
        /// </summary>
        /// <param name="memberTypeAlias"></param>
        /// <returns></returns>
        public virtual RegisterModel CreateRegistrationModel(string memberTypeAlias = null)
        {
            memberTypeAlias ??= Constants.Conventions.MemberTypes.DefaultAlias;
            IMemberType memberType = _memberTypeService.Get(memberTypeAlias);
            if (memberType == null)
            {
                throw new InvalidOperationException("Could not find a member type with alias " + memberTypeAlias);
            }

            var builtIns = ConventionsHelper.GetStandardPropertyTypeStubs(_shortStringHelper).Select(x => x.Key).ToArray();
            var model = RegisterModel.CreateModel();
            model.MemberTypeAlias = memberTypeAlias;
            model.MemberProperties = GetMemberPropertiesViewModel(memberType, builtIns).ToList();
            return model;
        }

        private IEnumerable<UmbracoProperty> GetMemberPropertiesViewModel(IMemberType memberType, IEnumerable<string> builtIns, IMember member = null)
        {
            var viewProperties = new List<UmbracoProperty>();

            foreach (IPropertyType prop in memberType.PropertyTypes
                    .Where(x => builtIns.Contains(x.Alias) == false && memberType.MemberCanEditProperty(x.Alias))
                    .OrderBy(p => p.SortOrder))
            {
                var value = string.Empty;
                if (member != null)
                {
                    IProperty propValue = member.Properties[prop.Alias];
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
        public async Task<IEnumerable<string>> GetCurrentUserRoles() => await GetUserRoles(CurrentUserName);

        /// <summary>
        /// Gets a user's roles.
        /// </summary>
        /// <remarks>Roles are cached per user name, at request level.</remarks>
        public async Task<IEnumerable<string>> GetUserRoles(string userName)
        {
            // optimize by caching per-request (v7 cached per PublishedRequest, in PublishedRouter)
            var key = "Umbraco.Web.Security.MembershipHelper__Roles__" + userName;
            return await _appCaches.RequestCache.GetCacheItem(key, () => GetRolesForUser(userName));
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

            IMember member = GetCurrentPersistedMember();
            //this shouldn't happen but will if the member is deleted in the back office while the member is trying
            // to use the front-end!
            if (member == null)
            {
                //log them out since they've been removed
                //TODO: use identity SigninManager
                //FormsAuthentication.SignOut();
                model.IsLoggedIn = false;
                return model;
            }
            model.Name = member.Name;
            model.Username = member.Username;
            model.Email = member.Email;

            model.IsLoggedIn = true;
            return model;
        }

        /// <summary>
        /// Check if a member is logged in
        /// </summary>
        /// <returns></returns>
        public bool IsLoggedIn()
        {
            //TODO: change to identity
            HttpContextBase httpContext = _httpContextAccessor.HttpContext;
            return httpContext?.User != null && httpContext.User.Identity.IsAuthenticated;
        }

        /// <summary>
        /// Returns the currently logged in member's username
        /// </summary>
        /// TODO: this has been changed for identity
        public string CurrentUserName => _httpContextAccessor.GetRequiredHttpContext().User.Identity.Name;
        //await GetCurrentMember();


        /// <summary>
        /// Returns true or false if the currently logged in member is authorized based on the parameters provided
        /// </summary>
        /// <param name="allowTypes"></param>
        /// <param name="allowGroups"></param>
        /// <param name="allowMembers"></param>
        /// <returns></returns>
        public virtual async Task<bool> IsMemberAuthorized(
            IEnumerable<string> allowTypes = null,
            IEnumerable<string> allowGroups = null,
            IEnumerable<int> allowMembers = null)
        {
            if (allowTypes == null)
            {
                allowTypes = Enumerable.Empty<string>();
            }

            if (allowGroups == null)
            {
                allowGroups = Enumerable.Empty<string>();
            }

            if (allowMembers == null)
            {
                allowMembers = Enumerable.Empty<int>();
            }

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
                IMember member = GetCurrentPersistedMember();
                // If a member could not be resolved from the provider, we are clearly not authorized and can break right here
                if (member == null)
                {
                    return false;
                }

                username = member.Username;

                // If types defined, check member is of one of those types
                IList<string> allowTypesList = allowTypes as IList<string> ?? allowTypes.ToList();
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

                // If groups defined, check member is of one of those groups
                IList<string> allowGroupsList = allowGroups as IList<string> ?? allowGroups.ToList();
                if (allowAction && allowGroupsList.Any(allowGroup => allowGroup != string.Empty))
                {
                    // Allow only if member is assigned to a group in the list
                    IEnumerable<string> groups = await GetRolesForUser(username);
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
        /// <param name="lastLoginDate"></param>
        /// <param name="lastActivityDate"></param>
        /// <param name="comment"></param>
        /// <returns>
        /// Returns successful if the membership user required updating, otherwise returns failed if it didn't require updating.
        /// </returns>
        internal Attempt<MemberIdentityUser> UpdateMember(MemberIdentityUser member,
            string email = null,
            bool? isApproved = null,
            DateTime? lastLoginDate = null,
            DateTime? lastActivityDate = null,
            string comment = null)
        {
            var update = false;

            if (email != null)
            {
                if (member.Email != email)
                {
                    update = true;
                }

                member.Email = email;
            }
            if (isApproved.HasValue)
            {
                if (member.IsApproved != isApproved.Value)
                {
                    update = true;
                }

                member.IsApproved = isApproved.Value;
            }
            if (lastLoginDate.HasValue)
            {
                if (member.LastLoginDateUtc != lastLoginDate.Value)
                {
                    update = true;
                }

                member.LastLoginDateUtc = lastLoginDate.Value;
            }

            //TODO: implement for identity
            //if (lastActivityDate.HasValue)
            //{
            //    if (member.LastActivityDate != lastActivityDate.Value)
            //    {
            //        update = true;
            //    }

            //    member.LastActivityDate = lastActivityDate.Value;
            //}
            //if (comment != null)
            //{
            //    if (member.Comment != comment)
            //    {
            //        update = true;
            //    }

            //    member.Comment = comment;
            //}

            if (update == false)
            {
                return Attempt<MemberIdentityUser>.Fail(member);
            }

            //TODO: revisit this
            UpdateUser(member);
            return Attempt<MemberIdentityUser>.Succeed(member);
        }

        /// <summary>
        /// Returns the currently logged in IMember object - this should never be exposed to the front-end since it's returning a business logic entity!
        /// </summary>
        /// <returns></returns>
        private IMember GetCurrentPersistedMember()
        {
            string username = GetCurrentUserName();

            //TODO: is this still true?
            // The result of this is cached by the MemberRepository
            //MemberIdentityUser member = await _memberManager.FindByNameAsync(username);
            IMember member = _memberService.GetByUsername(username);
            return member;
        }

        /// <summary>
        /// This will check if the member has access to this path
        /// </summary>
        /// <param name="path"></param>
        /// <param name="roleProvider"></param>
        /// <returns></returns>
        private async Task<bool> HasAccess(string path)
        {
            string[] userRoles = null;
            return _publicAccessService.HasAccess(path, CurrentUserName, username => getUserRolesAsync(username, ref userRoles));
        }

        private async Task<IEnumerable<string>> GetRolesForUser(string username)
        {
            MemberIdentityUser currentMember = await _memberManager.FindByNameAsync(username);

            IEnumerable<string> list = new List<string>();

            //TODO: return groups or roles
            IEnumerable<IdentityUserRole<string>> currentMemberRoles = currentMember.Roles;
            foreach (IdentityUserRole<string> role in currentMemberRoles)
            {
                list.Append(role.RoleId);
            }

            return list;
        }

        private async Task<IDictionary<string, bool>> HasAccess(IEnumerable<string> paths)
        {
            // ensure we only lookup user roles once
            string[] userRoles = null;

            var result = new Dictionary<string, bool>();
            foreach (var path in paths)
            {
                result[path] = IsLoggedIn() && _publicAccessService.HasAccess(path, CurrentUserName, username => getUserRolesAsync(username, ref userRoles));
            }
            return result;
        }

        private string[] getUserRolesAsync(string username, ref string[] userRoles)
        {
            if (userRoles != null)
            {
                return userRoles;
            }

            //TODO: amend
            userRoles = (string[])GetRolesForUser(username).Result;
            return userRoles;
        }


        /// <summary>
        /// Sets member online and save
        /// </summary>
        /// <param name="identityMember"></param>
        /// <param name="member"></param>
        private void SetMemberOnline(MemberIdentityUser identityMember, Member member)
        {
            // Set member online
            // TODO: move to identity
            // logic and comments taken from legacy UmbracoMembershipProvider
            // update the database data directly instead of a full member save which requires DB locks

            DateTime now = DateTime.Now;
            _memberService.SetLastLogin(identityMember.UserName, now);
            member.LastLoginDate = now;
            member.UpdateDate = now;

            //Log them in
            //TODO: Use identity sign-in manager
            //FormsAuthentication.SetAuthCookie(membershipUser.UserName, model.CreatePersistentLoginCookie);
        }

        /// <summary>
        /// Updates e-mail  approved status, lock status and comment on a user.
        /// </summary>
        /// <param name="user">A <see cref="T:System.Web.Security.MembershipUser"></see> object that represents the user to update and the updated information for the user.</param>
        private void UpdateUser(MemberIdentityUser user)
        {
            IMember m = _memberService.GetByUsername(user.UserName);

            if (m == null)
            {
                throw new Exception(string.Format("No member with the username '{0}' found", user.UserName));
            }

            if (_requiresUniqueEmail && user.Email.Trim().IsNullOrWhiteSpace() == false)
            {
                IEnumerable<IMember> byEmail = _memberService.FindByEmail(user.Email.Trim(), 0, int.MaxValue, out long totalRecs, StringPropertyMatchType.Exact);
                if (byEmail.Count(x => x.Id != m.Id) > 0)
                {
                    throw new Exception(string.Format("A member with the email '{0}' already exists", user.Email));
                }
            }
            m.Email = user.Email;

            m.IsApproved = user.IsApproved;
            m.IsLockedOut = user.IsLockedOut;
            if (user.IsLockedOut)
            {
                m.LastLockoutDate = DateTime.Now;
            }

            //TODO: implement comment for member
            //m.Comments = user.Comment;

            _memberService.Save(m);
        }

        /// <summary>
        /// Returns the currently logged in MembershipUser
        /// </summary>
        /// <returns></returns>
        private async Task<MemberIdentityUser> GetCurrentUser()
        {
            string username = GetCurrentUserName();
            return username.IsNullOrWhiteSpace()
                ? null
                : await GetUser(username, true);
        }


        /// <summary>
        /// Returns the currently logged in MembershipUser and flags them as being online - use sparingly (i.e. login)
        /// </summary>
        /// <param name="membershipProvider"></param>
        /// <returns></returns>
        private async Task<MemberIdentityUser> GetCurrentUserOnline()
        {
            string username = GetCurrentUserName();
            return username.IsNullOrWhiteSpace()
                ? null
                : await GetUser(username, true);
        }

        private async Task<MemberIdentityUser> GetUser(string username, bool isOnline)
        {
            MemberIdentityUser membershipUser;

            if (username.IsNullOrWhiteSpace())
            {
                return null;
            }
            else
            {
                membershipUser = await _memberManager.FindByNameAsync(username);
                //NOTE: This should never happen since they are logged in
                if (membershipUser == null)
                {
                    throw new InvalidOperationException("Could not find member with username " + username);
                }

                if (isOnline)
                {
                    IMember member = _memberService.GetByUsername(username);
                    SetMemberOnline(membershipUser, (Member)member);
                }

                return membershipUser;
            }
        }

        private string GetCurrentUserName()
        {
            //TODO: rework for Identity
            if (HostingEnvironment.IsHosted)
            {
                HttpContext current = HttpContext.Current;
                if (current != null && current.User != null && current.User.Identity != null)
                {
                    return current.User.Identity.Name;
                }
            }
            IPrincipal currentPrincipal = Thread.CurrentPrincipal;
            if (currentPrincipal == null || currentPrincipal.Identity == null)
            {
                return string.Empty;
            }
            else
            {
                return currentPrincipal.Identity.Name;
            }
        }
    }
}
