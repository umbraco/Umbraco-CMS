using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services.AuthorizationStatus;

namespace Umbraco.Cms.Core.Services;

/// <inheritdoc />
internal sealed class DocumentBlueprintPermissionService : IDocumentBlueprintPermissionService
{
    private readonly IEntityService _entityService;
    private readonly AppCaches _appCaches;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DocumentBlueprintPermissionService" /> class.
    /// </summary>
    /// <param name="entityService">The entity service.</param>
    /// <param name="appCaches">The application caches.</param>
    public DocumentBlueprintPermissionService(
        IEntityService entityService,
        AppCaches appCaches)
    {
        _entityService = entityService;
        _appCaches = appCaches;
    }

    /// <inheritdoc/>
    public Task<DocumentBlueprintAuthorizationStatus> AuthorizeAccessAsync(IUser user, IEnumerable<Guid> documentBlueprintKeys)
    {
        Guid[] keysArray = documentBlueprintKeys.ToArray();

        if (keysArray.Length == 0)
        {
            return Task.FromResult(DocumentBlueprintAuthorizationStatus.Success);
        }

        // A key may identify either a blueprint or one of the containers holding them. They are separate
        // object types but share the same start nodes, so both are resolved here and checked the same way.
        TreeEntityPath[] entityPaths = _entityService
            .GetAllPaths(
                [UmbracoObjectTypes.DocumentBlueprint, UmbracoObjectTypes.DocumentBlueprintContainer],
                keysArray)
            .ToArray();

        if (entityPaths.Length == 0)
        {
            return Task.FromResult(DocumentBlueprintAuthorizationStatus.NotFound);
        }

        var startNodeIds = user.CalculateDocumentBlueprintStartNodeIds(_entityService, _appCaches);
        foreach (TreeEntityPath entityPath in entityPaths)
        {
            if (ContentPermissions.HasPathAccessWithoutRecycleBin(entityPath.Path, startNodeIds) is false)
            {
                return Task.FromResult(DocumentBlueprintAuthorizationStatus.UnauthorizedMissingPathAccess);
            }
        }

        return Task.FromResult(DocumentBlueprintAuthorizationStatus.Success);
    }

    /// <inheritdoc/>
    public Task<DocumentBlueprintAuthorizationStatus> AuthorizeRootAccessAsync(IUser user)
        => Task.FromResult(user.HasDocumentBlueprintRootAccess(_entityService, _appCaches)
            ? DocumentBlueprintAuthorizationStatus.Success
            : DocumentBlueprintAuthorizationStatus.UnauthorizedMissingRootAccess);
}
