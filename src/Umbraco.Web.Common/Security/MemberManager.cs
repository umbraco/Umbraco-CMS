using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Net;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Security;

public class MemberManager : UmbracoUserManager<MemberIdentityUser, MemberPasswordConfigurationSettings>, IMemberManager
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IPublicAccessService _publicAccessService;
    private readonly IMemberUserStore _store;
    private MemberIdentityUser? _currentMember;

    public MemberManager(
        IIpResolver ipResolver,
        IMemberUserStore store,
        IOptions<IdentityOptions> optionsAccessor,
        IPasswordHasher<MemberIdentityUser> passwordHasher,
        IEnumerable<IUserValidator<MemberIdentityUser>> userValidators,
        IEnumerable<IPasswordValidator<MemberIdentityUser>> passwordValidators,
        IdentityErrorDescriber errors,
        IServiceProvider services,
        ILogger<UserManager<MemberIdentityUser>> logger,
        IOptionsSnapshot<MemberPasswordConfigurationSettings> passwordConfiguration,
        IPublicAccessService publicAccessService,
        IHttpContextAccessor httpContextAccessor)
        : base(
            ipResolver,
            store,
            optionsAccessor,
            passwordHasher,
            userValidators,
            passwordValidators,
            errors,
            services,
            logger,
            passwordConfiguration)
    {
        _store = store;
        _publicAccessService = publicAccessService;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
    public async Task<bool> IsMemberAuthorizedAsync(
        IEnumerable<string>? allowTypes = null,
        IEnumerable<string>? allowGroups = null,
        IEnumerable<int>? allowMembers = null)
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
            MemberIdentityUser? currentMember = await GetCurrentMemberAsync();

            // If a member could not be resolved from the provider, we are clearly not authorized and can break right here
            if (currentMember == null)
            {
                return false;
            }

            var memberId = int.Parse(currentMember.Id, CultureInfo.InvariantCulture);

            // If types defined, check member is of one of those types
            IList<string> allowTypesList = allowTypes as IList<string> ?? allowTypes.ToList();
            if (allowTypesList.Any(allowType => allowType != string.Empty))
            {
                // Allow only if member's type is in list
                allowAction = allowTypesList.Select(x => x.ToLowerInvariant())
                    .Contains(currentMember.MemberTypeAlias?.ToLowerInvariant());
            }

            // If specific members defined, check member is of one of those
            var allowMembersList = allowMembers.ToList();
            if (allowAction && allowMembersList.Any())
            {
                // Allow only if member's Id is in the list
                allowAction = allowMembersList.Contains(memberId);
            }

            // If groups defined, check member is of one of those groups
            IList<string> allowGroupsList = allowGroups as IList<string> ?? allowGroups.ToList();
            if (allowAction && allowGroupsList.Any(allowGroup => allowGroup != string.Empty))
            {
                // Allow only if member is assigned to a group in the list
                IList<string> groups = await GetRolesAsync(currentMember);
                allowAction = allowGroupsList.Select(s => s.ToLowerInvariant())
                    .Intersect(groups.Select(myGroup => myGroup.ToLowerInvariant())).Any();
            }
        }

        return allowAction;
    }

    /// <inheritdoc />
    public bool IsLoggedIn()
    {
        HttpContext? httpContext = _httpContextAccessor.HttpContext;
        return httpContext?.User.Identity?.IsAuthenticated ?? false;
    }

    /// <inheritdoc />
    public async Task<bool> MemberHasAccessAsync(string path)
    {
        if (await IsProtectedAsync(path))
        {
            return await HasAccessAsync(path);
        }

        return true;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<string, bool>> MemberHasAccessAsync(IEnumerable<string> paths)
    {
        IReadOnlyDictionary<string, bool> protectedPaths = await IsProtectedAsync(paths);

        IEnumerable<string> pathsWithProtection = protectedPaths.Where(x => x.Value).Select(x => x.Key);
        IReadOnlyDictionary<string, bool> pathsWithAccess = await HasAccessAsync(pathsWithProtection);

        var result = new Dictionary<string, bool>();
        foreach (var path in paths)
        {
            pathsWithAccess.TryGetValue(path, out var hasAccess);

            // if it's not found it's false anyways
            result[path] = !pathsWithProtection.Contains(path) || hasAccess;
        }

        return result;
    }

    /// <inheritdoc />
    /// <remarks>
    ///     this is a cached call
    /// </remarks>
    public Task<bool> IsProtectedAsync(string path) => Task.FromResult(_publicAccessService.IsProtected(path).Success);

    /// <inheritdoc />
    public Task<IReadOnlyDictionary<string, bool>> IsProtectedAsync(IEnumerable<string> paths)
    {
        var result = new Dictionary<string, bool>();
        foreach (var path in paths)
        {
            // this is a cached call
            result[path] = _publicAccessService.IsProtected(path).Success;
        }

        return Task.FromResult((IReadOnlyDictionary<string, bool>)result);
    }

    /// <inheritdoc />
    public async Task<MemberIdentityUser?> GetCurrentMemberAsync()
    {
        if (_currentMember == null)
        {
            if (!IsLoggedIn())
            {
                return null;
            }

            _currentMember = await GetUserAsync(_httpContextAccessor.HttpContext?.User);
        }

        return _currentMember;
    }

    public IPublishedContent? AsPublishedMember(MemberIdentityUser user) => _store.GetPublishedMember(user);

    /// <summary>
    ///     This will check if the member has access to this path
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private async Task<bool> HasAccessAsync(string path)
    {
        MemberIdentityUser? currentMember = await GetCurrentMemberAsync();
        if (currentMember == null || !currentMember.IsApproved || currentMember.IsLockedOut)
        {
            return false;
        }

        return await _publicAccessService.HasAccessAsync(
            path,
            currentMember.UserName,
            async () => await GetRolesAsync(currentMember));
    }

    private async Task<IReadOnlyDictionary<string, bool>> HasAccessAsync(IEnumerable<string> paths)
    {
        var result = new Dictionary<string, bool>();
        MemberIdentityUser? currentMember = await GetCurrentMemberAsync();

        if (currentMember == null || !currentMember.IsApproved || currentMember.IsLockedOut)
        {
            return result;
        }

        // ensure we only lookup user roles once
        IList<string>? userRoles = null;

        async Task<IList<string>> GetUserRolesAsync()
        {
            if (userRoles != null)
            {
                return userRoles;
            }

            userRoles = await GetRolesAsync(currentMember);
            return userRoles;
        }

        foreach (var path in paths)
        {
            result[path] = await _publicAccessService.HasAccessAsync(
                path,
                currentMember.UserName,
                async () => await GetUserRolesAsync());
        }

        return result;
    }
}
