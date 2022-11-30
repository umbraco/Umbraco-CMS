// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.BackOffice.Controllers;

namespace Umbraco.Cms.Web.BackOffice.Authorization;

/// <summary>
///     Authorizes that the current user has access to the user group Id in the request
/// </summary>
public class UserGroupHandler : MustSatisfyRequirementAuthorizationHandler<UserGroupRequirement>
{
    private readonly AppCaches _appCaches;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IContentService _contentService;
    private readonly IEntityService _entityService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMediaService _mediaService;
    private readonly IUserService _userService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UserGroupHandler" /> class.
    /// </summary>
    /// <param name="httpContextAccessor">Accessor for the HTTP context of the current request.</param>
    /// <param name="userService">Service for user related operations.</param>
    /// <param name="contentService">Service for content related operations.</param>
    /// <param name="mediaService">Service for media related operations.</param>
    /// <param name="entityService">Service for entity related operations.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back-office security.</param>
    public UserGroupHandler(
        IHttpContextAccessor httpContextAccessor,
        IUserService userService,
        IContentService contentService,
        IMediaService mediaService,
        IEntityService entityService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        AppCaches appCaches)
    {
        _httpContextAccessor = httpContextAccessor;
        _userService = userService;
        _contentService = contentService;
        _mediaService = mediaService;
        _entityService = entityService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _appCaches = appCaches;
    }

    /// <inheritdoc />
    protected override Task<bool> IsAuthorized(AuthorizationHandlerContext context, UserGroupRequirement requirement)
    {
        IUser? currentUser = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser;

        StringValues? querystring = _httpContextAccessor.HttpContext?.Request.Query[requirement.QueryStringName];
        if (querystring is null)
        {
            // Must succeed this requirement since we cannot process it.
            return Task.FromResult(true);
        }

        if (querystring.Value.Count == 0)
        {
            // Must succeed this requirement since we cannot process it.
            return Task.FromResult(true);
        }

        var intIds = querystring.Value.ToString().Split(Constants.CharArrays.Comma)
            .Select(x =>
                int.TryParse(x, NumberStyles.Integer, CultureInfo.InvariantCulture, out var output)
                    ? Attempt<int>.Succeed(output)
                    : Attempt<int>.Fail())
            .Where(x => x.Success).Select(x => x.Result).ToArray();

        var authHelper = new UserGroupEditorAuthorizationHelper(
            _userService,
            _contentService,
            _mediaService,
            _entityService,
            _appCaches);

        Attempt<string?> isAuth = authHelper.AuthorizeGroupAccess(currentUser, intIds);

        return Task.FromResult(isAuth.Success);
    }
}
