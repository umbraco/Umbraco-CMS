using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Security;

public class BackOfficeSecurity : IBackOfficeSecurity
{
    private readonly Lock _currentUserLock = new();
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserService _userService;
    private IUser? _currentUser;

    public BackOfficeSecurity(
        IUserService userService,
        IHttpContextAccessor httpContextAccessor)
    {
        _userService = userService;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
    public IUser? CurrentUser
    {
        get
        {
            // only load it once per instance! (but make sure groups are loaded)
            if (_currentUser == null)
            {
                lock (_currentUserLock)
                {
                    // Check again
                    if (_currentUser == null)
                    {
                        Attempt<Guid> keyAttempt = GetUserKey();
                        _currentUser = keyAttempt.Success ? _userService.GetAsync(keyAttempt.Result).GetAwaiter().GetResult() : null;
                    }
                }
            }

            return _currentUser;
        }
    }

    private Attempt<Guid> GetUserKey()
    {
        ClaimsIdentity? identity = _httpContextAccessor.HttpContext?.GetCurrentIdentity();

        Guid? id = identity?.GetUserKey();
        return id.HasValue is false ? Attempt.Fail<Guid>() : Attempt.Succeed(id.Value);
    }

    /// <inheritdoc />
    public Attempt<int> GetUserId()
    {
        ClaimsIdentity? identity = _httpContextAccessor.HttpContext?.GetCurrentIdentity();

        var id = identity?.GetId();
        return id.HasValue is false ? Attempt.Fail<int>() : Attempt.Succeed(id.Value);
    }

    /// <inheritdoc />
    public bool IsAuthenticated()
    {
        HttpContext? httpContext = _httpContextAccessor.HttpContext;
        return httpContext?.User != null && (httpContext.User.Identity?.IsAuthenticated ?? false) &&
               httpContext.GetCurrentIdentity() != null;
    }

    /// <inheritdoc />
    public bool UserHasSectionAccess(string section, IUser user) => user.HasSectionAccess(section);
}
