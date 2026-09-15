using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security.Authorization;

namespace Umbraco.Cms.Api.Management.Security.Authorization.DocumentBlueprint;

/// <summary>
///     Authorizes that the current user has access to the document blueprint(s) specified in the request.
/// </summary>
public class DocumentBlueprintPermissionHandler : MustSatisfyRequirementAuthorizationHandler<DocumentBlueprintPermissionRequirement, DocumentBlueprintPermissionResource>
{
    private readonly IAuthorizationHelper _authorizationHelper;
    private readonly IDocumentBlueprintPermissionAuthorizer _documentBlueprintPermissionAuthorizer;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DocumentBlueprintPermissionHandler" /> class.
    /// </summary>
    /// <param name="documentBlueprintPermissionAuthorizer">Authorizer for document blueprint access.</param>
    /// <param name="authorizationHelper">The authorization helper.</param>
    public DocumentBlueprintPermissionHandler(
        IDocumentBlueprintPermissionAuthorizer documentBlueprintPermissionAuthorizer,
        IAuthorizationHelper authorizationHelper)
    {
        _documentBlueprintPermissionAuthorizer = documentBlueprintPermissionAuthorizer;
        _authorizationHelper = authorizationHelper;
    }

    /// <inheritdoc />
    protected override async Task<bool> IsAuthorized(
        AuthorizationHandlerContext context,
        DocumentBlueprintPermissionRequirement requirement,
        DocumentBlueprintPermissionResource resource)
    {
        var result = true;

        IUser user = _authorizationHelper.GetUmbracoUser(context.User);
        if (resource.CheckRoot)
        {
            result &= await _documentBlueprintPermissionAuthorizer.IsDeniedAtRootLevelAsync(user) is false;
        }

        if (resource.DocumentBlueprintKeys.Any())
        {
            result &= await _documentBlueprintPermissionAuthorizer.IsDeniedAsync(user, resource.DocumentBlueprintKeys) is false;
        }

        return result;
    }
}
