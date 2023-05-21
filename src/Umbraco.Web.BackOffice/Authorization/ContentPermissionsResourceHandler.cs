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
public class ContentPermissionsResourceHandler : MustSatisfyRequirementAuthorizationHandler<
    ContentPermissionsResourceRequirement, ContentPermissionsResource>
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly ContentPermissions _contentPermissions;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentPermissionsResourceHandler" /> class.
    /// </summary>
    /// <param name="backOfficeSecurityAccessor">Accessor for back-office security.</param>
    /// <param name="contentPermissions">Helper for content authorization checks.</param>
    public ContentPermissionsResourceHandler(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        ContentPermissions contentPermissions)
    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _contentPermissions = contentPermissions;
    }

    /// <inheritdoc />
    protected override Task<bool> IsAuthorized(AuthorizationHandlerContext context,
        ContentPermissionsResourceRequirement requirement, ContentPermissionsResource resource)
    {
        ContentPermissions.ContentAccess permissionResult = resource.NodeId.HasValue
            ? _contentPermissions.CheckPermissions(
                resource.NodeId.Value,
                _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser,
                out IContent? _,
                resource.PermissionsToCheck)
            : _contentPermissions.CheckPermissions(
                resource.Content,
                _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser,
                resource.PermissionsToCheck);

        return Task.FromResult(permissionResult != ContentPermissions.ContentAccess.Denied);
    }
}
