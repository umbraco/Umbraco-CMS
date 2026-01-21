using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services.AuthorizationStatus;

namespace Umbraco.Cms.Core.Services;

/// <inheritdoc />
internal sealed class MediaPermissionService : IMediaPermissionService
{
    private readonly IEntityService _entityService;
    private readonly AppCaches _appCaches;

    public MediaPermissionService(
        IMediaService mediaService,
        IEntityService entityService,
        AppCaches appCaches)
    {
        _entityService = entityService;
        _appCaches = appCaches;
    }

    /// <inheritdoc/>
    public Task<MediaAuthorizationStatus> AuthorizeAccessAsync(IUser user, IEnumerable<Guid> mediaKeys)
    {
        Guid[] keysArray = mediaKeys.ToArray();

        if (keysArray.Length == 0)
        {
            return Task.FromResult(MediaAuthorizationStatus.Success);
        }

        // Use GetAllPaths instead of loading full media items - we only need paths for authorization
        TreeEntityPath[] entityPaths = _entityService.GetAllPaths(UmbracoObjectTypes.Media, keysArray).ToArray();

        if (entityPaths.Length == 0)
        {
            return Task.FromResult(MediaAuthorizationStatus.NotFound);
        }

        // Check if all requested keys were found
        if (entityPaths.Length != keysArray.Length)
        {
            return Task.FromResult(MediaAuthorizationStatus.NotFound);
        }

        // Check path access using the paths directly
        int[]? startNodeIds = user.CalculateMediaStartNodeIds(_entityService, _appCaches);
        foreach (TreeEntityPath entityPath in entityPaths)
        {
            if (ContentPermissions.HasPathAccess(entityPath.Path, startNodeIds, Constants.System.RecycleBinMedia) == false)
            {
                return Task.FromResult(MediaAuthorizationStatus.UnauthorizedMissingPathAccess);
            }
        }

        return Task.FromResult(MediaAuthorizationStatus.Success);
    }

    /// <inheritdoc/>
    public Task<MediaAuthorizationStatus> AuthorizeRootAccessAsync(IUser user)
        => Task.FromResult(user.HasMediaRootAccess(_entityService, _appCaches)
            ? MediaAuthorizationStatus.Success
            : MediaAuthorizationStatus.UnauthorizedMissingRootAccess);

    /// <inheritdoc/>
    public Task<MediaAuthorizationStatus> AuthorizeBinAccessAsync(IUser user)
        => Task.FromResult(user.HasMediaBinAccess(_entityService, _appCaches)
            ? MediaAuthorizationStatus.Success
            : MediaAuthorizationStatus.UnauthorizedMissingBinAccess);

    /// <inheritdoc/>
    public Task<ISet<Guid>> FilterAuthorizedAccessAsync(IUser user, IEnumerable<Guid> mediaKeys)
    {
        Guid[] keysArray = mediaKeys.ToArray();

        if (keysArray.Length == 0)
        {
            return Task.FromResult<ISet<Guid>>(new HashSet<Guid>());
        }

        // Retrieve paths in a a single database query for all keys.
        TreeEntityPath[] entityPaths = _entityService.GetAllPaths(UmbracoObjectTypes.Media, keysArray).ToArray();

        if (entityPaths.Length == 0)
        {
            return Task.FromResult<ISet<Guid>>(new HashSet<Guid>());
        }

        var authorizedKeys = new HashSet<Guid>();
        int[]? startNodeIds = user.CalculateMediaStartNodeIds(_entityService, _appCaches);

        foreach (TreeEntityPath entityPath in entityPaths)
        {
            // Check path access (media doesn't have granular permissions like content)
            if (ContentPermissions.HasPathAccess(entityPath.Path, startNodeIds, Constants.System.RecycleBinMedia) == false)
            {
                continue;
            }

            // Get the key for this entity's ID
            Attempt<Guid> keyAttempt = _entityService.GetKey(entityPath.Id, UmbracoObjectTypes.Media);
            if (keyAttempt.Success)
            {
                authorizedKeys.Add(keyAttempt.Result);
            }
        }

        return Task.FromResult<ISet<Guid>>(authorizedKeys);
    }
}
