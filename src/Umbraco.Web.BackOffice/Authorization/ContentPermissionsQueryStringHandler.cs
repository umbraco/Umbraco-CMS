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
///     Used to authorize if the user has the correct permission access to the content for the content id specified in a
///     query string.
/// </summary>
public class
    ContentPermissionsQueryStringHandler : PermissionsQueryStringHandler<ContentPermissionsQueryStringRequirement>
{
    private readonly ContentPermissions _contentPermissions;

    protected override UmbracoObjectTypes KeyParsingFilterType => UmbracoObjectTypes.Document;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentPermissionsQueryStringHandler" /> class.
    /// </summary>
    /// <param name="backOfficeSecurityAccessor">Accessor for back-office security.</param>
    /// <param name="httpContextAccessor">Accessor for the HTTP context of the current request.</param>
    /// <param name="entityService">Service for entity operations.</param>
    /// <param name="contentPermissions">Helper for content authorization checks.</param>
    public ContentPermissionsQueryStringHandler(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IHttpContextAccessor httpContextAccessor,
        IEntityService entityService,
        ContentPermissions contentPermissions)
        : base(backOfficeSecurityAccessor, httpContextAccessor, entityService) =>
        _contentPermissions = contentPermissions;

    /// <inheritdoc />
    protected override Task<bool> IsAuthorized(AuthorizationHandlerContext context, ContentPermissionsQueryStringRequirement requirement)
    {
        int nodeId;
        if (requirement.NodeId.HasValue == false)
        {
            if (HttpContextAccessor.HttpContext is null || requirement.QueryStringName is null ||
                !HttpContextAccessor.HttpContext.Request.Query.TryGetValue(requirement.QueryStringName, out StringValues routeVal))
            {
                // Must succeed this requirement since we cannot process it
                return Task.FromResult(true);
            }

            // Handle case where the incoming querystring could contain more than one value (e.g. ?id=1000&id=1001).
            // It's the first one that'll be processed by the protected method so we should verify that.
            var argument = routeVal.Count == 1
                ? routeVal.ToString()
                : routeVal.FirstOrDefault()?.ToString() ?? string.Empty;

            if (!TryParseNodeId(argument, out nodeId))
            {
                // Must succeed this requirement since we cannot process it.
                return Task.FromResult(true);
            }
        }
        else
        {
            nodeId = requirement.NodeId.Value;
        }

        ContentPermissions.ContentAccess permissionResult = _contentPermissions.CheckPermissions(
            nodeId,
            BackOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser,
            out IContent? contentItem,
            new[] { requirement.PermissionToCheck });

        if (HttpContextAccessor.HttpContext is not null && contentItem is not null)
        {
            // Store the content item in request cache so it can be resolved in the controller without re-looking it up.
            HttpContextAccessor.HttpContext.Items[typeof(IContent).ToString()] = contentItem;
        }

        return permissionResult switch
        {
            ContentPermissions.ContentAccess.Denied => Task.FromResult(false),
            _ => Task.FromResult(true)
        };
    }
}
