using Microsoft.AspNetCore.Authorization;

namespace Umbraco.Cms.Api.Management.Security.Authorization.Content;

/// <summary>
///     Authorizes that the current user has the correct permission access to the content recycle bin item.
/// </summary>
public class ContentRecycleBinPermissionHandler : MustSatisfyRequirementAuthorizationHandler<ContentPermissionRequirement, IEnumerable<char>>
{
    private readonly IContentPermissionAuthorizer _contentPermissionAuthorizer;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentRecycleBinPermissionHandler" /> class.
    /// </summary>
    /// <param name="contentPermissionAuthorizer">Authorizer for content access.</param>
    public ContentRecycleBinPermissionHandler(IContentPermissionAuthorizer contentPermissionAuthorizer)
        => _contentPermissionAuthorizer = contentPermissionAuthorizer;

    /// <inheritdoc />
    protected override async Task<bool> IsAuthorized(
        AuthorizationHandlerContext context,
        ContentPermissionRequirement requirement,
        IEnumerable<char> resource) =>
        await _contentPermissionAuthorizer.IsAuthorizedAtRecycleBinLevelAsync(context.User, resource.ToList().AsReadOnly());
}
