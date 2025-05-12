// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Web.BackOffice.Authorization;

/// <summary>
///     Used to authorize if the user has the correct permission access to the media for the media id specified in a query
///     string.
/// </summary>
public class MediaPermissionsQueryStringHandler : PermissionsQueryStringHandler<MediaPermissionsQueryStringRequirement>
{
    private readonly MediaPermissions _mediaPermissions;

    protected override UmbracoObjectTypes KeyParsingFilterType => UmbracoObjectTypes.Media;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaPermissionsQueryStringHandler" /> class.
    /// </summary>
    /// <param name="backOfficeSecurityAccessor">Accessor for back-office security.</param>
    /// <param name="httpContextAccessor">Accessor for the HTTP context of the current request.</param>
    /// <param name="entityService">Service for entity operations.</param>
    /// <param name="mediaPermissions">Helper for media authorization checks.</param>
    public MediaPermissionsQueryStringHandler(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IHttpContextAccessor httpContextAccessor,
        IEntityService entityService,
        MediaPermissions mediaPermissions)
        : base(backOfficeSecurityAccessor, httpContextAccessor, entityService) => _mediaPermissions = mediaPermissions;

    /// <inheritdoc />
    protected override Task<bool> IsAuthorized(AuthorizationHandlerContext context,
        MediaPermissionsQueryStringRequirement requirement)
    {
        if (HttpContextAccessor.HttpContext is null ||
            !HttpContextAccessor.HttpContext.Request.Query.TryGetValue(requirement.QueryStringName,
                out StringValues routeVal))
        {
            // Must succeed this requirement since we cannot process it.
            return Task.FromResult(true);
        }

        // Handle case where the incoming querystring could contain more than one value (e.g. ?id=1000&id=1001).
        // It's the first one that'll be processed by the protected method so we should verify that.
        var argument = routeVal.Count == 1
            ? routeVal.ToString()
            : routeVal.FirstOrDefault()?.ToString() ?? string.Empty;

        if (!TryParseNodeId(argument, out var nodeId))
        {
            // Must succeed this requirement since we cannot process it.
            return Task.FromResult(true);
        }

        MediaPermissions.MediaAccess permissionResult = _mediaPermissions.CheckPermissions(
            BackOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser,
            nodeId,
            out IMedia? mediaItem);

        if (mediaItem != null)
        {
            // Store the media item in request cache so it can be resolved in the controller without re-looking it up.
            HttpContextAccessor.HttpContext.Items[typeof(IMedia).ToString()] = mediaItem;
        }

        return permissionResult switch
        {
            MediaPermissions.MediaAccess.Denied => Task.FromResult(false),
            _ => Task.FromResult(true)
        };
    }
}
