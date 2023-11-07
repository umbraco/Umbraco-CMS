using Microsoft.AspNetCore.Authorization;

namespace Umbraco.Cms.Api.Management.Security.Authorization.Content.Branch;

/// <summary>
///     Authorizes that the current user has the correct permission access to the descendants of the specified content item.
/// </summary>
public class ContentBranchPermissionHandler : MustSatisfyRequirementAuthorizationHandler<ContentPermissionRequirement, ContentBranchPermissionResource>
{
    private readonly IContentPermissionAuthorizer _contentPermissionAuthorizer;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentBranchPermissionHandler" /> class.
    /// </summary>
    /// <param name="contentPermissionAuthorizer">Authorizer for content access.</param>
    public ContentBranchPermissionHandler(IContentPermissionAuthorizer contentPermissionAuthorizer)
        => _contentPermissionAuthorizer = contentPermissionAuthorizer;

    protected override async Task<bool> IsAuthorized(
        AuthorizationHandlerContext context,
        ContentPermissionRequirement requirement,
        ContentBranchPermissionResource resource) =>
        await _contentPermissionAuthorizer.IsAuthorizedWithDescendantsAsync(context.User, resource.ParentKey, resource.PermissionsToCheck);
}
