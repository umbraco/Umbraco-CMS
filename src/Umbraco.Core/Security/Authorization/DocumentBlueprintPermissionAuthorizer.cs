using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.AuthorizationStatus;

namespace Umbraco.Cms.Core.Security.Authorization;

/// <inheritdoc />
internal sealed class DocumentBlueprintPermissionAuthorizer : IDocumentBlueprintPermissionAuthorizer
{
    private readonly IDocumentBlueprintPermissionService _documentBlueprintPermissionService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DocumentBlueprintPermissionAuthorizer" /> class.
    /// </summary>
    /// <param name="documentBlueprintPermissionService">The document blueprint permission service.</param>
    public DocumentBlueprintPermissionAuthorizer(IDocumentBlueprintPermissionService documentBlueprintPermissionService) =>
        _documentBlueprintPermissionService = documentBlueprintPermissionService;

    /// <inheritdoc />
    public async Task<bool> IsDeniedAsync(IUser currentUser, IEnumerable<Guid> documentBlueprintKeys)
    {
        var documentBlueprintKeyList = documentBlueprintKeys.ToList();
        if (documentBlueprintKeyList.Count == 0)
        {
            // Must succeed this requirement since we cannot process it.
            return true;
        }

        DocumentBlueprintAuthorizationStatus result =
            await _documentBlueprintPermissionService.AuthorizeAccessAsync(currentUser, documentBlueprintKeyList);

        // A blueprint that cannot be found is left to the endpoint to report as missing, so it is not denied here.
        return result is not (DocumentBlueprintAuthorizationStatus.Success or DocumentBlueprintAuthorizationStatus.NotFound);
    }

    /// <inheritdoc/>
    public async Task<bool> IsDeniedAtRootLevelAsync(IUser currentUser)
    {
        DocumentBlueprintAuthorizationStatus result =
            await _documentBlueprintPermissionService.AuthorizeRootAccessAsync(currentUser);

        return result is not DocumentBlueprintAuthorizationStatus.Success;
    }
}
