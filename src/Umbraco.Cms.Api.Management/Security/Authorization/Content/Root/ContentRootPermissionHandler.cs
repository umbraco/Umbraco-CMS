using Microsoft.AspNetCore.Authorization;

namespace Umbraco.Cms.Api.Management.Security.Authorization.Content.Root;

/// <summary>
///     Authorizes that the current user has the correct permission access to the content root item.
/// </summary>
public class ContentRootPermissionHandler : MustSatisfyRequirementAuthorizationHandler<ContentRootPermissionRequirement, IEnumerable<char>>
{
    private readonly IContentPermissionAuthorizer _contentPermissionAuthorizer;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentRootPermissionHandler" /> class.
    /// </summary>
    /// <param name="contentPermissionAuthorizer">Authorizer for content access.</param>
    public ContentRootPermissionHandler(IContentPermissionAuthorizer contentPermissionAuthorizer)
        => _contentPermissionAuthorizer = contentPermissionAuthorizer;

    /// <inheritdoc />
    protected override async Task<bool> IsAuthorized(
        AuthorizationHandlerContext context,
        ContentRootPermissionRequirement requirement,
        IEnumerable<char> resource) =>
        await _contentPermissionAuthorizer.IsAuthorizedAtRootLevelAsync(context.User, resource.ToList().AsReadOnly());
}
