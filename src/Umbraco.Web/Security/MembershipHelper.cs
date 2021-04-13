using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Security;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;
using Umbraco.Web.Security.Providers;

namespace Umbraco.Web.Security
{
    // MIGRATED TO NETCORE
    // TODO: Analyse all - much can be moved/removed since most methods will occur on the manager via identity implementation

    /// <summary>
    /// Helper class containing logic relating to the built-in Umbraco members macros and controllers for:
    /// - Registration
    /// - Updating
    /// - Logging in
    /// - Current status
    /// </summary>
    public class MembershipHelper
    {
        private readonly MembersMembershipProvider _membershipProvider;
        private readonly RoleProvider _roleProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMemberService _memberService;
        private readonly IMemberTypeService _memberTypeService;
        private readonly IPublicAccessService _publicAccessService;
        private readonly AppCaches _appCaches;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;
        private readonly IShortStringHelper _shortStringHelper;
        private readonly IEntityService _entityService;

        #region Constructors

        public MembershipHelper
        (
            IHttpContextAccessor httpContextAccessor,
            IPublishedMemberCache memberCache,
            MembersMembershipProvider membershipProvider,
            RoleProvider roleProvider,
            IMemberService memberService,
            IMemberTypeService memberTypeService,
            IPublicAccessService publicAccessService,
            AppCaches appCaches,
            ILoggerFactory loggerFactory,
            IShortStringHelper shortStringHelper,
            IEntityService entityService
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

            _membershipProvider = membershipProvider ?? throw new ArgumentNullException(nameof(membershipProvider));
            _roleProvider = roleProvider ?? throw new ArgumentNullException(nameof(roleProvider));
            _entityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
        }

        #endregion

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
            foreach (var path in paths)
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
                if (userRoles != null)
                    return userRoles;
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
            if (guidUdi == null)
                return null;

            var umbracoType = UdiEntityTypeHelper.ToUmbracoObjectType(udi.EntityType);

            switch (umbracoType)
            {
                case UmbracoObjectTypes.Member:
                    // TODO: need to implement Get(guid)!
                    var memberAttempt = _entityService.GetId(guidUdi.Guid, umbracoType);
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
        /// Check if a member is logged in
        /// </summary>
        /// <returns></returns>
        public bool IsLoggedIn()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            return httpContext?.User != null && httpContext.User.Identity.IsAuthenticated;
        }

        /// <summary>
        /// Returns the currently logged in username
        /// </summary>
        public string CurrentUserName => _httpContextAccessor.GetRequiredHttpContext().User.Identity.Name;

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
        /// Returns the currently logged in IMember object - this should never be exposed to the front-end since it's returning a business logic entity!
        /// </summary>
        /// <returns></returns>
        private IMember GetCurrentPersistedMember()
        {
            var provider = _membershipProvider;

            var username = provider.GetCurrentUserName();
            // The result of this is cached by the MemberRepository
            var member = _memberService.GetByUsername(username);
            return member;
        }

    }
}
