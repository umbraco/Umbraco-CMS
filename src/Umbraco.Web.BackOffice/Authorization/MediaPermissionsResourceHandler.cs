// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Web.BackOffice.Authorization;

/// <summary>
///     Used to authorize if the user has the correct permission access to the content for the <see cref="IContent" />
///     specified.
/// </summary>
public class MediaPermissionsResourceHandler : MustSatisfyRequirementAuthorizationHandler<
    MediaPermissionsResourceRequirement, MediaPermissionsResource>
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly MediaPermissions _mediaPermissions;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaPermissionsResourceHandler" /> class.
    /// </summary>
    /// <param name="backOfficeSecurityAccessor">Accessor for back-office security.</param>
    /// <param name="mediaPermissions">Helper for media authorization checks.</param>
    public MediaPermissionsResourceHandler(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        MediaPermissions mediaPermissions)
    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _mediaPermissions = mediaPermissions;
    }

    /// <inheritdoc />
    protected override Task<bool> IsAuthorized(AuthorizationHandlerContext context,
        MediaPermissionsResourceRequirement requirement, MediaPermissionsResource resource)
    {
        MediaPermissions.MediaAccess permissionResult = resource.NodeId.HasValue
            ? _mediaPermissions.CheckPermissions(
                _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser,
                resource.NodeId.Value,
                out _)
            : _mediaPermissions.CheckPermissions(
                resource.Media,
                _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser);

        return Task.FromResult(permissionResult != MediaPermissions.MediaAccess.Denied);
    }
}
