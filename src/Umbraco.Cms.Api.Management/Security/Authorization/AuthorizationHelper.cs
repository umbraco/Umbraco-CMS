using System.Security.Claims;
using System.Security.Principal;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Security.Authorization;

/// <inheritdoc />
internal sealed class AuthorizationHelper : IAuthorizationHelper
{
    private readonly IUserService _userService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AuthorizationHelper" /> class.
    /// </summary>
    /// <param name="userService">Service for user related operations.</param>
    public AuthorizationHelper(IUserService userService)
        => _userService = userService;

    /// <inheritdoc/>
    public IUser? GetCurrentUser(IPrincipal? currentUser)
    {
        IUser? user = null;
        ClaimsIdentity? umbIdentity = currentUser?.GetUmbracoIdentity();
        Guid? currentUserKey = umbIdentity?.GetUserKey();

        if (currentUserKey is null)
        {
            var currentUserId = umbIdentity?.GetUserId<int>();
            if (currentUserId.HasValue)
            {
                user = _userService.GetUserById(currentUserId.Value);
            }
        }
        else
        {
            user = _userService.GetAsync(currentUserKey.Value).GetAwaiter().GetResult();
        }

        return user;
    }
}
